using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using TeamDevTool.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using Task = System.Threading.Tasks.Task;

namespace TeamDevTool.Services.FileInfoService
{
    public class FileInfoService : IFileInfoService, SFileInfoService
    {
        #region Private Varient
        /// <summary>
        /// 用于获取服务
        /// </summary>
        private IAsyncServiceProvider _AsyncServiceProvider;

        /// <summary>
        /// GUID项目字典
        /// </summary>
        private ConcurrentDictionary<Guid, Project> _ProjectDict = new ConcurrentDictionary<Guid, Project>();

        /// <summary>
        /// 项目文件字典
        /// </summary>
        private ConcurrentDictionary<Guid, HashSet<ProjectItem>> _ProjectItemDict = new ConcurrentDictionary<Guid, HashSet<ProjectItem>>();

        /// <summary>
        /// 文件信息字典
        /// </summary>
        private ConcurrentDictionary<ProjectItem, ProjectItemInfo> _ProjectItemInfoDict = new ConcurrentDictionary<ProjectItem, ProjectItemInfo>();

        private object _LockObj = new object();
        #endregion

        #region Perporty

        
        #endregion

        #region Service

        public DTE DTE { get; private set; }

        public DTE2 DTE2 { get; private set; }

        public IVsSolution Solution { get; private set; }

        public IVsShell Shell { get; private set; }

        #endregion

        #region .Ctor
        public FileInfoService(IAsyncServiceProvider provider)
        {
            // 构造函数只用于简单初始化
            // 所有复杂操作均放在InitializeAsync()中，用以增强性能
            _AsyncServiceProvider = provider;
        }


        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            // 切换到低优先级线程
            // 在切换到主线程前完成IO操作
            // do background operations that involve IO or other async methods
            await TaskScheduler.Default;
            #region IO操作
            #endregion
            // 使用VS服务前需要切换到主线程（除非文档明确他们对线程没有要求）
            // 原因是使用VS服务时可能需要操作COM。
            // query Visual Studio services on main thread unless they are documented as free threaded explicitly.
            // The reason for this is the final cast to service interface (such as IVsShell) may involve COM operations to add/release references.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            #region 初始化系统服务

            Shell = await _AsyncServiceProvider.GetServiceAsync(typeof(SVsShell)) as IVsShell;
            Assumes.Present(Shell);

            DTE = await _AsyncServiceProvider.GetServiceAsync(typeof(DTE)) as DTE;
            Assumes.Present(DTE);

            DTE2 = DTE as DTE2;
            Assumes.Present(DTE2);

            Solution = await _AsyncServiceProvider.GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            Assumes.Present(Solution);

            #endregion
            var projects = GetAllProjectInCurrentSolution();
            UpdateProjectGuidDict(projects);
            RegisterProjectEvent();
            RegisterProjectItemEvent();
            GenerateFileDict();
            //切换到低优先线程处理文件
            await TaskScheduler.Default;
        }

        #endregion

        #region Interface
        public ProjectItemInfo GetProjectItemInfo(ProjectItem projectItem)
        {
            return _ProjectItemInfoDict[projectItem];
        }

        public IEnumerable<ProjectItemInfo> GetProjectItemByProject(Project project)
        {
            var guid = _ProjectDict.First(p => p.Value == project).Key;
            foreach (var item in _ProjectItemDict[guid])
            {
                yield return _ProjectItemInfoDict[item];
            }
        }
        #endregion

        #region Private Function
        /// <summary>
        /// 注册解决方案项目变动事件
        /// </summary>
        private void RegisterProjectEvent()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE.Events.SolutionEvents.ProjectAdded += (project) => { UpdateProjectInfo(project); };
            DTE.Events.SolutionEvents.ProjectRemoved += (project) => { UpdateProjectInfo(project); };
        }

        private void UpdateProjectInfo(Project project)
        {
            var guid = GetProjectGuid(project);
            if (_ProjectDict.ContainsKey(guid)) _ProjectDict.TryRemove(guid, out project);
            else _ProjectDict.TryAdd(guid, project);
        }

        private Guid GetProjectGuid(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsHierarchy hierarchy;
            Solution.GetProjectOfUniqueName(project.FullName, out hierarchy);

            if (hierarchy != null)
            {
                Guid projectGuid = Guid.Empty;
                Solution.GetGuidOfProject(hierarchy, out projectGuid);
                return projectGuid;
            }
            return Guid.Empty;
        }

        /// <summary>
        /// 注册项目文件事件
        /// </summary>
        private void RegisterProjectItemEvent()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Events2 events2 = DTE.Events as Events2;
            var commonEvent = events2.ProjectItemsEvents;
            RegisterProjectItemEventInternal(commonEvent);
            var cSharpEvent = DTE.Events.GetObject("CSharpProjectItemsEvents") as ProjectItemsEvents;
            RegisterProjectItemEventInternal(cSharpEvent);
        }

        private void RegisterProjectItemEventInternal(ProjectItemsEvents projectItemsEvents)
        {
            if (projectItemsEvents == null) throw new ArgumentNullException(nameof(projectItemsEvents));
            projectItemsEvents.ItemAdded += projectItem => { };
            projectItemsEvents.ItemRemoved += projectItem => { };
            projectItemsEvents.ItemRenamed += (projectItem, newName) => { };
        }

        private void UpdateProjectGuidDict(IEnumerable<Project> projects)
        {
            foreach (var project in projects)
            {
                var guid = GetProjectGuid(project);
                lock (_LockObj)
                {
                    var result = _ProjectDict.TryAdd(guid, project);
                    Assumes.True(result, "项目字典生成失败");
                }
            }
        }

        /// <summary>
        /// 获取当前解决方案所有项目
        /// </summary>
        /// <param name="DTE"></param>
        /// <returns></returns>
        private IEnumerable<Project> GetAllProjectInCurrentSolution()
        {
            // 访问项目前必须确保在UI线程
            ThreadHelper.ThrowIfNotOnUIThread();
            return this.DTE2.Solution.Projects
                /* OfType必须引入System.Linq;命名空间 */
                .OfType<Project>().SelectMany(GetProjects)
                .Where(project => { ThreadHelper.ThrowIfNotOnUIThread(); return File.Exists(project.FullName); });
        } 
        
        /// <summary>
        /// 获取子项目
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private IEnumerable<Project> GetProjects(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (project == null) return Enumerable.Empty<Project>();
            if (project.Kind != ProjectKinds.vsProjectKindSolutionFolder) return new[] { project };
            return project.ProjectItems.OfType<ProjectItem>()
                .SelectMany(p => { ThreadHelper.ThrowIfNotOnUIThread(); return GetProjects(p.SubProject); });
        }

        /// <summary>
        /// 获取项目内Item字典
        /// </summary>
        private void GenerateFileDict()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (var pair in _ProjectDict)
            {
                HashSet<ProjectItem> projectItems = new HashSet<ProjectItem>();
                foreach (ProjectItem item in pair.Value.ProjectItems)
                {
                    GenerateProjcetItemDict(item, ref projectItems);
                }

                foreach (ProjectItem item in pair.Value.ProjectItems)
                {
                    SetRelation(item, null);
                }
                //UpdateProjectInfo(pair.Value.ProjectItems, null, ref projectItems);
                _ProjectItemDict.TryAdd(pair.Key, projectItems);
            }
        }

        private void SetRelation(ProjectItem current, ProjectItem parent)
        {
            if (current == null) return;
            ThreadHelper.ThrowIfNotOnUIThread();
            _ProjectItemInfoDict[current].Parent = parent != null ? _ProjectItemInfoDict[parent] : null;
            foreach (ProjectItem item in current.ProjectItems)
            {
                _ProjectItemInfoDict[current].Children.Add(_ProjectItemInfoDict[item]);
                SetRelation(item, current);
            }
        }

        private void GenerateProjcetItemDict(ProjectItem current, ref HashSet<ProjectItem> projectItems)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (current == null) return;
            projectItems.Add(current);
            ProjectItemInfo projectItemInfo = new ProjectItemInfo()
            {
                Name = current.Name,
                Self = current,
                /* 一个项目item（比如他是个文件夹，当然也可以有多个文件名）
                 * 索引从1开始
                 * https://docs.microsoft.com/en-us/dotnet/api/envdte.projectitem.filenames?view=visualstudiosdk-2019#EnvDTE_ProjectItem_FileNames_System_Int16_              
                 */
                Path = current.FileNames[1],
                ItemType = VsConstants.ProjectItemTypeDict[current.Kind],
                Project = current.ContainingProject
            };
            _ProjectItemInfoDict.TryAdd(current, projectItemInfo);
            foreach (ProjectItem item in current.ProjectItems)
            {
                GenerateProjcetItemDict(item, ref projectItems);
            }
        }

        //private void UpdateProjectInfo(ProjectItems projectItems, ProjectItem parent, ref HashSet<ProjectItem> resultHashSet)
        //{
        //    ThreadHelper.ThrowIfNotOnUIThread();
        //    if (projectItems == null) throw new ArgumentNullException(nameof(projectItems));
        //    if (resultHashSet == null) throw new ArgumentNullException(nameof(resultHashSet));
        //    foreach (ProjectItem item in projectItems)
        //    {
        //        resultHashSet.Add(item);
        //        ProjectItemInfo projectItemInfo = new ProjectItemInfo()
        //        {
        //            Name = item.Name,
        //            /* 一个项目item（比如他是个文件夹，当然也可以有多个文件名）
        //             * 索引从1开始
        //             * https://docs.microsoft.com/en-us/dotnet/api/envdte.projectitem.filenames?view=visualstudiosdk-2019#EnvDTE_ProjectItem_FileNames_System_Int16_              
        //             */
        //            Path = item.FileNames[1],
        //            ItemType = VsConstants.ProjectItemTypeDict[item.Kind],
        //            Parent = parent,
        //            Children = CastAsIEnumerable(item.ProjectItems),
        //            Project = projectItems.ContainingProject
        //        };
        //        _ProjectItemInfoDict.TryAdd(item, projectItemInfo);
        //        if (item.ProjectItems.Count > 0)
        //            UpdateProjectInfo(item.ProjectItems, item, ref resultHashSet);
        //    }
            
        //}

        //private IEnumerable<ProjectItem> CastAsIEnumerable(ProjectItems projectItems)
        //{
        //    foreach (ProjectItem item in projectItems)
        //    {
        //        yield return item;
        //    }
        //}

        #endregion
    }
}

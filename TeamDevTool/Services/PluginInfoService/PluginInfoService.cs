using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using TeamDevTool.Services.FileInfoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Task = System.Threading.Tasks.Task;
using System.Windows.Controls;

namespace TeamDevTool.Services.PluginInfoService
{
    /// <summary>
    /// 用来管理插件，显示当前设备，读取协议，提供插件不同信息
    /// </summary>
    public class PluginInfoService
    {
        #region Private Varient
        /// <summary>
        /// 用于获取服务
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider _AsyncServiceProvider;

        #endregion

        #region Service

        public DTE DTE { get; private set; }

        public DTE2 DTE2 { get; private set; }

        public IFileInfoService FileInfoService { get; private set; }

        #endregion

        #region Event
        /// <summary>
        /// 当前工作插件更换
        /// </summary>
        public event Action<ProjectPluginInfo> ActivePluginChanged;
        #endregion

        #region .Ctor
        public PluginInfoService(Microsoft.VisualStudio.Shell.IAsyncServiceProvider provider)
        {
            // 构造函数只用于简单初始化
            // 所有复杂操作均放在InitializeAsync()中，用以增强性能
            _AsyncServiceProvider = provider;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await TaskScheduler.Default;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            DTE = await _AsyncServiceProvider.GetServiceAsync(typeof(DTE)) as DTE;
            Assumes.Present(DTE);

            DTE2 = DTE as DTE2;
            Assumes.Present(DTE2);
            FileInfoService = await _AsyncServiceProvider.GetServiceAsync(typeof(SFileInfoService)) as IFileInfoService;
            RegisterDocumentEvent();
        }
        #endregion

        #region Private Function
        private void RegisterDocumentEvent()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // https://stackoverflow.com/questions/24163986/how-to-get-current-activedocument-in-visual-studio-extension-using-mef
            DTE2.Events.WindowEvents.WindowActivated += (Window GotFocus, Window LostFocus) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (null != GotFocus.Document)
                {
                    var currentItem = GotFocus.Document.ProjectItem;
                    ProjectItemInfo info = FileInfoService.GetProjectItemInfo(currentItem);
                    ProjectPluginInfo projectPluginInfo = GetPluginsByProject(info.Project);
                    ActivePluginChanged?.Invoke(projectPluginInfo);
                }
            };
        }

        private ProjectPluginInfo GetPluginsByProject(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (ProjectItem projectItem in project.ProjectItems)
            {
                ProjectItemInfo info = FileInfoService.GetProjectItemInfo(projectItem);
                if (info.Parent == null && info.ItemType == Utils.ProjectItemType.PhysicalFolder && info.Name == "Issue")
                {
                    ProjectPluginInfo projectPluginInfo = new ProjectPluginInfo();
                    foreach (ProjectItem item in info.Children)
                    {
                        var item_in_issue_folder = FileInfoService.GetProjectItemInfo(item);
                        if (item_in_issue_folder.Name == "Common")
                        {
                            projectPluginInfo.ProjecIssuetCommonInfo = item_in_issue_folder;
                        }
                        else
                        {
                            var config_file = item_in_issue_folder.Children.First(i => i.Name == "Config.xml");
                            var document = XDocument.Load(config_file.Path);
                            string id = QueryComponentID(document);
                            projectPluginInfo.PluginIssueInfo.Add(new PluginInfo()
                            {
                                ComponentID = id,
                                IssueProjectItemInfo = item_in_issue_folder,
                                ProjectPluginInfo = projectPluginInfo
                            });
                        }
                    }
                    return projectPluginInfo;
                }
            }
            return null;
        }

        private string QueryComponentID(XDocument document)
        {
            var query = from element in document.Root.Elements()
                        where element.Attribute("Name").Value == "ID"
                        select element.Attribute("Value").Value.ToString();
            return query.First();
        }
        #endregion
    }
}

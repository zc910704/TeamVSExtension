using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using TeamDevTool.Services.FileInfoService;
using TeamDevTool.Services.PluginInfoService;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using TeamDevTool.Views;

namespace TeamDevTool
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [ProvideService(typeof(SFileInfoService), IsAsyncQueryable = true)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    // 仅当需要时，Vspackage 才会加载到 Visual Studio 中,强制加载使用：https://docs.microsoft.com/zh-cn/visualstudio/extensibility/loading-vspackages?view=vs-2019 
    // 该API已在VS2019中弃用，加载时会提示
    // [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    // https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-use-asyncpackage-to-load-vspackages-in-the-background?view=vs-2019
    // 现在使用 BackgroundLoad + AsyncPackage 如下
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(DevToolPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class DevToolPackage : AsyncPackage
    {
        /// <summary>
        /// InternalDevToolPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "6658947c-e4ea-48c0-b4fc-6dd8a77c395b";

        /// <summary>
        /// Initializes a new instance of the <see cref="DevToolPackage"/> class.
        /// </summary>
        public DevToolPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            // 初始化前，必须首先将切换到UI线程
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await base.InitializeAsync(cancellationToken, progress);
            // 参数2：回调 参数3：是否外部可见
            // The creation callback to be invoked when an instance of the service is needed.This is only invoked one time and the result is cached.
            // 2:回调函数仅仅在调用服务时被调用一次
            this.AddService(typeof(SFileInfoService), FileInfoServiceCreateCallbackAsync, true);
            this.AddService(typeof(SPluginInfoService), PluginInfoServiceCreateCallbackAsync, true);
            // 在向vs服务注册前不能调用该服务
            var myservice1 = await this.GetServiceAsync(typeof(SFileInfoService)) as IFileInfoService;
            var myservice2 = await this.GetServiceAsync(typeof(SPluginInfoService)) as IPluginInfoService;
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            InitializeStatusBar();
        }

        private async Task<object> FileInfoServiceCreateCallbackAsync(IAsyncServiceContainer container, CancellationToken cancellationToken, Type serviceType)
        {
            FileInfoService service = new FileInfoService(this);
            await service.InitializeAsync(cancellationToken);
            return service;
        }

        private async Task<object> PluginInfoServiceCreateCallbackAsync(IAsyncServiceContainer container, CancellationToken cancellationToken, Type serviceType)
        {
            PluginInfoService service = new PluginInfoService(this);
            await service.InitializeAsync(cancellationToken);
            return service;
        }


        public void InitializeStatusBar()
        {
            // Find the status bar dock panel
            DockPanel statusBarDockPanel = GetStatusBarDockPanel();
            if (statusBarDockPanel == null)
            {
                System.Diagnostics.Debug.WriteLine("Error: Could not find status bar dock panel, therefore cannot add new button to status bar.");
                return;
            }

            // Create the new status bar button in a new status bar
            PluginStatusBar windowManagementStatusBar = new PluginStatusBar();

            // Add the Window Management status bar to the Status Bar dock panel at position 0 (far left)
            statusBarDockPanel.Children.Insert(0, windowManagementStatusBar);

        }

        private DockPanel GetStatusBarDockPanel()
        {
            DependencyObject rootGrid = VisualTreeHelper.GetChild(Application.Current.MainWindow, 0);

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(rootGrid); i++)
            {
                object o = VisualTreeHelper.GetChild(rootGrid, i);
                if (o != null && o is DockPanel)
                {
                    DockPanel dockPanel = o as DockPanel;
                    if (dockPanel.Name == "StatusBarPanel")
                    {
                        return dockPanel;
                    }
                }
            }

            //DependencyObject ddd = VisualTreeHelper.GetChild(rootGrid, 0);

            //for (int i = 0; i < VisualTreeHelper.GetChildrenCount(ddd); i++)
            //{
            //    object o = VisualTreeHelper.GetChild(ddd, i);
            //    if (o != null && o is DockPanel)
            //    {
            //        DockPanel dockPanel = o as DockPanel;
            //        if (dockPanel.Name == "StatusBarPanel")
            //        {
            //            return dockPanel;
            //        }
            //    }
            //}
            return null;
        }

        #endregion
    }
}

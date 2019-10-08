using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace TTExecuter.Commands
{
    internal sealed class RunAllTemplatesCommand
    {
        public const int CommandId = 0x1000;

        public static readonly Guid CommandSet = new Guid("03d68cc2-f822-4688-81f5-5e431f51c62d");

        private readonly AsyncPackage _package;

        private ProjectItemManager _manager;

        private RunAllTemplatesCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _manager = new ProjectItemManager();
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static RunAllTemplatesCommand Instance
        {
            get;
            private set;
        }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this._package;
            }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new RunAllTemplatesCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            var projects = dte.GetProjectsInBuildScope(vsBuildScope.vsBuildScopeSolution);
            var projectItems = _manager.GetT4ProjectItems(projects);

            _manager.ExecuteAllTemplates(projectItems);
        }
    }
}

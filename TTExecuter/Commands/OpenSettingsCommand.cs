using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Linq;
using VSLangProj;
using Task = System.Threading.Tasks.Task;

namespace TTExecuter
{
    internal sealed class OpenSettingsCommand
    {
        public const int CommandId = 256;
        public static readonly Guid CommandSet = new Guid("77605b7c-d09f-47b2-928e-552295580232");
        private readonly AsyncPackage package;
        private readonly ProjectItemManager _manager;

        private OpenSettingsCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _manager = new ProjectItemManager();
            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static OpenSettingsCommand Instance
        {
            get;
            private set;
        }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenSettingsCommand(package, commandService);
        }

        string GetTemplateName(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var templateVsProjectItem = item.Object as VSProjectItem;
            return templateVsProjectItem.ProjectItem.Name;
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            var projects = dte.GetProjectsInBuildScope(vsBuildScope.vsBuildScopeSolution);
            var projectItems = _manager.GetT4ProjectItems(projects);

            var templates = projectItems.Select(x => { ThreadHelper.ThrowIfNotOnUIThread(); return x.Name; }).ToArray();

            var window = new System.Windows.Window
            {
                Content = new TTExecuterSettingsControl(templates),
                Width = 520,
                Height = 320
            };

            window.Show();
        }
    }
}

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text.RegularExpressions;
using VSLangProj;
using Task = System.Threading.Tasks.Task;

namespace TTExecuter
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenSettingsCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("77605b7c-d09f-47b2-928e-552295580232");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSettingsCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private OpenSettingsCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OpenSettingsCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in OpenSettingsCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenSettingsCommand(package, commandService);
        }

        public IEnumerable<ProjectItem> GetT4ProjectItems(IEnumerable<Project> projects)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var regex = new Regex(@"\.[Tt][Tt]$");
            foreach (var project in projects)
            {
                foreach (var item in FindProjectItems(regex, project.ProjectItems))
                {
                    yield return item;
                }
            }
        }

        IEnumerable<ProjectItem> FindProjectItems(Regex regex, ProjectItems projectItems)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (ProjectItem projectItem in projectItems)
            {
                if (regex.IsMatch(projectItem.Name ?? ""))
                    yield return projectItem;

                if (projectItem.ProjectItems != null)
                {
                    foreach (var subItem in FindProjectItems(regex, projectItem.ProjectItems))
                        yield return subItem;
                }
                if (projectItem.SubProject != null)
                {
                    foreach (var subItem in FindProjectItems(regex, projectItem.SubProject.ProjectItems))
                        yield return subItem;
                }
            }
        }

        string GetTemplateName(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var templateVsProjectItem = item.Object as VSProjectItem;
            return templateVsProjectItem.ProjectItem.Name;
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            var projects = dte.GetProjectsInBuildScope(vsBuildScope.vsBuildScopeSolution);
            var projectItems = GetT4ProjectItems(projects);

            var templates = projectItems.Select(x => { ThreadHelper.ThrowIfNotOnUIThread(); return x.Name; }).ToArray();

            var window = new System.Windows.Window();
            window.Content = new TTExecuterSettingsControl(templates);
            window.Width = 400;
            window.Show();
        }
    }
}

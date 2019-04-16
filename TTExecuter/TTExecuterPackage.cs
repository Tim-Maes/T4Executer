using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace TTExecuter
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(TTExecuterPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class TTExecuterPackage : AsyncPackage
    {
        public const string PackageGuidString = "358d8597-b05d-4c5c-9078-5805b3bb7731";
        private DTE _dte;
        private BuildEvents _buildEvents;
        private readonly ProjectItemManager _manager;

        public TTExecuterPackage()
        {
            _manager = new ProjectItemManager();
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            base.Initialize();

            _dte = await GetServiceAsync(typeof(SDTE)) as DTE;
            if (_dte == null)
                return;

            RegisterEvents();
            await EnableDisableTTExecuterCommand.InitializeAsync(this);
            await OpenSettingsCommand.InitializeAsync(this);
        }

        private void RegisterEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _buildEvents = _dte.Events.BuildEvents;
            _buildEvents.OnBuildBegin += OnBuildBegin;
        }

        private void OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            var projects = _dte.GetProjectsInBuildScope(vsBuildScope.vsBuildScopeSolution);
            var projectItems = _manager.GetT4ProjectItems(projects);

            if (Settings.Default.EnableTTExecuter)
            {
                _manager.ExecuteTemplates(projectItems, scope);
            }
        }
    }
}

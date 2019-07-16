using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace T4Executer
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(PackageGuidString)]
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

            Assumes.Present(_dte);

            if (_dte == null)
                return;

            RegisterEvents();
            await EnableDisableTTExecuterCommand.InitializeAsync(this);
            await OpenSettingsCommand.InitializeAsync(this);
            await Commands.RunAllTemplatesCommand.InitializeAsync(this);
        }

        private void RegisterEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _buildEvents = _dte.Events.BuildEvents;
            _buildEvents.OnBuildBegin += OnBuildBegin;
            _buildEvents.OnBuildDone += OnBuildDone;
        }

        private IEnumerable<ProjectItem> GetProjectItems()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var projects = _dte.GetProjectsInBuildScope();
            return _manager.GetT4ProjectItems(projects);
        }

        private void OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IEnumerable<ProjectItem> projectItems = GetProjectItems();

            if (Settings.Default.EnableTTExecuter)
            {
                _manager.ExecuteTemplatesBeforeBuild(projectItems);
            }
        }

        private void OnBuildDone(vsBuildScope scope, vsBuildAction action)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IEnumerable<ProjectItem> projectItems = GetProjectItems();

            if (Settings.Default.EnableTTExecuter)
            {
                _manager.ExecuteTemplatesAfterBuild(projectItems);
            }
        }
    }
}

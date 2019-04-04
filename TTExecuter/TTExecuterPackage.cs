using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using VSLangProj;
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
       
        public TTExecuterPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            base.Initialize();

            _dte = await GetServiceAsync(typeof(SDTE)) as DTE;
            if (_dte == null)
                return;

            RegisterEvents();
        }

        private void RegisterEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _buildEvents = _dte.Events.BuildEvents;
            _buildEvents.OnBuildBegin += OnBuildBegin;
        }

        private void OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            ExecuteTemplates(scope);
        }

        private void ExecuteTemplates(vsBuildScope scope)
        {
            var projects = _dte.GetProjectsWithinBuildScope(vsBuildScope.vsBuildScopeSolution);
            var projectItems = GetT4ProjectItems(projects);

            foreach (var item in projectItems)
            {
                ExecuteTemplate(item);
            }
        }

        public void ExecuteTemplate(ProjectItem template)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var templateVsProjectItem = template.Object as VSProjectItem;
            if (templateVsProjectItem != null)
            {
                templateVsProjectItem.RunCustomTool();
            }
            else
            {
                if (!template.IsOpen)
                {
                    var window = template.Open();
                    template.Save();
                    window.Close();
                }
                else
                {
                    template.Save();
                }
            }
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
    }
}

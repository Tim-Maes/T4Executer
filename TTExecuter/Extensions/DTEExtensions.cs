using EnvDTE;
using System.Collections.Generic;
using System.Linq;

namespace TTExecuter
{
    public static class DTEExtensions
    {
        public static IEnumerable<Project> GetProjectsInBuildScope(this DTE dte, vsBuildScope scope)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            IEnumerable<Project> projects = null;

            switch (scope)
            {
                case vsBuildScope.vsBuildScopeSolution:
                    projects = dte.Solution.Projects.OfType<Project>();
                    break;
                case vsBuildScope.vsBuildScopeProject:
                    projects = ((object[])dte.ActiveSolutionProjects).OfType<Project>();
                    break;
            }

            return projects ?? Enumerable.Empty<Project>();
        }
    }
}

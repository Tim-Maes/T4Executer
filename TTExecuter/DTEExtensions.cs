using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTExecuter
{
    public static class DTEExtensions
    {
        public static IEnumerable<Project> GetProjectsInBuildScope(this DTE dte, vsBuildScope scope)
        {
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

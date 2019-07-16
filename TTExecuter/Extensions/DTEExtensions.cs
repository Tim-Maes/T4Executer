using EnvDTE;
using System.Collections.Generic;
using System.Linq;

namespace T4Executer
{
    public static class DTEExtensions
    {
        public static IEnumerable<Project> GetProjectsInBuildScope(this DTE dte)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            IEnumerable<Project> projects = null;
            projects = dte.Solution.Projects.OfType<Project>();
            return projects ?? Enumerable.Empty<Project>();
        }
    }
}

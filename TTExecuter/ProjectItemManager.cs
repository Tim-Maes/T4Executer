﻿using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VSLangProj;

namespace TTExecuter
{
    public class ProjectItemManager
    {
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

        public void ExecuteTemplate(ProjectItem template)
        {
            var ignoredTemplates = Settings.Default.IgnoreList;

            ThreadHelper.ThrowIfNotOnUIThread();
            var templateVsProjectItem = template.Object as VSProjectItem;

            if (templateVsProjectItem != null)
            {
                if (!ignoredTemplates.Contains(templateVsProjectItem.ProjectItem.Name)) templateVsProjectItem.RunCustomTool();
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
    }
}
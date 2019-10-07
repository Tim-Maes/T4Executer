using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using VSLangProj;

namespace TTExecuter
{
    public class ProjectItemManager
    {
        public void ExecuteTemplate(ProjectItem template)
        {
            var ignoredTemplates = Settings.Default.IgnoreList;

            ThreadHelper.ThrowIfNotOnUIThread();
            var templateVsProjectItem = template.Object as VSProjectItem;

            if (templateVsProjectItem != null)
            {
                bool ignore = false;
                if (ignoredTemplates != null && ignoredTemplates.Count > 0)
                {
                    if (ignoredTemplates.Contains(templateVsProjectItem.ProjectItem.Name))
                        ignore = true;
                }

                if (!ignore)
                {
                    string outputPath = Settings.Default.PreserveGeneratedFileTimestamp ? GetOutputPath(templateVsProjectItem) : null;
                    byte[] oldContent = null;
                    DateTime? lastWriteTime = null;
                    if (outputPath != null)
                    {
                        try
                        {
                            lastWriteTime = File.GetLastWriteTime(outputPath);
                            oldContent = File.ReadAllBytes(outputPath);
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    templateVsProjectItem.RunCustomTool();

                    // restore the generated file's write time
                    if (oldContent != null)
                    {
                        try
                        {
                            byte[] newContent = File.ReadAllBytes(outputPath);
                            if (CompareArrays(oldContent, newContent))
                            {
                                File.SetLastWriteTime(outputPath, lastWriteTime.Value);

                                // workaround for a Team Explorer bug
                                // open/close file to remove the pending change
                                TeamExplorerWorkaround(templateVsProjectItem, outputPath);
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
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

        private bool CompareArrays(byte[] array1, byte[] array2)
        {
            if (array1 == null && array2 == null) return true;

            if (array1 == null || array2 == null) return false;

            if (array1.Length != array2.Length) return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            }

            return true;
        }

        private void TeamExplorerWorkaround(VSProjectItem templateVsProjectItem, string outputPath)
        {
            var subItems = templateVsProjectItem.ProjectItem.ProjectItems;
            if (subItems.Count == 1)
            {
                var subItem = subItems.Item(1);
                if (subItem.FileNames[0] == outputPath)
                {
                    if (!subItem.IsOpen)
                    {
                        var window = subItem.Open();
                        window.Close();
                    }
                }
            }
        }

        private string GetOutputPath(VSProjectItem vsProjectItem)
        {
            try
            {
                var pi = vsProjectItem.ProjectItem;
                var templatePath = pi.FileNames[0];

                string templateContent = File.ReadAllText(templatePath);
                const string outputTag = "<#@ output extension=\"";
                int idx = templateContent.IndexOf(outputTag);
                if (idx == -1) return null;

                templateContent = templateContent.Substring(idx + outputTag.Length); 
                idx = templateContent.IndexOf("\"");
                if (idx == -1) return null;

                string extension = templateContent.Substring(0, idx);
                if (!extension.StartsWith(".")) extension = "." + extension;

                return Path.Combine(Path.GetDirectoryName(templatePath), 
                    Path.GetFileNameWithoutExtension(templatePath) + extension);
            }
            catch
            {
                // ignore everything
            }

            return null;
        }

        public void ExecuteTemplatesBeforeBuild(IEnumerable<ProjectItem> projectItems)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var beforeBuildList = Settings.Default.BeforeBuildList;
            if (Settings.Default.RunAll)
            {
                foreach (var item in projectItems)
                {
                    if (item.Object != null) ExecuteTemplate(item);
                }
            }
            ThreadHelper.ThrowIfNotOnUIThread();
            if (beforeBuildList != null)
            {
                foreach (var item in projectItems)
                {
                    if (item.Object != null)
                    {
                        if (beforeBuildList.Contains(item.Name)) ExecuteTemplate(item);
                    }
                }
            }
        }

        public void ExecuteAllTemplates(IEnumerable<ProjectItem> projectItems)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (var item in projectItems)
            {
                if (item.Object != null) ExecuteTemplate(item);
            }
        }

        public void ExecuteTemplatesAfterBuild(IEnumerable<ProjectItem> projectItems)
        {
            var afterBuildList = Settings.Default.AfterBuildList;

            ThreadHelper.ThrowIfNotOnUIThread();
            if (afterBuildList != null)
            {
                foreach (var item in projectItems)
                {
                    if (item.Object != null)
                    {
                        if (afterBuildList.Contains(item.Name)) ExecuteTemplate(item);
                    }
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

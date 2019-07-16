using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace T4Executer
{
    public partial class TTExecuterSettingsControl : UserControl
    {
        private List<string> _templates;
        private StringCollection templateList = new StringCollection();
        private StringCollection ignoreList = new StringCollection();
        private StringCollection beforeBuildList = new StringCollection();
        private StringCollection afterBuildList = new StringCollection();

        public TTExecuterSettingsControl(string[] templates)
        {
            InitializeComponent();
            _templates = templates.ToList();

            if (Settings.Default.AfterBuildList != null && Settings.Default.AfterBuildList.Count > 0)
            {
                afterBuildList = Settings.Default.AfterBuildList;
                foreach(var template in templates)
                {
                    if (afterBuildList.Contains(template))
                        _templates.Remove(template);
                }
            }
            if (Settings.Default.BeforeBuildList != null && Settings.Default.BeforeBuildList.Count > 0)
            {
                beforeBuildList = Settings.Default.BeforeBuildList;
                foreach (var template in templates)
                {
                    if (beforeBuildList.Contains(template))
                        _templates.Remove(template);
                }
            }
            if (Settings.Default.IgnoreList != null && Settings.Default.IgnoreList.Count > 0)
            {
                ignoreList = Settings.Default.IgnoreList;
                foreach (var template in templates)
                {
                    if (ignoreList.Contains(template))
                        _templates.Remove(template);
                }
            }

            _templates.ForEach(x => templateList.Add(x));

            _templates = templates.ToList();

            ConfirmButton.IsEnabled = false;

            ApplyDataBinding();
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            ConfirmButton.IsEnabled = false;
            ApplyDataBinding();
        }

        private void IgnoreButton_Click(object sender, RoutedEventArgs e)
        {
            var templatesToIgnore = templatesListBox.SelectedItems;

            foreach (var template in templatesToIgnore)
            {
                ignoreList.Add(template.ToString());
                templateList.Remove(template.ToString());
            }
            ApplyDataBinding();
            ConfirmButton.IsEnabled = true;
        }

        private void BeforeBuildButton_Click(object sender, RoutedEventArgs e)
        {
            var templatesBeforeBuild = templatesListBox.SelectedItems;

            foreach (var template in templatesBeforeBuild)
            {
                beforeBuildList.Add(template.ToString());
                templateList.Remove(template.ToString());
            }
            ApplyDataBinding();
            ConfirmButton.IsEnabled = true;
        }

        private void AfterBuildButton_Click(object sender, RoutedEventArgs e)
        {
            var templatesAfterBuild = templatesListBox.SelectedItems;

            foreach (var template in templatesAfterBuild)
            {
                afterBuildList.Add(template.ToString());
                templateList.Remove(template.ToString());
            }
            ApplyDataBinding();
            ConfirmButton.IsEnabled = true;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.IgnoreList = ignoreList;
            Settings.Default.BeforeBuildList = beforeBuildList;
            Settings.Default.AfterBuildList = afterBuildList;
            Settings.Default.RunAll = false;
            Settings.Default.Save();
            var myWindow = Window.GetWindow(this);
            myWindow.Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            IgnoreListBox.ItemsSource = null;
            BeforeBuildListbox.ItemsSource = null;
            AfterBuildListBox.ItemsSource = null;
            templateList.Clear();

            foreach (var template in _templates)
            {
                templateList.Add(template);
            }

            templatesListBox.ItemsSource = null;
            templatesListBox.ItemsSource = templateList;

            if (Settings.Default.AfterBuildList != null) Settings.Default.AfterBuildList.Clear();
            if (Settings.Default.IgnoreList != null) Settings.Default.IgnoreList.Clear();
            if (Settings.Default.AfterBuildList != null) Settings.Default.BeforeBuildList.Clear();
            beforeBuildList.Clear();
            afterBuildList.Clear();
            ignoreList.Clear();

            Settings.Default.RunAll = true;
            Settings.Default.Save();
            ConfirmButton.IsEnabled = false;
        }

        private void ApplyDataBinding()
        {
            IgnoreListBox.ItemsSource = null;
            BeforeBuildListbox.ItemsSource = null;
            AfterBuildListBox.ItemsSource = null;
            templatesListBox.ItemsSource = null;
            templatesListBox.ItemsSource = templateList;
            IgnoreListBox.ItemsSource = ignoreList;
            BeforeBuildListbox.ItemsSource = beforeBuildList;
            AfterBuildListBox.ItemsSource = afterBuildList;
        }
    }
}

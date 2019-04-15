using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace TTExecuter
{
    /// <summary>
    /// Interaction logic for TTExecuterSettingsControl.xaml
    /// </summary>
    public partial class TTExecuterSettingsControl : UserControl
    {
        string[] _templates;

        public TTExecuterSettingsControl(string[] templates)
        {
            InitializeComponent();
            _templates = templates;
            foreach (var template in _templates)
            {
                templatesListBox.Items.Add(template);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var templatesToIgnore = templatesListBox.SelectedItems;
            var ignoreList = new StringCollection();

            foreach (var template in templatesToIgnore)
            {
                ignoreList.Add(template.ToString());
            }
            Settings.Default.IgnoreList = ignoreList;
            Settings.Default.Save();
            var myWindow = Window.GetWindow(this);
            myWindow.Close();
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.IgnoreList == null) return;
            if (Settings.Default.IgnoreList.Count > 0)
            {
                foreach (var template in Settings.Default.IgnoreList)
                {
                    templatesListBox.SelectedItems.Add(template);
                }
            }
            else
            {
                return;
            }
        }
    }
}

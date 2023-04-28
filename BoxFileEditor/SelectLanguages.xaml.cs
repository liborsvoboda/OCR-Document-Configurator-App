using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;
using System.Windows.Documents;

namespace BoxFileEditor
{
    /// <summary>
    /// Interaction logic for SelectedBoxWnd.xaml
    /// </summary>
    public partial class SelectLanguagesWnd : Window
    {

        private bool autoclosing = false;
        public SelectLanguagesWnd()
        {
            InitializeComponent();
            this.LanguageList.ItemsSource = LoginWnd.language_list;
            this.SelectedList.ItemsSource = LoginWnd.selected_generatorlist;
            App.Current.MainWindow.IsEnabled = false;
        }

        private void generate_Click(object sender, RoutedEventArgs e)
        {
            autoclosing = true;
            string selectedLanguages = String.Join("+", this.SelectedList.Items.Cast<string>().ToArray()).Length > 0 ? " -l " + String.Join("+", this.SelectedList.Items.Cast<string>().ToArray()) : "";
            Functions.GenerateBoxFile(MainWindow.opennedFile, selectedLanguages);
            MainWindow._viewModel.Load(System.IO.Path.Combine(LoginWnd.setting_folder, "generated.box"), true);
            MainWindow.LoadedBox = true;
            MainWindow.LoadedBoxOpacity = 0;
            MainWindow.DataChanged = true;
            LoginWnd._mainWnd.IsEnabled = true;
            this.Close();
        }



        private void btn_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Functions.SetHandCursor(true);
        }

        private void btn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Functions.SetHandCursor(false);
        }

        private void AddSelected_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoginWnd.selected_generatorlist.Add(this.LanguageList.SelectedItem.ToString());
            LoginWnd.language_list.Remove(this.LanguageList.SelectedItem.ToString());
            RefreshLists();
        }

        private void RemoveSelected_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoginWnd.language_list.Add(this.SelectedList.SelectedItem.ToString());
            LoginWnd.selected_generatorlist.Remove(this.SelectedList.SelectedItem.ToString());
            RefreshLists();
        }

        private void UpSelected_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string temp = this.SelectedList.SelectedItem.ToString();
            LoginWnd.selected_generatorlist.Remove(this.SelectedList.SelectedItem.ToString());
            LoginWnd.selected_generatorlist.Insert(this.SelectedList.SelectedIndex - 1, this.SelectedList.SelectedItem.ToString());
            RefreshLists();
        }

        private void DownSelected_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string temp = this.SelectedList.SelectedItem.ToString();
            LoginWnd.selected_generatorlist.Remove(this.SelectedList.SelectedItem.ToString());
            LoginWnd.selected_generatorlist.Insert(this.SelectedList.SelectedIndex + 1, this.SelectedList.SelectedItem.ToString());
            RefreshLists();
        }

        private void checkEnabled(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.SelectedList.Items.Refresh();
            this.LanguageList.Items.Refresh();
            this.AddSelected.IsEnabled = this.LanguageList.SelectedIndex >= 0;
            this.RemoveSelected.IsEnabled = this.SelectedList.SelectedIndex >= 0;
            this.UpSelected.IsEnabled = this.SelectedList.SelectedIndex > 0;
            this.DownSelected.IsEnabled = this.SelectedList.SelectedIndex < this.SelectedList.Items.Count - 1 && this.SelectedList.SelectedIndex >= 0;
        }

        private void RefreshLists()
        {
            this.SelectedList.Items.Refresh();
            this.LanguageList.Items.Refresh();
            this.AddSelected.IsEnabled = this.LanguageList.SelectedIndex >= 0;
            this.RemoveSelected.IsEnabled = this.SelectedList.SelectedIndex >= 0;
            this.UpSelected.IsEnabled = this.SelectedList.SelectedIndex > 0;
            this.DownSelected.IsEnabled = this.SelectedList.SelectedIndex < this.SelectedList.Items.Count - 1 && this.SelectedList.SelectedIndex >= 0;
         
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!autoclosing) {
                MessageBoxResult result = MessageBox.Show("Opravdu chcete zrušit generování textové mapy?", "Zavření generátoru", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                if (result == MessageBoxResult.Yes)
                {
                    LoginWnd._mainWnd.IsEnabled = true;
                }
            }

        }
    }
}

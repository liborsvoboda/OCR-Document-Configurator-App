using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;
using System.Windows.Documents;
using System.Diagnostics;
using SprávceŠablonOCR.Classes;

namespace BoxFileEditor
{
    /// <summary>
    /// Interaction logic for SelectedBoxWnd.xaml
    /// </summary>
    public partial class SettingsWnd : Window
    {
        private bool changedsettings = false;
        private bool autoclosing = false;
        public SettingsWnd()
        {
            InitializeComponent();
            this.LanguageList.ItemsSource = LoginWnd.language_list;
            this.SelectedList.ItemsSource = LoginWnd.selected_list;
            App.Current.MainWindow.IsEnabled = false;
        }

        private void InstallLanguage_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Pro nainstalování nových jazyků je nutný restart aplikace. Chcete pokračovat?", "Instalace nových jazyků", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {

            }
            if (result == MessageBoxResult.Yes)
            {

                if (Environment.Is64BitOperatingSystem)
                {
                    LoginWnd._mainWnd.Hide();
                    this.Hide();
                    Functions.runExternalApp(Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "ThirdParty", "tesseract-ocr-w64-setup-v5.0.0-alpha.20190708.exe"));
                    Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }
                else
                {
                    LoginWnd._mainWnd.Hide();
                    this.Hide();
                    Functions.runExternalApp(Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "ThirdParty", "tesseract-ocr-w32-setup-v5.0.0-alpha.20190708.exe"));
                    Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }
            }

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
            LoginWnd.selected_list.Add(this.LanguageList.SelectedItem.ToString());
            LoginWnd.language_list.Remove(this.LanguageList.SelectedItem.ToString());
            RefreshLists();
            changedsettings = true;
        }

        private void RemoveSelected_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoginWnd.language_list.Add(this.SelectedList.SelectedItem.ToString());
            LoginWnd.selected_list.Remove(this.SelectedList.SelectedItem.ToString());
            RefreshLists();
            changedsettings = true;
        }

        private void UpSelected_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string temp = this.SelectedList.SelectedItem.ToString();
            LoginWnd.selected_list.Remove(this.SelectedList.SelectedItem.ToString());
            LoginWnd.selected_list.Insert(this.SelectedList.SelectedIndex - 1, this.SelectedList.SelectedItem.ToString());
            RefreshLists();
            changedsettings = true;
        }

        private void DownSelected_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string temp = this.SelectedList.SelectedItem.ToString();
            LoginWnd.selected_list.Remove(this.SelectedList.SelectedItem.ToString());
            LoginWnd.selected_list.Insert(this.SelectedList.SelectedIndex + 1, this.SelectedList.SelectedItem.ToString());
            RefreshLists();
            changedsettings = true;
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
            if (!autoclosing && changedsettings) {
                MessageBoxResult result = MessageBox.Show("Opravdu chcete zrušit provedené změny v nastavení?", "Zavřít nastavení", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                if (result == MessageBoxResult.Yes)
                {
                    LoginWnd.selected_list = new List<string>(LoginWnd.userSettings.languages);
                    LoginWnd._mainWnd.IsEnabled = true;
                }
            } else { LoginWnd._mainWnd.IsEnabled = true; }

        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            autoclosing = true;
            LoginWnd.userSettings.languages = LoginWnd.selected_list;
            Functions.saveSettings(LoginWnd.userSettings);
            LoginWnd._mainWnd.IsEnabled = true;
            this.Close();
        }
    }
}

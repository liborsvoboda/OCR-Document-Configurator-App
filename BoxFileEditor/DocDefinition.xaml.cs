using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;
using System.Windows.Documents;
using System.Diagnostics;
using SprávceŠablonOCR.Classes;
using System.Windows.Controls;
using System.Web.Script.Serialization;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace BoxFileEditor
{
    public partial class DocDefinitionWnd : Window
    {
        private bool changedsettings = false;
        private bool autoclosing = false;
        private string operationMode = null;
        private Array definitionHeaderDataTemp = null;
        private TemplateHeaderData definitionHeaderData = new TemplateHeaderData();
        private DefinitionTemplateList definitionData = null;
        public DocDefinitionWnd(int selectedIndex, bool imageLoaded, string mode = null, object data = null)
        {
            InitializeComponent();
            this.definitionHeader.ItemsSource = MainWindow.DefinitionHeaderList;
            this.definitionHeader.SelectedIndex = selectedIndex;
            App.Current.MainWindow.IsEnabled = false;

            operationMode = mode;
            definitionData = (DefinitionTemplateList)data;
            definitionHeaderDataTemp = (definitionData == null)? null : ((Array)(new JavaScriptSerializer().DeserializeObject(definitionData.data_header)));

            if (definitionHeaderDataTemp != null)
            {
                foreach (Dictionary<string, object> headerItem in definitionHeaderDataTemp)
                {
                    if (headerItem.TryGetValue("identDoc", out object value)) definitionHeaderData.identDoc = (string)value;
                    if (headerItem.TryGetValue("sizeX", out object valueX)) definitionHeaderData.sizeX = (string)valueX;
                    if (headerItem.TryGetValue("sizeY", out object valueY)) definitionHeaderData.sizeY = (string)valueY;
                }
            }

            insertSize.IsEnabled = imageLoaded;

            if (operationMode == "copy")
            {
                this.documentName.Text = "New_" + definitionData.nazev;
                this.identStringText.Text = definitionHeaderData.identDoc;
                this.sizeX.Text = definitionHeaderData.sizeX;
                this.sizeY.Text = definitionHeaderData.sizeY;
                this.note.Text = definitionData.poznamky;
                MainWindow.definitionTemplateSaveRequest.data = setDataFields();
            } else if (operationMode == "edit") {
                this.documentName.Text = definitionData.nazev;
                this.identStringText.Text = definitionHeaderData.identDoc;
                this.sizeX.Text = definitionHeaderData.sizeX;
                this.sizeY.Text = definitionHeaderData.sizeY;
                this.note.Text = definitionData.poznamky;
                MainWindow.definitionTemplateSaveRequest.data = setDataFields();
            }



            changedsettings = false;
        }

      
        private void btn_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Functions.SetHandCursor(true);
        }

        private void btn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Functions.SetHandCursor(false);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!autoclosing && changedsettings) {
                MessageBoxResult result = MessageBox.Show("Opravdu chcete zrušit provedené změny v definici šablony?", "Zavřít definici šablony", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
            MainWindow.definitionTemplateSaveRequest.id_typu_dokladu = ((ComboBoxItem)definitionHeader.SelectedItem).Tag.ToString();
            MainWindow.definitionTemplateSaveRequest.nazev = this.documentName.Text;
            MainWindow.definitionTemplateSaveRequest.data_header = setHeaderDataFields();
            MainWindow.definitionTemplateSaveRequest.poznamky = this.note.Text;
            
            MainWindow.definitionTemplateSaveRequest.data = (operationMode == null) ? "[]": setDataFields();
            if (operationMode == "copy" || operationMode == null)
            {
                if (Functions.processData(LoginWnd.apiSettings.templateUrl, "POST", 2))
                {
                    Functions.processData(LoginWnd.apiSettings.templateUrl, "GET", 3);
                    LoginWnd._mainWnd.IsEnabled = true;
                    this.Close();
                }
                else
                {
                    autoclosing = false;
                    MessageBox.Show("Šablonu se nepodařilo uložit", "Chyba", MessageBoxButton.OK);
                }
            } else if (operationMode == "edit")
            {
                MainWindow.definitionTemplateEditRequest.nazev = this.documentName.Text;
                MainWindow.definitionTemplateEditRequest.data_header = setHeaderDataFields();
                MainWindow.definitionTemplateEditRequest.poznamky = this.note.Text;
                MainWindow.definitionTemplateEditRequest.data = setDataFields();
                if (Functions.processData(LoginWnd.apiSettings.templateUrl + "/" + definitionData.id_ocr_sablony, "PUT", 5))
                {
                    Functions.processData(LoginWnd.apiSettings.templateUrl, "GET", 3);
                    LoginWnd._mainWnd.IsEnabled = true;
                    this.Close();
                }
                else
                {
                    autoclosing = false;
                    MessageBox.Show("Šablonu se nepodařilo uložit", "Chyba", MessageBoxButton.OK);
                }
            }
        }

        private void DefinitionSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            changedsettings = true;
        }

        private void dataChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            changedsettings = true;
        }

        private string setDataFields()
        {
            string data = "[";
            double count = 0;

            foreach (TessBoxControl item in MainWindow._viewModel.Boxes)
            {

                if (count > 0) data += ",";
                DataItem tempItem = new DataItem();
                tempItem.Top = item.Top;
                tempItem.Left = item.Left;
                tempItem.Height = item.Height;
                tempItem.Width = item.Width;
                tempItem.GroupValueIndex = item.GroupValueIndex;
                tempItem.GroupValue = item.GroupValue;
                tempItem.GroupSubValueIndex = item.GroupSubValueIndex;
                tempItem.GroupSubValue = item.GroupSubValue;
                data += new JavaScriptSerializer().Serialize(tempItem);
                count++;
            }
            data += "]";
            return data;
        }

        private string setHeaderDataFields()
        {
            string data = "[";
            TemplateHeaderData tempItem = new TemplateHeaderData() {
                    identDoc = this.identStringText.Text,
                    sizeX = this.sizeX.Text,
                    sizeY = this.sizeY.Text
            };
            data += new JavaScriptSerializer().Serialize(tempItem);
            data += "]";
            return data;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");

            if (((TextBox)sender).Name == "sizeX" && !regex.IsMatch(e.Text) && Convert.ToInt32(sizeX.Text.Insert(((TextBox)e.Source).CaretIndex, e.Text)) >= 1 && Convert.ToInt32(sizeX.Text.Insert(((TextBox)e.Source).CaretIndex, e.Text)) <= 20000)
            {
                //sizeX = sizeX.Text.Insert(((TextBox)e.Source).CaretIndex, e.Text);
                
                e.Handled = regex.IsMatch(e.Text);
            }

            else if (((TextBox)sender).Name == "sizeY" && !regex.IsMatch(e.Text) && Convert.ToInt32(sizeY.Text.Insert(((TextBox)e.Source).CaretIndex, e.Text)) >= 1 && Convert.ToInt32(sizeY.Text.Insert(((TextBox)e.Source).CaretIndex, e.Text)) <= 20000)
            {
                //setOfflineValueStat = offline_time.Text.Insert(((TextBox)e.Source).CaretIndex, e.Text);
                e.Handled = regex.IsMatch(e.Text);
            }

            else e.Handled = true;
        }

        private void insertSize_Click(object sender, RoutedEventArgs e)
        {
            sizeX.Text = MainWindow._viewModel.Image.PixelWidth.ToString();
            sizeY.Text = MainWindow._viewModel.Image.PixelHeight.ToString();
        }
    }
}

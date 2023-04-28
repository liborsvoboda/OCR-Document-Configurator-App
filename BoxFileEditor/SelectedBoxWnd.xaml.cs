using SprávceŠablonOCR.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BoxFileEditor
{

    public partial class SelectedBoxWnd : Window
    {

      
       

    

        public SelectedBoxWnd()
        {
            InitializeComponent();
            comboBoxGroupSub.SelectedIndex = (string.IsNullOrWhiteSpace(MainWindow._viewModel.SelectedGroupSubValueIndex)) ? -1 : Convert.ToInt32(MainWindow._viewModel.SelectedGroupSubValueIndex);
            comboBoxGroup.SelectedIndex = (string.IsNullOrWhiteSpace(MainWindow._viewModel.SelectedGroupValueIndex)) ? -1 : Convert.ToInt32(MainWindow._viewModel.SelectedGroupValueIndex);
            comboBoxGroup.Items.Refresh();
          
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //if (btnApplyNextAndAdvance.IsEnabled)
                //    Dispatcher.BeginInvoke(new RoutedEventHandler(btnApplyNextAndAdvance_Click), new object[] { this, new RoutedEventArgs() });
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            var mainWnd = Owner as MainWindow;
            mainWnd.ApplyValueToSelectedBoxes(textBoxValue.Text, false);
           // MainWindow.DataChanged = true;
            Close();
        }

        private void btnApplyPrevAndAdvance_Click(object sender, RoutedEventArgs e)
        {
            var mainWnd = Owner as MainWindow;
            mainWnd.ApplyValueToSelectedBoxes(textBoxValue.Text, true, -1);
           // MainWindow.DataChanged = true;
        }

        private void btnApplyNextAndAdvance_Click(object sender, RoutedEventArgs e)
        {
            var mainWnd = Owner as MainWindow;
            mainWnd.ApplyValueToSelectedBoxes(textBoxValue.Text, true, 1);
           // MainWindow.DataChanged = true;
        }

        public void SelectAndFocusValue()
        {
            Dispatcher.BeginInvoke(new Action(AsyncFocusTextBoxValue));
        }

        private void AsyncFocusTextBoxValue()
        {
            textBoxValue.Focus();
            textBoxValue.SelectAll();
        }

        private void ComboBoxGroupListSelectChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow._viewModel.SelectedGroupValueIndex = comboBoxGroup.SelectedIndex.ToString();
            MainWindow._viewModel.SelectedGroupValue = ((ComboBoxItem)comboBoxGroup.SelectedItem).Content.ToString();
            MainViewModel.ComboBoxValueList.Clear();
            List<DefinitionHeaderResponse> resItems = (from data in MainWindow.definitionHeaderResponse
                                                       where data.nazev == MainWindow.selectedDefinition
                                                       select data).ToList();
            if (resItems.Count() > 0)
            {
                foreach (Polozky2 subItem in resItems.First().polozky.ElementAt(comboBoxGroup.SelectedIndex).polozky)
                {
                    var checkIfExists = (from data1 in MainWindow._viewModel.Boxes
                                 where data1.GroupValue == MainWindow._viewModel.SelectedGroupValue
                                 && data1.GroupSubValue == subItem.nazev
                                 && data1.GroupSubValue != MainWindow._viewModel.SelectedGroupSubValue
                                         select 1).ToList();

                    if (checkIfExists.Count == 0) { 
                        comboBoxGroupSub.IsEnabled = true;
                        ComboBoxItem item = new ComboBoxItem
                        {
                            Content = subItem.nazev,
                            Tag = subItem.id_doklady_definice
                        };
                        MainViewModel.ComboBoxValueList.Add(item);
                        comboBoxGroupSub.Items.Refresh();
                    }
                }

             
            }
        }

        private void ComboBoxValueListSelectChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow._viewModel.SelectedGroupSubValueIndex = comboBoxGroupSub.SelectedIndex.ToString();
            MainWindow._viewModel.SelectedGroupSubValue = (comboBoxGroupSub.SelectedItem == null) ? "" : ((ComboBoxItem)comboBoxGroupSub.SelectedItem).Content.ToString();

            if (MainWindow._viewModel.SelectedItemValue != MainWindow._viewModel.SelectedGroupSubValue) {
                MainWindow.DataChanged = true;
            }
           
            MainWindow._viewModel.SelectedItemValue = (comboBoxGroupSub.SelectedItem == null) ? "" : ((ComboBoxItem)comboBoxGroupSub.SelectedItem).Content.ToString();
            textBoxValue.Text = (comboBoxGroupSub.SelectedItem == null) ? "" : ((ComboBoxItem)comboBoxGroupSub.SelectedItem).Content.ToString();
           
        }

        private void dataFieldValueChanged(object sender, TextChangedEventArgs e)
        {
          
        }
    }
}

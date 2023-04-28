using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using SprávceŠablonOCR.Classes;

namespace BoxFileEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //temp values
        private static int documentVariantIndex = 0;
        private static int templateVariantIndex = 0;
        private static bool returnStep = false;
        public static string selectedDefinition = null;
        private static bool loadFile = false;
        // end temp

        public static MainViewModel _viewModel = null;
        public static string opennedFile = null;
        public static bool loadedBox = false;
        public static bool selectedBox = false;
        public static bool selectedOneBox = false;
        public static bool dataChanged = false;
        public static string docWidth = null;
        public static string docHeight = null;

        private static SolidColorBrush dataChangedColor = Brushes.Transparent;
        private SelectedBoxWnd _boxWnd = null;
        private bool _suppressEventHandlers = false;
        private SelectLanguagesWnd _selectLanguagesWnd = null;
        private SettingsWnd _settingsWnd = null;
        private DocDefinitionWnd _docDefinitionWnd = null;

        public static List<ComboBoxItem> definitionHeaderList = new List<ComboBoxItem>() { new ComboBoxItem() { Content = "Vyberte typ dokumentu", Tag = null, IsSelected = true } };
        public static DefinitionTemplateSaveRequest definitionTemplateSaveRequest = new DefinitionTemplateSaveRequest();

        public static DefinitionTemplateSaveResponse definitionTemplateSaveResponse = new DefinitionTemplateSaveResponse();
        public static List<DataItem> dataItemList = new List<DataItem>();

        public static DefinitionTemplateEditRequest definitionTemplateEditRequest = new DefinitionTemplateEditRequest();
        public static List<DefinitionHeaderResponse> definitionHeaderResponse = new List<DefinitionHeaderResponse>();

        public static List<DefinitionTemplateList> fullDefinitionTemplateList = new List<DefinitionTemplateList>() { new DefinitionTemplateList() };
        public static List<DefinitionTemplateList> definitionTemplateList = new List<DefinitionTemplateList>();

        private static string statusLabelText = "Přihlášen: ";
        private static string versionLabelText = "Verze: ";
        public static event EventHandler StatusLabelTextChanged, DefinitionHeaderListChanged, DefinitionTemplateListChanged, OpennedFileChanged, OpennedFileOpacityChanged, LoadedBoxChanged, LoadedBoxOpacityChanged, SelectedBoxChanged, SelectedBoxOpacityChanged, SelectedOneBoxChanged, SelectedOneBoxOpacityChanged, DocHeightChanged, DocWidthChanged, DataChangedHandler, VersionLabelTextChanged;
        public static event PropertyChangedEventHandler StaticPropertyChanged;


        private static void OnStaticPropertyChanged(string propertyName)
        {
            var handler = StaticPropertyChanged;
            if (handler != null)
            {
                handler(null, new PropertyChangedEventArgs(propertyName));
            }
        }


        public static string VersionLabelText
        {
            get => versionLabelText;
            set
            {
                versionLabelText = value;
                VersionLabelTextChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public static string StatusLabelText
        {
            get => statusLabelText;
            set
            {
                statusLabelText = value;
                StatusLabelTextChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool DataChanged
        {
            get => dataChanged;
            set
            {
                dataChanged = value;
                MenuSaveBackground = (value) ? Brushes.Orange : Brushes.Transparent;
                DataChangedHandler?.Invoke(null, EventArgs.Empty);
            }
        }

        public static SolidColorBrush MenuSaveBackground
        {
            get { return dataChangedColor; }
            set
            {
                dataChangedColor = value;
                OnStaticPropertyChanged("MenuSaveBackground");
            }
        }

        public static bool OpennedFile
        {
            get => !String.IsNullOrWhiteSpace(opennedFile);
            set
            {
                OpennedFileChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static double OpennedFileOpacity
        {
            get => (!String.IsNullOrWhiteSpace(opennedFile)) ? 1 : 0.5;
            set
            {
                OpennedFileOpacityChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool LoadedBox
        {
            get => loadedBox;
            set
            {
                loadedBox = value;
                LoadedBoxChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static double LoadedBoxOpacity
        {
            get => (loadedBox) ? 1 : 0.5;
            set
            {
                LoadedBoxOpacityChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool SelectedBox
        {
            get => selectedBox;
            set
            {
                selectedBox = value;
                SelectedBoxChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static double SelectedBoxOpacity
        {
            get => (selectedBox) ? 1 : 0.5;
            set
            {
                SelectedBoxOpacityChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool SelectedOneBox
        {
            get => selectedOneBox;
            set
            {
                selectedOneBox = value;
                SelectedOneBoxChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static string DocHeight
        {
            get => docHeight;
            set
            {
                docHeight = value;
                DocHeightChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static string DocWidth
        {
            get => docWidth;
            set
            {
                docWidth = value;
                DocWidthChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static double SelectedOneBoxOpacity
        {
            get => (selectedOneBox) ? 1 : 0.5;
            set
            {
                SelectedOneBoxOpacityChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static List<ComboBoxItem> DefinitionHeaderList
        {
            get => definitionHeaderList;
            set
            {
                definitionHeaderList = value;
                DefinitionHeaderListChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static List<DefinitionTemplateList> DefinitionTemplateList
        {
            get => definitionTemplateList;
            set
            {
                definitionTemplateList = value;
                DefinitionTemplateListChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public MainWindow()
        {

            InitializeComponent();
            _viewModel = new MainViewModel();
            //            _viewModel.Languages = LoginWnd.language_list;
            DataContext = _viewModel;

            Functions.processData(LoginWnd.apiSettings.definitionUrl, "GET", 1);
            Functions.processData(LoginWnd.apiSettings.templateUrl, "GET", 3);



            //System.Windows.Interop.HwndSource.DefaultAcquireHwndFocusInMenuMode = false;
            //Keyboard.DefaultRestoreFocusMode = RestoreFocusMode.None;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


            //_boxWnd = new SelectedBoxWnd();
            //_boxWnd.Owner = this;
            //_boxWnd.ShowActivated = false;
            //_boxWnd.DataContext = _viewModel;
            //_boxWnd.Show();

            /*
            _zoomToolWindow = new Window();
            _zoomToolWindow.Owner = this;
            _zoomToolWindow.WindowStyle = WindowStyle.ToolWindow;
            _zoomToolWindow.Background = SystemColors.AppWorkspaceBrush;
            _zoomToolWindow.ShowInTaskbar = false;
            _zoomToolWindow.Width = 200;
            _zoomToolWindow.Height = 200;
            _zoomToolWindow.DataContext = DataContext;
            _zoomToolWindow.Title = "Selected Box View";

            var img = new Image();
            img.Margin = new Thickness(10);
            img.Stretch = Stretch.Uniform;
            img.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
            img.SetBinding(Image.SourceProperty, new Binding("SelectedBoxImage"));
            _zoomToolWindow.Content = img;
            
            _zoomToolWindow.Show();
            */
            VersionLabelText = "Verze: " + Assembly.GetEntryAssembly().GetName().Version.ToString().Substring(0, Assembly.GetEntryAssembly().GetName().Version.ToString().Length - 2);
            //_viewModel.Load(System.IO.Path.Combine(LoginWnd.setting_folder, "empty.png"), false);
            //DocHeight = "Výška dokumentu: " + _viewModel.Image.Height.ToString();
            //DocWidth = "Šířka dokumentu: " + _viewModel.Image.Width.ToString();

        }



        private void boxView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            menuMerge.IsEnabled = (e.AddedItems.Count == 0) ? false : true;
            menuDelete.IsEnabled = (e.AddedItems.Count == 0) ? false : true;
            menuMerge.Opacity = (e.AddedItems.Count == 0) ? 0.5 : 1;
            menuDelete.Opacity = (e.AddedItems.Count == 0) ? 0.5 : 1;


            if (!_suppressEventHandlers)
            {
                _suppressEventHandlers = true;
                foreach (var removedItem in e.RemovedItems)
                    boxList.SelectedItems.Remove(removedItem);
                foreach (var addedItem in e.AddedItems)
                    boxList.SelectedItems.Add(addedItem);
                _viewModel.SelectedBoxes = boxView.SelectedItems.Cast<TessBoxControl>();
                _suppressEventHandlers = false;
            }

            if (_viewModel.SelectedItem != null)
                boxList.ScrollIntoView(_viewModel.SelectedItem);

            SelectedBox = (_viewModel.SelectedBoxes.Count() > 0) ? true : false;
            SelectedBoxOpacity = 0;

            SelectedOneBox = (_viewModel.SelectedBoxes.Count() == 1) ? true : false;
            SelectedOneBoxOpacity = 0;

        }

        private void boxList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_suppressEventHandlers)
            {
                _suppressEventHandlers = true;
                foreach (var removedItem in e.RemovedItems)
                    boxView.SelectedItems.Remove(removedItem);
                foreach (var addedItem in e.AddedItems)
                    boxView.SelectedItems.Add(addedItem);
                _viewModel.SelectedBoxes = boxView.SelectedItems.Cast<TessBoxControl>();
                _suppressEventHandlers = false;
            }
            if (_viewModel.SelectedItem != null)
                boxView.ScrollIntoView(_viewModel.SelectedItem);

            SelectedBox = (_viewModel.SelectedBoxes.Count() > 0) ? true : false;
            SelectedBoxOpacity = 0;

            SelectedOneBox = (_viewModel.SelectedBoxes.Count() == 1) ? true : false;
            SelectedOneBoxOpacity = 0;


        }

        private void btnCreateBox_Click(object sender, RoutedEventArgs e)
        {
            if (dataChanged)
            {
                MessageBoxResult result = MessageBox.Show("Provedené změny nebyly uloženy. Chcete změny zahodit?", "Neuložené změny", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    DataChanged = false;
                    _selectLanguagesWnd = new SelectLanguagesWnd();
                    _selectLanguagesWnd.Owner = this;
                    _selectLanguagesWnd.ShowActivated = true;
                    _selectLanguagesWnd.Show();
                    this.IsEnabled = false;
                }
            }
            else
            {
                DataChanged = false;
                _selectLanguagesWnd = new SelectLanguagesWnd();
                _selectLanguagesWnd.Owner = this;
                _selectLanguagesWnd.ShowActivated = true;
                _selectLanguagesWnd.Show();
                this.IsEnabled = false;
            }

        }

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            DataChanged = true;
            _viewModel.MergeSelectedBoxes(boxView.SelectedItems.Cast<TessBoxControl>());
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataChanged = true;
            _viewModel.DeleteSelectedBoxes(boxView.SelectedItems.Cast<TessBoxControl>());
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.PageUp)
            {
                if (_viewModel.SelPageIndex < _viewModel.MaxPageIndex)
                    _viewModel.SelPageIndex++;
                e.Handled = true;
            }
            else if (e.Key == Key.PageDown)
            {
                if (_viewModel.SelPageIndex > 0)
                    _viewModel.SelPageIndex--;
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        protected internal void ApplyValueToSelectedBoxes(string value, bool advance, int move = 0)
        {
            _viewModel.SelectedItemValue = value;
            if (advance)
            {
                var selectedItem = boxView.SelectedItem as TessBoxControl;
                if (selectedItem != null)
                {
                    var index = boxView.ItemContainerGenerator.IndexFromContainer(selectedItem);

                    index += ((index > 0 && move < 0) || move > 0) ? move : 0;
                    selectedItem = boxView.ItemContainerGenerator.ContainerFromIndex(index) as TessBoxControl;
                    boxView.SelectedItem = selectedItem;
                    _boxWnd.SelectAndFocusValue();
                }
            }
        }

        private void menuSettings_Click(object sender, RoutedEventArgs e)
        {
            _settingsWnd = new SettingsWnd();
            _settingsWnd.Owner = this;
            _settingsWnd.ShowActivated = true;
            _settingsWnd.Show();
            this.IsEnabled = false;
        }



        private void templateCopy_Click(object sender, MouseButtonEventArgs e)
        {
            if (definitionTemplate.SelectedIndex >= 0) {
                _docDefinitionWnd = new DocDefinitionWnd(this.definitionHeader.SelectedIndex, OpennedFile, "copy", definitionTemplate.SelectedItem);
                _docDefinitionWnd.Closed += delegate
                {
                    if (definitionTemplate.Items.Count >= 0) definitionTemplate.Items.Refresh();
                };
                _docDefinitionWnd.Owner = this;
                _docDefinitionWnd.ShowActivated = true;
                _docDefinitionWnd.Show();
                this.IsEnabled = false;
            }

        }


        private void templateEdit_Click(object sender, MouseButtonEventArgs e)
        {
            if (definitionTemplate.SelectedIndex >= 0)
            {
                _docDefinitionWnd = new DocDefinitionWnd(this.definitionHeader.SelectedIndex, OpennedFile, "edit", definitionTemplate.SelectedItem);
                _docDefinitionWnd.Owner = this;
                _docDefinitionWnd.ShowActivated = true;
                _docDefinitionWnd.Show();
                this.IsEnabled = false;
            }
        }

        private void templateDelete_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Opravdu chcete smazat vybranou šablonu?" + Environment.NewLine + ((DefinitionTemplateList)this.definitionTemplate.SelectedItem).nazev, "Odstranění šablony", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (Functions.processData(LoginWnd.apiSettings.templateUrl + "/" + ((DefinitionTemplateList)this.definitionTemplate.SelectedItem).id_ocr_sablony, "DELETE", 4))
                {
                    Functions.processData(LoginWnd.apiSettings.templateUrl, "GET", 3);

                    if (definitionTemplate.Items.Count >= 0) definitionTemplate.Items.Refresh();
                }
            }
        }

        private void menuLoad_Click(object sender, RoutedEventArgs e)
        {

            if (dataChanged)
            {
                MessageBoxResult result = MessageBox.Show("Provedené změny nebyly uloženy. Chcete změny zahodit?", "Neuložené změny", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    DataChanged = false;
                    _viewModel.Close();
                   
                    var openDlg = new OpenFileDialog();
                    openDlg.Filter = "PNG Image Files (*.png)|*.png|TIFF Image Files (*.tiff, *.tif)|*.tiff;*.tif|All Files (*.*)|*.*";
                    openDlg.Multiselect = false;
                    openDlg.CheckPathExists = true;
                    if (openDlg.ShowDialog() == true)
                    {
                        try
                        {
                            _viewModel.Load(openDlg.FileName, false);
                            opennedFile = openDlg.FileName;
                            loadFile = true;
                            OpennedFile = true;
                            OpennedFileOpacity = 0;
                            DocHeight = "Výška dokumentu: " + _viewModel.Image.PixelHeight.ToString();
                            DocWidth = "Šířka dokumentu: " + _viewModel.Image.PixelWidth.ToString();
                        }
                        catch (Exception ex)
                        {
                            DocHeight = null;
                            DocWidth = null;
                            MessageBox.Show(string.Format("Unable to load '{0}', {1}", openDlg.FileName, ex.GetBaseException().Message), MainViewModel.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        if (definitionTemplate.SelectedIndex >= 0)
                        {
                            int tempindex = definitionTemplate.SelectedIndex;
                            definitionTemplate.SelectedIndex = -1;
                            definitionTemplate.SelectedIndex = tempindex;
                        }
                        //definitionTemplate.SelectedIndex = -1;
                    }
                }
            } else
            {
                var openDlg = new OpenFileDialog();
                openDlg.Filter = "PNG Image Files (*.png)|*.png|TIFF Image Files (*.tiff, *.tif)|*.tiff;*.tif|All Files (*.*)|*.*";
                openDlg.Multiselect = false;
                openDlg.CheckPathExists = true;
                if (openDlg.ShowDialog() == true)
                {
                    try
                    {
                        DataChanged = false;
                        _viewModel.Close();
                        _viewModel.Load(openDlg.FileName, false);
                        opennedFile = openDlg.FileName;
                        loadFile = true;
                        OpennedFile = true;
                        OpennedFileOpacity = 0;
                        DocHeight = "Výška dokumentu: " + _viewModel.Image.PixelHeight.ToString();
                        DocWidth = "Šířka dokumentu: " + _viewModel.Image.PixelWidth.ToString();
                    }
                    catch (Exception ex)
                    {
                        DocHeight = null;
                        DocWidth = null;
                        MessageBox.Show(string.Format("Unable to load '{0}', {1}", openDlg.FileName, ex.GetBaseException().Message), MainViewModel.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    if (definitionTemplate.SelectedIndex >= 0)
                    {
                        int tempindex = definitionTemplate.SelectedIndex;
                        definitionTemplate.SelectedIndex = -1;
                        definitionTemplate.SelectedIndex = tempindex;
                    }

                    //definitionTemplate.SelectedIndex = -1;
                }

            }

         
            //this.menuLoad.IsEnabled = (this.definitionHeader.SelectedIndex > 0 && DocHeight != null) ? true : false;
            //this.menuLoad.Opacity = (this.definitionHeader.SelectedIndex > 0 && DocHeight != null) ? 1 : 0.5;
        }

        private void menuClean_Click(object sender, RoutedEventArgs e)
        {
            if (dataChanged)
            {
                MessageBoxResult result = MessageBox.Show("Provedené změny nebyly uloženy. Chcete změny zahodit?", "Neuložené změny", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    DataChanged = false;
                    loadFile = false;
                    opennedFile = null;
                    _viewModel.Close();

                    OpennedFile = false;
                    OpennedFileOpacity = 0;
                    LoadedBox = false;
                    LoadedBoxOpacity = 0;

                    _viewModel.Load(System.IO.Path.Combine(LoginWnd.setting_folder, "empty.png"), false);
                    DocHeight = "Výška dokumentu: " + _viewModel.Image.PixelHeight.ToString();
                    DocWidth = "Šířka dokumentu: " + _viewModel.Image.PixelWidth.ToString();
                    definitionTemplate.SelectedIndex = -1;
                    //this.menuLoad.IsEnabled = (this.definitionHeader.SelectedIndex > 0 && DocHeight != null) ? true : false;
                    //this.menuLoad.Opacity = (this.definitionHeader.SelectedIndex > 0 && DocHeight != null) ? 1 : 0.5;
                }
            }
            else
            {
                DataChanged = false;
                opennedFile = null;
                _viewModel.Close();

                OpennedFile = false;
                OpennedFileOpacity = 0;
                LoadedBox = false;
                LoadedBoxOpacity = 0;

                _viewModel.Load(System.IO.Path.Combine(LoginWnd.setting_folder, "empty.png"), false);
                DocHeight = "Výška dokumentu: " + _viewModel.Image.PixelHeight.ToString();
                DocWidth = "Šířka dokumentu: " + _viewModel.Image.PixelWidth.ToString();
                definitionTemplate.SelectedIndex = -1;
                //this.menuLoad.IsEnabled = (this.definitionHeader.SelectedIndex > 0 && DocHeight != null) ? true : false;
                //this.menuLoad.Opacity = (this.definitionHeader.SelectedIndex > 0 && DocHeight != null) ? 1 : 0.5;
            }
        }

        private void menuSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (checkAllFields())
                {
                    if (definitionTemplate.SelectedIndex > -1)
                    {
                        definitionTemplateEditRequest.nazev = ((DefinitionTemplateList)definitionTemplate.SelectedItem).nazev;
                        definitionTemplateEditRequest.poznamky = ((DefinitionTemplateList)definitionTemplate.SelectedItem).poznamky;
                        definitionTemplateEditRequest.data_header = ((DefinitionTemplateList)definitionTemplate.SelectedItem).data_header;
                        definitionTemplateEditRequest.data ="[";

                        double count = 0;
                        foreach (TessBoxControl item in _viewModel.Boxes)
                        {
                            if (count > 0) definitionTemplateEditRequest.data += ",";
                            DataItem tempItem = new DataItem();
                            tempItem.Top = item.Top;
                            tempItem.Left = item.Left;
                            tempItem.Height = item.Height;
                            tempItem.Width = item.Width;
                            tempItem.GroupValueIndex = item.GroupValueIndex;
                            tempItem.GroupValue = item.GroupValue;
                            tempItem.GroupSubValueIndex = item.GroupSubValueIndex;
                            tempItem.GroupSubValue = item.GroupSubValue;
                            definitionTemplateEditRequest.data +=new JavaScriptSerializer().Serialize(tempItem);
                            count++;
                        }
                        definitionTemplateEditRequest.data += "]";
                        if (Functions.processData(LoginWnd.apiSettings.templateUrl + "/" + ((DefinitionTemplateList)definitionTemplate.SelectedItem).id_ocr_sablony, "PUT", 5))
                        {
                            MessageBoxResult result = MessageBox.Show("Uložení do databáze proběhlo v pořádku.", "Uložení do Databáze", MessageBoxButton.OK, MessageBoxImage.Information);
                            DataChanged = false;
                        }
                        else
                        {
                            MessageBoxResult result = MessageBox.Show("Šablonu se nepodařilo uložit", "Uložení do Databáze", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show("Není vybrána žádná šablona.", "Uložení do Databáze", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Unable to save box file, {0}", ex.GetBaseException().Message), MainViewModel.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void boxView_CreateBox(object sender, Rect bounds)
        {
            boxView.SelectedItems.Clear();

            var box = new TessBoxControl();
            box.Value = "?";
            box.Width = bounds.Width;
            box.Height = bounds.Height;
            Canvas.SetLeft(box, bounds.Left);
            Canvas.SetTop(box, bounds.Top);
            _viewModel.Boxes.Add(box);
        }

        private void btnEmptyBox_Click(object sender, RoutedEventArgs e)
        {
            if (dataChanged)
            {
                MessageBoxResult result = MessageBox.Show("Provedené změny nebyly uloženy. Chcete změny zahodit?", "Neuložené změny", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    DataChanged = false;
                    _viewModel.Load(System.IO.Path.Combine(LoginWnd.setting_folder, "empty.box"), true);
                    LoadedBox = true;
                    LoadedBoxOpacity = 0;
                    DataChanged = true;
                }
            }
            else
            {
                DataChanged = false;
                _viewModel.Load(System.IO.Path.Combine(LoginWnd.setting_folder, "empty.box"), true);
                LoadedBox = true;
                LoadedBoxOpacity = 0;
                DataChanged = true;
            }
        }

        private void boxView_MergeSelected(object sender, EventArgs e)
        {
            _viewModel.MergeSelectedBoxes(boxView.SelectedItems.Cast<TessBoxControl>());
        }

        private void menuControl_Click(object sender, RoutedEventArgs e)
        {
            if (checkAllFields()) {
                MessageBoxResult result = MessageBox.Show("Při kontrole nebyly nalezeny žádné chyby.", "Kontrola dat", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        
        }

        private void templateCreate_Click(object sender, MouseButtonEventArgs e)
        {
            _docDefinitionWnd = new DocDefinitionWnd(this.definitionHeader.SelectedIndex, OpennedFile);
            _docDefinitionWnd.Closed += delegate
            {
                if (definitionTemplate.Items.Count >= 0) definitionTemplate.Items.Refresh();
            };
            _docDefinitionWnd.Owner = this;
            _docDefinitionWnd.ShowActivated = true;
            _docDefinitionWnd.Show();
            this.IsEnabled = false;
        }

        private void help_Click(object sender, RoutedEventArgs e)
        {

        }

        private void boxView_DeleteSelected(object sender, EventArgs e)
        {
            _viewModel.DeleteSelectedBoxes(boxView.SelectedItems.Cast<TessBoxControl>());
        }

        private void boxCreate_Click(object sender, MouseButtonEventArgs e)
        {
            DataChanged = true;
            boxView.SelectedItems.Clear();

            var box = new TessBoxControl();
            box.Value = "?";
            box.Width = 130;
            box.Height = 30;
            Canvas.SetLeft(box, 3);
            Canvas.SetTop(box, 20);
            _viewModel.Boxes.Add(box);

            _boxWnd = new SelectedBoxWnd();
            _boxWnd.Owner = this;
            _boxWnd.ShowActivated = false;
            _boxWnd.DataContext = _viewModel;
            _boxWnd.Show();
            boxView.SelectedItems.Add(box);

        }

        private void boxDelete_Click(object sender, MouseButtonEventArgs e)
        {
            DataChanged = true;
            _viewModel.DeleteSelectedBoxes(boxView.SelectedItems.Cast<TessBoxControl>());
        }

        private void boxCopy_Click(object sender, MouseButtonEventArgs e)
        {
            DataChanged = true;
            if (_viewModel.SelectedBoxes.Count() == 1)
            {
                var box = new TessBoxControl();
                box.Value = ((TessBoxControl)boxList.SelectedItem).Value;
                box.Width = ((TessBoxControl)boxList.SelectedItem).Width;
                box.Height = ((TessBoxControl)boxList.SelectedItem).Height;
                //Canvas.SetLeft(box, ((TessBoxControl)boxList.SelectedItem).Left + 5);
                //Canvas.SetTop(box, ((TessBoxControl)boxList.SelectedItem).Top + 5);
                Canvas.SetLeft(box, 3);
                Canvas.SetTop(box, 20);
                boxView.SelectedItems.Clear();
                _viewModel.Boxes.Add(box);

                boxView.SelectedItems.Add(box);

            }
        }

        private void BoxList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _boxWnd = new SelectedBoxWnd();
            _boxWnd.Owner = this;
            _boxWnd.ShowActivated = false;
            _boxWnd.DataContext = _viewModel;
            _boxWnd.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Opravdu chcete ukončit aplikaci?", "Zavření aplikace", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            if (result == MessageBoxResult.Yes)
            {
                App.Current.MainWindow.Close();
            }

        }

        private void open_boxdetail(object sender, MouseButtonEventArgs e)
        {
            if (_boxWnd == null)
            {
                if (_viewModel.SelectedBoxes.Count() > 0)
                {
                    _boxWnd = new SelectedBoxWnd();
                    _boxWnd.Owner = this;
                    _boxWnd.ShowActivated = false;
                    _boxWnd.DataContext = _viewModel;
                    _boxWnd.Show();
                }
            } else if (!_boxWnd.IsVisible)
            {
                if (_viewModel.SelectedBoxes.Count() > 0)
                {
                    _boxWnd = new SelectedBoxWnd();
                    _boxWnd.Owner = this;
                    _boxWnd.ShowActivated = false;
                    _boxWnd.DataContext = _viewModel;
                    _boxWnd.Show();
                }
            } else
            {
                _boxWnd.DataContext = _viewModel;
            }
        }

        private void Logout_click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void logout_mouseMove(object sender, MouseEventArgs e)
        {
            Functions.SetHandCursor(true);
        }

        private void selectDodumentDefinition(object sender, SelectionChangedEventArgs e)
        {
            if (returnStep)
            {
                returnStep = false;
                return;
            }

            this.definitionCopy.IsEnabled = this.definitionTemplate.SelectedIndex >= 0 ? true : false;
            this.definitionDelete.IsEnabled = this.definitionTemplate.SelectedIndex >= 0 ? true : false;
            this.definitionCopy.Opacity = this.definitionTemplate.SelectedIndex >= 0 ? 1 : 0.5;
            this.definitionDelete.Opacity = this.definitionTemplate.SelectedIndex >= 0 ? 1 : 0.5;

            if (dataChanged)
            {
                MessageBoxResult result = MessageBox.Show("Provedené změny nebyly uloženy. Chcete změny zahodit?", "Neuložené změny", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    DataChanged = false;

                    if (_viewModel.Boxes != null) _viewModel.Boxes.Clear();

                    if (loadFile == false && definitionTemplate.SelectedIndex == -1)
                    {
                        opennedFile = null;
                        _viewModel.Close();

                        LoadedBox = false;
                        LoadedBoxOpacity = 0;

                        _viewModel.Load(System.IO.Path.Combine(LoginWnd.setting_folder, "empty.png"), false);
                        DocHeight = "Výška dokumentu: " + _viewModel.Image.PixelHeight.ToString();
                        DocWidth = "Šířka dokumentu: " + _viewModel.Image.PixelWidth.ToString();
                    }

                    if (this.definitionTemplate.SelectedIndex >= 0)
                    {
                      

                        Functions.processData(LoginWnd.apiSettings.templateUrl + "/" + ((DefinitionTemplateList)definitionTemplate.SelectedItem).id_ocr_sablony, "GET", 6);
                        foreach (DataItem item in MainWindow.dataItemList)
                        {
                            DataChanged = false;
                            boxView.SelectedItems.Clear();

                            var box = new TessBoxControl();
                            box.Value = item.GroupSubValue;
                            box.GroupValueIndex = item.GroupValueIndex;
                            box.GroupValue = item.GroupValue;
                            box.GroupSubValueIndex = item.GroupSubValueIndex;
                            box.GroupSubValue = item.GroupSubValue;
                            box.Width = item.Width;
                            box.Height = item.Height;
                            Canvas.SetLeft(box, item.Left);
                            Canvas.SetTop(box, item.Top);
                            _viewModel.Boxes.Add(box);
                        }
                        LoadedBox = true;
                        LoadedBoxOpacity = 0;

                       
                    }
                    else
                    {
                        LoadedBox = false;
                        LoadedBoxOpacity = 0;
                    }
                }
                else { returnStep = true; this.definitionTemplate.SelectedIndex = templateVariantIndex; }
            } else
            {
                if (_viewModel.Boxes != null) _viewModel.Boxes.Clear();

                if (loadFile == false && definitionTemplate.SelectedIndex == -1)
                {
                    opennedFile = null;
                    _viewModel.Close();

                    LoadedBox = false;
                    LoadedBoxOpacity = 0;

                    _viewModel.Load(System.IO.Path.Combine(LoginWnd.setting_folder, "empty.png"), false);
                    DocHeight = "Výška dokumentu: " + _viewModel.Image.PixelHeight.ToString();
                    DocWidth = "Šířka dokumentu: " + _viewModel.Image.PixelWidth.ToString();
                }

                if (this.definitionTemplate.SelectedIndex >= 0)
                {
                   

                    Functions.processData(LoginWnd.apiSettings.templateUrl + "/" + ((DefinitionTemplateList)definitionTemplate.SelectedItem).id_ocr_sablony, "GET", 6);
                    foreach (DataItem item in MainWindow.dataItemList)
                    {
                        DataChanged = false;
                        boxView.SelectedItems.Clear();

                        var box = new TessBoxControl();
                        box.Value = item.GroupSubValue;
                        box.GroupValueIndex = item.GroupValueIndex;
                        box.GroupValue = item.GroupValue;
                        box.GroupSubValueIndex = item.GroupSubValueIndex;
                        box.GroupSubValue = item.GroupSubValue;
                        box.Width = item.Width;
                        box.Height = item.Height;
                        Canvas.SetLeft(box, item.Left);
                        Canvas.SetTop(box, item.Top);
                        _viewModel.Boxes.Add(box);
                    }
                    LoadedBox = true;
                    LoadedBoxOpacity = 0;
                   
                }
                else
                {
                    LoadedBox = false;
                    LoadedBoxOpacity = 0;
                }
            }

            OpennedFile = this.definitionTemplate.SelectedIndex >= 0 ? true : false;
            OpennedFileOpacity = this.definitionTemplate.SelectedIndex >= 0 ? 1 : 0.5;

            templateVariantIndex = this.definitionTemplate.SelectedIndex;
            if (!returnStep && loadFile && definitionTemplate.SelectedIndex == -1)
            {
                loadFile = false;
            }
        }

        private void btnCreateNew_Click(object sender, RoutedEventArgs e)
        {
            DataChanged = true;
            boxView.SelectedItems.Clear();

            var box = new TessBoxControl();
            box.Value = "?";
            box.Width = 130;
            box.Height = 30;
            Canvas.SetLeft(box, 3);
            Canvas.SetTop(box, 20);
            _viewModel.Boxes.Add(box);

        }


        private void definitionHeaderChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedDefinition = null;
            if (returnStep)
            {
                returnStep = false;
                return;
            }

            if (dataChanged)
            {
                MessageBoxResult result = MessageBox.Show("Provedené změny nebyly uloženy. Chcete změny zahodit?", "Neuložené změny", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    DataChanged = false;
                    if (definitionHeader.SelectedIndex == 0)
                    {
                        opennedFile = null;
                        if (_viewModel != null) { _viewModel.Close(); }
                        MainViewModel.ComboBoxValueList.Clear();
                        MainViewModel.ComboBoxValueGroup.Clear();

                        OpennedFile = false;
                        OpennedFileOpacity = 0;
                        LoadedBox = false;
                        LoadedBoxOpacity = 0;
                        DocHeight = null;
                        DocWidth = null;
                    } else if (definitionHeader.SelectedIndex > 0)
                    {
                        _viewModel.Load(System.IO.Path.Combine(LoginWnd.setting_folder, "empty.png"), false);
                        DocHeight = "Výška dokumentu: " + _viewModel.Image.PixelHeight.ToString();
                        DocWidth = "Šířka dokumentu: " + _viewModel.Image.PixelWidth.ToString();
                    }
                   
                    this.definitionCreate.IsEnabled = this.definitionHeader.SelectedIndex > 0 ? true : false;
                    this.definitionCreate.Opacity = this.definitionHeader.SelectedIndex > 0 ? 1 : 0.5;

                    //this.menuLoad.IsEnabled = (this.definitionHeader.SelectedIndex > 0 && DocHeight != null) ? true : false;
                    //this.menuLoad.Opacity = (this.definitionHeader.SelectedIndex > 0 && DocHeight != null) ? 1 : 0.5;

                    DefinitionTemplateList.Clear();
                    DefinitionTemplateList = new List<DefinitionTemplateList>();

                    List<ComboBoxItem> res = (from data in definitionHeaderList
                                              where data.IsSelected == true
                                              select data).ToList();
                    selectedDefinition = (string)res.First().Content;
                    foreach (DefinitionTemplateList item in fullDefinitionTemplateList)
                    {
                        if (item.id_typu_dokladu.ToString() == (string)res.First().Tag)
                        {
                            DefinitionTemplateList.Add(item);
                            this.definitionTemplate.Items.Refresh();
                        }
                    }
                }
                else { returnStep = true; this.definitionHeader.SelectedIndex = documentVariantIndex; }
            }
            else
            {
                DataChanged = false;
                if (definitionHeader.SelectedIndex == 0)
                {
                    opennedFile = null;
                    if (_viewModel != null) { _viewModel.Close(); }
                    OpennedFile = false;
                    OpennedFileOpacity = 0;
                    LoadedBox = false;
                    LoadedBoxOpacity = 0;
                    DocHeight = null;
                    DocWidth = null;
                }
                else if (definitionHeader.SelectedIndex > 0)
                {
                    _viewModel.Load(System.IO.Path.Combine(LoginWnd.setting_folder, "empty.png"), false);
                    DocHeight = "Výška dokumentu: " + _viewModel.Image.PixelHeight.ToString();
                    DocWidth = "Šířka dokumentu: " + _viewModel.Image.PixelWidth.ToString();
                }

                this.definitionCreate.IsEnabled = this.definitionHeader.SelectedIndex > 0 ? true : false;
                this.definitionCreate.Opacity = this.definitionHeader.SelectedIndex > 0 ? 1 : 0.5;

                //this.menuLoad.IsEnabled = (this.definitionHeader.SelectedIndex > 0 && DocHeight != null) ? true : false;
                //this.menuLoad.Opacity = (this.definitionHeader.SelectedIndex > 0 && DocHeight != null) ? 1 : 0.5;

                DefinitionTemplateList.Clear();
                DefinitionTemplateList = new List<DefinitionTemplateList>();

                List<ComboBoxItem> res = (from data in definitionHeaderList
                                          where data.IsSelected == true
                                          select data).ToList();

                selectedDefinition = (string)res.First().Content;
                foreach (DefinitionTemplateList item in fullDefinitionTemplateList)
                {
                    if (item.id_typu_dokladu.ToString() == (string)res.First().Tag)
                    {
                        DefinitionTemplateList.Add(item);
                        this.definitionTemplate.Items.Refresh();
                    }
                }
            }
            documentVariantIndex = this.definitionHeader.SelectedIndex;

            //selectedDefinition
            List<DefinitionHeaderResponse> resItems = (from data in definitionHeaderResponse
                                                       where data.nazev == selectedDefinition
                                                       select data).ToList();
            if (resItems.Count() > 0)
            {
                foreach (Polozky mainItem in resItems.First().polozky)
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = mainItem.nazev;
                    item.Tag = mainItem.id_doklady_definice;
                    MainViewModel.ComboBoxValueGroup.Add(item);
                  
                }
            }


           
        }

        private void logout_mouseLeave(object sender, MouseEventArgs e)
        {
            Functions.SetHandCursor(false);
        }

        private bool checkAllFields()
        {
            List <TessBoxControl> failItems = new List<TessBoxControl>();

            var dataResult = true;
            foreach (TessBoxControl item in _viewModel.Boxes)
            {
                if (string.IsNullOrEmpty(item.GroupSubValue) || string.IsNullOrEmpty(item.GroupValue))
                {
                    item.IsFail = false;
                    item.IsFail = true;
                    dataResult = false;
                    failItems.Add(item);
                }

            }
            if (!dataResult)
            {
                MessageBoxResult result = MessageBox.Show("Byly nalezeny chybné definice." + Environment.NewLine + "Chcete chybná pole odstranit?", "Kontrola dat", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (TessBoxControl item in failItems)
                    {
                        if (string.IsNullOrEmpty(item.GroupSubValue) || string.IsNullOrEmpty(item.GroupValue))
                        {
                            _viewModel.Boxes.Remove(item);
                        }
                    }
                    dataResult = true;
                }
            }
            return dataResult;
        }



    }
}

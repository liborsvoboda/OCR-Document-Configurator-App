using SprávceŠablonOCR.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
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

    public partial class LoginWnd : Window
    {
        public static string setting_folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppDomain.CurrentDomain.FriendlyName.Split('.')[0].Replace("Service", ""));
        public static string userConfig = "Application.json";
        public static string apiFile = "Api.json";
        public static string logFile = "Application.log";
        public static double interval = 3000000;

        public static List<string> language_list = null;
        public static List<string> selected_list = new List<string>();
        public static List<string> selected_generatorlist = new List<string>();
        public static MainWindow _mainWnd = null;
        public static Settings userSettings = new Settings();
        public static apiSettings apiSettings = new apiSettings();
        public static bool offlineStatus = false;
        public static apiTokenRequest apiToken = new apiTokenRequest();
        public static apiTokenRefreshRequest apiTokenRefreshRequest = new apiTokenRefreshRequest() { refresh = true };
        public static apiBearerToken userBearerToken = new apiBearerToken();
        public static userInfoResponse userInfoResponse = new userInfoResponse();
        private static System.Timers.Timer userLoginApiTimer = new System.Timers.Timer() { Enabled = true, Interval = interval };
      //  public static templateListResponse templateListResponse = new templateListResponse();
        public static List<DefinitionHeaderResponse> definitionHeaderResponse = new List<DefinitionHeaderResponse>();

        public LoginWnd()
        {
            userLoginApiTimer.Elapsed += new ElapsedEventHandler(OnElapsedTime);

            InitializeComponent();
            Functions.generateFolderStructure();
            Functions.GetInstalledLanguages();
            Functions.loadSettings();

            //App.Current.MainWindow.IsEnabled = false;
            this.LoginName.Focus();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            apiToken.username = this.LoginName.Text;
            apiToken.password = this.Password.Password;
            apiToken.salt = new DateTimeOffset(DateTime.UtcNow).AddHours(0).ToUnixTimeMilliseconds(); ;
            apiToken.password = SHA1Util.SHA1HashStringForUTF8String(MD5Utils.Hex_md5(apiToken.password.Trim() + apiToken.username.Trim()) + apiToken.salt);

            if (Functions.getToken(LoginWnd.apiToken))
            {
                userSettings.lastLogin = DateTime.Now;
                Functions.getUserInfo();
                MainWindow.StatusLabelText = "Přihlášen: " + LoginWnd.userInfoResponse.userName;

                App.Current.MainWindow.Hide();
                _mainWnd = new MainWindow();
                _mainWnd.Owner = this;
                _mainWnd.ShowActivated = true;
                _mainWnd.Show();
            }
            else
            {
                MessageBox.Show("Chybné jméno nebo heslo", "Chybné přihlášení", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }

        }

        public static void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            userLoginApiTimer.Enabled = true;
            if (!String.IsNullOrEmpty(userBearerToken.bearerToken))
            {
                if (Functions.RefreshToken())
                {
                    Functions.getUserInfo();
                    MainWindow.StatusLabelText = "Přihlášen: " + LoginWnd.userInfoResponse.userName;
                }
            }
        }

    }
}

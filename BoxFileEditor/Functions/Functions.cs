using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.Web.Script.Serialization;
using SprávceŠablonOCR.Classes;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Windows.Controls;

namespace BoxFileEditor
{
    public class Functions
    {

        public static string GetTesseractFolder()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Tesseract-OCR"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("UninstallString");
                        return System.IO.Path.GetDirectoryName(o.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public static void GenerateBoxFile(string selectedImage, string selectedLanguages)
        {
            SetWaitingCursor(true);

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppDomain.CurrentDomain.FriendlyName.Split('.')[0],"generated.box")))
            {
                File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppDomain.CurrentDomain.FriendlyName.Split('.')[0], "generated.box"));
            }

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(GetTesseractFolder(), "tesseract.exe"),
                    Arguments ="\"" + selectedImage + "\"" + " generated "+ selectedLanguages + " batch.nochop makebox",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppDomain.CurrentDomain.FriendlyName.Split('.')[0])
                }
            };

            proc.Start();
            proc.WaitForExit();
            SetWaitingCursor(false);
        }

        public static void runExternalApp(string runApp)
        {
            SetWaitingCursor(true);
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = runApp,
                    Arguments = "",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppDomain.CurrentDomain.FriendlyName.Split('.')[0])
                }
            };

            proc.Start();
            proc.WaitForExit();
            SetWaitingCursor(false);
        }

        public static void GetInstalledLanguages()
        {
            SetWaitingCursor(true);
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(GetTesseractFolder(), "tesseract.exe"),
                    Arguments = " --list-langs",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppDomain.CurrentDomain.FriendlyName.Split('.')[0])
                }
            };

            proc.Start();
            var input = proc.StandardOutput.ReadToEnd();
            LoginWnd.language_list = input.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
            LoginWnd.language_list.RemoveAt(0);
            LoginWnd.language_list.Remove("");
            //string err = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            SetWaitingCursor(false);
        }

        public static void SetWaitingCursor(bool status)
        {
            if (status) {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            } else
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        public static void SetHandCursor(bool status)
        {
            if (status)
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Hand;
            }
            else
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }

       

        public static bool saveSettings(Settings settings)
        {
            LoginWnd.selected_generatorlist = new List<string>(settings.languages);
            LoginWnd.selected_list = new List<string>(settings.languages);
            LoginWnd.userSettings.languages = new List<string>(settings.languages);
            if (File.Exists(Path.Combine(LoginWnd.setting_folder, LoginWnd.userConfig)))
            {
                File.Delete(Path.Combine(LoginWnd.setting_folder, LoginWnd.userConfig));
            }
            Functions.WriteToFile(Path.Combine(LoginWnd.setting_folder, LoginWnd.userConfig), new JavaScriptSerializer().Serialize(settings));
            return true;
        }

        public static void loadSettings()
        {
            if (File.Exists(Path.Combine(LoginWnd.setting_folder, LoginWnd.userConfig)))
            {
                using (StreamReader sr = new StreamReader(Path.Combine(LoginWnd.setting_folder, LoginWnd.userConfig)))
                {
                    string line = sr.ReadToEnd();
                    LoginWnd.userSettings = new JavaScriptSerializer().Deserialize<Settings>(line);
                    sr.Close();
                }
            }

            if (LoginWnd.userSettings != null) {
                LoginWnd.selected_generatorlist = new List<string>(LoginWnd.userSettings.languages);
                LoginWnd.selected_list = new List<string>(LoginWnd.userSettings.languages);
            } 
           

            if (File.Exists(Path.Combine(LoginWnd.setting_folder, LoginWnd.apiFile)))
            {
                using (StreamReader sr = new StreamReader(Path.Combine(LoginWnd.setting_folder, LoginWnd.apiFile)))
                {
                    string line = sr.ReadToEnd();
                    LoginWnd.apiSettings = new JavaScriptSerializer().Deserialize<apiSettings>(line);
                    sr.Close();
                }
            }
        }

        private static void WriteToFile(string file, string Message)
        {
            if (!File.Exists(file))
            {
                using (StreamWriter sw = File.CreateText(file))
                {
                    sw.WriteLine(Message);
                    sw.Close();
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(file))
                {
                    sw.WriteLine(Message);
                    sw.Close();
                }
            }
        }

        public string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static bool getToken(apiTokenRequest apiTokenRequest)
        {
            try
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(LoginWnd.apiSettings.serverIP);

                if (pingReply.Status == IPStatus.Success)
                {
                    LoginWnd.offlineStatus = false;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;
                    ServicePointManager.ServerCertificateValidationCallback =
                    delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                    {
                        return true;
                    };

                    HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(LoginWnd.apiSettings.loginUrl);
                    httpRequest.ContentType = "application/json";
                    httpRequest.Method = "POST";
                    httpRequest.Credentials = CredentialCache.DefaultCredentials;


                    using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                    {
                        streamWriter.Write(new JavaScriptSerializer().Serialize(apiTokenRequest));
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        streamReader.Close();
                        if (LoginWnd.apiSettings.writeToLog)
                        {
                            WriteToFile(Path.Combine(LoginWnd.setting_folder, LoginWnd.logFile), "resultOfLoginPost: " + result.ToString());
                        }
                        //MessageBox.Show(result);
                        if (String.IsNullOrEmpty(new JavaScriptSerializer().Deserialize<apiTokenResponse>(result).access_token))
                            return false;
                        else
                        {
                            LoginWnd.userBearerToken.token_type = new JavaScriptSerializer().Deserialize<apiTokenResponse>(result).token_type;
                            LoginWnd.userBearerToken.bearerToken = new JavaScriptSerializer().Deserialize<apiTokenResponse>(result).access_token;
                            return true;
                        }
                    }
                }
                else LoginWnd.offlineStatus = true;
            }
            catch (Exception e)
            {
                LoginWnd.offlineStatus = true;
                return false;
            }
            return false;
        }

        public static bool RefreshToken()
        {
            try
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(LoginWnd.apiSettings.serverIP);

                if (pingReply.Status == IPStatus.Success)
                {
                    LoginWnd.offlineStatus = false;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;
                    ServicePointManager.ServerCertificateValidationCallback =
                    delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                    {
                        return true;
                    };

                    HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(LoginWnd.apiSettings.refreshUrl);
                    httpRequest.ContentType = "application/json";
                    httpRequest.Method = "POST";
                    httpRequest.Credentials = CredentialCache.DefaultCredentials;
                    httpRequest.Headers.Add("Authorization", LoginWnd.userBearerToken.token_type + " " + LoginWnd.userBearerToken.bearerToken);

                    using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                    {
                        streamWriter.Write(new JavaScriptSerializer().Serialize(LoginWnd.apiTokenRefreshRequest));
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        streamReader.Close();
                        if (LoginWnd.apiSettings.writeToLog)
                        {
                            WriteToFile(Path.Combine(LoginWnd.setting_folder, LoginWnd.logFile), "resultOfRefreshPost: " + result.ToString());
                        }
                        if (!new JavaScriptSerializer().Deserialize<apiTokenRefreshResponse>(result).success)
                            return false;
                        else
                        {
                            return true;
                        }
                    }
                }
                else LoginWnd.offlineStatus = true;
            }
            catch (Exception e)
            {
                LoginWnd.offlineStatus = true;
                return false;
            }
            return false;
        }

        public static bool Logout()
        {
            try
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(LoginWnd.apiSettings.serverIP);

                if (pingReply.Status == IPStatus.Success)
                {
                    LoginWnd.offlineStatus = false;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;
                    ServicePointManager.ServerCertificateValidationCallback =
                    delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                    {
                        return true;
                    };

                    HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(LoginWnd.apiSettings.logoutUrl);
                    //httpRequest.ContentType = "application/json";
                    httpRequest.Method = "GET";
                    httpRequest.Credentials = CredentialCache.DefaultCredentials;
                    httpRequest.Headers.Add("Authorization", LoginWnd.userBearerToken.token_type + " " + LoginWnd.userBearerToken.bearerToken);

                   var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        streamReader.Close();
                        if (LoginWnd.apiSettings.writeToLog)
                        {
                            WriteToFile(Path.Combine(LoginWnd.setting_folder, LoginWnd.logFile), "resultOfLogoutPost: " + result.ToString());
                        }
                        if (!new JavaScriptSerializer().Deserialize<apiTokenRefreshResponse>(result).success)
                            return false;
                        else
                        {
                            return true;
                        }
                    }
                }
                else LoginWnd.offlineStatus = true;
            }
            catch (Exception e)
            {
                LoginWnd.offlineStatus = true;
                return false;
            }
            return false;
        }

        public static bool getUserInfo()
        {
            try
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(LoginWnd.apiSettings.serverIP);

                if (pingReply.Status == IPStatus.Success)
                {
                    LoginWnd.offlineStatus = false;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;
                    ServicePointManager.ServerCertificateValidationCallback =
                    delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                    {
                        return true;
                    };

                    HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(LoginWnd.apiSettings.userInfoUrl);
                    //httpRequest.ContentType = "application/json";
                    httpRequest.Method = "GET";
                    httpRequest.Credentials = CredentialCache.DefaultCredentials;

                    httpRequest.Headers.Add("Authorization", LoginWnd.userBearerToken.token_type + " " + LoginWnd.userBearerToken.bearerToken);

                    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        streamReader.Close();
                        if (LoginWnd.apiSettings.writeToLog)
                        {
                            WriteToFile(Path.Combine(LoginWnd.setting_folder, LoginWnd.logFile), "resultOfUserInfoPost: " + result.ToString());
                        }
                        if (String.IsNullOrEmpty(new JavaScriptSerializer().Deserialize<userInfoResponse>(result).userName))
                            return false;
                        else
                        {
                            LoginWnd.userInfoResponse = new JavaScriptSerializer().Deserialize<userInfoResponse>(result);
                            return true;
                        }
                    }
                }
                else LoginWnd.offlineStatus = true;
            }
            catch (Exception e)
            {
                LoginWnd.offlineStatus = true;
                return false;
            }
            return false;
        }

        public static void generateFolderStructure()
        {
            if (!Directory.Exists(LoginWnd.setting_folder))
            {
                Directory.CreateDirectory(LoginWnd.setting_folder);
                DirectoryInfo FolderInfo = new DirectoryInfo(LoginWnd.setting_folder);
                DirectorySecurity FolderAcl = new DirectorySecurity();
                FolderAcl.AddAccessRule(new FileSystemAccessRule("Users", FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                FolderInfo.SetAccessControl(FolderAcl);
            }

            if (!File.Exists(Path.Combine(LoginWnd.setting_folder, LoginWnd.userConfig)))
            {
                File.Create(Path.Combine(LoginWnd.setting_folder, LoginWnd.userConfig)).Close();
            }
        }

        public static bool processData(string url,string requestType, int selectedRequest)
        {
            try
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(LoginWnd.apiSettings.serverIP);

                if (pingReply.Status == IPStatus.Success)
                {
                    LoginWnd.offlineStatus = false;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;
                    ServicePointManager.ServerCertificateValidationCallback =
                    delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                    {
                        return true;
                    };

                    HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
                    //httpRequest.ContentType = "application/json";
                    httpRequest.Method = requestType;
                    httpRequest.Credentials = CredentialCache.DefaultCredentials;

                    httpRequest.Headers.Add("Authorization", LoginWnd.userBearerToken.token_type + " " + LoginWnd.userBearerToken.bearerToken);

                    switch (selectedRequest)
                    {
                        case 2:
                            httpRequest.ContentType = "application/json";
                            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                            {
                                streamWriter.Write(new JavaScriptSerializer().Serialize(MainWindow.definitionTemplateSaveRequest));
                                streamWriter.Flush();
                                streamWriter.Close();
                            }
                            break;
                        case 5:
                            httpRequest.ContentType = "application/json";
                            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                            {
                                streamWriter.Write(new JavaScriptSerializer().Serialize(MainWindow.definitionTemplateEditRequest));
                                streamWriter.Flush();
                                streamWriter.Close();
                            }
                            break;
                        case 6:
                            break;
                        default:
                            break;
                    }

                    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        streamReader.Close();
                        if (LoginWnd.apiSettings.writeToLog)
                        {
                            WriteToFile(Path.Combine(LoginWnd.setting_folder, LoginWnd.logFile), "resultOfDataRequest: " + result.ToString());
                        }

                        switch (selectedRequest)
                        {
                            case 1:
                                LoginWnd.definitionHeaderResponse = new JavaScriptSerializer().Deserialize<List<DefinitionHeaderResponse>>(result);
                                MainWindow.definitionHeaderResponse = LoginWnd.definitionHeaderResponse;
                                foreach (DefinitionHeaderResponse document in LoginWnd.definitionHeaderResponse)
                                {
                                    ComboBoxItem item = new ComboBoxItem();
                                    item.Content = document.nazev;
                                    item.Tag = document.id_doklady_definice;
                                    MainWindow.DefinitionHeaderList.Add(item);
                                }
                                break;
                            //case 2:
                            //    LoginWnd.templateListResponse = new JavaScriptSerializer().Deserialize<templateListResponse>(result);
                            //    break;
                            case 2:
                                MainWindow.definitionTemplateSaveResponse = new JavaScriptSerializer().Deserialize<DefinitionTemplateSaveResponse>(result);
                                break;
                            case 3:
                                MainWindow.fullDefinitionTemplateList = new JavaScriptSerializer().Deserialize<List<DefinitionTemplateList>>(result);

                                MainWindow.DefinitionTemplateList.Clear();
                                MainWindow.DefinitionTemplateList = new List<DefinitionTemplateList>();

                                List<ComboBoxItem> res = (from data in MainWindow.definitionHeaderList
                                                          where data.IsSelected == true
                                                          select data).ToList();

                                foreach (DefinitionTemplateList item in MainWindow.fullDefinitionTemplateList)
                                {
                                    if (item.id_typu_dokladu.ToString() == (string)res.First().Tag)
                                    {
                                        MainWindow.DefinitionTemplateList.Add(item);
                                    }
                                }

                                break;
                            case 4:
                                break;
                            case 5:
                                break;
                            case 6:
                                MainWindow.definitionTemplateSaveResponse = new JavaScriptSerializer().Deserialize<DefinitionTemplateSaveResponse>(result);
                                MainWindow.dataItemList.Clear();
                                try
                                {
                                    MainWindow.dataItemList = new JavaScriptSerializer().Deserialize<List<DataItem>>(MainWindow.definitionTemplateSaveResponse.data);
                                }
                                catch (Exception e)
                                {

                                }
                                break;
                            default:
                                break;
                        }
                        return true;
                       
                    }
                }
               
            }
            catch (Exception e)
            {
                return false;
            }
            return false;
        }

    }

}
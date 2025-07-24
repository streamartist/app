// Controls the basic HTML view.


using AForge.Video.DirectShow;
using Microsoft.Web.WebView2.WinForms;
using StreamArtist.Domain;
using StreamArtist.Services;
using StreamArtistLib.Services;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Timer = System.Windows.Forms.Timer;

namespace StreamArtist.Controllers
{
    public class AppController
    {
        internal WebView2 MainView;
        public SettingsController SettingsController;
        public WebServerService WebServerService = new WebServerService();
        private readonly NetworkService _networkService = new NetworkService();
        private readonly SettingsService _settingsService = new SettingsService();

        private readonly YouTubeChatService youTubeChatService = new YouTubeChatService();

        private PdgSceneService _pdgSceneService;
        private Timer _timer;

        // TODO: rename cloudstream-streamkey to streamartist-streamkey
        string[] fieldIds = ["server-address", "youtube-streamkey", "twitch-streamkey", "cloudstream-streamkey", "tos", "shorts-streamkey", "shorts-filter", "obs-password", "obs-port"];
        string[] statusFieldNames = ["status-text", "control-server-status", "streaming-server-status", "control-server-security", "effects-server-status"];
        bool docLoaded = false;


        // Add constructor
        public AppController(WebView2 webView)
        {
            MainView = webView;
            SettingsController = new SettingsController(this);
        }

        public void OnDocumentLoaded()
        {
            // TODO: why is this loaded twice?
            if (docLoaded) return;
            docLoaded = true;

            SettingsController.LoadGoogleSignInStatus();
            WebServerService.StartLocalServer();
            LoggingService.Instance.Log("Starting log.");

            var settings = _settingsService.GetSettings();
            if (int.TryParse(settings["obs-port"], out int port))
            {
                var obsService = new OBSService(port, settings["obs-password"]);
                _pdgSceneService = new PdgSceneService(obsService, youTubeChatService, _settingsService);
            }
            else
            {
                Debug.WriteLine("OBS port not configured or invalid.");
            }

            _timer = new Timer();
            _timer.Tick += TimerTickEvent;
            _timer.Interval = 10000;
            _timer.Enabled = true;
        }


        private async void TimerTickEvent(object source, EventArgs e)
        {
            CheckHttpConnection();

            if (_pdgSceneService != null && FlagService.GetFlag(FlagId.ArEffectsEnabled).Value)
            {
                await _pdgSceneService.Update();
                _timer.Interval = (int) youTubeChatService.PollingIntervalMillis;

                // *****************************************************************************************************
                _timer.Enabled = false;

                LoggingService.Instance.Log("Timer is at " + _timer.Interval.ToString());
            }

            // YouTubeChatService youTubeChatService = new YouTubeChatService(_settingsService);
            // var logger = LoggingService.Instance;

            // var channelId = await youTubeChatService.GetConnectedChannelId();
            // var videos = await youTubeChatService.GetLiveStreams(channelId);
            // if (videos.Count > 0)

            // {
            //     var video = videos[0];
            //     var chats = await youTubeChatService.GetNewChatMessages(video.VideoId);

            //     foreach (var chat in chats)
            //     {
            //         string logMessage = $"Author: {chat.AuthorName}, Message: {chat.Message}";
            //         if (chat.IsSuperChat)
            //         {
            //             logMessage += $", SuperChat Amount: {chat.Amount}";
            //         }
            //         logger.Log(logMessage);
            //     }
            // }
        }

        private async void CheckHttpConnection()
        {
            // server-status-text
            var server = _settingsService.GetSettings()["server-address"];
            //GetSecureStorageValueSync("server-address");
            foreach (string status in statusFieldNames)
            {
                // Debug.WriteLine("Checking " + status);
                Eval($"setEl(status + '-text','Waiting...')");
            }
            Eval($"setEl('server-status-text','Connecting to {server}...')");


            post("check");

        }

        private async void post(string action)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            try
            {
                // TODO: This doesn't belong here.
                result = await _networkService.PostAsync(WebServerService.HealthCheckUrl, action, new Dictionary<string, string>());
            }
            catch (Exception err)
            {
                result["effects-server-status"] = "Unknown error.";
            }

            if (FlagService.GetFlag(FlagId.SimulstreamEnabled).Value)
            {
                try
                {
                    var server = _settingsService.GetSettings()["server-address"];
                    var tos = _settingsService.GetSettings()["tos"];
                    if (tos != "true")
                    {
                        return;
                    }

                    Dictionary<string, string> requestDict = GetSettings();
                    var r = await _networkService.PostAsync(server, action, requestDict);
                    foreach (KeyValuePair<string, string> entry in r)
                    {
                        result[entry.Key] = entry.Value;
                    }
                }
                catch (Exception err)
                {
                    result["status-text"] = "Unknown error. Server up?";
                    result["control-server-status-text"] = "Unknown error. Server up?";
                    result["streaming-server-status-text"] = "Unknown error. Server up?";
                    result["control-server-security-text"] = "Unknown error. Server up?";
                }
            }
            UpdateStatusUI(result);
        }

        public void OnWebViewEvent(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri.StartsWith("csharp://"))
            {
                e.Cancel = true; // Cancel the navigation

                if (e.Uri.StartsWith("csharp://save-settings/"))
                {
                    var p = System.Web.HttpUtility.ParseQueryString(e.Uri.Split('?')[1]);
                    OnSave(p["values"]);

                    return;
                }

                if (e.Uri.StartsWith("csharp://send-gift/"))
                {
                    // ********************************************************************
                    TimerTickEvent(null, null);

                    var p = System.Web.HttpUtility.ParseQueryString(e.Uri.Split('?')[1]);
                    OnSendGift(p["name"], p["message"], float.Parse(p["amount"]));



                    

                    return;
                }

                if (e.Uri.StartsWith("csharp://start"))
                {
                    OnStart();
                    return;
                }

                if (e.Uri.StartsWith("csharp://connect-youtube"))
                {
                    SettingsController.OnGoogleSignInClicked();
                    return;
                }
                if (e.Uri.StartsWith("csharp://document-loaded"))
                {
                    OnDocumentLoaded();
                    return;
                }
                if (e.Uri.StartsWith("csharp://open-logs-directory"))
                {
                    SettingsController.LoadGoogleSignInStatus();

                    //Launcher.OpenAsync("file://" + LoggingService.Instance.LogDirectory); //(new LauncherOptions { Uri = new Uri(LoggingService.Instance.LogDirectory });
                    Process myProcess = new Process();
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = "file://" + LoggingService.Instance.LogDirectory;
                    myProcess.Start();
                    return;
                }
            }
        }

        private void UpdateStatusUI(Dictionary<string, string> status)
        {
            string json = JsonSerializer.Serialize(status);
            string jsd = $"updateStatus('{json}')";
            Eval(jsd);
        }

        public Task<string> Eval(string code)
        {
            // TODO: is this good?
            code = code.ReplaceLineEndings("");

            if (code.Contains("\\n") || code.Contains("\n"))
            {
                throw new Exception("Can't have newlines in javascript");
            }
            Debug.WriteLine($"JS => {code}");
            return MainView.ExecuteScriptAsync(code);
            //MainView.Dispatcher.DispatchAsync(() =>
            //{

            //    return MainView.EvaluateJavaScriptAsync(code);
            //});
        }

        public void OnSendGift(string name, string message, double amount)
        {
            youTubeChatService.AddLocalChatMessage(name, message, true, amount);
        }

        public void OnStart()
        {
            GoogleOAuthService service = new GoogleOAuthService();
            var accessToken = service.GetAccessToken();
            Console.WriteLine("Access token: " + accessToken);

            //post("start");
        }

        public void OnSave(String values)
        {
            _settingsService.SaveSettings(values);
        }

        private Dictionary<string, string> GetSettings()
        {
            return _settingsService.GetSettings();
        }



        public async void LoadHtml()
        {

            

            // Get the assembly containing the embedded resource
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Add this at the beginning of LoadEmbeddedHtml to debug
            var names = assembly.GetManifestResourceNames();
            //MessageBox.Show("Available Resources:\n" + string.Join("\n", names));

            // Get the name of the embedded resource (adjust based on your project structure and file name)
            // The format is typically: Namespace.Folder.FileName.Extension
            string resourceName = "StreamArtist.Resources.Raw.index.html";
            var osService = new OSService();

            // Read the embedded resource as a stream
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    // Read the HTML content from the stream
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string htmlContent = reader.ReadToEnd();

                        var keys = JsonSerializer.Serialize(GetSettings());
                        var webcams  = JsonSerializer.Serialize(osService.GetWebCams());
                        htmlContent = htmlContent.Replace("{/*fieldValues*/ }", keys);
                        htmlContent = htmlContent.Replace("{/*flags*/ }", JsonSerializer.Serialize(FlagService.GetFlags()));
                        htmlContent = htmlContent.Replace("{/*webcams*/ }", webcams);
                        // Load the HTML content into the WebView2
                        MainView.NavigateToString(htmlContent);
                    }
                }
                else
                {
                    // Handle the case where the resource is not found (e.g., log an error)
                    MessageBox.Show($"Embedded resource '{resourceName}' not found.");
                }
            }
        }

        private string GetBaseUrl()
        {
            //if (DeviceInfo.Platform == DevicePlatform.WinUI)
            //{
            //    return $"ms-appx-web:///Resources/Raw/";
            //}
            //else if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            //{
            //    var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Resources), "Raw");
            //    return $"file://{basePath}/";
            //}
            //else
            //{
            // Assuming this covers Android and iOS
            //return $"file:///android_asset/";
            //}
            return "";
        }
    }
}
// Controls the basic HTML view.


using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Timers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using StreamArtist.Services;
using StreamArtist.Domain;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using System.Threading.Tasks;
using StreamArtistLib.Services;

namespace StreamArtist.Controllers
{
    public class AppController
    {
        private WebView MainView;
        public SettingsController SettingsController;
        public WebServerService WebServerService = new WebServerService();

        private readonly NetworkService _networkService = new NetworkService();
        private readonly SettingsService _settingsService = new SettingsService();

        private readonly YouTubeChatService youTubeChatService = new YouTubeChatService();

        private PdgSceneService _pdgSceneService;
        private Timer _timer;

        // TODO: rename cloudstream-streamkey to streamartist-streamkey
        string[] fieldIds = ["server-address", "youtube-streamkey", "twitch-streamkey", "cloudstream-streamkey", "tos", "shorts-streamkey", "shorts-filter","obs-password","obs-port"];
        string[] statusFieldNames = ["status-text", "control-server-status", "streaming-server-status", "control-server-security", "effects-server-status"];


        // Add constructor
        public AppController(WebView webView)
        {
            MainView = webView;
            SettingsController = new SettingsController(this);
        }

        public void OnDocumentLoaded()
        {
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

            _timer = new System.Timers.Timer();
            _timer.Elapsed += new ElapsedEventHandler(TimerTickEvent);
            _timer.Interval = 5000;
            _timer.Enabled = true;
        }

        private async void TimerTickEvent(object source, ElapsedEventArgs e)
        {
            CheckHttpConnection();

            if (_pdgSceneService != null && FlagService.GetFlag(FlagId.ArEffectsEnabled).Value)
            {
                await _pdgSceneService.Update();
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

        public void OnWebViewEvent(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.StartsWith("csharp://"))
            {
                e.Cancel = true; // Cancel the navigation

                if (e.Url.StartsWith("csharp://save-settings/"))
                {
                    var p = System.Web.HttpUtility.ParseQueryString(e.Url.Split('?')[1]);
                    OnSave(p["values"]);

                    return;
                }

                if (e.Url.StartsWith("csharp://send-gift/"))
                {
                    var p = System.Web.HttpUtility.ParseQueryString(e.Url.Split('?')[1]);
                    OnSendGift(p["name"],p["message"], float.Parse(p["amount"]));

                    return;
                }

                if (e.Url.StartsWith("csharp://start"))
                {
                    OnStart();
                    return;
                }

                if (e.Url.StartsWith("csharp://connect-youtube"))
                {
                    SettingsController.OnGoogleSignInClicked();
                    return;
                }
                if (e.Url.StartsWith("csharp://document-loaded"))
                {
                    OnDocumentLoaded();
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
            return MainView.Dispatcher.DispatchAsync(() =>
            {
                Debug.WriteLine($"JS => {code}");
                return MainView.EvaluateJavaScriptAsync(code);
            });
        }

        public void OnSendGift(string name, string message, double amount)
        {
            youTubeChatService.AddLocalChatMessage(name,message,true,amount);
        }

        public void OnStart()
        {
            GoogleOAuthService service =   new GoogleOAuthService();
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

            using var stream = await FileSystem.OpenAppPackageFileAsync("index.html");

            using var reader = new StreamReader(stream);

            var contents = reader.ReadToEnd();
            // Note the space after the comment. File formatter
            // wants this.
            contents = contents.Replace("{/*fieldValues*/ }", JsonSerializer.Serialize(GetSettings()));

            contents = contents.Replace("{/*flags*/ }", JsonSerializer.Serialize(FlagService.GetFlags()));

            HtmlWebViewSource source = new HtmlWebViewSource
            {
                Html = contents,
            };
            MainView.Source = source;
            MainView.EvaluateJavaScriptAsync(@"
            updateStatus('Waiting...');
        ");
        }

        private string GetBaseUrl()
        {
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                return $"ms-appx-web:///Resources/Raw/";
            }
            else if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            {
                var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Resources), "Raw");
                return $"file://{basePath}/";
            }
            else
            {
                // Assuming this covers Android and iOS
                return $"file:///android_asset/";
            }
        }
    }
}
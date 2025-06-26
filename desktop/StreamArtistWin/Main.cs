using StreamArtist.Controllers;
using StreamArtist.Services;
using StreamArtist.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Text.Json;

namespace StreamArtist
{
    public partial class Main : Form
    {

        private readonly NetworkService _networkService = new NetworkService();
        private readonly SettingsService _settingsService = new SettingsService();

        private AppController _appController;

        string[] fieldIds = ["server-address", "youtube-streamkey", "twitch-streamkey", "streamartist-streamkey", "tos", "shorts-streamkey", "shorts-filter"];
        string[] statusFieldNames = ["status-text", "control-server-status", "streaming-server-status", "control-server-security"];


        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            InitializeWebView2();
        }
        private async void InitializeWebView2()
        {
            // Ensure the CoreWebView2 is initialized
            await MainView.EnsureCoreWebView2Async(null);

            // Load the embedded HTML file
            //LoadHtml();

            MainView.NavigationStarting += MainView_NavigationStarting;
            MainView.NavigationCompleted += MainView_NavigationCompleted;
            _appController = new AppController(MainView);
            LoadHtml();


            System.Timers.Timer timer = new System.Timers.Timer();
            // timer.Elapsed += new ElapsedEventHandler(TimerTickEvent);
            // timer.Interval = 15000;
            // timer.Enabled = false;
        }

        // Implement a call with the right signature for events going off
        private async void TimerTickEvent(object source, ElapsedEventArgs e)
        {
            // CheckHttpConnection();

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

        private void MainView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MainView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
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

                if (e.Uri.StartsWith("csharp://start"))
                {
                    OnStart();
                    return;
                }

                if (e.Uri.StartsWith("csharp://connect-youtube"))
                {
                    _appController.SettingsController.OnGoogleSignInClicked();
                    return;
                }
                if (e.Uri.StartsWith("csharp://document-loaded"))
                {
                    _appController.OnDocumentLoaded();
                    return;
                }
            }
        }

        private void LoadEmbeddedHtml()
        {
            
        }

        private async void CheckHttpConnection()
        {
            // server-status-text
            var server = _settingsService.GetSettings()["server-address"];
            //GetSecureStorageValueSync("server-address");
            foreach (string status in statusFieldNames)
            {
                // Debug.WriteLine("Checking " + status);
                _appController.Eval($"setEl(status + '-text','Waiting...')");
            }
            _appController.Eval($"setEl('server-status-text','Connecting to {server}...')");


            post("check");

        }

        private async void post(string action)
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
                var result = await _networkService.PostAsync(server, action, requestDict);
                UpdateStatusUI(result);
            }
            catch (Exception err)
            {
                UpdateStatusUI(new Dictionary<string, string>
                {
                    ["control-server-status"] = "Unknown error. Server up? "
                });
            }
        }

        private void UpdateStatusUI(Dictionary<string, string> status)
        {
            string json = JsonSerializer.Serialize(status);
            string jsd = $"updateStatus('{json}')";
            _appController.Eval(jsd);
        }

        //private async void OnNavigated(object sender, WebNavigatedEventArgs e)
        //{
        //    _appController.Eval("updateStatus('Waiting... Yay');");

        //}

        //// Old maui code
        //private void OnNavigating(object sender, WebNavigatingEventArgs e)
        //{
        //    if (e.Url.StartsWith("csharp://"))
        //    {
        //        e.Cancel = true; // Cancel the navigation

        //        if (e.Url.StartsWith("csharp://save-settings/"))
        //        {
        //            var p = System.Web.HttpUtility.ParseQueryString(e.Url.Split('?')[1]);
        //            OnSave(p["values"]);

        //            return;
        //        }

        //        if (e.Url.StartsWith("csharp://start"))
        //        {
        //            OnStart();
        //            return;
        //        }

        //        if (e.Url.StartsWith("csharp://connect-youtube"))
        //        {
        //            _appController.SettingsController.OnGoogleSignInClicked();
        //            return;
        //        }
        //        if (e.Url.StartsWith("csharp://document-loaded"))
        //        {
        //            _appController.OnDocumentLoaded();
        //            return;
        //        }
        //    }
        //}

        private void OnStart()
        {
            post("start");
        }

        private void OnSave(String values)
        {
            _settingsService.SaveSettings(values);
        }

        private Dictionary<string, string> GetSettings()
        {
            return _settingsService.GetSettings();
        }



        protected async void LoadHtml()
        {
            // Get the assembly containing the embedded resource
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Add this at the beginning of LoadEmbeddedHtml to debug
            var names = assembly.GetManifestResourceNames();
            //MessageBox.Show("Available Resources:\n" + string.Join("\n", names));

            // Get the name of the embedded resource (adjust based on your project structure and file name)
            // The format is typically: Namespace.Folder.FileName.Extension
            string resourceName = "StreamArtist.Resources.Raw.index.html";

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
                        htmlContent = htmlContent.Replace("{/*fieldValues*/ }",keys );
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

        //    using var stream = await FileSystem.OpenAppPackageFileAsync("index.html");

        //    using var reader = new StreamReader(stream);

        //    var contents = reader.ReadToEnd();
        //    // Note the space after the comment. File formatter
        //    // wants this.
        //    contents = contents.Replace("{/*fieldValues*/ }", JsonSerializer.Serialize(GetSettings()));

        //    HtmlWebViewSource source = new HtmlWebViewSource
        //    {
        //        Html = contents,
        //    };
        //    MainView.Source = source;
        //    MainView.EvaluateJavaScriptAsync(@"
        //    updateStatus('Waiting...');
        //");
        }

        //private string GetBaseUrl()
        //{
        //    if (DeviceInfo.Platform == DevicePlatform.WinUI)
        //    {
        //        return $"ms-appx-web:///Resources/Raw/";
        //    }
        //    else if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
        //    {
        //        var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Resources), "Raw");
        //        return $"file://{basePath}/";
        //    }
        //    else
        //    {
        //        // Assuming this covers Android and iOS
        //        return $"file:///android_asset/";
        //    }
        //}
    }
}
namespace StreamArtist;

using Microsoft.Maui.Controls;
using System.Text.Json;
using System;
using StreamArtist.Services;
using StreamArtist.Controllers;
using System.Timers;

public partial class MainPage : ContentPage
{
    private readonly NetworkService _networkService = new NetworkService();
    private readonly SettingsService _settingsService = new SettingsService();

    private AppController _appController;

    string[] fieldIds = ["server-address", "youtube-streamkey", "twitch-streamkey", "streamartist-streamkey", "tos", "shorts-streamkey", "shorts-filter"];
    string[] statusFieldNames = ["status-text", "control-server-status", "streaming-server-status", "control-server-security"];

    public MainPage()
    {
        InitializeComponent();        
        _appController = new AppController(MainView);
        LoadHtml();
        MainView.Navigated += OnNavigated;
        MainView.Navigating += OnNavigating;

        _networkService = new NetworkService();
        _settingsService = new SettingsService();

        Timer timer = new System.Timers.Timer();
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

    private async void OnNavigated(object sender, WebNavigatedEventArgs e)
    {
        _appController.Eval("updateStatus('Waiting... Yay');");

    }

    private void OnNavigating(object sender, WebNavigatingEventArgs e)
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

            if (e.Url.StartsWith("csharp://start"))
            {
                OnStart();
                return;
            }

            if (e.Url.StartsWith("csharp://connect-youtube"))
            {
                _appController.SettingsController.OnGoogleSignInClicked();
                return;
            }
            if (e.Url.StartsWith("csharp://document-loaded"))
            {
                _appController.OnDocumentLoaded();
                return;
            }
        }
    }

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

        using var stream = await FileSystem.OpenAppPackageFileAsync("index.html");

        using var reader = new StreamReader(stream);

        var contents = reader.ReadToEnd();
        // Note the space after the comment. File formatter
        // wants this.
        contents = contents.Replace("{/*fieldValues*/ }", JsonSerializer.Serialize(GetSettings()));

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


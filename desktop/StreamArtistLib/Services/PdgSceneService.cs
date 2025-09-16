using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using StreamArtist.Domain;
using StreamArtist.Services;
using Timer = System.Timers.Timer;

namespace StreamArtist.Services
{
    // TODO: Update this to refactor domain code out. gemini just
    // shoved everything in here.
    public class PdgSceneService
    {
        private readonly OBSService obsService;
        private readonly YouTubeChatService youTubeChatService;
        private readonly SettingsService settingsService;
        private Timer sceneTimer;
        private Timer pollChatTimer;
        private List<string> scenes;
        private string defaultScene;
        private int currentSceneIndex = 0;
        private bool sceneOverride = false;
        private const int SCENE_DURATION = 30000; // 30 seconds
        private bool joelSaysHiUsed = false;
        private string videoIdOverride = "";
        // private string currentVideoId = "";


        public PdgSceneService(OBSService obsService, YouTubeChatService youTubeChatService, SettingsService settingsService, string testVideoId)
        {
#if DEBUG
            videoIdOverride = testVideoId; //"jnkZFwIps2Y";
#endif
            this.CurrentVideoId = "";
            this.obsService = obsService;
            this.youTubeChatService = youTubeChatService;
            this.settingsService = settingsService;
            
            sceneTimer = new Timer(SCENE_DURATION);
            sceneTimer.Elapsed += OnSceneTimerElapsed;
            sceneTimer.AutoReset = true;

            pollChatTimer = new Timer(1);
            pollChatTimer.Elapsed += PollChatTimer_Elapsed;
            // ********************
            // Enable if you want to bench stuff or get polled chats.
            pollChatTimer.Enabled = false;

            LoadScenesFromSettings();

            // Subscribe to chat message event and process immediately
            this.youTubeChatService.OnChatMessageReceived += ProcessChatMessage;
        }

        private async void PollChatTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            
            // Uncomment if you also want to get polled chats.

            //var msgs = await youTubeChatService.GetNewChatMessages(videoIdOverride);
            //pollChatTimer.Interval = youTubeChatService.PollingIntervalMillis;
            //if (msgs != null)
            //{
            //    foreach (var chat in msgs)
            //    {
            //        LoggingService.Instance.Log($"<{chat.PublishedAt}> Polled Chat ({chat.Id}) => " + chat.AuthorName + ": ||||||||" + chat.Message);
            //    }
            //}
            //var testMessage = Guid.NewGuid().ToString();
            //LoggingService.Instance.Log($"Test message {testMessage}");
            //youTubeChatService.SendMessage(youTubeChatService.LiveChatId, testMessage);

        }

        public string CurrentVideoId { get; set; }


        private void LoadScenesFromSettings()
        {
            var settings = settingsService.GetSettings();
            var scenesSetting = settings["obs-scenes"];
            if (!string.IsNullOrEmpty(scenesSetting))
            {
                scenes = scenesSetting.Split(',').Select(s => s.Trim()).ToList();
                if (scenes.Count > 0)
                {
                    defaultScene = scenes[0];
                    obsService.Connect();
                }
                else
                {
                    LoggingService.Instance.Log("No scenes found in settings.");
                }
            }
            else
            {
                LoggingService.Instance.Log("obs-scenes setting is empty or not found.");
            }
        }

        public async Task Update()
        {
            if (videoIdOverride == "" && (scenes == null || scenes.Count <= 1)) // Need at least two scenes (default + one other)
            {
                LoggingService.Instance.Log("Not enough scenes configured. Check your settings.");
                return;
            }

            if (videoIdOverride == "" && obsService == null) {
                LoggingService.Instance.Log("ObsService is null");
                return;
            }

            var localMessages = await youTubeChatService.GetNewLocalChatMessages();

            var channelId = await youTubeChatService.GetConnectedChannelId();
            if (localMessages.Count == 0 && string.IsNullOrEmpty(channelId))
            {
                LoggingService.Instance.Log("No channels found.");
                return;
            }

            var streams = await youTubeChatService.GetLiveStreams(channelId);
            if (localMessages.Count == 0 && (streams == null || streams.Count == 0) && videoIdOverride=="")
            {
                LoggingService.Instance.Log("No active live streams found.");
                return;
            } else
            {
                //LoggingService.Instance.Log("Looking at video " + streams[0].VideoId);
            }

            var videoId = (string.IsNullOrEmpty(videoIdOverride) ? streams[0].VideoId : videoIdOverride);

            if (string.IsNullOrEmpty(this.CurrentVideoId))
            {
                CurrentVideoId = videoId;
            } else if (this.CurrentVideoId != videoId)
            {
                youTubeChatService.StopChatListener();
            }

            // Idempontent
            youTubeChatService.StartChatListener(videoId);
        }

        private void ProcessChatMessage(ChatMessage chat)
        {
            var isPDG = chat.IsSuperChat || chat.IsSuperSticker || chat.IsChannelMembership;
            LoggingService.Instance.Log($"<{chat.PublishedAt}> Chat ({chat.Id}) => ${(isPDG ? "(PDG)" : "")} " + chat.AuthorName + ": ||||||||" + chat.Message);

            if (chat.Message == "joelsayshi" && !joelSaysHiUsed)
            {
                joelSaysHiUsed = true;
                if (!sceneOverride && (isPDG || chat.Message == "joelsayshi"))
                {
                    sceneOverride = true;
                    sceneTimer.Stop();
                    SwitchToNextScene();
                    sceneTimer.Start();
                }
            }
            else if (isPDG)
            {
                if (!sceneOverride)
                {
                    sceneOverride = true;
                    sceneTimer.Stop();
                    SwitchToNextScene();
                    sceneTimer.Start();
                }
            }
        }

        private void SwitchToNextScene()
        {
            currentSceneIndex = (currentSceneIndex + 1) % (scenes.Count - 1);
            if (currentSceneIndex == 0) { currentSceneIndex = 1; }
            var nextScene = scenes[currentSceneIndex];
            LoggingService.Instance.Log($"Switching to scene: {nextScene}");
            obsService.SwitchScene(nextScene);
        }

        private void OnSceneTimerElapsed(object sender, ElapsedEventArgs e)
        {
            sceneOverride = false;
            obsService.SwitchScene(defaultScene);
        }
    }
    }

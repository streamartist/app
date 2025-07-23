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
        private List<string> scenes;
        private string defaultScene;
        private int currentSceneIndex = 0;
        private bool sceneOverride = false;
        private const int SCENE_DURATION = 30000; // 30 seconds
        private bool joelSaysHiUsed = false;
        private string currentVideoId = "";


        public PdgSceneService(OBSService obsService, YouTubeChatService youTubeChatService, SettingsService settingsService)
        {
            this.obsService = obsService;
            this.youTubeChatService = youTubeChatService;
            this.settingsService = settingsService;
            sceneTimer = new Timer(SCENE_DURATION);
            sceneTimer.Elapsed += OnSceneTimerElapsed;
            sceneTimer.AutoReset = false;

            LoadScenesFromSettings();

            // Subscribe to chat message event and process immediately
            this.youTubeChatService.OnChatMessageReceived += ProcessChatMessage;
        }

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
            // If you want to follow another stream.
            string videoIdOverride = "";

                
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

            if (string.IsNullOrEmpty(this.currentVideoId))
            {
                currentVideoId = videoId;
            } else if (this.currentVideoId != videoId)
            {
                youTubeChatService.StopChatListener();
            }

                // Idempontent
                youTubeChatService.StartChatListener(videoId);
        }

        private void ProcessChatMessage(ChatMessage chat)
        {
            if (chat.Message == "joelsayshi" && !joelSaysHiUsed)
            {
                joelSaysHiUsed = true;
                LoggingService.Instance.Log("Got chats");
                LoggingService.Instance.Log("Chat => " + chat.AuthorName + ": " + chat.Message);
                if (!sceneOverride && (chat.IsSuperChat || chat.IsSuperSticker || chat.IsChannelMembership || chat.Message == "joelsayshi"))
                {
                    sceneOverride = true;
                    sceneTimer.Stop();
                    SwitchToNextScene();
                    sceneTimer.Start();
                }
            }
            else if (chat.IsSuperChat || chat.IsSuperSticker || chat.IsChannelMembership)
            {
                LoggingService.Instance.Log("Got chats");
                LoggingService.Instance.Log("Chat => " + chat.AuthorName + ": " + chat.Message);
                if (!sceneOverride)
                {
                    sceneOverride = true;
                    sceneTimer.Stop();
                    SwitchToNextScene();
                    sceneTimer.Start();
                }
            }
            else
            {
                LoggingService.Instance.Log("Chat => " + chat.AuthorName + ": " + chat.Message);
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

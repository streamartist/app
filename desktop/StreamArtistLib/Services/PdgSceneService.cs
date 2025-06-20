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
using StreamArtistLib.Services;

namespace StreamArtistLib.Services
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
        private const int SCENE_DURATION = 10000; // 10 seconds

        private bool debuggerOn = false;

        public PdgSceneService(OBSService obsService, YouTubeChatService youTubeChatService, SettingsService settingsService)
        {
            this.obsService = obsService;
            this.youTubeChatService = youTubeChatService;
            this.settingsService = settingsService;
            sceneTimer = new Timer(SCENE_DURATION);
            sceneTimer.Elapsed += OnSceneTimerElapsed;
            sceneTimer.AutoReset = false;

            LoadScenesFromSettings();
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
                    Debug.WriteLine("No scenes found in settings.");
                }
            }
            else
            {
                Debug.WriteLine("obs-scenes setting is empty or not found.");
            }
        }

        public async Task Update()
        {
            // TODO: Don't forget to remove *******************************************************************
            //if (debuggerOn) return;

            debuggerOn = true;
            if (scenes == null || scenes.Count <= 1) // Need at least two scenes (default + one other)
            {
                Debug.WriteLine("Not enough scenes configured. Check your settings.");
                return;
            }

            if (obsService == null) {
                Debug.WriteLine("ObsService is null");
                return;
            }

            var channelId = await youTubeChatService.GetConnectedChannelId();
            if (string.IsNullOrEmpty(channelId))
            {
                Debug.WriteLine("Unable to get connected channel ID.");
                return;
            }

            var streams = await youTubeChatService.GetLiveStreams(channelId);
            if (streams == null || streams.Count == 0)
            {
                Debug.WriteLine("No active live streams found.");
                return;
            } else
            {
                Debug.WriteLine("Looking at video " + streams[0].VideoId);
            }

            var chats = await youTubeChatService.GetNewChatMessages(streams[0].VideoId);
            if (chats.Count > 0)
            {
                Debug.WriteLine("Got chats");
            } else
            {
                Debug.WriteLine("No chats");
            }
            //if (true && !sceneOverride)
            //if (!sceneOverride && chats != null && chats.Any(chat => chat.IsSuperChat))
            if (!sceneOverride && chats != null && chats.Any(chat => true))
            {
                sceneOverride = true;
                sceneTimer.Stop();
                SwitchToNextScene();
                sceneTimer.Start();
            }
        }

        private void SwitchToNextScene()
        {
            currentSceneIndex = (currentSceneIndex + 1) % (scenes.Count - 1);
            if (currentSceneIndex == 0) { currentSceneIndex = 1; }
            var nextScene = scenes[currentSceneIndex];
            Debug.WriteLine($"Switching to scene: {nextScene}");
            obsService.SwitchScene(nextScene);
        }

        private void OnSceneTimerElapsed(object sender, ElapsedEventArgs e)
        {
            sceneOverride = false;
            obsService.SwitchScene(defaultScene);
        }
    }
    }

using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using StreamArtist.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StreamArtist.Repositories;
using System.IO;
using Newtonsoft.Json;

namespace StreamArtist.Services
{
    public class YouTubeChatService
    {
        private readonly SettingsService _settingsService;
        private string _liveChatId;
        private string _lastPageToken;
        private const string STATE_FILE = "youtube_chat_state.json";

        public YouTubeChatService()
        {
            _settingsService = new SettingsService();
        }

        private async Task<YouTubeService> InitializeYouTubeService()
        {
            var googleOAuthService = new GoogleOAuthService();
            string token = await googleOAuthService.GetAccessToken();
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = GoogleCredential.FromAccessToken(token),
                ApplicationName = "StreamArtist"
            });
            return youtubeService;
        }

        public async Task<string> GetConnectedChannelName()
        {
            var googleOAuthService = new GoogleOAuthService();
            string token = await googleOAuthService.GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return string.Empty;
            }

            try
            {
                var youtubeService = await InitializeYouTubeService();
                var channelsListRequest = youtubeService.Channels.List("snippet");
                channelsListRequest.Mine = true;

                var channelsListResponse = await channelsListRequest.ExecuteAsync();

                if (channelsListResponse.Items.Count > 0)
                {
                    return channelsListResponse.Items[0].Snippet.Title;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                // TODO: error handling.
                //Console.WriteLine($"Error fetching channel name: {ex.Message}");
                LoggingService.Instance.Log($"Error fetching channel name: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<string> GetConnectedChannelId()
        {
            try
            {
                var youtubeService = await InitializeYouTubeService();
                var channelsListRequest = youtubeService.Channels.List("id");
                channelsListRequest.Mine = true;

                var channelsListResponse = await channelsListRequest.ExecuteAsync();

                if (channelsListResponse.Items.Count > 0)
                {
                    return channelsListResponse.Items[0].Id;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching channel ID: {ex.Message}");
                return string.Empty;
            }
        }

        

        public async Task<List<ChatMessage>> GetNewChatMessages(string videoId)
        {
            // TODO: what is this for?
            if (string.IsNullOrEmpty(_liveChatId))
            {
                await SetLiveChatId(videoId);
            }

            //LoadState();

            var youtubeService = await InitializeYouTubeService();
            var chatMessages = new List<ChatMessage>();
            var request = youtubeService.LiveChatMessages.List(_liveChatId, "snippet,authorDetails");
            request.PageToken = _lastPageToken;

            try
            {
                var response = await request.ExecuteAsync();
                CurrencyConverter currencyConverter = new CurrencyConverter();
                foreach (var message in response.Items)
                {
                    //var json = JsonConvert.SerializeObject(message);
                    //if (json.Contains("newSponsorEvent"))
                    //{
                    //    LoggingService.Instance.Log(json);
                    //}
                    var obj = new ChatMessage
                    {
                        AuthorName = message.AuthorDetails.DisplayName,
                        Message = message.Snippet.DisplayMessage,
                        // Add memberMilestoneChatEvent 
                        IsSuperChat = message?.Snippet?.Type?.Contains("superChatEvent") == true,
                        IsChannelMembership = (message?.Snippet?.Type?.Contains("newSponsorEvent") == true || message?.Snippet?.Type?.Contains("membershipGiftingEvent") == true),
                        IsSuperSticker = message?.Snippet?.Type == "superStickerEvent",
                        Amount = (double)(message.Snippet.SuperChatDetails != null ? message.Snippet.SuperChatDetails?.AmountMicros / 1000000 : 0),
                        DisplayAmount = message.Snippet.SuperChatDetails?.AmountDisplayString,
                        USDAmount = (message.Snippet.SuperChatDetails != null ? currencyConverter.GetUSD(message.Snippet.SuperChatDetails.Currency, (double)message.Snippet.SuperChatDetails?.AmountMicros / 1000000) : 0)
                    };
                    chatMessages.Add(obj);
                }

                _lastPageToken = response.NextPageToken;
                SaveState();
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Log(ex.Message);
                Console.WriteLine($"Error fetching chat messages: {ex.Message}");
            }

            // Add local chat messages
            chatMessages.AddRange(LocalChatRepository.Instance.GetAndRemoveAllChats());

            return chatMessages;
        }

        public async Task<List<ChatMessage>> GetNewLocalChatMessages() {
            return LocalChatRepository.Instance.GetAndRemoveAllChats();
        }

        private void LoadState()
        {
            if (File.Exists(STATE_FILE))
            {
                var json = File.ReadAllText(STATE_FILE);
                var state = JsonConvert.DeserializeObject<ChatState>(json);
                _lastPageToken = state.LastPageToken;
                _liveChatId = state.LiveChatId;
            }
        }

        private void SaveState()
        {
            var state = new ChatState
            {
                LastPageToken = _lastPageToken,
                LiveChatId = _liveChatId
            };
            var json = JsonConvert.SerializeObject(state);
            File.WriteAllText(STATE_FILE, json);
        }

        private async Task SetLiveChatId(string videoId)
        {
            var youtubeService = await InitializeYouTubeService();
            var videoRequest = youtubeService.Videos.List("liveStreamingDetails");
            videoRequest.Id = videoId;

            try
            {
                var videoResponse = await videoRequest.ExecuteAsync();
                _liveChatId = videoResponse.Items[0].LiveStreamingDetails.ActiveLiveChatId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting live chat ID: {ex.Message}");
            }
        }

        public async Task<List<LiveStreamInfo>> GetLiveStreams(string channelId)
        {
            var liveStreams = new List<LiveStreamInfo>();
            var youtubeService = await InitializeYouTubeService();
            var liveBroadcastRequest = youtubeService.LiveBroadcasts.List("snippet,contentDetails");
            liveBroadcastRequest.BroadcastStatus = LiveBroadcastsResource.ListRequest.BroadcastStatusEnum.Active;
            liveBroadcastRequest.BroadcastType = LiveBroadcastsResource.ListRequest.BroadcastTypeEnum.All;

            try
            {
                var liveBroadcastResponse = await liveBroadcastRequest.ExecuteAsync();
                foreach (var item in liveBroadcastResponse.Items)
                {
                    if (item.Snippet.ChannelId == channelId)
                    {
                        liveStreams.Add(new LiveStreamInfo
                        {
                            VideoId = item.Id,
                            Title = item.Snippet.Title,
                            Description = item.Snippet.Description,
                            ThumbnailUrl = item.Snippet.Thumbnails.Default__.Url
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching live streams: {ex.Message}");
            }

            return liveStreams;
        }

        public void AddLocalChatMessage(string authorName, string message, bool isSuperChat = false, double amount = 0, bool isChannelMembership = false, bool isSuperSticker = false)
        {
            var ChatMessage = new ChatMessage
            {
                AuthorName = authorName,
                Message = message,
                IsSuperChat = isSuperChat,
                IsChannelMembership = isChannelMembership,
                IsSuperSticker = isSuperSticker,
                Amount = amount,
                USDAmount = amount,
                DisplayAmount = "$" + amount.ToString()
            };
            Console.Write("Adding chat message from " + authorName);
            LocalChatRepository.Instance.AddChat(ChatMessage);
        }
    }

    

    public class LiveStreamInfo
    {
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
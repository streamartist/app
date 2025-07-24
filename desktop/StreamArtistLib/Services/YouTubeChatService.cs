using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Grpc.Core;
using Newtonsoft.Json;
using StreamArtist.Domain;
using StreamArtist.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using static Youtube.Api.V3.V3DataLiveChatMessageService;
using Grpc.Net.Client;
using System.Collections.Concurrent;
using Youtube.Api.V3;
using System.Diagnostics;

namespace StreamArtist.Services
{
    public class YouTubeChatService
    {
        private readonly SettingsService _settingsService;
        private string _liveChatId;
        private string _lastPageToken;
        private const string STATE_FILE = "youtube_chat_state.json";

        // Add event for new chat messages
        public event Action<ChatMessage> OnChatMessageReceived;
        private Thread ChatListenerThread;
        private CancellationTokenSource ChatListenerCts;
        private string CurrentVideoId;
        private readonly object ChatListenerLock = new object();

        public YouTubeChatService()
        {
            _settingsService = new SettingsService();
            // Set default value, as per the documentation.
            PollingIntervalMillis = 1000;
        }

        public long PollingIntervalMillis { get; set; }

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
            LoggingService.Instance.Log("Calling chat per timer.");
            var request = youtubeService.LiveChatMessages.List(_liveChatId, "snippet,authorDetails");
            request.PageToken = _lastPageToken;

            try
            {
                var response = await request.ExecuteAsync();
                CurrencyConverter currencyConverter = new CurrencyConverter();

                this.PollingIntervalMillis = (response.PollingIntervalMillis.HasValue ? response.PollingIntervalMillis.Value : 5000);

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

        // Start the background chat listener thread
        public void StartChatListener(string videoId)
        {
            lock (ChatListenerLock)
            {
                if (ChatListenerThread != null && ChatListenerThread.IsAlive)
                {
                    // Already running
                    return;
                }
                ChatListenerCts = new CancellationTokenSource();
                CurrentVideoId = videoId;
                ChatListenerThread = new Thread(() => ChatListenerLoop(videoId, ChatListenerCts.Token))
                {
                    IsBackground = true
                };
                ChatListenerThread.Start();
            }
        }

        // Stop the background chat listener thread
        public void StopChatListener()
        {
            lock (ChatListenerLock)
            {
                if (ChatListenerCts != null)
                {
                    ChatListenerCts.Cancel();
                }
                if (ChatListenerThread != null && ChatListenerThread.IsAlive)
                {
                    ChatListenerThread.Join(2000); // Wait up to 2s
                }
                ChatListenerThread = null;
                ChatListenerCts = null;
            }
        }

        // The permanent background loop for receiving chat messages via gRPC
        private void ChatListenerLoop(string videoId, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    RunGrpcChatStream(videoId, cancellationToken).GetAwaiter().GetResult();
                }
                catch (OperationCanceledException ex)
                {
                    // Graceful exit
                    LoggingService.Instance.Log($"Error in chat listener loop ${ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    LoggingService.Instance.Log($"ChatListenerLoop error: {ex.Message}");
                    Debug.WriteLine($"ChatListenerLoop error: {ex}");
                }
                // Wait before reconnecting
                if (!cancellationToken.IsCancellationRequested)
                {
                    Thread.Sleep(3000);
                }
            }
        }

        // The gRPC chat stream logic, refactored for background use
        private async Task RunGrpcChatStream(string videoId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_liveChatId))
            {
                await SetLiveChatId(videoId);
            }

            LoggingService.Instance.Log("Starting gRPC chat stream loop.");
            string address = "https://youtube.googleapis.com:443";
            var googleOAuthService = new GoogleOAuthService();
            var client = await googleOAuthService.CreateGrpcClient<V3DataLiveChatMessageServiceClient>(address);
            string token = await googleOAuthService.GetAccessToken();

            var callOptions = new CallOptions().WithHeaders(new Metadata { { "Authorization", $"Bearer {token}" } });
            var request = new LiveChatMessageListRequest
            {
                LiveChatId = _liveChatId,
                Part = { "snippet", "authorDetails" },
                MaxResults = 20,
            };

            if (!string.IsNullOrEmpty(_lastPageToken))
            {
                request.PageToken = _lastPageToken;
            }

            using (var call = client.StreamList(request, callOptions))
            {
                LoggingService.Instance.Log($"Connected to chat at ${DateTime.UtcNow}");
                var responseStream = call.ResponseStream;
                CurrencyConverter currencyConverter = new CurrencyConverter();
                while (await responseStream.MoveNext(cancellationToken))
                {
                    var response = responseStream.Current;
                    if (response != null && response.Items != null)
                    {
                        foreach (var message in response.Items)
                        {
                            var chatMessage = new ChatMessage
                            {
                                AuthorName = message.AuthorDetails.DisplayName,
                                Message = message.Snippet.DisplayMessage,
                                IsSuperChat = message?.Snippet?.Type == Youtube.Api.V3.LiveChatMessageSnippet.Types.TypeWrapper.Types.Type.SuperChatEvent,
                                IsChannelMembership = message?.Snippet?.Type == Youtube.Api.V3.LiveChatMessageSnippet.Types.TypeWrapper.Types.Type.NewSponsorEvent || message?.Snippet?.Type == Youtube.Api.V3.LiveChatMessageSnippet.Types.TypeWrapper.Types.Type.MembershipGiftingEvent,
                                IsSuperSticker = message?.Snippet?.Type == Youtube.Api.V3.LiveChatMessageSnippet.Types.TypeWrapper.Types.Type.SuperStickerEvent,
                                Amount = (double)(message.Snippet.SuperChatDetails != null ? message.Snippet.SuperChatDetails?.AmountMicros / 1000000 : 0),
                                DisplayAmount = message.Snippet.SuperChatDetails?.AmountDisplayString,
                                USDAmount = (message.Snippet.SuperChatDetails != null ? currencyConverter.GetUSD(message.Snippet.SuperChatDetails.Currency, (double)message.Snippet.SuperChatDetails?.AmountMicros / 1000000) : 0)
                            };
                            // Fire the event for each received message
                            OnChatMessageReceived?.Invoke(chatMessage);
                        }
                    }
                    var localMessages = LocalChatRepository.Instance.GetAndRemoveAllChats();
                    foreach(var chat in localMessages)
                    {
                        OnChatMessageReceived?.Invoke(chat);
                    }

                    _lastPageToken = response.NextPageToken;
                    SaveState();
                }
            }
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
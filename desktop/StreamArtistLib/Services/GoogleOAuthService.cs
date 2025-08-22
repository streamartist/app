//using Microsoft.Maui.ApplicationModel;
using StreamArtist.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using Grpc.Core;
using Grpc.Net.Client;
using System.Threading.Tasks;
using System.Web;
using WatsonWebserver;

namespace StreamArtist.Services
{

    public class GoogleOAuthService
    {
        private string ClientId = string.Empty;
        // This is a server auth function that keeps the client secret more secret.
        // You don't need it if you're using a local secret.
        private string AuthUrl = string.Empty;
        private const int Port = 42983;
        private readonly string RedirectUri = $"http://localhost:{Port}/";
        private const string Scope = "https://www.googleapis.com/auth/youtube";

        private WatsonWebserver.Server? _server;
        private TaskCompletionSource<string>? _authorizationCodeTask;

        // Add this event declaration
        public event EventHandler? AuthComplete;

        private readonly SettingsService _settingsService;
        private readonly HttpClient _httpClient;

        public GoogleOAuthService()
        {
            _settingsService = new SettingsService();
            _httpClient = new HttpClient();
            
            ClientId = _settingsService.GetSetting("google-auth-client-id");
            AuthUrl = _settingsService.GetSetting("auth-url");
        }

        public async Task AuthenticateAsync()
        {
            _authorizationCodeTask = new TaskCompletionSource<string>();
            StartLocalServer();

            string authorizationUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={ClientId}&redirect_uri={RedirectUri}&response_type=code&scope={Scope}&access_type=offline";
            //await Browser.OpenAsync(authorizationUrl, BrowserLaunchMode.SystemPreferred);
            Process myProcess = new Process();

            // true is the default, but it is important not to set it to false
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.FileName = authorizationUrl;
            myProcess.Start();
        }

        private void StartLocalServer()
        {
            StopLocalServer();

            _server = new Server("127.0.0.1", 42983, false, HandleAuthCallback);
            _server.Start();
        }

        private void StopLocalServer()
        {
            _server?.Stop();
            _server = null;
        }

        private async Task HandleAuthCallback(HttpContext context)
        {
            var scopeExists = context.Request.QuerystringExists("code", false);
            if (!scopeExists)
            {
                context.Response.StatusCode = 500;
                return;
            }
            string code = context.Request.QuerystringEntries["code"];
            if (!string.IsNullOrEmpty(code))
            {
                _authorizationCodeTask.SetResult(code);

                // Save the OAuth2 code using SettingsService
                var settingsService = new SettingsService();
                settingsService.SetGoogleOAuthToken(code);

                // Trigger the AuthComplete event
                OnAuthComplete();
            }

            string responseString = "<html><body><h1>Authentication successful!</h1><p>You can close this window now.</p></body></html>";
            await context.Response.Send(responseString);
        }

        protected virtual async void OnAuthComplete()
        {
            try
            {
                await ExchangeCodeForTokenAsync();
            }
            catch (Exception ex)
            {
                // TODO: Deal with this better.
                Console.WriteLine($"Error exchanging authorization code for token: {ex.Message}");
                LoggingService.Instance.Log($"Error exchanging authorization code for token: {ex.Message}");
            }
            finally
            {
                AuthComplete?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task<string> GetAccessToken()
        {
            if (!_settingsService.HasGoogleAccessTokenExpired())
            {
                return _settingsService.GetGoogleAccessToken();
            }

            // If the access token is expired, we must refresh it.
            // If we can't refresh, we'll return null, and the user will need to re-authenticate.
            return await RefreshAccessTokenAsync();
        }

        private async Task<string> RefreshAccessTokenAsync()
        {
            string RefreshToken = _settingsService.GetGoogleRefreshToken();

            if (string.IsNullOrEmpty(RefreshToken))
            {
                return null;
            }

            var Content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("client_id", ClientId),
            new KeyValuePair<string, string>("refresh_token", RefreshToken),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("client_secret", Environment.GetEnvironmentVariable("STREAMARTIST_GOOGLE_CLIENT_SECRET"))
        });

            return await SendTokenRequestAsync(Content);
        }

        private async Task<string> ExchangeCodeForTokenAsync()
        {
            string Code = _settingsService.GetGoogleOAuthToken();
            var Content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("code", Code),
            new KeyValuePair<string, string>("client_id", ClientId),
            new KeyValuePair<string, string>("redirect_uri", RedirectUri),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("client_secret", Environment.GetEnvironmentVariable("STREAMARTIST_GOOGLE_CLIENT_SECRET"))
        });

            return await SendTokenRequestAsync(Content);
        }

        private async Task<string> SendTokenRequestAsync(FormUrlEncodedContent Content)
        {
            var Secret = Environment.GetEnvironmentVariable("STREAMARTIST_GOOGLE_CLIENT_SECRET");
            var ResponseString = "";

            if (Secret != null && Secret != "")
            {
                ResponseString = await LocalhostOauth(Content, Secret);
            }
            else
            {
                var Response = await _httpClient.PostAsync(this.AuthUrl, Content);
                ResponseString = await Response.Content.ReadAsStringAsync();
            }

            var jsonDocument = JsonDocument.Parse(ResponseString);

            if (jsonDocument.RootElement.TryGetProperty("error", out var Error))
            {
                LoggingService.Instance.Log($"Couldn't get access token: {Error.GetString()}");
                // TODO: Clean up user error handling.
                //throw new GoogleAccessTokenException(Error.GetString());
            }

            var ExpiryInSeconds = jsonDocument.RootElement.GetProperty("expires_in").GetInt32();
            var Expiry = DateTime.Now.AddSeconds(ExpiryInSeconds);
            var AccessToken = jsonDocument.RootElement.GetProperty("access_token").GetString();

            _settingsService.SetGoogleAccessTokenExpiry(Expiry);
            _settingsService.SetGoogleAccessToken(AccessToken);

            if (jsonDocument.RootElement.TryGetProperty("refresh_token", out var RefreshTokenElement))
            {
                var RefreshToken = RefreshTokenElement.GetString();
                _settingsService.SetGoogleRefreshToken(RefreshToken);
            }

            return AccessToken;
        }

        public async Task<string> LocalhostOauth(FormUrlEncodedContent Content, string Secret)
        {
            using (var Client = new HttpClient())
            {
                var Response = await Client.PostAsync("https://oauth2.googleapis.com/token", Content);
                var ResponseString = await Response.Content.ReadAsStringAsync();
                return ResponseString;
            }
        }

        public async Task<T> CreateGrpcClient<T>(string address) where T : ClientBase
        {
            var token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Failed to get Google access token.");
            }

            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                metadata.Add("Authorization", $"Bearer {token}");
                return Task.CompletedTask;
            });

            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
            });

            // The gRPC client types generated by Grpc.Tools have a constructor that takes a GrpcChannel.
            var client = (T)Activator.CreateInstance(typeof(T), channel);
            return client;
        }
    }
}
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Generic;
using System.Net.Http;
using System;
using WatsonWebserver;
using Microsoft.Maui.ApplicationModel;
using StreamArtist.Domain;

namespace StreamArtist.Services{

public class GoogleOAuthService
{
    private const string ClientId = "761671456404-3uguov085qn482k6q9ol9ngf8fiiub16.apps.googleusercontent.com";
    private const int Port = 42983;
    private readonly string RedirectUri = $"http://localhost:{Port}/";
    private const string Scope = "https://www.googleapis.com/auth/youtube";

    private WatsonWebserver.Server? _server;
    private TaskCompletionSource<string>? _authorizationCodeTask;

    // Add this event declaration
    public event EventHandler? AuthComplete;

    public async Task AuthenticateAsync()
    {
        _authorizationCodeTask = new TaskCompletionSource<string>();
        StartLocalServer();

        string authorizationUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={ClientId}&redirect_uri={RedirectUri}&response_type=code&scope={Scope}";
        await Browser.OpenAsync(authorizationUrl, BrowserLaunchMode.SystemPreferred);

        //string authorizationCode = await _authorizationCodeTask.Task;
        // StopLocalServer();

        //return await ExchangeCodeForTokenAsync(authorizationCode);
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
        await ExchangeCodeForTokenAsync();
        AuthComplete?.Invoke(this, EventArgs.Empty);
    }

    public async Task<string> GetAccessToken() {
        var SettingsService = new SettingsService();
        var AccessToken = SettingsService.GetGoogleAccessToken();
        var Expiry = SettingsService.GetGoogleAccessTokenExpiry();

        if (!SettingsService.HasGoogleAccessTokenExpired()) {
            return AccessToken; 
        }

        return await ExchangeCodeForTokenAsync(); 
    }

    private async Task<string> ExchangeCodeForTokenAsync()
    {
        var settingsService = new SettingsService();
        string code = settingsService.GetGoogleOAuthToken();
        using (var client = new HttpClient())
        {
            var content = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("redirect_uri", RedirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });

            var response = await client.PostAsync("https://us-central1-cloud-stream-431915.cloudfunctions.net/artist-connect", content);
            var responseString = await response.Content.ReadAsStringAsync();

            var jsonDocument = JsonDocument.Parse(responseString);
            
            // Check for error field in the response
            if (jsonDocument.RootElement.TryGetProperty("error", out var error))
            {
                Console.WriteLine($"Couldn't get access token ${error.GetString()}");
                throw new GoogleAccessTokenException(error.GetString());
            }

            var expiryInSeconds = jsonDocument.RootElement.GetProperty("expires_in").GetInt32();
            var expiry = DateTime.Now.AddSeconds(expiryInSeconds);
            var accessToken = jsonDocument.RootElement.GetProperty("access_token").GetString();
            
            settingsService.SetGoogleAccessTokenExpiry(expiry);
            settingsService.SetGoogleAccessToken(accessToken);
            return accessToken;
        }
    }
}}
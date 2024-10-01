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

        string authorizationUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={ClientId}&redirect_uri={RedirectUri}&response_type=code&scope={Scope}&access_type=offline";
        await Browser.OpenAsync(authorizationUrl, BrowserLaunchMode.SystemPreferred);
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

    public async Task<string> GetAccessToken()
    {
        if (!_settingsService.HasGoogleAccessTokenExpired())
        {
            return _settingsService.GetGoogleAccessToken();
        }

        var refreshedToken = await RefreshAccessTokenAsync();
        return refreshedToken ?? await ExchangeCodeForTokenAsync();
    }

    private readonly SettingsService _settingsService;
    private readonly HttpClient _httpClient;

    public GoogleOAuthService()
    {
        _settingsService = new SettingsService();
        _httpClient = new HttpClient();
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
            new KeyValuePair<string, string>("grant_type", "refresh_token")
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
            new KeyValuePair<string, string>("grant_type", "authorization_code")
        });

        return await SendTokenRequestAsync(Content);
    }

    private async Task<string> SendTokenRequestAsync(FormUrlEncodedContent Content)
    {
        var Response = await _httpClient.PostAsync("https://us-central1-cloud-stream-431915.cloudfunctions.net/artist-connect", Content);
        var ResponseString = await Response.Content.ReadAsStringAsync();

        var jsonDocument = JsonDocument.Parse(ResponseString);

        if (jsonDocument.RootElement.TryGetProperty("error", out var Error))
        {
            Console.WriteLine($"Couldn't get access token: {Error.GetString()}");
            throw new GoogleAccessTokenException(Error.GetString());
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
}}
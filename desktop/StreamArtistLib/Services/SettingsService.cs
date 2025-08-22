using System;
using System.Text.Json;
using System.Collections.Generic;

namespace StreamArtist.Services
{
    public class SettingsService
    {
        // TODO: rename cloudstream-streamkey to streamartist-streamkey
        private readonly string[] _fieldIds = ["server-address", "youtube-streamkey", "twitch-streamkey", "cloudstream-streamkey", "tos", "shorts-streamkey", "shorts-filter","obs-port","obs-password","obs-scenes","test-video-id","google-auth-client-id", "auth-url"];
        private readonly Repositories.Settings _settings = new Repositories.Settings();

        public Dictionary<string, string> GetSettings()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (string id in _fieldIds)
            {
                dict[id] = _settings.GetSecureStorageValueSync(id);
                if (dict[id] == null)
                {
                    dict[id] = "";
                }
            }

            // YouTubeChatService chatService = new YouTubeChatService(this);
            // var task = chatService.GetConnectedChannelName();
            // task.Wait();
            // dict["youtube-account"] = task.Result;
            return dict;
        }

        public void SaveSettings(string values)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(values);
            foreach (string id in _fieldIds)
            {
                _settings.SetSecureValue(id, dict[id]);
            }
        }

        public string GetSetting(string id)
        {
            return _settings.GetSecureStorageValueSync(id);
        }


        public void SetGoogleOAuthToken(string token)
        {
            // Implement the logic to save the token
            // For example, using Preferences:
            _settings.SetSecureValue("GoogleOAuthToken", token);
        }

        public string GetGoogleOAuthToken()
        {
            // Retrieve the token from Preferences
            return _settings.GetSecureStorageValueSync("GoogleOAuthToken");
        }

        public void SetGoogleRefreshToken(string token)
        {
            _settings.SetSecureValue("GoogleRefreshToken", token);
        }

        public string GetGoogleRefreshToken()
        {
            // Retrieve the token from Preferences
            return _settings.GetSecureStorageValueSync("GoogleRefreshToken");
        }

        public void SetGoogleAccessTokenExpiry(DateTime Expiry)
        {
            _settings.SetSecureValue("GoogleAccessTokenExpiry", Expiry.ToString());
        }

        public bool HasGoogleAccessTokenExpired() {
            var expiry = GetGoogleAccessTokenExpiry();
            if (expiry == null) {
                return true;
            }

            return DateTime.Now >= expiry;
        }

        public DateTime? GetGoogleAccessTokenExpiry()
        {
            var expiryString = _settings.GetSecureStorageValueSync("GoogleAccessTokenExpiry");
            if (DateTime.TryParse(expiryString, out DateTime expiry))
            {
                return expiry;
            }
            return null;
        }

        public void SetGoogleAccessToken(string AccessToken)
    {
            _settings.SetSecureValue("GoogleAccessToken", AccessToken);
        }

        public string GetGoogleAccessToken()
        {
            return _settings.GetSecureStorageValueSync("GoogleAccessToken");
        }
    }
}
using System.Text.Json;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace StreamArtist.Repositories
{
    public class Settings
    {

        public void SetSecureValue(string key, string value)
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                SecureStorage.Default.SetAsync(key, value);
            }
            else
            {
                Preferences.Set(key, value);
            }
        }

        public string GetSecureStorageValueSync(string key)
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                return Task.Run(async () => await SecureStorage.Default.GetAsync(key)).Result;
            }
            else
            {
                return Preferences.Get(key, "");
            }
        }

        public void SetGoogleOAuthToken(string token)
        {
            // Implement the logic to save the token
            // For example, using Preferences:
            // TODO: Move to secure storage
            Preferences.Set("GoogleOAuthToken", token);
        }

        public string GetGoogleOAuthToken()
        {
            // Retrieve the token from Preferences
            return Preferences.Get("GoogleOAuthToken", string.Empty);
        }
        
    }
}
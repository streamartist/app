using Microsoft.Win32;
using System;
using System.Text.Json;
//using Microsoft.Maui.Devices;
//using Microsoft.Maui.Storage;
//using Windows.Security.Credentials;
using System.Threading.Tasks;
using System.Security.Cryptography;

// This file used to be x-platform using Maui. See history.

namespace StreamArtist.Repositories
{
    public class Settings
    {


        public void SetSecureValue(string key, string value)
        {
            // Encrypt the data
            byte[] encryptedData = ProtectedData.Protect(
                System.Text.Encoding.UTF8.GetBytes(value),
                null,
                DataProtectionScope.CurrentUser);

            // Convert the encrypted data to a string (e.g., Base64)
            string encryptedString = Convert.ToBase64String(encryptedData);

            // Store the encrypted string in the registry
            using (RegistryKey rkey = Registry.CurrentUser.CreateSubKey("StreamArtist"))
            {
                rkey.SetValue(key, encryptedString);
            }
        }

        public string GetSecureStorageValueSync(string key)
        {
            string encryptedString;
            using (RegistryKey rkey = Registry.CurrentUser.OpenSubKey("StreamArtist"))
            {
                if (rkey != null)
                {
                    encryptedString = (string)rkey.GetValue(key);
                }
                else
                {
                    return null; // Or handle the case where the key doesn't exist
                }
            }

            if (encryptedString != null)
            {
                // Convert the string back to byte array
                byte[] encryptedData = Convert.FromBase64String(encryptedString);

                // Decrypt the data
                byte[] decryptedData = ProtectedData.Unprotect(
                    encryptedData,
                    null,
                    DataProtectionScope.CurrentUser);

                // Convert the decrypted data back to a string
                return System.Text.Encoding.UTF8.GetString(decryptedData);
            }
            else
            {
                return null;
            }
        }

        public void SetGoogleOAuthToken(string token)
        {
            // Implement the logic to save the token
            // For example, using Preferences:
            // TODO: Move to secure storage
            //Preferences.Set("GoogleOAuthToken", token);
            SetSecureValue("GoogleOAuthToken", token);
        }

        public string GetGoogleOAuthToken()
        {
            // Retrieve the token from Preferences
            //return Preferences.Get("GoogleOAuthToken", string.Empty);
            return this.GetSecureStorageValueSync("GoogleOAuthToken");
        }
        
    }
}
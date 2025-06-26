
using StreamArtist.Services;
using System;
using System.Windows.Forms; // Required for MethodInvoker

namespace StreamArtist.Controllers
{
    public class SettingsController
    {
        private readonly SettingsService _settingsService = new SettingsService();
        private AppController _appController;


        public SettingsController(AppController appController)
        {
            _appController = appController;
        }

        public async void OnGoogleSignInClicked()
        {
            var googleOAuthService = new GoogleOAuthService();
            googleOAuthService.AuthComplete += OnGoogleAuthComplete;
            try
            {
                googleOAuthService.AuthenticateAsync();
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Log(ex.Message);
                // await DisplayAlert("Error", $"Authentication failed: {ex.Message}", "OK");
            }
        }

        private void OnGoogleAuthComplete(object sender, EventArgs e)
        {
            // The AuthComplete event is likely raised on a background thread by WatsonWebserver.
            // UI operations must be marshaled to the UI thread.
            _appController.MainView.Invoke((MethodInvoker)delegate
            {
                LoadGoogleSignInStatus();
            });
        }

        public async void LoadGoogleSignInStatus()
        {
            try
            {
                YouTubeChatService service = new YouTubeChatService();
                var name = await service.GetConnectedChannelName();
                name = name.Replace("'", "\\'");
                var s = await _appController.Eval($"setField('youtube-account', '{name}')");
            }
            catch (Exception err)
            {
                await _appController.Eval($"setField('youtube-account', 'Error')");
            }
        }
    }
}
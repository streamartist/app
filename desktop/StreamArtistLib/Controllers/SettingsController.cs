
using StreamArtist.Services;
using System;

namespace StreamArtist.Controllers{
public class SettingsController
{
    private readonly SettingsService _settingsService = new SettingsService();
    private AppController _appController;


    public SettingsController(AppController appController) {
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
            // await DisplayAlert("Error", $"Authentication failed: {ex.Message}", "OK");
        }
    }

    private void OnGoogleAuthComplete(object sender, EventArgs e)
    {
        LoadGoogleSignInStatus();
    }

    public async void LoadGoogleSignInStatus() {
        try {
            YouTubeChatService service = new YouTubeChatService();
            var name = await service.GetConnectedChannelName();
            await _appController.Eval($"setField('youtube-account', '{name}')");
        } 
        catch(Exception err) {
            await _appController.Eval($"setField('youtube-account', 'Error')");
        }
    }
}}
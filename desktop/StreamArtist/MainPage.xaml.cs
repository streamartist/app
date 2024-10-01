namespace StreamArtist;

using Microsoft.Maui.Controls;

using System;
using StreamArtist.Services;
using StreamArtist.Controllers;
using System.Timers;

public partial class MainPage : ContentPage
{

    private AppController _appController;


    public MainPage()
    {
        InitializeComponent();        
        _appController = new AppController(MainView);
        _appController.LoadHtml();
        MainView.Navigated += OnNavigated;
        MainView.Navigating += OnNavigating;

        // _networkService = new NetworkService();
        // _settingsService = new SettingsService();
    }

    private async void OnNavigated(object sender, WebNavigatedEventArgs e)
    {
        _appController.Eval("updateStatus('Waiting... Yay');");

    }

    private void OnNavigating(object sender, WebNavigatingEventArgs e)
    {
        _appController.OnWebViewEvent(sender, e);
    }

    

    
}


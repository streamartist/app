// Controls the basic HTML view.


using System;
using System.Diagnostics;
using StreamArtist.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System.Threading.Tasks;

namespace StreamArtist.Controllers{
public class AppController
{
    private WebView MainView;
    public SettingsController SettingsController;
    public WebServerService WebServerService = new WebServerService();

    // Add constructor
    public AppController(WebView webView)
    {
        MainView = webView;
        SettingsController = new SettingsController(this);
    }

    public void OnDocumentLoaded()
    {
        SettingsController.LoadGoogleSignInStatus();
        WebServerService.StartLocalServer();
    }    

    public Task<string> Eval(string code)
    {
        // TODO: is this good?
        code = code.ReplaceLineEndings("");

        if (code.Contains("\\n") || code.Contains("\n"))
        {
            throw new Exception("Can't have newlines in javascript");
        }
        return MainView.Dispatcher.DispatchAsync(() =>
        {
            Debug.WriteLine($"JS => {code}");
            return MainView.EvaluateJavaScriptAsync(code);
        });
    }
}}
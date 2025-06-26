using StreamArtist.Controllers;
using StreamArtist.Services;
using StreamArtist.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Text.Json;

namespace StreamArtist
{
    public partial class Main : Form
    {

        private readonly NetworkService _networkService = new NetworkService();
        private readonly SettingsService _settingsService = new SettingsService();

        private AppController _appController;

        string[] fieldIds = ["server-address", "youtube-streamkey", "twitch-streamkey", "streamartist-streamkey", "tos", "shorts-streamkey", "shorts-filter"];
        string[] statusFieldNames = ["status-text", "control-server-status", "streaming-server-status", "control-server-security"];


        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            InitializeWebView2();
        }
        private async void InitializeWebView2()
        {
            // Ensure the CoreWebView2 is initialized
            await MainView.EnsureCoreWebView2Async(null);

            // Load the embedded HTML file
            //LoadHtml();

            MainView.NavigationStarting += MainView_NavigationStarting;
            MainView.NavigationCompleted += MainView_NavigationCompleted;
            _appController = new AppController(MainView);
            _appController.LoadHtml();


        }

 

        private void MainView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void MainView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            _appController.OnWebViewEvent(sender, e);
            
        }




        
    }
}
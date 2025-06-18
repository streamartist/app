using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ObsWebSocket.Net;

namespace StreamArtistLib.Services
{
    public class OBSService
    {
        private readonly string host;
        private readonly string password;
        private readonly ObsWebSocketClient obs;

        public OBSService(int port, string password)
        {
            host = $"ws://127.0.0.1:{port}";
            password = password;
            ObsWebSocketClientOptions options = new ObsWebSocketClientOptions
            {
                Address = "127.0.0.1",
                Port = port,
                Password = password,
                // What is this?
                UseMsgPack = false,
                AutoReconnect = true,
                AutoReconnectWaitSeconds = 1
            }; ;
            obs = new ObsWebSocketClient(options);
            
        }

        public async void Connect()
        {
            obs.Connect();
        }

        public async void SwitchScene(string sceneName)
        {
            //if (!obs.conn)
            //{
            //    Console.WriteLine("Not connected to OBS. Please connect first.");
            //    return;
            //}

            //try
            //{
            obs.SetCurrentProgramScene("potato");
                //await obs.SetCurrentProgramSceneAsync(sceneName);
                //Console.WriteLine($"Switched OBS scene to: {sceneName}");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Failed to switch scene to {sceneName}: {ex.Message}");
            //}
        }

        /// <summary>
        /// Disconnects from the OBS WebSocket server.
        /// </summary>
        public async void Disconnect()
        {
            //if (obs.IsConnected)
            //{
            //    await obs.DisconnectAsync();
            //    Console.WriteLine("Disconnected from OBS.");
            //}
            //else
            //{
            //    Console.WriteLine("Not connected to OBS.");
            //}
        }
    }
}

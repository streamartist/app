﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
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
            this.password = password;
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
            obs.OnConnectionFailed += Obs_OnConnectionFailed;
            obs.OnConnected += Obs_OnConnected;
            
        }

        private void Obs_OnConnected()
        {
            Debug.WriteLine("Connected to OBS");
        }

        private void Obs_OnConnectionFailed(Exception exception)
        {
            Debug.WriteLine(exception.ToString());
        }

        public void Connect()
        {
            try
            {
                Debug.WriteLine("Connecting to OBS WebSocket...");
                obs.Connect();
                Debug.WriteLine("Successfully connected to OBS WebSocket.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to connect to OBS: {ex.Message}");
            }
        }

        public void SwitchScene(string sceneName)
        {
            Debug.WriteLine($"Attempting to switch OBS scene to: {sceneName}");
            try
            {
                
                obs.SetCurrentProgramScene(sceneName);
                Debug.WriteLine($"Successfully switched OBS scene to: {sceneName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to switch scene to {sceneName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Disconnects from the OBS WebSocket server.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                Debug.WriteLine("Disconnecting from OBS WebSocket...");
                //obs.Disconnect();
                obs.Close();
                Debug.WriteLine("Successfully disconnected from OBS WebSocket.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to disconnect from OBS: {ex.Message}");
            }
        }
    }
}

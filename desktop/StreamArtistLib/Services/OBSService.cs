﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using StreamArtistLib.Services;
using ObsWebSocket.Net;
using StreamArtist.Services;

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
            //LoggingService.Instance.Log("Connected to OBS");
        }

        private void Obs_OnConnectionFailed(Exception exception)
        {
            LoggingService.Instance.Log(exception.ToString());
        }

        public void Connect()
        {
            try
            {
                //LoggingService.Instance.Log("Connecting to OBS WebSocket...");
                obs.Connect();
                //LoggingService.Instance.Log("Successfully connected to OBS WebSocket.");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Log($"Failed to connect to OBS: {ex.Message}");
            }
        }

        public void SwitchScene(string sceneName)
        {
            LoggingService.Instance.Log($"Attempting to switch OBS scene to: {sceneName}");
            try
            {
                
                obs.SetCurrentProgramScene(sceneName);
                LoggingService.Instance.Log($"Successfully switched OBS scene to: {sceneName}");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Log($"Failed to switch scene to {sceneName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Disconnects from the OBS WebSocket server.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                LoggingService.Instance.Log("Disconnecting from OBS WebSocket...");
                //obs.Disconnect();
                obs.Close();
                //LoggingService.Instance.Log("Successfully disconnected from OBS WebSocket.");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Log($"Failed to disconnect from OBS: {ex.Message}");
            }
        }
    }
}

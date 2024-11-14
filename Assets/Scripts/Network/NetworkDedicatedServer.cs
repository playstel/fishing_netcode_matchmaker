using System;
using System.Threading.Tasks;
using Menu;
using Network;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplay;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkServer
{
    public class NetworkDedicatedServer : MonoBehaviour
    {
        public static NetworkDedicatedServer Instance;

        [SerializeField] private ushort maxPlayers = 4;
        [SerializeField] private string serverName = "MyServer";
        [SerializeField] private string gameType = "MyGameType";
        [SerializeField] private string buildId = "8";
        [SerializeField] private string map = "TestMap";

        private IServerQueryHandler _serverQueryHandler;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else Destroy(gameObject);
        }

        private async void Start()
        {
            if (Application.platform == RuntimePlatform.LinuxServer)
            {
                try
                {
                    Application.targetFrameRate = 60;

                    await UnityServices.InitializeAsync();

                    ServerConfig serverConfig = MultiplayService.Instance.ServerConfig;

                    Debug.Log("--- Starting the server with port: " + serverConfig.Port + " | Ip: " + serverConfig.IpAddress);
                    
                    _serverQueryHandler = await MultiplayService.Instance
                        .StartServerQueryHandlerAsync(maxPlayers, serverName, gameType, buildId, map);

                    if (serverConfig.AllocationId != string.Empty)
                    {
                        var result = NetworkUnityServices.Instance.StartDedicatedServer(serverConfig);

                        if (result)
                        {
                            Debug.Log("--- Server has started | Port: " + serverConfig.Port + " | Ip: " + serverConfig.IpAddress);
                            await MultiplayService.Instance.ReadyServerForPlayersAsync();
                        }
                        else
                        {
                            Debug.LogError("--- Failed to start dedicated server | Port: " + serverConfig.Port + " | Ip: " + serverConfig.IpAddress);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("--- Failed to start linux dedicated server: " + e);
                    throw;
                }
            }
        }

        private async void Update()
        {
            if (Application.platform == RuntimePlatform.LinuxServer)
            {
                if (_serverQueryHandler != null)
                {
                    if (NetworkManager.Singleton == null)
                    {
                        Debug.LogError("--- NetworkManager.Singleton is null");
                        _serverQueryHandler = null;
                        return;
                    }
                    
                    if (NetworkManager.Singleton.ConnectedClients == null)
                    {
                        Debug.LogError("--- NetworkManager.Singleton.ConnectedClients is null");
                        _serverQueryHandler = null;
                        return;
                    }
                    
                    _serverQueryHandler.CurrentPlayers = (ushort) NetworkManager.Singleton.ConnectedClients.Count;
                    
                    _serverQueryHandler.UpdateServerCheck();
                    
                    await Task.Delay(200);
                }
            }
        }

        public bool JoinToServer(string ipAddress, string port)
        {
            try
            {
                var result = NetworkUnityServices.Instance.StartClient(ipAddress, port);
                
                Debug.Log("Joined the server: " + result);
                
                if (result)
                {
                    NetworkStatusInfo.Instance.SetInfo($"Joined the server");
                    NetworkSceneLoader.Instance.LoadGameScene(true);
                }
                else
                {
                    NetworkStatusInfo.Instance.SetInfo($"Failed to join the server. Check the IP and PORT addresses");
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Server error: {e.Message}");
                NetworkStatusInfo.Instance.SetInfo($"Server error: {e.Message}");
                return false;
            }
        }
    }
}
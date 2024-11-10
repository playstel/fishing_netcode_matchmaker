using System;
using System.Threading.Tasks;
using Menu;
using Network;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
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
                Application.targetFrameRate = 60;

                await UnityServices.InitializeAsync();

                ServerConfig serverConfig = MultiplayService.Instance.ServerConfig;

                _serverQueryHandler = await MultiplayService.Instance
                    .StartServerQueryHandlerAsync(maxPlayers, "MyServer", "MyGameType", "0", "TestMap");

                if (serverConfig.AllocationId != string.Empty)
                {
                    var result = NetworkUnityServices.Instance.StartDedicatedServer(serverConfig);

                    if (result)
                    {
                        Debug.Log("Server has started");
                        await MultiplayService.Instance.ReadyServerForPlayersAsync();
                    }
                    else
                    {
                        Debug.LogError("Failed to start the server");
                    }
                }
            }
        }

        private async void Update()
        {
            if (Application.platform == RuntimePlatform.LinuxServer)
            {
                if (_serverQueryHandler != null)
                {
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
                    NetworkSceneLoader.Instance.LoadGameSceneAsServerClient();
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
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
    public class NetworkServer : MonoBehaviour
    {
        public static NetworkServer Instance;

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
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", serverConfig.Port, "0.0.0.0");

                    NetworkManager.Singleton.StartServer();

                    await MultiplayService.Instance.ReadyServerForPlayersAsync();
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
                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            
                transport.SetConnectionData(ipAddress, ushort.Parse(port));

                var result = NetworkManager.Singleton.StartClient();
                
                Debug.Log("Joined the server: " + result);
                
                if (result)
                {
                    NetworkStatusInfo.Instance.SetInfo($"Joined the server");
                    MenuRoomLoader.Instance.LoadGameSceneAsServerClient();
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
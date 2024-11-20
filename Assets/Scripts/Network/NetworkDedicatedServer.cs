using System;
using Cysharp.Threading.Tasks;
using Menu;
using Network;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;

#if SERVER
using Unity.Services.Multiplay;
#endif

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

#if SERVER
        private IServerQueryHandler _serverQueryHandler;
#endif

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else Destroy(gameObject);
        }

        private bool _serverHasStarted;

        private async void Start()
        {
            #if SERVER
            
            try
            {
                Application.targetFrameRate = 60;

                await UnityServices.InitializeAsync();
                    
                _serverQueryHandler = await MultiplayService.Instance
                    .StartServerQueryHandlerAsync(maxPlayers, serverName, gameType, buildId, map);

                ServerConfig serverConfig = MultiplayService.Instance.ServerConfig;
                
                await UniTask.WaitUntil(() => serverConfig.AllocationId != string.Empty);

                var result = NetworkUnityServices.Instance.StartDedicatedServer(serverConfig.Port);

                if (result)
                {
                    Debug.Log("--- Server has started | Port: " + serverConfig.Port + " | Ip: " + serverConfig.IpAddress);
                    await MultiplayService.Instance.ReadyServerForPlayersAsync();
                    _serverHasStarted = true;
                }
                else
                {
                    Debug.LogError("--- Failed to start dedicated server | Port: " + serverConfig.Port + " | Ip: " + serverConfig.IpAddress);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("--- Failed to start linux dedicated server: " + e);
                throw;
            }
            
            #endif
        }

        private async void Update()
        {
            #if SERVER
            
            if (_serverHasStarted && _serverQueryHandler != null)
            {
                if (NetworkManager.Singleton == null)
                {
                    Debug.LogError("--- NetworkManager.Singleton is null");
                    _serverQueryHandler = null;
                    return;
                }

                if (!NetworkManager.Singleton.IsServer)
                {
                    Debug.LogError("--- NetworkManager Update: is not a Server");
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

                await UniTask.Delay(200);
            }
            
            #endif
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
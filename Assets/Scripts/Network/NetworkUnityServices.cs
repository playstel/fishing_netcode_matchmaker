using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    [RequireComponent(typeof(NetworkManager))]
    public class NetworkUnityServices : MonoBehaviour
    {
        public static NetworkUnityServices Instance;

        public bool signedIn;
        
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
            if (Application.platform != RuntimePlatform.LinuxServer)
            {
                await UnityServices.InitializeAsync();

                AuthenticationService.Instance.SignedIn += () =>
                {
                    Debug.Log("Signed in: " + AuthenticationService.Instance.PlayerId);
                };

                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }
        
        private void OnClientConnected(ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log($"Client {clientId} connected. Current scene: {SceneManager.GetActiveScene().name}");
            }
        }

        public bool StartMultiplayerClientSession(MultiplayAssignment multiplayAssignment)
        {
            return StartClient(multiplayAssignment.Ip,multiplayAssignment.Port.ToString());
        }

        public bool StartClient(string ipAddress, string port)
        {
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            
            transport.SetConnectionData(ipAddress, ushort.Parse(port));

            return NetworkManager.Singleton.StartClient();
        }
        
        public bool StartDedicatedServer(ServerConfig serverConfig)
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(serverConfig.IpAddress, serverConfig.Port);

            return NetworkManager.Singleton.StartServer();
        }

        public bool StartRelayClient(RelayServerData relayServerData)
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            return NetworkManager.Singleton.StartClient();
        }

        public bool StartRelayHost(RelayServerData relayServerData)
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
            return NetworkManager.Singleton.StartHost();
        }
    }
}
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    [RequireComponent(typeof(NetworkManager))]
    public class NetworkUnityServices : MonoBehaviour
    {
        public static NetworkUnityServices Instance;

        public const string InternalServerIp = "0.0.0.0";

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
            #if !SERVER
            
            await UnityServices.InitializeAsync();
            
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignedIn += () =>
                {
                    Debug.Log("Signed in: " + AuthenticationService.Instance.PlayerId);

                    signedIn = true;
                };

                var profileName = GenerateValidProfileName();
                Debug.Log("SwitchProfile | profileName: " + profileName);
                AuthenticationService.Instance.SwitchProfile(profileName);

                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
            else
            {
                Debug.LogError("AuthenticationService is signed already");
            }

#endif
        }
        
        private string GenerateValidProfileName()
        {
            // Укорачиваем GUID до 8 символов, чтобы гарантировать уникальность и соответствие требованиям
            return System.Guid.NewGuid().ToString("N").Substring(0, 8);
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
        
        public bool StartDedicatedServer(ushort port)
        {
            Debug.Log("--- StartDedicatedServer | UnityTransport | SetConnectionData: " + InternalServerIp + " " + port);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(InternalServerIp, port);

            Debug.Log("--- StartDedicatedServer | StartServer... ");
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
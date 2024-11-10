using System;
using System.Threading.Tasks;
using Menu;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    [RequireComponent(typeof(NetworkManager))]
    public class NetworkRelay : MonoBehaviour
    {
        public static NetworkRelay Instance;
        
        private string _transportProtocol = "dtls";

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
                Debug.Log("Relay disabled");
                return;
            }
            
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in: " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        
        private void OnClientConnected(ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log($"Client {clientId} connected. Current scene: {SceneManager.GetActiveScene().name}");
            }
        }
        
        public async Task<bool> CreateRelay()
        {
            try
            {
                NetworkStatusInfo.Instance.SetInfo("Creating Relay");
                
                var allocation = await Unity.Services.Relay.Relay.Instance.CreateAllocationAsync(3);
            
                var joinCode = await Unity.Services.Relay.Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

                var relayServerData = new RelayServerData(allocation, _transportProtocol);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
                var result = NetworkManager.Singleton.StartHost();
            
                NetworkStatusInfo.Instance.SetJoinCode(joinCode);
                
                if (result)
                {
                    NetworkStatusInfo.Instance.SetInfo($"You are the host");
                    MenuRoomLoader.Instance.LoadGameScene();
                }
                else
                {
                    NetworkStatusInfo.Instance.SetInfo($"Failed to create a host");
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Relay error: {e.Message}");
                NetworkStatusInfo.Instance.SetInfo($"Relay error: {e.Message}");
                return false;
            }
        }

        public async Task<bool> JoinRelay(string code)
        {
            try
            {
                NetworkStatusInfo.Instance.SetInfo($"Joining relay");
                
                var joinAllocation = await Unity.Services.Relay.Relay.Instance.JoinAllocationAsync(code);

                var relayServerData = new RelayServerData(joinAllocation, _transportProtocol);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                var result = NetworkManager.Singleton.StartClient();

                if (result)
                {
                    NetworkStatusInfo.Instance.SetInfo($"Joined to the host");
                }
                else
                {
                    NetworkStatusInfo.Instance.SetInfo($"Failed to join to the host");
                }

                return result;
            }
            catch(Exception e)
            {
                Debug.LogError($"Host error: {e.Message}");
                NetworkStatusInfo.Instance.SetInfo($"Host error: {e.Message}");
                
                return false;
            }
        }
    }
}
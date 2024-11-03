using System;
using System.Threading.Tasks;
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
    public class NetworkRelay : MonoBehaviour
    {
        public static NetworkRelay Instance;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private async void Start()
        {
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
        
        public async Task CreateRelay()
        {
            try
            {
                NetworkMessages.Instance.SetInfo("Creating Relay");
                
                var allocation = await Unity.Services.Relay.Relay.Instance.CreateAllocationAsync(3);
            
                var joinCode = await Unity.Services.Relay.Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

                var relayServerData = new RelayServerData(allocation, "dtls");

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
                var result = NetworkManager.Singleton.StartHost();
            
                NetworkMessages.Instance.SetJoinCode(joinCode);
                NetworkMessages.Instance.SetInfo($"Created: {result}");

                if (result) StartGame(); 
            }
            catch (Exception e)
            {
                Debug.LogError($"Relay error: {e.Message}");
                NetworkMessages.Instance.SetInfo($"Relay error: {e.Message}");
            }
        }

        private void StartGame()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Fishing", LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError("This instance is not the host.");
                NetworkMessages.Instance.SetInfo("This instance is not the host.");
            }
        }

        public async Task JoinRelay(string code)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrWhiteSpace(code))
            {
                NetworkMessages.Instance.SetInfo($"Code is null");
                return;
            }
            
            try
            {
                NetworkMessages.Instance.SetInfo($"Joining relay");
                
                var joinAllocation = await Unity.Services.Relay.Relay.Instance.JoinAllocationAsync(code);

                var relayServerData = new RelayServerData(joinAllocation, "dtls");

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                var result = NetworkManager.Singleton.StartClient();

                NetworkMessages.Instance.SetInfo($"Joined: {result}");
            }
            catch(Exception e)
            {
                Debug.LogError($"Relay error: {e.Message}");
                NetworkMessages.Instance.SetInfo($"Relay error: {e.Message}");
            }
        }
    }
}
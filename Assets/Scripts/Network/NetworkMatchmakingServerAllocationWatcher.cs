using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

namespace Network
{
    public class NetworkMatchmakingServerAllocationWatcher : NetworkBehaviour
    {
        private NetworkManager _networkManager;
        private bool _isDeallocating = false;
        private bool _deallocationCancellationToken = false;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                _networkManager = NetworkManager.Singleton;
            }
        }
        
        private void Update()
        {
            return;
            
            if (Application.platform == RuntimePlatform.LinuxServer)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    if (NetworkManager.Singleton == null)
                    {
                        Debug.LogError("--- Failed to find network manager on Update in NetworkMatchmakingServerAllocationWatcher");
                        return;
                    }
                
                    if (NetworkManager.Singleton.ConnectedClientsList.Count == 0 && !_isDeallocating)
                    {
                        _isDeallocating = true;
                        _deallocationCancellationToken = false;
                        Deallocate();
                    }
                
                    if (NetworkManager.Singleton.ConnectedClientsList.Count != 0)
                    {
                        _isDeallocating = false;
                        _deallocationCancellationToken = true;
                    }
                }
            }
        }

        private async void Deallocate()
        {
            await UniTask.Delay(60 * 1000);

            if (!_deallocationCancellationToken)
            {
                Application.Quit();
            }
        }

        private void OnApplicationQuit()
        {
            if (Application.platform != RuntimePlatform.LinuxServer)
            {
                if (_networkManager == null)
                {
                    Debug.LogError("--- Failed to find network manager OnApplicationQuit in NetworkMatchmakingServerAllocationWatcher");
                    return;
                }
                
                if (_networkManager.IsConnectedClient)
                {
                    if (NetworkManager.Singleton.IsServer)
                    {
                        Debug.Log("--- Shutdown the server...");
                        _networkManager.Shutdown(true);
                        Debug.Log("--- Disconnecting the server...");
                        _networkManager.DisconnectClient(OwnerClientId);
                    }
                }
            }
        }
    }
}
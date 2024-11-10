using System.Threading.Tasks;
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
            _networkManager = NetworkManager.Singleton;
        }
        
        private void Update()
        {
            if (Application.platform == RuntimePlatform.LinuxServer)
            {
                if (_networkManager.ConnectedClientsList.Count == 0 && !_isDeallocating)
                {
                    _isDeallocating = true;
                    _deallocationCancellationToken = false;
                    Deallocate();
                }
                
                if (_networkManager.ConnectedClientsList.Count != 0)
                {
                    _isDeallocating = false;
                    _deallocationCancellationToken = true;
                }
            }
        }

        private async void Deallocate()
        {
            await Task.Delay(60 * 1000);

            if (!_deallocationCancellationToken)
            {
                Application.Quit();
            }
        }

        private void OnApplicationQuit()
        {
            if (Application.platform != RuntimePlatform.LinuxServer)
            {
                if (_networkManager.IsConnectedClient)
                {
                    _networkManager.Shutdown(true);
                    _networkManager.DisconnectClient(OwnerClientId);
                }
            }
        }
    }
}
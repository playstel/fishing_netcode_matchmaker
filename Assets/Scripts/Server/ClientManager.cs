using System;
using Unity.Netcode.Transports.UTP;

namespace Server
{
    using UnityEngine;
    using Unity.Services.Relay;
    using Unity.Services.Relay.Models;
    using Unity.Networking.Transport.Relay;
    using System.Threading.Tasks;
    using Unity.Netcode;

    public class ClientManager : MonoBehaviour
    {
        public async void ConnectToServer()
        {
            // Пример подключения через Unity Relay
            string joinCode = await RequestJoinCodeFromRelay(); // Метод для получения кода подключения
            if (!string.IsNullOrEmpty(joinCode))
            {
                await ConnectWithRelay(joinCode);
            }
            else
            {
                Debug.LogError("Failed to get join code.");
            }
        }

        private async Task<string> RequestJoinCodeFromRelay()
        {
            try
            {
                var allocation = await Unity.Services.Relay.Relay.Instance.CreateAllocationAsync(2); // Создает новую сессию Relay
                return await Unity.Services.Relay.Relay.Instance.GetJoinCodeAsync(allocation.AllocationId); // Возвращает join-код
            }
            catch (Exception e)
            {
                Debug.LogError($"Relay service error: {e.Message}");
                return null;
            }
        }

        private async Task ConnectWithRelay(string joinCode)
        {
            try
            {
                var joinAllocation = await Unity.Services.Relay.Relay.Instance.JoinAllocationAsync(joinCode);
                var relayServerData = new RelayServerData(joinAllocation, "udp");

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartClient();

                Debug.Log("Connected to server via Relay.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Relay service error: {e.Message}");
            }
        }

        private void OnEnable()
        {
            //NetworkManager.Singleton.OnClientConnectedCallback += OnConnected;
            //NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
        }

        private void OnDisable()
        {
            //NetworkManager.Singleton.OnClientConnectedCallback -= OnConnected;
            //NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;
        }

        private void OnConnected(ulong clientId)
        {
            Debug.Log("Connected to server with client ID: " + clientId);
            // Логика после успешного подключения
        }

        private void OnDisconnected(ulong clientId)
        {
            Debug.Log("Disconnected from server with client ID: " + clientId);
            // Логика после отключения от сервера
        }
    }
}
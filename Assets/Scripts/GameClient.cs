using Unity.Netcode;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;

public class GameClient : MonoBehaviour
{
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        JoinRelayServer();
    }

    private async void JoinRelayServer()
    {
        // Получите данные для подключения к существующему серверу через Relay
        string joinCode = ""; // Получите код присоединения от сервера

        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetClientRelayData(
            joinAllocation.RelayServer.IpV4,
            (ushort)joinAllocation.RelayServer.Port,
            joinAllocation.AllocationIdBytes,
            joinAllocation.Key,
            joinAllocation.ConnectionData,
            joinAllocation.HostConnectionData
        );

        NetworkManager.Singleton.StartClient();
    }
}
using Unity.Netcode;
using UnityEngine;

public class GameServer : NetworkBehaviour
{
    public int maxPlayers = 4;
    private int currentPlayers = 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        currentPlayers++;
        if (currentPlayers == 1)
        {
            StartGame();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        currentPlayers--;
    }

    private void StartGame()
    {
        Debug.Log("Game starting with the first player.");
        // Запуск игровых процессов или стартовый отсчет
    }

    private void OnDestroy()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}
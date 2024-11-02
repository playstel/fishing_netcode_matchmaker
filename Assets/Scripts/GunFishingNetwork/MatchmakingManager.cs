using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker;
using Unity.Services.Multiplay;
using UnityEngine;
using Player = Unity.Services.Matchmaker.Models.Player;

public class MatchmakingManager : MonoBehaviour
{
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        StartMatchmaking();
    }

    private async void StartMatchmaking()
    {
        // Пользовательские данные игрока, которые будут учитываться при подборе

        var data = new Dictionary<string, PlayerDataObject>
        {
            {"SkillLevel", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "5")},
            {"Region", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "NA")}
        };

        var player = new Player(AuthenticationService.Instance.PlayerId, data);

        var players = new List<Player>
        {
            player
        };
            
        var options = new CreateTicketOptions
        {
            QueueName = "FishingQueue",
            Attributes = null
        };
        
        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);

        Debug.Log($"Matchmaking ticket created: {ticketResponse.Id}");

        ConnectToServer();
    }

    private void ConnectToServer()
    {
        string ipAddress = MultiplayService.Instance.ServerConfig.IpAddress;
        int port = MultiplayService.Instance.ServerConfig.Port;

        Debug.Log($"Connecting to server at {ipAddress}:{port}");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, (ushort)port);
        NetworkManager.Singleton.StartClient();
    }
}
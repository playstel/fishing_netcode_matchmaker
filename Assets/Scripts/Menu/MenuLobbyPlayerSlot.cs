using System;
using Network;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using static Network.NetworkLobby;

namespace Menu
{
    public class MenuLobbyPlayerSlot : MonoBehaviour
    {
        [SerializeField] private TMP_Text textLobbyPlayerName;
        [SerializeField] private TMP_Text textLobbyPlayerStatus;

        public void SetInfo(LobbyPlayer lobbyPlayer)
        {
            textLobbyPlayerName.text = lobbyPlayer.playerName;
            textLobbyPlayerStatus.text = lobbyPlayer.playerStatus;
        }
    }
}
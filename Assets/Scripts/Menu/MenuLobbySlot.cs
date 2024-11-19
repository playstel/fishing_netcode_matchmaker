using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    [RequireComponent(typeof(Button))]
    public class MenuLobbySlot : MonoBehaviour
    {
        [SerializeField] private TMP_Text textLobbyName;
        [SerializeField] private TMP_Text textLobbyPlayers;

        private Lobby _lobby;
        
        public void SetInfo(Lobby lobby)
        {
            _lobby = lobby;
            textLobbyName.text = lobby.Name;
            textLobbyPlayers.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
            GetComponent<Button>().enabled = lobby.Players.Count < lobby.MaxPlayers;
        }

        public Lobby GetLobby()
        {
            return _lobby;
        }
    }
}
using System;
using Network;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class MenuLobby : MonoBehaviour
    {
        private MenuLobbyList _lobbyList;
        
        [Header("Buttons")]
        [SerializeField] private Button buttonCreateLobby;
        [SerializeField] private Button buttonStartGame;
        
        [Header("Setup")]
        [SerializeField] private InputField inputLobbyName;
        [SerializeField] private Toggle toggleRelay;
        
        [Header("List")]
        [SerializeField] private GameObject prefabLobbySlot;
        [SerializeField] private Transform transformLobbyList;
        [SerializeField] private Transform transformPlayerList;

        private void Start()
        {
            AuthenticationService.Instance.SignedIn += StartShowingLobby;
            buttonCreateLobby.onClick.AddListener(CreateLobby);
        }

        private void StartShowingLobby()
        {
            InvokeRepeating(nameof(ShowLobbyList), 2, 2);
        }

        private async void ShowLobbyList()
        {
            _lobbyList = MenuLobbyList.Instance;

            var lobbies = await _lobbyList.GetLobbies();

            foreach (var lobby in lobbies)
            {
                var slotInstance = Instantiate(prefabLobbySlot, transformLobbyList);
                
                if (slotInstance.TryGetComponent(out MenuLobbySlot lobbySlot))
                {
                    //lobbySlot.GetComponent<Button>().onClick.AddListener(JoinLobby(lobbySlot.Lobby));
                }
            }
        }
        
        public async void JoinLobby(Lobby lobby)
        {
            MenuPanels.Instance.PanelLoading(true);
            var result = await _lobbyList.JoinLobby(lobby);
            MenuPanels.Instance.PanelLoading(false);
            if(result) MenuPanels.Instance.PanelLobby(true);
        }

        private async void CreateLobby()
        {
            MenuPanels.Instance.PanelLoading(true);
            var result = await _lobbyList.CreateLobby(inputLobbyName.text, toggleRelay.isOn);
            MenuPanels.Instance.PanelLoading(false);
            if(result) MenuPanels.Instance.PanelLobby(true);
        }
    }
}
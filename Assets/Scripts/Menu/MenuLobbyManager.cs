using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Network;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    public class MenuLobbyManager : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button buttonCreateLobby;
        
        [Header("Setup")]
        [SerializeField] private InputField inputLobbyName;
        [SerializeField] private Toggle toggleRelay;
        
        [Header("Lobbies List")]
        [SerializeField] private GameObject prefabLobbySlot;
        [SerializeField] private Transform transformLobbyList;

        [Header("Lobby Details")] 
        [SerializeField] private GameObject panelLobbyDetails;
        [SerializeField] private Button buttonStartGame;
        [SerializeField] private GameObject prefabPlayerSlot;
        [SerializeField] private Transform transformPlayerList;
        [SerializeField] private TMP_Text textLobbyName;

        private void Start()
        {
            buttonCreateLobby.onClick.AddListener(CreateLobby);
            buttonStartGame.onClick.AddListener(StartGameAsHost);
            ClearContainer(transformLobbyList);
            ClearContainer(transformPlayerList);
            StartShowingLobby();
        }

        private async void StartShowingLobby()
        {
            if(Application.platform == RuntimePlatform.LinuxServer) return;
            
            #if !SERVER
            Debug.Log("--- StartShowingLobby");
            await UniTask.WaitUntil(() => NetworkUnityServices.Instance.signedIn);
            Debug.Log("--- StartShowingLobby - signedIn");
            InvokeRepeating(nameof(ShowLobbyList), 2, 5);
            #endif
        }
        
        private List<Lobby> _lobbies = new();
        private async void ShowLobbyList()
        {
            Debug.Log("--- ShowLobbyList");
            
            if(SceneManager.GetActiveScene().buildIndex > 0) return;
            
            _lobbies = await NetworkLobby.Instance.GetLobbies();
            
            ClearContainer(transformLobbyList);

            for (var i = 0; i < _lobbies.Count; i++)
            {
                var slotInstance = Instantiate(prefabLobbySlot, transformLobbyList);
                
                if (slotInstance.TryGetComponent(out MenuLobbySlot lobbySlot))
                {
                    lobbySlot.SetInfo(_lobbies[i]);
                    var elementIndex = i;
                    lobbySlot.GetComponent<Button>().onClick.AddListener(() => OnListItemClicked(elementIndex));
                }
            }
        }
        
        private void OnListItemClicked(int index)
        { 
            JoinLobby(_lobbies[index]);
        }
        
        public async void JoinLobby(Lobby lobby)
        {
            Debug.Log("JoinLobby: " + lobby.Id);
            MenuLoading.Instance.PanelLoading(true);
            var result = await NetworkLobby.Instance.JoinLobby(lobby);
            MenuLoading.Instance.PanelLoading(false);

            if (result)
            {
                OpenLobbyDetails();
                StartShowingLobbyDetails();
            }
        }

        private async void CreateLobby()
        {
            MenuLoading.Instance.PanelLoading(true);
            var result = await NetworkLobby.Instance.CreateLobby(inputLobbyName.text, toggleRelay.isOn);
            MenuLoading.Instance.PanelLoading(false);

            if (result)
            {
                OpenLobbyDetails();
                StartShowingLobbyDetails();
            }
        }

        private void StartShowingLobbyDetails()
        {
            InvokeRepeating(nameof(OpenLobbyDetailsWithoutLoadingScreen), 1, 5);
        }

        private void OpenLobbyDetailsWithoutLoadingScreen()
        {
            OpenLobbyDetails(false);
        }

        private async void OpenLobbyDetails(bool loadingScreen = true)
        {
            Debug.Log("OpenLobbyDetails");
            
            if(loadingScreen) MenuLoading.Instance.PanelLoading(true);
            
            var lobby = await NetworkLobby.Instance.GetCurrentLobby();

            if (lobby == null)
            {
                Debug.LogError("Failed to find current lobby");
                return;
            }

            textLobbyName.text = lobby.Name;
            
            panelLobbyDetails.SetActive(true);

            ClearContainer(transformPlayerList);

            var players = NetworkLobby.Instance.CreateCurrentLobbyPlayers(lobby);

            Debug.Log("players: " + lobby.Players.Count);
            
            foreach (var player in players)
            {
                var slotInstance = Instantiate(prefabPlayerSlot, transformPlayerList);
                
                if (slotInstance.TryGetComponent(out MenuLobbyPlayerSlot slot))
                {
                    slot.SetInfo(player);
                }
            }
            
            CheckStartButton();
            
            if(loadingScreen) MenuLoading.Instance.PanelLoading(false);
        }

        private void CheckStartButton()
        {
            var isOwner = NetworkLobby.Instance.CheckLobbyOwnership();
            buttonStartGame.gameObject.SetActive(isOwner);
        }

        private void StartGameAsHost()
        {
            Debug.Log("StartGameAsHost");
            NetworkLobby.Instance.StartGame();
        }

        private void ClearContainer(Transform container)
        {
            for (int i = 0; i < container.childCount; i++)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }
    }
}
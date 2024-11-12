using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Menu;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static Unity.Services.Lobbies.Models.PlayerDataObject.VisibilityOptions;

namespace Network
{
    public class MenuLobbyList : MonoBehaviour
    {
        private Lobby _joinedLobby;
        private Player _playerData;
        
        public static MenuLobbyList Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else Destroy(gameObject);
        }
        
        public void CreateProfilePlayerData(string playerName = "Default player")
        {
            var playerDataObjectName = new PlayerDataObject(Public, GenerateRandomName(playerName));

            _playerData = new Player(id: AuthenticationService.Instance.PlayerId, data: 
                new Dictionary<string, PlayerDataObject>
                {
                    {"Name", playerDataObjectName} 
                });
        }

        private static string GenerateRandomName(string playerName)
        {
            return playerName + UnityEngine.Random.Range(1,10000);
        }

        public async Task<bool> CreateLobby(string lobbyName, bool relay = false, int maxPlayers = 4, string password = null)
        {
            if (_playerData == null)
            {
                CreateProfilePlayerData();
            }
            
            try
            {
                var options = new CreateLobbyOptions();
                
                options.IsPrivate = false;
                options.Player = _playerData;
                
                if (relay)
                {
                    var relayCode = await NetworkRelay.Instance.CreateRelay(loadGameScene: false);
                    
                    options.Data = new Dictionary<string, DataObject> 
                    {
                        {"RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode ) }
                    };
                }

                if (!string.IsNullOrEmpty(password))
                {
                    options.Password = password;
                }
                
                _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("JoinLobby error: " + e);
                NetworkStatusInfo.Instance.SetInfo("Failed to create lobby: " + e);
                return false;
            }
        }

        public void StartGame()
        {
            if (CheckLobbyOwnership())
            {
                NetworkSceneLoader.Instance.LoadGameScene();
            }
        }

        public async Task<bool> JoinLobby(Lobby lobby, string password = null)
        {
            if (_playerData == null)
            {
                CreateProfilePlayerData();
            }
            
            try
            {
                JoinLobbyByIdOptions options = new JoinLobbyByIdOptions();
                
                options.Player = _playerData;

                if (!string.IsNullOrEmpty(password))
                {
                    options.Password = password;
                }

                if (!string.IsNullOrEmpty(lobby.Data["RelayJoinCode"].Value))
                {
                    var code = lobby.Data["RelayJoinCode"].Value;
                    await NetworkRelay.Instance.JoinRelay(code);
                    return true;
                }
                
                var result = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, options);

                if (result == null) return false;
                
                _joinedLobby = result;
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("JoinLobby error: " + e);
                NetworkStatusInfo.Instance.SetInfo("JoinLobby error: " + e);
                return false;
            }
        }

        public async Task<List<Lobby>> GetLobbies(string filterByLobbyName = null)
        {
            var queryResponse = new QueryResponse();
            
            if (string.IsNullOrEmpty(filterByLobbyName))
            {
                queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            }
            else
            {
                var queryLobbiesOptions = new QueryLobbiesOptions();

                queryLobbiesOptions.Filters = new List<QueryFilter>();
                queryLobbiesOptions.Filters.Add(new QueryFilter(QueryFilter.FieldOptions.Name, filterByLobbyName, QueryFilter.OpOptions.CONTAINS));
                queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            }

            if (queryResponse?.Results != null)
            {
                return queryResponse.Results;
            }

            return new List<Lobby>();
        }

        public async void LobbyStatus(Lobby lobby)
        {
            while (true)
            {
                if(lobby == null) return;

                await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);

                await Task.Delay(15 * 1000);
            }
        }
        
        public class LobbyPlayer
        {
            public string playerName;
            public string playerStatus;
        }
        
        public List<LobbyPlayer> GetCurrentLobbyPlayers()
        {
            var players = new List<LobbyPlayer>();
            
            foreach (var player in _joinedLobby.Players)
            {
                var lobbyPlayer = new LobbyPlayer();

                lobbyPlayer.playerName = player.Data["Name"].Value;
                lobbyPlayer.playerStatus = (_joinedLobby.HostId == player.Id) ? "Owner" : "User";
                
                players.Add(lobbyPlayer);
            }

            return players;
        }

        public bool CheckLobbyOwnership()
        {
            if (_joinedLobby == null)
            {
                Debug.LogError("You are not in the lobby");
                return false;
            }
            
            return AuthenticationService.Instance.PlayerId == _joinedLobby.HostId;
        }

        public bool CheckCurrentLobby()
        {
            return _joinedLobby != null;
        }
    }
}
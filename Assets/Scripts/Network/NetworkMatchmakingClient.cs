using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Menu;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

namespace Network
{
    public class NetworkMatchmakingClient : NetworkBehaviour
    {
        [SerializeField] private string matchmakerQueueName = "Fishing";
        
        private class PlayerCustomData
        {
            public int SkillLevel;
        }

        public static NetworkMatchmakingClient Instance;
        
        private string _createTicketResponseId;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else Destroy(gameObject);
        }

        public async UniTask<bool> CreateMultiplayerTicketAndSession()
        {
            //Dictionary<string, object> hardModeAttribute = new Dictionary<string, object> {{ "GameMode", "Hard" }};

            var ticketOption = new CreateTicketOptions(matchmakerQueueName);
            var playerId = AuthenticationService.Instance.PlayerId;

            List<Unity.Services.Matchmaker.Models.Player> players = new List<Unity.Services.Matchmaker.Models.Player>()
            {
                new Unity.Services.Matchmaker.Models.Player(playerId) //, new PlayerCustomData {SkillLevel = 100})
            };
            
            CreateTicketResponse createTicketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, ticketOption);
            _createTicketResponseId = createTicketResponse.Id;
            
            Debug.Log($"Player {playerId} create a ticket: {_createTicketResponseId}");

            while (true)
            {
                TicketStatusResponse ticketStatusResponse =
                    await MatchmakerService.Instance.GetTicketAsync(createTicketResponse.Id);

                if (ticketStatusResponse.Type == typeof(MultiplayAssignment))
                {
                    var assignment = (MultiplayAssignment) ticketStatusResponse.Value;

                    Debug.Log("Match assignment status: " + assignment.Status);
                    
                    if (assignment.Status == MultiplayAssignment.StatusOptions.Found)
                    {
                        var result = NetworkUnityServices.Instance.StartMultiplayerClientSession(assignment);

                        NetworkStatusInfo.Instance.SetInfo($"Match has started successfully: {result}");
                        Debug.Log($"Match has started successfully: {result}");

                        if (result)
                        {
                            NetworkSceneLoader.Instance.LoadGameScene();
                        }
                        
                        return result;
                    }
                    else if (assignment.Status == MultiplayAssignment.StatusOptions.Timeout)
                    {
                        NetworkStatusInfo.Instance.SetInfo($"Match timeout");
                        Debug.LogError("Match loading timeout");
                        return false;
                    }
                    else if (assignment.Status == MultiplayAssignment.StatusOptions.Failed)
                    {
                        NetworkStatusInfo.Instance.SetInfo($"Match failed: {assignment.Message}");
                        Debug.LogError($"Match loading failed: {assignment.Message}");
                        return false;
                    }
                    else if (assignment.Status == MultiplayAssignment.StatusOptions.InProgress)
                    {
                        NetworkStatusInfo.Instance.SetInfo($"Match in progress");
                        Debug.Log("Match loading in progress");
                    }
                }

                await UniTask.Delay(1000);
            }
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

namespace Network
{
    public class NetworkMatchmakingServerBackfillTickets : NetworkBehaviour
    {
        // a backfill ticket helps players join after the game has started
        private string _backfillTicketId;
        private PayloadAllocation _payloadAllocation;
        private NetworkManager _networkManager;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private async void Start()
        {
            return;
            
            _networkManager = NetworkManager.Singleton;
            
            if (Application.platform == RuntimePlatform.LinuxServer)
            {
                _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
                _networkManager.OnClientConnectedCallback += OnClientConnected;
                
                while (UnityServices.State == ServicesInitializationState.Uninitialized || 
                       UnityServices.State == ServicesInitializationState.Initializing)
                {
                    await Task.Yield();
                }

                _payloadAllocation = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<PayloadAllocation>();
                _backfillTicketId = _payloadAllocation.BackfillTicketId;
            }
            
        }

        private async void Update()
        {
            return;
            
            if (Application.platform == RuntimePlatform.LinuxServer)
            {
                if (_backfillTicketId != null && _networkManager.ConnectedClientsList.Count < 4)
                {
                    BackfillTicket backfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(_backfillTicketId);
                    _backfillTicketId = backfillTicket.Id;
                }

                await Task.Delay(1000);
            }
        }
        
        private void OnClientDisconnected(ulong clientId)
        {
            UpdateBackfillTicket();
        }
        
        private void OnClientConnected(ulong clientId)
        {
            UpdateBackfillTicket();
        }

        private async void UpdateBackfillTicket()
        {
            return;
            if (Application.platform == RuntimePlatform.LinuxServer)
            {
                List<Player> players = new();

                foreach (var playerId in _networkManager.ConnectedClientsIds)
                {
                    players.Add(new Player(playerId.ToString()));
                }

                var matchProperties = new MatchProperties(null, players, null, _backfillTicketId);

                await MatchmakerService.Instance.UpdateBackfillTicketAsync(_payloadAllocation.BackfillTicketId,
                    new BackfillTicket(_backfillTicketId, properties: new BackfillTicketProperties(matchProperties)));
            }
        }

        [System.Serializable]
        public class PayloadAllocation
        {
            public MatchProperties MatchProperties;
            public string GeneratorName;
            public string QueueName;
            public string PoolName;
            public string EnviromentId;
            public string BackfillTicketId;
            public string MatchId;
            public string PoolId;
        }
    }
}
using Network;
using NetworkServer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class MenuMainButtons : MonoBehaviour
    {
        [Header("Matchmaking")] 
        [SerializeField] private Button buttonCreateTicket;
        
        [Header("Dedicated Server")] 
        [SerializeField] private Button buttonJoinServer;
        [SerializeField] private InputField inputServerIp;
        [SerializeField] private InputField inputServerPort;
        
        [Header("Relay")]
        [SerializeField] private Button buttonHostRelay;
        [SerializeField] private Button buttonJoinRelay;
        [SerializeField] private InputField inputJoinRelay;

        private void Awake()
        {
            buttonHostRelay.onClick.AddListener(CreateRelay);
            buttonJoinRelay.onClick.AddListener(() => JoinRelay(inputJoinRelay.text));
            buttonJoinServer.onClick.AddListener(() => JoinServer(inputServerIp.text, inputServerPort.text));
            buttonCreateTicket.onClick.AddListener(CreateMultiplayerTicket);
        }

        private void JoinServer(string ip, string port)
        {
            if (string.IsNullOrEmpty(ip) || string.IsNullOrWhiteSpace(ip))
            {
                NetworkStatusInfo.Instance.SetInfo($"Server Ip is null");
                return;
            }
            
            
            if (string.IsNullOrEmpty(port) || string.IsNullOrWhiteSpace(port))
            {
                NetworkStatusInfo.Instance.SetInfo($"Server Port is null");
                return;
            }

            MenuLoading.Instance.PanelLoading(true);
            var result = NetworkServer.NetworkDedicatedServer.Instance.JoinToServer(ip, port);
            if(!result) MenuLoading.Instance.PanelLoading(false);
        }
        
        private async void JoinRelay(string inputJoinText)
        {
            if (string.IsNullOrEmpty(inputJoinText) || string.IsNullOrWhiteSpace(inputJoinText))
            {
                NetworkStatusInfo.Instance.SetInfo($"Code is null");
                return;
            }
            
            MenuLoading.Instance.PanelLoading(true);
            var result = await NetworkRelay.Instance.JoinRelay(inputJoinText);
            if(!result) MenuLoading.Instance.PanelLoading(false);
        }

        private async void CreateRelay()
        {
            MenuLoading.Instance.PanelLoading(true);
            var result = await NetworkRelay.Instance.CreateRelay();
            if(result == null) MenuLoading.Instance.PanelLoading(false);
        }
        
        private async void CreateMultiplayerTicket()
        {
            MenuLoading.Instance.PanelLoading(true);
            var result = await NetworkMatchmakingClient.Instance.CreateMultiplayerTicketAndSession();
            if(!result) MenuLoading.Instance.PanelLoading(false);
        }
    }
}
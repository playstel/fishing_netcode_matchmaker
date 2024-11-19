using System;
using Cysharp.Threading.Tasks;
using Menu;
using Unity.Networking.Transport.Relay;
using UnityEngine;

namespace Network
{
    public class NetworkRelay : MonoBehaviour
    {
        public static NetworkRelay Instance;
        
        [SerializeField] private string _transportProtocol = "dtls";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else Destroy(gameObject);
        }
        
        public async UniTask<string> CreateRelay(bool loadGameScene = true)
        {
            try
            {
                NetworkStatusInfo.Instance.SetInfo("Creating Relay");
                
                var allocation = await Unity.Services.Relay.Relay.Instance.CreateAllocationAsync(3);
            
                var relayJoinCode = await Unity.Services.Relay.Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

                var relayServerData = new RelayServerData(allocation, _transportProtocol);

                var result = NetworkUnityServices.Instance.StartRelayHost(relayServerData);
            
                NetworkStatusInfo.Instance.SetJoinCode(relayJoinCode);
                
                if (result)
                {
                    NetworkStatusInfo.Instance.SetInfo($"You are the host");
                    
                    if (loadGameScene)
                    {
                        NetworkSceneLoader.Instance.LoadGameScene();
                    }
                }
                else
                {
                    NetworkStatusInfo.Instance.SetInfo($"Failed to create a host");
                    return null;
                }

                return relayJoinCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"Relay error: {e.Message}");
                NetworkStatusInfo.Instance.SetInfo($"Relay error: {e.Message}");
                return null;
            }
        }
        
        public async UniTask<bool> JoinRelay(string code)
        {
            try
            {
                NetworkStatusInfo.Instance.SetInfo($"Joining relay");
                
                var joinAllocation = await Unity.Services.Relay.Relay.Instance.JoinAllocationAsync(code);

                var relayServerData = new RelayServerData(joinAllocation, _transportProtocol);

                var result = NetworkUnityServices.Instance.StartRelayClient(relayServerData);

                if (result)
                {
                    NetworkStatusInfo.Instance.SetInfo($"Joined to the host");
                }
                else
                {
                    NetworkStatusInfo.Instance.SetInfo($"Failed to join to the host");
                }

                return result;
            }
            catch(Exception e)
            {
                Debug.LogError($"Host error: {e.Message}");
                NetworkStatusInfo.Instance.SetInfo($"Host error: {e.Message}");
                
                return false;
            }
        }
    }
}
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Network;
using Unity.Netcode;
using Unity.Services.Multiplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu
{
    public class NetworkSceneLoader : NetworkBehaviour
    {
        public static NetworkSceneLoader Instance;
        
        [SerializeField] private string _gameSceneName = "2_Fishing";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else Destroy(gameObject);
        }

        public async void LoadGameScene(bool directLoading = false)
        {
            Debug.Log("LoadGameScene | IsClient: " + IsClient + " | IsServer: " + IsServer + " | isHost: " + IsHost + " | IsOwnedByServer: " + IsOwnedByServer);

            MenuLoading.Instance.PanelLoading(true);
            
            await UniTask.Delay(200);

            if (directLoading)
            {
                RequestSceneLoadServerRpc();
                return;
            }
            
            if (IsClient)
            {
                Debug.Log("Request game scene as a client");
                RequestSceneLoadServerRpc();
            }
            else if (IsServer || IsHost)
            {
                Debug.Log("Load game scene as a host or server"); NetworkManager.Singleton.SceneManager.LoadScene(_gameSceneName, LoadSceneMode.Single);
            }
            
            await UniTask.Delay(200);
            
            MenuLoading.Instance.PanelLoading(false);
            
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestSceneLoadServerRpc()
        {
            NetworkManager.Singleton.SceneManager.LoadScene(_gameSceneName, LoadSceneMode.Single);
        }
    }
}
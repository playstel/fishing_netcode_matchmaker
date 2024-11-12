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

        public void LoadGameScene()
        {
            if (IsClient && !IsHost)
            {
                Debug.LogError("Request game scene as a client");
                RequestSceneLoadServerRpc();
            }
            else if (IsServer)
            {
                Debug.LogError("Load game scene as a host or server");
                NetworkManager.Singleton.SceneManager.LoadScene(_gameSceneName, LoadSceneMode.Single);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestSceneLoadServerRpc()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(_gameSceneName, LoadSceneMode.Single);
            }
        }
    }
}
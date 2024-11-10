using Network;
using Unity.Netcode;
using Unity.Services.Multiplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu
{
    public class MenuRoomLoader : MonoBehaviour
    {
        public static MenuRoomLoader Instance;
        
        [SerializeField] private string _gameSceneName = "2_Fishing";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else Destroy(gameObject);
        }
        
        public void LoadGameSceneAsHost()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(_gameSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError("This instance is not the host.");
                NetworkStatusInfo.Instance.SetInfo("This instance is not the host.");
            }
        }
        
        public void LoadGameSceneAsServerClient()
        {
            Debug.Log("LoadGameSceneAsServerClient");
        }
    }
}
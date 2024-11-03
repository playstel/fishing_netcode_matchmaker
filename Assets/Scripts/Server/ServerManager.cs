namespace Server
{
    using Unity.Netcode;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class ServerManager : MonoBehaviour
    {
        private void Start()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log("Server has started.");
                LoadGameScene();
            }
        }

        public void LoadGameScene()
        {
            // Загружает игровую сцену на сервере и синхронизирует ее с клиентами
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected to the server.");
            // Дополнительная логика при подключении клиента (например, инициализация игрока)
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected from the server.");
            // Логика при отключении клиента (например, очистка данных клиента)
        }

        private void OnEnable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void OnDisable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

}
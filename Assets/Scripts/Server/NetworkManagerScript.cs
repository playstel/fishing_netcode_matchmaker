namespace Server
{
    using Unity.Netcode;
    using UnityEngine;

    public class NetworkManagerScript : MonoBehaviour
    {
        public static NetworkManagerScript Singleton;

        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // Запуск сервера или клиента в зависимости от аргументов командной строки
            if (IsServerBuild())
            {
                StartServer();
            }
            else
            {
                StartClient();
            }
        }

        private bool IsServerBuild()
        {
            // Проверка на запуск в головном режиме (Headless Mode)
            return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
        }

        public void StartServer()
        {
            Debug.Log("Starting Server...");
            Unity.Netcode.NetworkManager.Singleton.StartServer();
        }

        public void StartClient()
        {
            Debug.Log("Starting Client...");
            Unity.Netcode.NetworkManager.Singleton.StartClient();
        }

        public void StartHost()
        {
            Debug.Log("Starting Host...");
            Unity.Netcode.NetworkManager.Singleton.StartHost();
        }
    }

}
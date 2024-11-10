using TMPro;
using UnityEngine;

namespace Network
{
    public class NetworkStatusInfo : MonoBehaviour
    {
        [SerializeField] private TMP_Text textJoinInfo;
        [SerializeField] private TMP_Text textJoinCode;

        public static NetworkStatusInfo Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else Destroy(gameObject);
        }

        public void SetInfo(string info)
        {
            textJoinInfo.text = info;
        }

        public void SetJoinCode(string text)
        {
            textJoinCode.text = $"Join code: {text}";
            GUIUtility.systemCopyBuffer = text;
        }
    }
}
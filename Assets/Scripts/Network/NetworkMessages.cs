using TMPro;
using UnityEngine;

namespace Network
{
    public class NetworkMessages : MonoBehaviour
    {
        [SerializeField] private TMP_Text textJoinInfo;
        [SerializeField] private TMP_Text textJoinCode;

        public static NetworkMessages Instance;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
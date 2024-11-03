using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Network
{
    public class NetworkUi : MonoBehaviour
    {
        [SerializeField] private Button buttonJoin;
        [SerializeField] private Button buttonHost;
        [SerializeField] private TMP_InputField inputJoin;
        
        private void Awake()
        {
            buttonHost.onClick.AddListener(CreateRelay);
            buttonJoin.onClick.AddListener(() => JoinRelay(inputJoin.text));
        }

        private async void JoinRelay(string inputJoinText)
        {
            BlockButtons(true);
            await NetworkRelay.Instance.JoinRelay(inputJoinText);
            BlockButtons(false);
        }

        private async void CreateRelay()
        {
            BlockButtons(true);
            await NetworkRelay.Instance.CreateRelay();
            BlockButtons(false);
        }

        private void BlockButtons(bool state)
        {
            buttonJoin.interactable = !state;
            buttonHost.interactable = !state;
            inputJoin.interactable = !state;
        }
    }
}
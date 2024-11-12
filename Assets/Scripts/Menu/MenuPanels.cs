using System;
using UnityEngine;

namespace Menu
{
    public class MenuPanels : MonoBehaviour
    {
        public static MenuPanels Instance;

        [Header("Loading panel")] 
        [SerializeField] private GameObject panelLoading;
        [SerializeField] private GameObject panelLobby;
        [SerializeField] private GameObject panelMain;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else Destroy(gameObject);
        }
        
        public void PanelLoading(bool state)
        {
            panelLoading.SetActive(state);
        }

        public void PanelLobby(bool state)
        {
            panelLobby.SetActive(state);
        }
    }
}
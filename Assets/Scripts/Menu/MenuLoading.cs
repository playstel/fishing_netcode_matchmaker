using System;
using UnityEngine;

namespace Menu
{
    public class MenuLoading : MonoBehaviour
    {
        public static MenuLoading Instance;
        
        [SerializeField] private GameObject panelLoading;
        
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
    }
}
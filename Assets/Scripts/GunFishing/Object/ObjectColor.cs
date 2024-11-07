using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace GunFishing.Object
{
    public class ObjectColor : NetworkBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color[] colorList;
        [SerializeField] private NetworkObject bulletObject;
        
        public void Start()
        {
            var networkObjectId = (int)bulletObject.OwnerClientId;
            
            if (networkObjectId < colorList.Length)
            {
                spriteRenderer.color = colorList[networkObjectId];
            }
        }
        
    }
}
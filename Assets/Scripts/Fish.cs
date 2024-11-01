using Unity.Netcode;
using UnityEngine;

public class Fish : NetworkBehaviour
{
    public NetworkVariable<bool> isCaught = new NetworkVariable<bool>(false);

    private void Update()
    {
        if (isCaught.Value)
            return;

        MoveFish();
    }

    private void MoveFish()
    {
        // Логика случайного перемещения рыбы
    }

    [ServerRpc]
    public void CatchFishServerRpc(ulong playerId)
    {
        if (!isCaught.Value)
        {
            isCaught.Value = true;
            Debug.Log($"Fish caught by player {playerId}");
            // Дополнительная логика, например, начисление очков
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter: " + other.tag);
        if (other.CompareTag("Bullet")) 
        {
            Destroy(other.gameObject);
            Destroy(gameObject);      
        }
    }
}
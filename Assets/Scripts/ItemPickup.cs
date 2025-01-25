using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("ItemPickup: Initialized");
        // Verify collider setup
        if (!GetComponent<Collider2D>())
        {
            Debug.LogError("ItemPickup: Missing Collider2D!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"ItemPickup: Trigger entered by {other.gameObject.name}");
        
        if (other.TryGetComponent<BeetleBubble>(out var player))
        {
            Debug.Log($"Player {player.GetComponent<PlayerInput>()?.playerIndex ?? -1} picked up item");
            Destroy(gameObject);
        }
    }
}

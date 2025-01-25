using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType
    {
        Shield,
        PowerCharge
    }

    #region Serialized Fields
    [SerializeField] private ItemType m_ItemType = ItemType.Shield;
    #endregion

    #region Events
    public static event System.Action<ItemPickup> OnItemCollected;
    #endregion

    #region Properties
    public Transform SpawnPoint { get; set; }
    public ItemType Type 
    { 
        get => m_ItemType;
        set 
        {
            m_ItemType = value;
            UpdateVisuals();
        }
    }
    #endregion

    private void Awake()
    {
        Debug.Log("ItemPickup: Initialized");
        // Verify collider setup
        if (!GetComponent<Collider2D>())
        {
            Debug.LogError("ItemPickup: Missing Collider2D!");
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Set color based on type
        if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            spriteRenderer.color = m_ItemType == ItemType.Shield ? 
                new Color(0, 0.7f, 1f, 1f) :  // Blue for shield
                new Color(1f, 0.98f, 0, 1f);  // Yellow for power charge
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Get the parent BeetleBubble component
        var player = other.GetComponentInParent<BeetleBubble>();
        if (player != null)
        {
            Debug.Log($"Player {player.GetComponent<PlayerInput>()?.playerIndex ?? -1} picked up {m_ItemType}");
            
            ItemEffect effect = m_ItemType == ItemType.Shield ?
                player.gameObject.AddComponent<ShieldEffect>() :
                player.gameObject.AddComponent<PowerChargeEffect>();
            
            effect.Initialize(player);
            OnItemCollected?.Invoke(this);
            Destroy(gameObject);
        }
    }
}

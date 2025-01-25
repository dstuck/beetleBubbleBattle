using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private ItemPickup.ItemType m_ItemType = ItemPickup.ItemType.Shield;
    [SerializeField] private float m_SpawnDelay = 5f;
    #endregion

    #region Private Fields
    private bool m_IsSpawning;
    private ItemPickup m_CurrentItem;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        SpawnItem();
    }

    private void OnEnable()
    {
        ItemPickup.OnItemCollected += HandleItemCollected;
    }

    private void OnDisable()
    {
        ItemPickup.OnItemCollected -= HandleItemCollected;
    }
    #endregion

    #region Private Methods
    private void HandleItemCollected(ItemPickup _collectedItem)
    {
        if (_collectedItem == m_CurrentItem)
        {
            m_CurrentItem = null;
            StartSpawnCycle();
        }
    }

    private void StartSpawnCycle()
    {
        if (!m_IsSpawning)
        {
            StartCoroutine(SpawnCycleRoutine());
        }
    }

    private IEnumerator SpawnCycleRoutine()
    {
        m_IsSpawning = true;
        yield return new WaitForSeconds(m_SpawnDelay);
        SpawnItem();
        m_IsSpawning = false;
    }

    private void SpawnItem()
    {
        if (m_CurrentItem != null || GameManager.Instance == null) return;

        var itemPrefab = GameManager.Instance.ItemPickupPrefab;
        if (itemPrefab == null)
        {
            Debug.LogError("SpawnPoint: No item prefab set in GameManager!");
            return;
        }

        var item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
        m_CurrentItem = item.GetComponent<ItemPickup>();
        
        if (m_CurrentItem != null)
        {
            m_CurrentItem.SpawnPoint = transform;
            m_CurrentItem.Type = m_ItemType;
        }
        else
        {
            Debug.LogError("SpawnPoint: ItemPickup component not found on prefab!");
        }
    }
    #endregion
} 
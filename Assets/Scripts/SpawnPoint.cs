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
    private LevelManager m_LevelManager;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        m_LevelManager = FindFirstObjectByType<LevelManager>();
        if (m_LevelManager == null)
        {
            Debug.LogError("SpawnPoint: No LevelManager found in scene!");
            return;
        }
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
    private void SpawnItem()
    {
        if (m_CurrentItem != null || m_LevelManager == null) return;

        GameObject itemObj = Instantiate(m_LevelManager.GetItemPickupPrefab(), transform.position, Quaternion.identity);
        m_CurrentItem = itemObj.GetComponent<ItemPickup>();
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
    #endregion
} 
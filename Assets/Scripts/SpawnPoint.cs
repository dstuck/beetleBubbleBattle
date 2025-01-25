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
    private GameObject m_CurrentItem;
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        ItemPickup.OnItemCollected += HandleItemCollected;
    }

    private void OnDisable()
    {
        ItemPickup.OnItemCollected -= HandleItemCollected;
    }

    private void Start()
    {
        StartSpawnCycle();
    }
    #endregion

    #region Private Methods
    private void HandleItemCollected()
    {
        if (m_CurrentItem == null)
        {
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
        if (m_CurrentItem != null) return;

        var itemPrefab = GameManager.Instance.ItemPickupPrefab;
        if (itemPrefab == null)
        {
            Debug.LogError("SpawnPoint: No item prefab set in GameManager!");
            return;
        }

        m_CurrentItem = Instantiate(itemPrefab, transform.position, Quaternion.identity);
        var itemPickup = m_CurrentItem.GetComponent<ItemPickup>();
        itemPickup.SpawnPoint = transform;
        itemPickup.Type = m_ItemType;
    }
    #endregion
} 
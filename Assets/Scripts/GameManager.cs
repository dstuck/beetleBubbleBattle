using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

public class GameManager : MonoBehaviour
{
    #region Constants
    private const int c_TargetWidth = 1920;
    private const int c_TargetHeight = 1080;
    #endregion

    #region Events
    public delegate void ItemSpawnEvent();
    public static event ItemSpawnEvent OnItemSpawnRequested;
    #endregion

    #region Serialized Fields
    [SerializeField] private Transform[] m_SpawnPoints;
    [SerializeField] private GameObject m_ItemPickupPrefab;
    [SerializeField] private Transform[] m_ItemSpawnPoints;
    [SerializeField] private float m_ItemSpawnInterval = 5f;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Screen.SetResolution(c_TargetWidth, c_TargetHeight, FullScreenMode.Windowed);
    }

    private void Start()
    {
        // Start the first spawn cycle
        StartSpawnCycle();
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
    private void HandleItemCollected()
    {
        StartSpawnCycle();
    }

    private void StartSpawnCycle()
    {
        StartCoroutine(SpawnCycleRoutine());
    }

    private IEnumerator SpawnCycleRoutine()
    {
        yield return new WaitForSeconds(m_ItemSpawnInterval);
        OnItemSpawnRequested?.Invoke();
        SpawnItem();
    }

    private void SpawnItem()
    {
        if (m_ItemSpawnPoints == null || m_ItemSpawnPoints.Length == 0)
        {
            Debug.LogWarning("No item spawn points set!");
            return;
        }

        int randomIndex = Random.Range(0, m_ItemSpawnPoints.Length);
        Transform spawnPoint = m_ItemSpawnPoints[randomIndex];

        // Check if this spawn point already has an item
        ItemPickup[] existingItems = FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);
        foreach (var eItem in existingItems)
        {
            if (eItem.SpawnPoint == spawnPoint)
            {
                return;
            }
        }

        var item = Instantiate(m_ItemPickupPrefab, spawnPoint.position, Quaternion.identity);
        item.GetComponent<ItemPickup>().SpawnPoint = spawnPoint;
    }
    #endregion

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        int playerIndex = playerInput.playerIndex;
        string scheme = playerInput.currentControlScheme;
        Debug.Log($"Player {playerIndex} joined with control scheme: {scheme} (using device: {playerInput.devices.FirstOrDefault()?.name ?? "none"})");
        
        // Assign spawn point if available
        if (playerIndex < m_SpawnPoints.Length)
        {
            playerInput.transform.position = m_SpawnPoints[playerIndex].position;
            Debug.Log($"Spawned player {playerIndex} at position {m_SpawnPoints[playerIndex].position}");
        }
        else
        {
            Debug.LogWarning($"No spawn point available for player {playerIndex}");
        }
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        Debug.Log($"Player {playerInput.playerIndex} left with control scheme: {playerInput.currentControlScheme}");
    }
} 
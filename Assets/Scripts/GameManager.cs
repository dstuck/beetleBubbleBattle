using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class GameManager : MonoBehaviour
{
    #region Constants
    private const int c_TargetWidth = 1920;
    private const int c_TargetHeight = 1080;
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
        InvokeRepeating(nameof(SpawnItem), m_ItemSpawnInterval, m_ItemSpawnInterval);
    }
    #endregion

    #region Private Methods
    private void SpawnItem()
    {
        if (m_ItemSpawnPoints == null || m_ItemSpawnPoints.Length == 0)
        {
            Debug.LogWarning("No item spawn points set!");
            return;
        }

        int randomIndex = Random.Range(0, m_ItemSpawnPoints.Length);
        Vector3 spawnPosition = m_ItemSpawnPoints[randomIndex].position;
        Instantiate(m_ItemPickupPrefab, spawnPosition, Quaternion.identity);
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
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Transform[] m_SpawnPoints;
    [SerializeField] private GameObject m_ItemPickupPrefab;

    private void Start()
    {
        if (m_SpawnPoints == null || m_SpawnPoints.Length == 0)
        {
            Debug.LogError("LevelManager: No spawn points assigned!");
            return;
        }
        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        PlayerInput[] registeredPlayers = GameManager.Instance.GetRegisteredPlayers();
        
        for (int i = 0; i < registeredPlayers.Length; i++)
        {
            if (i < m_SpawnPoints.Length && m_SpawnPoints[i] != null)
            {
                PlayerInput playerInput = registeredPlayers[i];
                GameObject player = playerInput.gameObject;
                Vector3 spawnPosition = m_SpawnPoints[i].position;
                
                // Register spawn point before moving the player
                GameManager.Instance.RegisterSpawnPoint(playerInput, spawnPosition);
                
                // Position the player
                player.transform.position = spawnPosition;
                
                // Enable the Player action map
                playerInput.actions.FindActionMap("Player").Enable();
                
                // Enable visual components
                foreach (Transform child in player.transform)
                {
                    child.gameObject.SetActive(true);
                }
                
                Debug.Log($"Positioned player {i} at spawn point {i}: {spawnPosition}");
            }
            else
            {
                Debug.LogError($"Invalid spawn point for player {i}");
            }
        }
    }

    public Transform GetSpawnPoint(int index)
    {
        if (index < m_SpawnPoints.Length)
        {
            return m_SpawnPoints[index];
        }
        return null;
    }

    public GameObject GetItemPickupPrefab()
    {
        return m_ItemPickupPrefab;
    }
} 
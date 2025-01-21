using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform[] m_SpawnPoints;
    
    private void Awake()
    {
        Debug.Log($"GameManager: Found {m_SpawnPoints?.Length ?? 0} spawn points");
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        int playerIndex = playerInput.playerIndex;
        Debug.Log($"Player {playerIndex} joined with control scheme: {playerInput.currentControlScheme}");
        
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
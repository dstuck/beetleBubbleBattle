using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    #endregion

    #region Public Properties
    public GameObject ItemPickupPrefab => m_ItemPickupPrefab;
    #endregion

    #region Constants
    private const int c_TargetWidth = 1920;
    private const int c_TargetHeight = 1080;
    #endregion

    #region Serialized Fields
    [SerializeField] private Transform[] m_SpawnPoints;
    [SerializeField] private GameObject m_ItemPickupPrefab;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Screen.SetResolution(c_TargetWidth, c_TargetHeight, FullScreenMode.Windowed);
    }
    #endregion

    #region Public Methods
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
    #endregion
} 
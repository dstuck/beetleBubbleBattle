using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    #endregion

    #region Constants
    private const int c_TargetWidth = 1920;
    private const int c_TargetHeight = 1080;
    #endregion

    #region Private Fields
    [Header("Player Setup")]
    [SerializeField] private GameObject m_BeetleBubblePrefab;
    
    private List<PlayerInput> m_RegisteredPlayers = new List<PlayerInput>();
    private Dictionary<PlayerInput, Vector3> m_PlayerSpawnPoints = new Dictionary<PlayerInput, Vector3>();
    private PlayerInputManager m_PlayerInputManager;
    #endregion

    #region Events
    public System.Action<PlayerInput> OnPlayerJoinedEvent;
    public System.Action<PlayerInput> OnPlayerLeftEvent;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Configure PlayerInputManager
            m_PlayerInputManager = GetComponent<PlayerInputManager>();
            if (m_PlayerInputManager != null)
            {
                // Set the prefab to spawn initially disabled
                var prefab = m_PlayerInputManager.playerPrefab;
                if (prefab != null)
                {
                    prefab.SetActive(false);
                }
            }
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
        // Don't add players without devices
        if (!playerInput.devices.Any())
        {
            Debug.LogWarning("Rejecting player join with no device");
            return;
        }

        if (!m_RegisteredPlayers.Contains(playerInput))
        {
            m_RegisteredPlayers.Add(playerInput);
            DontDestroyOnLoad(playerInput.gameObject);
            
            // Disable only the Player action map, keep UI/Menu actions enabled
            playerInput.actions.FindActionMap("Player").Disable();
            
            // Disable visual components
            foreach (Transform child in playerInput.transform)
            {
                child.gameObject.SetActive(false);
            }
            
            Debug.Log($"Player {playerInput.playerIndex} joined with control scheme: {playerInput.currentControlScheme} (using device: {playerInput.devices.FirstOrDefault()?.name ?? "none"})");
            
            OnPlayerJoinedEvent?.Invoke(playerInput);
        }
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        m_RegisteredPlayers.Remove(playerInput);
        Debug.Log($"Player {playerInput.playerIndex} left with control scheme: {playerInput.currentControlScheme}");
        OnPlayerLeftEvent?.Invoke(playerInput);
    }

    public int GetPlayerCount() => m_RegisteredPlayers.Count;

    public void ClearPlayers()
    {
        m_RegisteredPlayers.Clear();
    }

    public void DisableJoining()
    {
        if (m_PlayerInputManager != null)
        {
            m_PlayerInputManager.enabled = false;
        }
    }

    public PlayerInput[] GetRegisteredPlayers()
    {
        return m_RegisteredPlayers.ToArray();
    }

    public GameObject CreatePlayerWithInput(int playerIndex, Vector3 spawnPosition)
    {
        if (playerIndex >= m_RegisteredPlayers.Count) return null;

        PlayerInput menuPlayerInput = m_RegisteredPlayers[playerIndex];
        GameObject beetleBubble = Instantiate(m_BeetleBubblePrefab, spawnPosition, Quaternion.identity);
        PlayerInput gamePlayerInput = beetleBubble.GetComponent<PlayerInput>();
            
        // Copy the device from the menu player input to the game player input
        gamePlayerInput.SwitchCurrentControlScheme(
            menuPlayerInput.currentControlScheme,
            menuPlayerInput.devices.First());

        return beetleBubble;
    }

    public void RespawnPlayer(PlayerInput playerInput)
    {
        if (m_PlayerSpawnPoints.TryGetValue(playerInput, out Vector3 spawnPosition))
        {
            playerInput.transform.position = spawnPosition;
            var beetleBubble = playerInput.GetComponent<BeetleBubble>();
            if (beetleBubble != null)
            {
                beetleBubble.ResetState();
            }
        }
    }

    public void RegisterSpawnPoint(PlayerInput playerInput, Vector3 spawnPosition)
    {
        if (!m_PlayerSpawnPoints.ContainsKey(playerInput))
        {
            m_PlayerSpawnPoints[playerInput] = spawnPosition;
        }
    }
    #endregion
} 
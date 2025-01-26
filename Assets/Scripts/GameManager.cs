using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
    [SerializeField] private Sprite[] m_BeetleSprites = new Sprite[4];
    
    private List<PlayerInput> m_RegisteredPlayers = new List<PlayerInput>();
    private Dictionary<PlayerInput, Vector3> m_PlayerSpawnPoints = new Dictionary<PlayerInput, Vector3>();
    private HashSet<PlayerInput> m_EliminatedPlayers = new HashSet<PlayerInput>();
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
            Debug.Log($"GameManager initialized. Registered players: {m_RegisteredPlayers?.Count ?? 0}");
            
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

    public Sprite GetBeetleSprite(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < m_BeetleSprites.Length)
        {
            return m_BeetleSprites[playerIndex];
        }
        return null;
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

    public void OnPlayerEliminated(PlayerInput playerInput)
    {
        if (!m_EliminatedPlayers.Contains(playerInput))
        {
            m_EliminatedPlayers.Add(playerInput);
            CheckWinCondition();
        }
    }

    private void CheckWinCondition()
    {
        int remainingPlayers = m_RegisteredPlayers.Count - m_EliminatedPlayers.Count;
        
        if (remainingPlayers <= 1)
        {
            StartCoroutine(LoadWinScene());
        }
    }

    private System.Collections.IEnumerator LoadWinScene()
    {
        yield return new WaitForSeconds(1f);
        
        // Destroy all player objects
        foreach (var player in m_RegisteredPlayers)
        {
            if (player != null)
            {
                Destroy(player.gameObject);
            }
        }
        
        SceneManager.LoadScene("WinScene");
    }

    public bool IsPlayerEliminated(PlayerInput playerInput)
    {
        return m_EliminatedPlayers.Contains(playerInput);
    }

    public void ResetGameState()
    {
        // Clear eliminated players
        m_EliminatedPlayers.Clear();
        
        // Destroy all player objects
        foreach (var player in m_RegisteredPlayers)
        {
            if (player != null)
            {
                Destroy(player.gameObject);
            }
        }
        m_RegisteredPlayers.Clear();
        m_PlayerSpawnPoints.Clear();
        
        // Re-enable player joining
        if (m_PlayerInputManager != null)
        {
            m_PlayerInputManager.enabled = true;
        }
        
        // Destroy the GameManager itself
        Destroy(gameObject);
    }

    public void EnablePlayerControls(PlayerInput playerInput, float delay = 0.5f)
    {
        StartCoroutine(EnablePlayerControlsRoutine(playerInput, delay));
    }

    private System.Collections.IEnumerator EnablePlayerControlsRoutine(PlayerInput playerInput, float delay)
    {
        Debug.Log($"Enabling player controls for player {playerInput.playerIndex} with delay {delay}");
        yield return new WaitForSeconds(delay);
        
        // Enable the Player action map
        playerInput.actions.FindActionMap("Player").Enable();
        Debug.Log($"Player controls enabled for player {playerInput.playerIndex}");
    }
    #endregion
} 
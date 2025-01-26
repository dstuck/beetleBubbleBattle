using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class StartScreenManager : MonoBehaviour
{
    #region Private Fields
    [Header("UI References")]
    [SerializeField] private GameObject[] m_PlayerSlots = new GameObject[4];
    [SerializeField] private TextMeshProUGUI[] m_JoinTexts = new TextMeshProUGUI[4];
    [SerializeField] private Image[] m_PlayerSprites = new Image[4];
    [SerializeField] private Image[] m_ChargeBubbles = new Image[4];
    [SerializeField] private Sprite m_DefaultBugSprite;
    
    [Header("Game Start Settings")]
    [SerializeField] private float m_ChargeTimeRequired = 1.0f;
    [SerializeField] private float m_MinBubbleScale = 0.8f;
    [SerializeField] private float m_MaxBubbleScale = 1.5f;
    [SerializeField] private Color m_BubbleColor = new Color(1f, 1f, 1f, 0.3f);
    
    private int m_PlayerCount = 0;
    private bool m_GameStarting = false;
    private float m_CurrentChargeTime = 0f;
    private Dictionary<PlayerInput, bool> m_PlayerChargingStates = new Dictionary<PlayerInput, bool>();
    private List<PlayerInput> m_RegisteredPlayers = new List<PlayerInput>();
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // Initialize all slots to show "Press A to Join"
        for (int i = 0; i < m_PlayerSlots.Length; i++)
        {
            m_JoinTexts[i].text = "Press A to Join";
            m_PlayerSprites[i].enabled = false;
            m_PlayerSprites[i].gameObject.SetActive(true);
        }

        GameManager.Instance.OnPlayerJoinedEvent += HandlePlayerJoined;
    }

    private void Update()
    {
        if (m_GameStarting) return;
        
        if (m_PlayerCount >= 2)
        {
            bool anyPlayerCharging = m_PlayerChargingStates.Any(kvp => kvp.Value);
            if (anyPlayerCharging)
            {
                m_CurrentChargeTime += Time.deltaTime;
                float chargeProgress = m_CurrentChargeTime / m_ChargeTimeRequired;
                UpdateChargeBubbles(chargeProgress);
                
                if (m_CurrentChargeTime >= m_ChargeTimeRequired)
                {
                    m_GameStarting = true;
                    StartGame();
                }
            }
            else
            {
                m_CurrentChargeTime = 0f;
                UpdateChargeBubbles(0f);
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerJoinedEvent -= HandlePlayerJoined;
            GameManager.Instance.OnPlayerLeftEvent -= HandlePlayerLeft;
        }

        // Clean up all subscriptions
        foreach (var player in m_RegisteredPlayers)
        {
            if (player != null)
            {
                player.actions["Charge"].performed -= OnChargePerformed;
                player.actions["Charge"].canceled -= OnChargePerformed;
            }
        }
    }
    #endregion

    #region Private Methods
    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        if (m_PlayerCount >= 4 || m_GameStarting) return;

        // Register player and subscribe to charge action
        m_RegisteredPlayers.Add(playerInput);
        m_PlayerChargingStates[playerInput] = false;
        
        // Debug log the action map and charge action
        var chargeAction = playerInput.actions["Charge"];
        
        if (chargeAction != null)
        {
            chargeAction.Enable();  // Make sure the action is enabled
            chargeAction.performed += OnChargePerformed;
            chargeAction.canceled += OnChargePerformed;
        }
        else
        {
            Debug.LogError($"Failed to find Charge action for Player {playerInput.playerIndex}");
        }

        // Update UI with the correct beetle sprite
        m_JoinTexts[m_PlayerCount].text = $"Player {m_PlayerCount + 1}";
        m_PlayerSprites[m_PlayerCount].enabled = true;
        m_PlayerSprites[m_PlayerCount].sprite = GameManager.Instance.GetBeetleSprite(m_PlayerCount);
        
        // Initialize charge bubble
        if (m_ChargeBubbles[m_PlayerCount] != null)
        {
            m_ChargeBubbles[m_PlayerCount].enabled = true;
            m_ChargeBubbles[m_PlayerCount].color = m_BubbleColor;
            m_ChargeBubbles[m_PlayerCount].transform.localScale = new Vector3(m_MinBubbleScale, m_MinBubbleScale, 1f);
        }
        
        m_PlayerCount++;

        // Update the join text to show ready prompt if we have 2+ players
        if (m_PlayerCount >= 2)
        {
            for (int i = 0; i < m_PlayerCount; i++)
            {
                m_JoinTexts[i].text = "Hold Charge to Start!";
            }
        }
    }

    private void HandlePlayerLeft(PlayerInput playerInput)
    {
        // Unsubscribe from charge action
        playerInput.actions["Charge"].performed -= OnChargePerformed;
        playerInput.actions["Charge"].canceled -= OnChargePerformed;
        m_RegisteredPlayers.Remove(playerInput);
    }

    public void OnChargePerformed(InputAction.CallbackContext context)
    {
        // Get the player input that triggered this action
        var playerInput = m_RegisteredPlayers.Find(p => p.actions == context.action.actionMap.asset);
        
        if (playerInput == null)
        {
            // If direct match fails, try matching by control scheme
            var controlScheme = context.action.actionMap.asset.name.Contains("Clone") ? "Keyboard" : "Gamepad";
            playerInput = m_RegisteredPlayers.Find(p => p.currentControlScheme == controlScheme);
        }
        
        if (playerInput == null)
        {
            Debug.LogError($"Could not find player for action {context.action.name}. " +
                         $"Action asset: {context.action.actionMap.asset.name}, " +
                         $"Registered players: {string.Join(", ", m_RegisteredPlayers.Select(p => $"P{p.playerIndex}:{p.currentControlScheme}"))}");
            return;
        }
        
        Debug.Log($"Charge input received from player {playerInput.playerIndex}: {context.phase}. " +
                 $"Registered players: {m_RegisteredPlayers.Count}, " +
                 $"Current charging states: {string.Join(", ", m_PlayerChargingStates.Select(kvp => $"P{kvp.Key.playerIndex}:{kvp.Value}"))}");
        
        if (context.performed && m_PlayerCount >= 2 && !m_GameStarting)
        {
            Debug.Log($"Player {playerInput.playerIndex} started charging");
            m_PlayerChargingStates[playerInput] = true;
        }
        else if (context.canceled)
        {
            Debug.Log($"Player {playerInput.playerIndex} canceled charging");
            m_PlayerChargingStates[playerInput] = false;
            m_CurrentChargeTime = 0f;
        }
    }

    private void StartGame()
    {
        // Reset all charging states before starting
        foreach (var player in m_RegisteredPlayers)
        {
            m_PlayerChargingStates[player] = false;
        }
        m_CurrentChargeTime = 0f;
        UpdateChargeBubbles(0f);
        
        GameManager.Instance.DisableJoining();
        SceneManager.LoadScene("GameScene");
    }

    private void UpdateChargeBubbles(float _chargeProgress)
    {
        for (int i = 0; i < m_PlayerCount; i++)
        {
            if (m_ChargeBubbles[i] != null)
            {
                // Find if this player is charging
                var player = m_RegisteredPlayers[i];
                bool isCharging = m_PlayerChargingStates.ContainsKey(player) && m_PlayerChargingStates[player];
                
                float scale = isCharging ? 
                    Mathf.Lerp(m_MinBubbleScale, m_MaxBubbleScale, _chargeProgress) : 
                    m_MinBubbleScale;
                    
                m_ChargeBubbles[i].transform.localScale = new Vector3(scale, scale, 1f);
                
                Color bubbleColor = m_BubbleColor;
                bubbleColor.a = isCharging ? 
                    m_BubbleColor.a * (0.5f + (_chargeProgress * 0.5f)) : 
                    m_BubbleColor.a * 0.5f;
                m_ChargeBubbles[i].color = bubbleColor;
            }
        }
    }
    #endregion
} 
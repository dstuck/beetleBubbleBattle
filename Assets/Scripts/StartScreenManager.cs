using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class StartScreenManager : MonoBehaviour
{
    #region Private Fields
    [Header("UI References")]
    [SerializeField] private GameObject[] m_PlayerSlots = new GameObject[4];
    [SerializeField] private TextMeshProUGUI[] m_JoinTexts = new TextMeshProUGUI[4];
    [SerializeField] private Image[] m_PlayerSprites = new Image[4];
    [SerializeField] private Sprite m_DefaultBugSprite;
    
    [Header("Game Start Settings")]
    [SerializeField] private float m_ChargeTimeRequired = 1.0f;
    
    private int m_PlayerCount = 0;
    private bool m_GameStarting = false;
    private float m_CurrentChargeTime = 0f;
    private bool m_IsCharging = false;
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
        if (m_IsCharging && !m_GameStarting && m_PlayerCount >= 2)
        {
            m_CurrentChargeTime += Time.deltaTime;
            Debug.Log($"Charging: {m_CurrentChargeTime}/{m_ChargeTimeRequired}");
            if (m_CurrentChargeTime >= m_ChargeTimeRequired)
            {
                Debug.Log("Starting game!");
                m_GameStarting = true;
                StartGame();
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
        var playerActions = new InputSystem_Actions();
        playerInput.actions["Charge"].performed += OnChargePerformed;
        playerInput.actions["Charge"].canceled += OnChargePerformed;

        // Update UI
        m_JoinTexts[m_PlayerCount].text = $"Player {m_PlayerCount + 1}";
        m_PlayerSprites[m_PlayerCount].enabled = true;
        m_PlayerSprites[m_PlayerCount].sprite = m_DefaultBugSprite;
        
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
        Debug.Log($"Charge input received: {context.phase}");
        
        if (context.performed && m_PlayerCount >= 2 && !m_GameStarting)
        {
            Debug.Log("Starting charge");
            m_IsCharging = true;
        }
        else if (context.canceled)
        {
            Debug.Log("Charge canceled");
            m_IsCharging = false;
            m_CurrentChargeTime = 0f;
        }
    }

    private void StartGame()
    {
        GameManager.Instance.DisableJoining();
        SceneManager.LoadScene("GameScene");
    }
    #endregion
} 
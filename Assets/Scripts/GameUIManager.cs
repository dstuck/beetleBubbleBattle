using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameUIManager : MonoBehaviour
{
    [System.Serializable]
    private class PlayerLivesUI
    {
        public Transform container;
        public Image[] lifeIcons;
    }

    [SerializeField] private PlayerLivesUI[] m_PlayerLivesUI = new PlayerLivesUI[4];
    [SerializeField] private GameObject m_LifeIconPrefab;
    [SerializeField] private int m_MaxLives = 3;

    private void Start()
    {
        InitializeLifeIcons();
        
        // Subscribe to player join events
        if (GameManager.Instance != null)
        {
            var players = GameManager.Instance.GetRegisteredPlayers();
            for (int i = 0; i < players.Length; i++)
            {
                SetupPlayerLives(i, players[i]);
            }
        }
    }

    private void InitializeLifeIcons()
    {
        for (int playerIndex = 0; playerIndex < m_PlayerLivesUI.Length; playerIndex++)
        {
            var livesUI = m_PlayerLivesUI[playerIndex];
            livesUI.lifeIcons = new Image[m_MaxLives];
            
            for (int life = 0; life < m_MaxLives; life++)
            {
                GameObject icon = Instantiate(m_LifeIconPrefab, livesUI.container);
                livesUI.lifeIcons[life] = icon.GetComponent<Image>();
            }
        }
    }

    private void SetupPlayerLives(int playerIndex, PlayerInput player)
    {
        if (playerIndex >= m_PlayerLivesUI.Length) return;

        // Set the sprite for all life icons
        Sprite beetleSprite = GameManager.Instance.GetBeetleSprite(playerIndex);
        foreach (var icon in m_PlayerLivesUI[playerIndex].lifeIcons)
        {
            icon.sprite = beetleSprite;
            icon.gameObject.SetActive(true);
        }

        // Subscribe to the player's BeetleBubble component for life updates
        var beetleBubble = player.GetComponent<BeetleBubble>();
        if (beetleBubble != null)
        {
            beetleBubble.OnLivesChanged += (livesRemaining) => UpdatePlayerLives(playerIndex, livesRemaining);
        }
    }

    private void UpdatePlayerLives(int playerIndex, int livesRemaining)
    {
        if (playerIndex >= m_PlayerLivesUI.Length) return;

        var livesUI = m_PlayerLivesUI[playerIndex];
        for (int i = 0; i < livesUI.lifeIcons.Length; i++)
        {
            livesUI.lifeIcons[i].gameObject.SetActive(i < livesRemaining);
        }
    }
} 
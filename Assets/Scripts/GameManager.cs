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
    private List<PlayerInput> m_RegisteredPlayers = new List<PlayerInput>();
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        if (!m_RegisteredPlayers.Contains(playerInput))
        {
            m_RegisteredPlayers.Add(playerInput);
            Debug.Log($"Player {playerInput.playerIndex} joined with control scheme: {playerInput.currentControlScheme} (using device: {playerInput.devices.FirstOrDefault()?.name ?? "none"})");
        }
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        m_RegisteredPlayers.Remove(playerInput);
        Debug.Log($"Player {playerInput.playerIndex} left with control scheme: {playerInput.currentControlScheme}");
    }

    public int GetPlayerCount() => m_RegisteredPlayers.Count;

    public void ClearPlayers()
    {
        m_RegisteredPlayers.Clear();
    }
    #endregion
} 
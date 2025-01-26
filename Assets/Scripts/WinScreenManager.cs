using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Linq;

public class WinScreenManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_WinnerText;
    [SerializeField] private float m_RestartDelay = 3f;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            var players = GameManager.Instance.GetRegisteredPlayers();
            var winner = players.FirstOrDefault(p => !GameManager.Instance.IsPlayerEliminated(p));
                
            if (winner != null)
            {
                m_WinnerText.text = $"Player {winner.playerIndex + 1} Wins!";
            }
        }

        StartCoroutine(RestartGameSequence());
    }

    private System.Collections.IEnumerator RestartGameSequence()
    {
        yield return new WaitForSeconds(m_RestartDelay);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameState();
        }
        
        SceneManager.LoadScene("StartScreen");
    }
} 
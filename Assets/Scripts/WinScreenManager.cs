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
            int winningPlayerIndex = GameManager.Instance.GetWinningPlayerIndex();
            if (m_WinnerText != null)
            {
                m_WinnerText.text = $"Player {winningPlayerIndex + 1} Wins!";
            }
            else
            {
                Debug.LogError("WinnerText reference is missing!");
            }
        }
        else
        {
            Debug.LogError("GameManager instance is null!");
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
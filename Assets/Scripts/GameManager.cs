using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Owns the coin display, win panel, and scene restart.</summary>
public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private TMP_Text coinText;
    [SerializeField] private GameObject winPanel;

    private int coins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
        coins = 0;
        RefreshCoinText();
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void AddCoin()
    {
        coins++;
        RefreshCoinText();
    }

    public void LevelComplete()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void RefreshCoinText()
    {
        if (coinText != null)
            coinText.text = $"Coins: {coins}";
    }
}

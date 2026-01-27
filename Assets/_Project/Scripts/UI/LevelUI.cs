using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Image radialProgressBar;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Level Complete")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TextMeshProUGUI completeText;
    [SerializeField] private Button nextLevelButton;

    [Header("Game Win")]
    [SerializeField] private GameObject winPanel;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameplayPanel; // Parent object for HUD
    [SerializeField] private Button resumeButton;

    private void Start()
    {
        // Setup button listeners
        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);

        // Hide overlays, show gameplay
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(true);
    }

    public void TogglePauseUI(bool isPaused)
    {
        if (pausePanel != null) pausePanel.SetActive(isPaused);
        if (gameplayPanel != null) gameplayPanel.SetActive(!isPaused);
    }

    private void OnResumeClicked()
    {
        LevelManager.Instance?.TogglePause();
    }

    public void UpdateProgress(float progress)
    {
        if (radialProgressBar != null)
        {
            radialProgressBar.fillAmount = Mathf.Clamp01(progress);
        }
    }

    public void UpdateLevelText(int levelIndex)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {levelIndex}";
        }
    }

    public void ShowLevelComplete(int levelIndex)
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            if (completeText != null)
            {
                completeText.text = $"Level {levelIndex} Complete!";
            }
        }
    }

    public void HideLevelComplete()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    public void ShowGameWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            // Optional: Hide other HUD elements
        }
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    private void OnNextLevelClicked()
    {
        // Notify LevelManager
        LevelManager.Instance?.StartNextLevel();
    }
}

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

    private void Start()
    {
        // Setup button listener
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        }

        // Hide panel initially
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
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

    private void OnNextLevelClicked()
    {
        // Notify LevelManager
        LevelManager.Instance?.StartNextLevel();
    }
}

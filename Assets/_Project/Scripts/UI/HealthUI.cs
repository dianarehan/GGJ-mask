using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple UI to display player health
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Image[] heartImages;           // Array of heart images
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private TextMeshProUGUI healthText;    // Optional text display

    [Header("Settings")]
    [SerializeField] private bool useHearts = true;
    [SerializeField] private bool useText = false;

    private void Start()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

        if (player != null)
        {
            player.OnHealthChanged += UpdateHealthDisplay;
            UpdateHealthDisplay(player.CurrentHealth, player.MaxHealth);
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealthDisplay;
        }
    }

    private void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        // Update heart images
        if (useHearts && heartImages != null && heartImages.Length > 0)
        {
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (i < maxHealth)
                {
                    heartImages[i].enabled = true;
                    heartImages[i].sprite = i < currentHealth ? fullHeartSprite : emptyHeartSprite;
                }
                else
                {
                    heartImages[i].enabled = false;
                }
            }
        }

        // Update text
        if (useText && healthText != null)
        {
            healthText.text = $"HP: {currentHealth}/{maxHealth}";
        }
    }
}

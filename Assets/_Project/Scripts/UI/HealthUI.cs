using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple UI to display player health using hearts, slider, or text
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    
    [Header("Hearts Display")]
    [SerializeField] private bool useHearts = false;
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    
    [Header("Slider Display")]
    [SerializeField] private bool useSlider = true;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;  // Optional: to change fill color based on health
    [SerializeField] private Gradient healthGradient;  // Optional: color gradient from low to full health
    
    [Header("Text Display")]
    [SerializeField] private bool useText = false;
    [SerializeField] private TextMeshProUGUI healthText;

    private void Start()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

        if (player != null)
        {
            player.OnHealthChanged += UpdateHealthDisplay;
            
            // Initialize slider
            if (healthSlider != null)
            {
                healthSlider.maxValue = player.MaxHealth;
                healthSlider.value = player.CurrentHealth;
            }
            
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
        // Update slider
        if (useSlider && healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            
            // Update fill color based on health percentage
            if (fillImage != null && healthGradient != null)
            {
                float healthPercent = (float)currentHealth / maxHealth;
                fillImage.color = healthGradient.Evaluate(healthPercent);
            }
        }
        
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
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
}


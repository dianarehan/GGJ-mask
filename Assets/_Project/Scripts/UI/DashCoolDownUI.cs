using UnityEngine;
using UnityEngine.UI;

public class DashCoolDownUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the Player GameObject here")]
    public ShipMovement player;

    [Tooltip("Drag the UI Image that is set to 'Filled' mode")]
    public Image cooldownFillImage;

    [Header("Visuals")]
    public Color readyColor = Color.white;
    public Color coolingDownColor = new Color(1, 1, 1, 0.5f);

    void Start()
    {
        if (player != null)
        {
            player.OnDashCooldownUpdated += UpdateDashUI;
        }

        // Start full (Ready)
        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount = 1f;
            cooldownFillImage.color = readyColor;
        }
    }

    void OnDestroy()
    {
        if (player != null)
        {
            player.OnDashCooldownUpdated -= UpdateDashUI;
        }
    }

    void UpdateDashUI(float currentTimer, float maxTime)
    {
        if (cooldownFillImage == null) return;

        if (currentTimer <= 0)
        {
            // Ready to fire
            cooldownFillImage.fillAmount = 1f;
            cooldownFillImage.color = readyColor;
        }
        else
        {
            // Cooling down (calculate percentage 0.0 to 1.0)
            float percentage = 1f - (currentTimer / maxTime);
            cooldownFillImage.fillAmount = percentage;
            cooldownFillImage.color = coolingDownColor;
        }
    }
}
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

    [Header("Transition Effect")]
    [SerializeField] private ParticleSystem transitionParticles;
    [SerializeField] private float transitionDuration = 2f;
    [SerializeField] private float scaleMultiplier = 5f;

    [Header("Game Over / Lose")]
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip sliceSound;
    [SerializeField] private ParticleSystem loseParticles;

    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSound;

    // ... (Start method remains same) ...

    private void Start()
    {
        // Setup button listeners
        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);

        // Hide overlays, show gameplay
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(true);
        
        if (transitionParticles != null) 
        {
            transitionParticles.Stop();
            transitionParticles.gameObject.SetActive(false);
        }
    }

    // ...

    public void ShowGameOver()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }
        if (gameplayPanel != null)
        {
            gameplayPanel.SetActive(false);
        }
        if (loseParticles != null)
        {
            loseParticles.Play();
        }
        
        StartCoroutine(PlaySliceSoundDelayed());
    }

    private System.Collections.IEnumerator PlaySliceSoundDelayed()
    {
        yield return new WaitForSecondsRealtime(1f);
        if (sfxSource != null && sliceSound != null)
        {
            sfxSource.PlayOneShot(sliceSound);
        }
    }

    private void OnRetryClicked()
    {
        if (buttonClickSound != null)
        {
            // Use PlayClipAtPoint to create a temporary source that survives the scene destruction
            AudioSource.PlayClipAtPoint(buttonClickSound, Camera.main.transform.position);
        }

        // Reload current scene
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
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

    [Header("Level Artifacts")]
    [SerializeField] private Image globalArtifactImage; // Single image filled across levels
    [SerializeField] private float fillDuration = 1.5f;

    public void AnimateArtifactFill(int levelIndex, int totalLevels)
    {
        if (globalArtifactImage != null)
        {
            float startFill = (float)levelIndex / totalLevels;
            float targetFill = (float)(levelIndex + 1) / totalLevels;
            StartCoroutine(FillImageRoutine(globalArtifactImage, startFill, targetFill));
        }
    }

    private System.Collections.IEnumerator FillImageRoutine(Image img, float start, float target)
    {
        float timer = 0f;
        img.fillAmount = start;
        
        while (timer < fillDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / fillDuration;
            // Smooth step ease
            t = t * t * (3f - 2f * t);
            
            img.fillAmount = Mathf.Lerp(start, target, t);
            yield return null;
        }
        img.fillAmount = target;
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
            StartCoroutine(PlayWinSequence());
        }
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    [Header("Win Sequence")]
    [SerializeField] private ParticleSystem winParticles;
    [SerializeField] private Image finalMaskImage; // The white mask in the middle
    [SerializeField] private Image movingArtifactImage; // A duplicate image inside WinPanel used for animation
    [SerializeField] private float moveDuration = 1.0f;
    [SerializeField] private float delayBeforeParticles = 0.2f; // Wait after arriving before bursting
    [SerializeField] private float particlePlayDuration = 2.0f; // Wait so particles can fly before we fade
    [SerializeField] private float fadeDuration = 1.0f;

    private System.Collections.IEnumerator PlayWinSequence()
    {
        // 0. STOP TIME & DISABLE PANELS IMMEDIATELY
        Time.timeScale = 0f;
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // Setup Initial State
        if (finalMaskImage != null)
        {
            finalMaskImage.canvasRenderer.SetAlpha(0f);
            // finalMaskImage.transform.localScale = Vector3.one; // Don't touch scale
            finalMaskImage.gameObject.SetActive(true);
        }

        // 1. Move "Moving Artifact" from HUD position to specific target (50,50)
        if (movingArtifactImage != null)
        {
            movingArtifactImage.gameObject.SetActive(true);
            
            // Sync to global artifact if available
            if (globalArtifactImage != null)
            {
                movingArtifactImage.transform.position = globalArtifactImage.transform.position;
                movingArtifactImage.sprite = globalArtifactImage.sprite;
                movingArtifactImage.fillAmount = globalArtifactImage.fillAmount;
            }
            
            RectTransform movingRect = movingArtifactImage.GetComponent<RectTransform>();
            Vector2 startAnchoredPos = movingRect.anchoredPosition;
            Vector2 targetAnchoredPos = new Vector2(50f, 50f); 
            
            float timer = 0f;
            while (timer < moveDuration)
            {
                timer += Time.unscaledDeltaTime;
                float t = timer / moveDuration;
                t = t * t * (3f - 2f * t);
                
                movingRect.anchoredPosition = Vector2.Lerp(startAnchoredPos, targetAnchoredPos, t);
                yield return null;
            }
            movingRect.anchoredPosition = targetAnchoredPos;
        }

        // Wait a small moment so we clearly see it "Arrive"
        if (delayBeforeParticles > 0f)
        {
             yield return new WaitForSecondsRealtime(delayBeforeParticles);
        }

        // 2. Play Particles & START FADING OUT BLACK MASK (Concurrent)
        if (winParticles != null)
        {
            winParticles.gameObject.SetActive(true); // User requested enabling it
            winParticles.Simulate(0, true, true); 
            winParticles.Play();
        }

        // Start fading out the moving artifact NOW (concurrently)
        StartCoroutine(FadeOutMovingArtifact());

        // Wait for particles to play out before switching to final mask
        if (particlePlayDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(particlePlayDuration);
        }
        
        // 3. Fade In Final (ALPHA ONLY) - Moving artifact should be gone by now
        float swapTimer = 0f;
        while (swapTimer < fadeDuration)
        {
            swapTimer += Time.unscaledDeltaTime;
            float t = swapTimer / fadeDuration;
            
            if (finalMaskImage != null)
            {
                finalMaskImage.canvasRenderer.SetAlpha(t);
            }
            
            yield return null;
        }
        
        if (finalMaskImage != null)
        {
            finalMaskImage.canvasRenderer.SetAlpha(1f);
        }
    }

    private System.Collections.IEnumerator FadeOutMovingArtifact()
    {
        if (movingArtifactImage == null) yield break;

        float timer = 0f;
        float duration = 0.5f; // Fast fade out
        Color startColor = movingArtifactImage.color;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;
            movingArtifactImage.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            yield return null;
        }
        movingArtifactImage.gameObject.SetActive(false);
        movingArtifactImage.color = startColor; // Reset for next time
    }

    [ContextMenu("Force Win Sequence")]
    public void DebugForceWin()
    {
        ShowGameWin();
    }

    private void OnNextLevelClicked()
    {
        if (sfxSource != null && buttonClickSound != null) sfxSource.PlayOneShot(buttonClickSound);

        if (transitionParticles != null)
        {
            StartCoroutine(PlayTransitionRoutine());
        }
        else
        {
            // Fallback if no particles
            LevelManager.Instance?.StartNextLevel();
        }
    }



    private System.Collections.IEnumerator PlayTransitionRoutine()
    {
        // 1. Hide the Next Level Panel immediately so we see the effect
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);

        // 2. Setup and Play Particles
        transitionParticles.gameObject.SetActive(true);
        transitionParticles.transform.localScale = Vector3.one; 
        transitionParticles.Play();

        // 3. Scale Up Animation
        float timer = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * scaleMultiplier;

        while (timer < transitionDuration)
        {
            // Use unscaled time because game is paused!
            timer += Time.unscaledDeltaTime;
            float t = timer / transitionDuration;
            
            // Smooth step ease
            t = t * t * (3f - 2f * t);
            
            transitionParticles.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        // 4. Cleanup and Notify Manager
        transitionParticles.Stop();
        transitionParticles.gameObject.SetActive(false);
        
        LevelManager.Instance?.StartNextLevel();
    }
}

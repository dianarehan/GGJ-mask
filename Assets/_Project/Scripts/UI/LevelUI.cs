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
    [SerializeField] private AudioClip levelLoadSound; // SFX for loading/transition
    [SerializeField] private float transitionDuration = 2f;
    [SerializeField] private float scaleMultiplier = 5f;

    [Header("Game Over / Lose")]
    [SerializeField] private Button retryButton;
    [SerializeField] private ParticleSystem loseParticles;
    [SerializeField] private GameObject maskObjectToHide; // User assigns the specific mask object here

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource; // For BGM / Win Music
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip sliceSound; // Played on Game Over
    [SerializeField] private AudioClip bgMusic;    // Background Music (Play on Start)
    [SerializeField] private AudioClip winMusic;   // Loop or track for Win Scene

    [Header("Menus")]
    [SerializeField] private GameObject losePanel;

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

        // Play BGM
        if (musicSource != null && bgMusic != null)
        {
            musicSource.clip = bgMusic;
            musicSource.loop = true;
            musicSource.Play();
            musicSource.volume = 0.5f;
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
        
        // Hide the specific mask object if assigned
        if (maskObjectToHide != null)
        {
            maskObjectToHide.SetActive(false);
        }
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
            AudioSource.PlayClipAtPoint(buttonClickSound, Camera.main.transform.position,1f);
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
    [SerializeField] private ParticleSystem fillEffectParticles; // Plays once when filled

    public void InitializeArtifact(int levelIndex, int totalLevels)
    {
        if (globalArtifactImage != null)
        {
            float fill = (float)levelIndex / totalLevels;
            globalArtifactImage.fillAmount = fill;
        }
    }

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
        // START PARTICLES
        if (fillEffectParticles != null) fillEffectParticles.Play();

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

        // STOP PARTICLES
        if (fillEffectParticles != null) fillEffectParticles.Stop();
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
            if (maskObjectToHide != null)
        {
            maskObjectToHide.SetActive(false);
        }
            StartCoroutine(PlayWinSequence());
        }
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    [Header("Win Sequence")]
    [SerializeField] private ParticleSystem winParticles;     // The first particles (Transition)
    [SerializeField] private ParticleSystem poofParticles;    // The second particles (Introduction/Poof)
    [SerializeField] private AudioClip poofSound;             // SFX for Poof
    [SerializeField] private Image finalMaskImage;            // The white mask in the middle
    [SerializeField] private Image movingArtifactImage;       // A duplicate image inside WinPanel used for animation
    [SerializeField] private float moveDuration = 1.0f;
    [SerializeField] private float delayBeforeParticles = 0.2f; 
    [SerializeField] private float particlePlayDuration = 2.0f; 
    [SerializeField] private float fadeDuration = 0.5f;       // Used for Pump duration now

    private System.Collections.IEnumerator PlayWinSequence()
    {
        // 0. STOP TIME & DISABLE PANELS IMMEDIATELY
        Time.timeScale = 0f;
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // PLAY WIN MUSIC
        if (musicSource != null && winMusic != null)
        {
            musicSource.Stop(); // Stop current BGM
            musicSource.clip = winMusic;
            musicSource.Play();
        }

        // Setup Initial State: Final Mask hidden by SCALE (Pump ready), Alpha is visible
        if (finalMaskImage != null)
        {
            finalMaskImage.canvasRenderer.SetAlpha(1f);
            finalMaskImage.transform.localScale = Vector3.zero; // Start small for pump
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
                t = t * t * (3f - 2f * t); // Smooth step
                
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

        // 2. Play Transition Particles & Fade Out Moving Artifact
        if (winParticles != null)
        {
            winParticles.gameObject.SetActive(true);
            winParticles.Simulate(0, true, true); 
            winParticles.Play();
        }

        StartCoroutine(FadeOutMovingArtifact());

        // Wait for transition particles to do their thing
        if (particlePlayDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(particlePlayDuration);
        }
        
        // 3. POOF & PUMP!
        
        // Play Poof Particles
        if (poofParticles != null)
        {
            if (sfxSource != null && poofSound != null) sfxSource.PlayOneShot(poofSound);
            
            poofParticles.gameObject.SetActive(true);
            // Don't auto-move. User places it.
            
            poofParticles.Simulate(0, true, true);
            poofParticles.Play();
        }

        // "Pump" Animation (Scale Up with bounce)
        float pumpTimer = 0f;
        while (pumpTimer < fadeDuration)
        {
            pumpTimer += Time.unscaledDeltaTime;
            float t = pumpTimer / fadeDuration;
            
            // Overshoot ease (BackOut)
            // c1 = 1.70158; c3 = c1 + 1; 
            // return 1 + c3 * (x - 1)^3 + c1 * (x - 1)^2;
            // Simplified Overshoot:
            float scale = Mathf.LerpUnclamped(0f, 1f, EaseOutBack(t));
            
            if (finalMaskImage != null)
            {
                finalMaskImage.transform.localScale = Vector3.one * scale;
            }
            
            yield return null;
        }
        
        if (finalMaskImage != null)
        {
            finalMaskImage.transform.localScale = Vector3.one;
        }
    }

    private float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
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
        if (sfxSource != null && levelLoadSound != null) sfxSource.PlayOneShot(levelLoadSound);

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

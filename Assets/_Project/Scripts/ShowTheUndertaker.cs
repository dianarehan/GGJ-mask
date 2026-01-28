using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowTheUndertaker : MonoBehaviour
{
    [Header("UI References")]
    public Image targetImage;
    public TextMeshProUGUI targetText;

    [Header("Global Settings")]
    [Tooltip("How fast the image fades in and out (applies to all slides)")]
    public float fadeDuration = 1.0f;

    [System.Serializable]
    public struct SlideData
    {
        [Tooltip("Leave empty if you just want to wait without an image")]
        public Sprite sprite;

        [TextArea]
        public string message;

        [Tooltip("How long THIS specific slide stays visible")]
        public float duration;
    }

    [Header("Slides List")]
    public List<SlideData> slides;

    private void Start()
    {
        if (slides.Count > 0)
        {
            // Ensure we start invisible
            Color c = targetImage.color;
            targetImage.color = new Color(c.r, c.g, c.b, 0f);

            StartCoroutine(PlaySlideshow());
        }
    }

    IEnumerator PlaySlideshow()
    {
        int index = 0;

        while (true)
        {
            SlideData currentSlide = slides[index];

            // --- Logic for "Blank Wait" vs "Image Slide" ---

            if (currentSlide.sprite == null)
            {
                // CASE 1: Blank Wait (No Image)
                // We assume Alpha is already 0 from the previous loop's FadeOut

                // Optional: Update text if you want text on a black screen
                targetText.text = currentSlide.message;

                // Just wait for the specific duration
                yield return new WaitForSeconds(currentSlide.duration);
            }
            else
            {
                // CASE 2: Standard Image Slide

                // 1. Setup Content
                targetImage.sprite = currentSlide.sprite;
                targetText.text = currentSlide.message;

                // 2. Fade IN
                yield return StartCoroutine(FadeImage(0f, 1f));

                // 3. Wait for THIS slide's specific duration
                yield return new WaitForSeconds(currentSlide.duration);

                // 4. Fade OUT
                yield return StartCoroutine(FadeImage(1f, 0f));
            }

            // Move to next index (Loop)
            index = (index + 1) % slides.Count;
        }
    }

    IEnumerator FadeImage(float startAlpha, float endAlpha)
    {
        float timeElapsed = 0f;
        Color currentColor = targetImage.color;

        while (timeElapsed < fadeDuration)
        {
            float t = timeElapsed / fadeDuration;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, t);

            targetImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        targetImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, endAlpha);
    }
}

using UnityEngine;

namespace Project.Scripts.Utilities
{
    public class SimpleAnimator : MonoBehaviour
    {
        public enum AnimationType
        {
            None,
            ShakePosition,      // Random shake
            RotateBackAndForth, // Pendulum-like rotation
            PulseScale          // Heartbeat-like scaling
        }

        [Header("Settings")]
        [SerializeField] private AnimationType animationType = AnimationType.RotateBackAndForth;
        [SerializeField] private float speed = 5.0f;
        
        [Tooltip("Rotation: Degrees \nScale: Multiplier (0.1 = 10%) \nShake: Distance units")]
        [SerializeField] private float magnitude = 15.0f; 
        
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool randomizeStart = false;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector3 initialScale;
        private RectTransform rectTransform;
        private float timeOffset;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            
            // Store initial values
            if (rectTransform != null)
            {
                initialPosition = rectTransform.anchoredPosition3D;
            }
            else
            {
                initialPosition = transform.localPosition;
            }
            
            initialRotation = transform.localRotation;
            initialScale = transform.localScale;

            if (randomizeStart)
            {
                timeOffset = Random.Range(0f, 100f);
            }
        }

        private void OnEnable()
        {
            // Optional: Reset on enable? usually good for UI
            if (rectTransform != null) initialPosition = rectTransform.anchoredPosition3D;
            else initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;
            initialScale = transform.localScale;
        }

        private void Update()
        {
            if (animationType == AnimationType.None) return;

            float time = (useUnscaledTime ? Time.unscaledTime : Time.time) + timeOffset;

            switch (animationType)
            {
                case AnimationType.RotateBackAndForth:
                    ApplyRotation(time);
                    break;
                case AnimationType.PulseScale:
                    ApplyPulse(time);
                    break;
                case AnimationType.ShakePosition:
                    ApplyShake(time);
                    break;
            }
        }

        private void ApplyRotation(float time)
        {
            // Sine wave for smooth back and forth
            float angle = Mathf.Sin(time * speed) * magnitude;
            // Rotate around Z axis (works for both 2D World and UI)
            transform.localRotation = initialRotation * Quaternion.Euler(0, 0, angle);
        }

        private void ApplyPulse(float time)
        {
            // Sine wave (-1 to 1) -> remapped to (1-mag) to (1+mag)
            float sine = Mathf.Sin(time * speed);
            float scaleFactor = 1.0f + (sine * magnitude);
            
            transform.localScale = initialScale * scaleFactor;
        }

        private void ApplyShake(float time)
        {
            // Perlin noise for more organic shake than random
            float x = (Mathf.PerlinNoise(time * speed, 0f) * 2f - 1f) * magnitude;
            float y = (Mathf.PerlinNoise(0f, time * speed) * 2f - 1f) * magnitude;

            Vector3 offset = new Vector3(x, y, 0);

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition3D = initialPosition + offset;
            }
            else
            {
                transform.localPosition = initialPosition + offset;
            }
        }
    }
}

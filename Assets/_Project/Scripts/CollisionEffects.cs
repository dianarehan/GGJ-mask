using UnityEngine;

/// <summary>
/// Handles collision VFX and SFX. Attach to Player or as a separate GameObject.
/// </summary>
public class CollisionEffects : MonoBehaviour
{
    public static CollisionEffects Instance { get; private set; }

    [Header("Particle System")]
    [SerializeField] private ParticleSystem collisionParticles;

    [Header("Colors")]
    [SerializeField] private Color enemyKillColor = Color.red;
    [SerializeField] private Color damageTakenColor = Color.yellow;
    [SerializeField] private Color wallHitColor = Color.white;
    [SerializeField] private Color projectileHitColor = Color.magenta;

    [Header("Audio")]
    [SerializeField] private AudioClip enemyKillSound;
    [SerializeField] private AudioClip damageTakenSound;
    [SerializeField] private AudioClip wallHitSound;
    [SerializeField] private AudioClip dashSound;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private AudioSource audioSource;
    private CartoonFX.CFXR_Effect cfxrEffect; // Reference to CFXR for camera shake reset

    public enum EffectType { EnemyKill, DamageTaken, WallHit, ProjectileHit, Dash }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Get CFXR_Effect if the particle system uses it
        if (collisionParticles != null)
        {
            cfxrEffect = collisionParticles.GetComponent<CartoonFX.CFXR_Effect>();
        }
    }

    /// <summary>
    /// Play collision effect with appropriate color and sound
    /// </summary>
    public void Play(EffectType type, Vector3 position)
    {
        Color color = GetColor(type);
        AudioClip sound = GetSound(type);

        // Play particles
        if (collisionParticles != null)
        {
            // Fully reset CFXR_Effect by toggling the GameObject off and on
            // This triggers OnDisable() which resets camera shake state
            collisionParticles.gameObject.SetActive(false);
            collisionParticles.gameObject.SetActive(true);
            
            var main = collisionParticles.main;
            main.startColor = color;
            collisionParticles.transform.position = position;
            collisionParticles.Play();
        }

        // Play sound
        if (sound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sound, sfxVolume);
        }
    }

    // Convenience methods
    public void PlayEnemyKill(Vector3 position) => Play(EffectType.EnemyKill, position);
    public void PlayDamageTaken(Vector3 position) => Play(EffectType.DamageTaken, position);
    public void PlayWallHit(Vector3 position) => Play(EffectType.WallHit, position);
    public void PlayProjectileHit(Vector3 position) => Play(EffectType.ProjectileHit, position);
    public void PlayDash(Vector3 position) => Play(EffectType.Dash, position);

    private Color GetColor(EffectType type)
    {
        return type switch
        {
            EffectType.EnemyKill => enemyKillColor,
            EffectType.DamageTaken => damageTakenColor,
            EffectType.WallHit => wallHitColor,
            EffectType.ProjectileHit => projectileHitColor,
            EffectType.Dash => enemyKillColor,
            _ => Color.white
        };
    }

    private AudioClip GetSound(EffectType type)
    {
        return type switch
        {
            EffectType.EnemyKill => enemyKillSound,
            EffectType.DamageTaken => damageTakenSound,
            EffectType.WallHit => wallHitSound,
            EffectType.ProjectileHit => damageTakenSound,
            EffectType.Dash => dashSound,
            _ => null
        };
    }
}

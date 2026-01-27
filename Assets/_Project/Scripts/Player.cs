using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float damageFlashSpeed = 10f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 15f;      // Speed during dash
    [SerializeField] private float dashDuration = 0.2f;  // How long the dash lasts
    [SerializeField] private float dashCooldown = 0.5f;  // Time between dashes

    [Header("Trail Settings")]
    [SerializeField] private bool useTrail = true;
    [SerializeField] private float trailTime = 0.3f;
    [SerializeField] private float trailStartWidth = 0.5f;
    [SerializeField] private float trailEndWidth = 0f;
    [SerializeField] private Color trailStartColor = new Color(0f, 1f, 1f, 1f);  // Cyan
    [SerializeField] private Color trailEndColor = new Color(0f, 1f, 1f, 0f);    // Transparent cyan

    [Header("Audio")]
    [SerializeField] private AudioClip dashSound;
    [Range(0f, 1f)]
    [SerializeField] private float dashVolume = 1f;

    // Events for UI/Game Manager
    public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public event Action OnPlayerDeath;

    // Health state
    private int currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // Current facing direction (supports 8 directions now)
    private Vector2 facingVector = Vector2.up;

    // Dash state
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float cooldownTimer = 0f;

    // Components
    private Rigidbody2D rb;
    private TrailRenderer trailRenderer;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Player requires a Rigidbody2D component!");
        }

        // Initialize health
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        SetupTrailRenderer();
    }

    private void SetupTrailRenderer()
    {
        if (!useTrail) return;

        // Get or create TrailRenderer
        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }

        // Configure trail settings
        trailRenderer.time = trailTime;
        trailRenderer.startWidth = trailStartWidth;
        trailRenderer.endWidth = trailEndWidth;
        trailRenderer.startColor = trailStartColor;
        trailRenderer.endColor = trailEndColor;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.emitting = false; // Only emit during dash
    }

    void Update()
    {
        HandleRotationInput();
        HandleDashInput();
        UpdateTimers();
        UpdateInvincibility();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            PerformDash();
        }
    }

    private void HandleRotationInput()
    {
        // Don't allow rotation while dashing
        if (isDashing) return;

        // Get input from both arrow keys and WASD
        float horizontal = 0f;
        float vertical = 0f;

        // Check horizontal input
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            horizontal = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            horizontal = -1f;

        // Check vertical input
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            vertical = 1f;
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            vertical = -1f;

        // Only update direction if there's input
        if (horizontal != 0f || vertical != 0f)
        {
            Vector2 inputDirection = new Vector2(horizontal, vertical).normalized;
            SetDirection(inputDirection);
        }
    }

    private void SetDirection(Vector2 direction)
    {
        facingVector = direction;

        // Calculate rotation angle from direction vector
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void HandleDashInput()
    {
        // Check for dash input (X key) and ensure not on cooldown
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && cooldownTimer <= 0f)
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;

        // Play dash sound
        if (dashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dashSound, dashVolume);
        }

        // Start trail effect
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
        }
    }

    private void PerformDash()
    {
        rb.linearVelocity = facingVector * dashSpeed;
    }

    private void UpdateTimers()
    {
        // Update dash timer
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }

        // Update cooldown timer
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void EndDash()
    {
        isDashing = false;
        rb.linearVelocity = Vector2.zero; // Stop the player after dash

        // Stop trail effect
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
    }

    private void UpdateInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;

        // Flash effect - oscillate between visible and semi-transparent
        if (spriteRenderer != null)
        {
            float alpha = Mathf.PingPong(Time.time * damageFlashSpeed, 1f) * 0.5f + 0.5f;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        if (invincibilityTimer <= 0f)
        {
            isInvincible = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    public void TakeDamage(int amount = 1)
    {
        // Can't take damage while dashing or invincible
        if (isDashing || isInvincible) return;

        currentHealth -= amount;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start invincibility frames
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
            Debug.Log($"Player took damage! Health: {currentHealth}/{maxHealth}");
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        OnPlayerDeath?.Invoke();
        // You can add death effects, respawn logic, or game over screen here
        gameObject.SetActive(false);
    }

    public void Heal(int amount = 1)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Handle wall collisions for VFX/SFX
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDashing) return;

        // Check if it's a wall/tilemap (not an enemy)
        if (collision.gameObject.GetComponent<EnemyBase>() != null) return;

        if (collision.gameObject.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() != null ||
            collision.gameObject.CompareTag("Wall"))
        {
            Vector3 hitPoint = collision.contacts.Length > 0 
                ? (Vector3)collision.contacts[0].point 
                : transform.position;
            CollisionEffects.Instance?.PlayWallHit(hitPoint);
        }
    }

    // Public getters for other scripts if needed
    public bool IsDashing => isDashing;
    public Vector2 FacingDirection => facingVector;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible => isInvincible;
}

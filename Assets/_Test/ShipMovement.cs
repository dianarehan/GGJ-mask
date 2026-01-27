using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ShipMovement : MonoBehaviour
{
    public enum ShipState
    {
        Normal,
        Dash
    }

    [Header("State Info")]
    public ShipState currentState = ShipState.Normal;

    [Header("Engine Settings (Normal)")]
    public float acceleration = 20f;
    public float maxSpeed = 10f;
    public float turnSpeed = 200f;

    [Header("Handling Settings (Normal)")]
    public float deceleration = 10f;
    [Range(0, 1)] public float driftFactor = 0.9f;

    [Header("Mask Ref")]
    public Mask mask;

    [Header("Dash Settings (Chaos)")]
    public float dashForce = 40f;
    public float dashDuration = 3f;
    [Tooltip("Time in seconds before you can dash again AFTER the dash finishes.")]
    public float dashCooldown = 2f; // NEW: The Reload Time
    public float minDashSpeed = 15f;
    public PhysicsMaterial2D normalMat;
    public PhysicsMaterial2D bouncyMat;

    [Header("Trail Settings")]
    [SerializeField] private bool useTrail = true;
    [SerializeField] private float trailTime = 0.3f;
    [SerializeField] private float trailStartWidth = 0.5f;
    [SerializeField] private float trailEndWidth = 0f;
    [SerializeField] private Color trailStartColor = new Color(0f, 1f, 1f, 1f);
    [SerializeField] private Color trailEndColor = new Color(0f, 1f, 1f, 0f);

    [Header("Dash Audio")]
    [SerializeField] private AudioClip dashSound;
    [Range(0f, 1f)]
    [SerializeField] private float dashVolume = 1f;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float damageFlashSpeed = 10f;

    // Events for UI/Game Manager
    public event Action<int, int> OnHealthChanged;
    public event Action OnPlayerDeath;
    // NEW: Event sends (CurrentTimer, MaxTime) for UI bars
    public event Action<float, float> OnDashCooldownUpdated;

    // Health state
    private int currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private Rigidbody2D rb;
    private Collider2D col;
    private TrailRenderer trailRenderer;
    private AudioSource audioSource;
    private float turnInput;
    private bool isGasPressed;
    private bool isDashPressed;
    private float dashTimer;

    // NEW: Cooldown tracking
    private float dashCooldownTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 2f;
    }

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

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

        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }

        trailRenderer.time = trailTime;
        trailRenderer.startWidth = trailStartWidth;
        trailRenderer.endWidth = trailEndWidth;
        trailRenderer.startColor = trailStartColor;
        trailRenderer.endColor = trailEndColor;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.emitting = false;
    }

    void Update()
    {
        turnInput = Input.GetAxisRaw("Horizontal");
        isGasPressed = Input.GetKey(KeyCode.W);
        isDashPressed = Input.GetKeyDown(KeyCode.Space);

        // --- NEW COOLDOWN LOGIC ---
        if (currentState == ShipState.Normal)
        {
            if (dashCooldownTimer > 0)
            {
                dashCooldownTimer -= Time.deltaTime;
                // Notify UI: (2.0, 2.0) -> (1.5, 2.0) -> (0, 2.0)
                OnDashCooldownUpdated?.Invoke(dashCooldownTimer, dashCooldown);
            }
        }

        // Only dash if Cooldown is 0
        if (isDashPressed && currentState == ShipState.Normal && dashCooldownTimer <= 0)
        {
            SwitchState(ShipState.Dash);
        }

        UpdateInvincibility();
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case ShipState.Normal:
                HandleNormalState();
                break;

            case ShipState.Dash:
                HandleDashState();
                break;
        }
    }

    void HandleNormalState()
    {
        if (turnInput != 0)
        {
            float forwardEffect = rb.linearVelocity.magnitude / maxSpeed;
            forwardEffect = Mathf.Clamp(forwardEffect, 0.3f, 1f);
            float newAngle = rb.rotation - (turnInput * turnSpeed * forwardEffect * Time.fixedDeltaTime);
            rb.MoveRotation(newAngle);
        }

        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        rb.linearVelocity = forwardVelocity + (rightVelocity * (1f - driftFactor));

        if (isGasPressed)
        {
            rb.AddRelativeForce(Vector2.up * acceleration);
        }
        else
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void HandleDashState()
    {
        dashTimer -= Time.fixedDeltaTime;

        if (dashTimer <= 0)
        {
            SwitchState(ShipState.Normal);
            return;
        }

        if (rb.linearVelocity.sqrMagnitude > 1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
            rb.MoveRotation(angle);
        }

        float currentSpeed = rb.linearVelocity.magnitude;
        if (currentSpeed < minDashSpeed)
        {
            if (currentSpeed > 0.1f)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * minDashSpeed;
            }
            else
            {
                rb.linearVelocity = transform.up * minDashSpeed;
            }
        }
    }

    public void SwitchState(ShipState newState)
    {
        currentState = newState;

        if (newState == ShipState.Dash)
        {
            dashTimer = dashDuration;

            rb.sharedMaterial = bouncyMat;
            col.sharedMaterial = bouncyMat;
            col.enabled = false; col.enabled = true;

            rb.linearDamping = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            rb.AddRelativeForce(Vector2.up * dashForce, ForceMode2D.Impulse);

            if (dashSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(dashSound, dashVolume);
            }

            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                trailRenderer.emitting = true;
            }
            if (mask != null) mask.MaskOn(dashDuration);
        }
        else if (newState == ShipState.Normal)
        {
            // --- NEW: START COOLDOWN ---
            dashCooldownTimer = dashCooldown;
            OnDashCooldownUpdated?.Invoke(dashCooldownTimer, dashCooldown); // Force UI update

            rb.sharedMaterial = normalMat;
            col.sharedMaterial = normalMat;
            col.enabled = false; col.enabled = true;

            rb.linearDamping = 0f;
            rb.angularDamping = 2f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.angularVelocity = 0f;

            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
            }
        }
    }

    public void ResetMovement()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        currentState = ShipState.Normal;
    }

    private void UpdateInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;

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
        if (currentState == ShipState.Dash || isInvincible) return;

        currentHealth -= amount;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }
    }

    private void Die()
    {
        OnPlayerDeath?.Invoke();
        gameObject.SetActive(false);
    }

    public void Heal(int amount = 1)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState != ShipState.Dash) return;
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

    public bool IsDashing => currentState == ShipState.Dash;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible => isInvincible;
}
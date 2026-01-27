using UnityEngine;

/// <summary>
/// Base class for all enemies. Handles common functionality like health, damage, and sprite setup.
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Base Enemy Settings")]
    [SerializeField] protected float health = 1f;
    [SerializeField] protected float damage = 1f;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    protected Player player;
    protected bool isAlive = true;

    protected virtual void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Start()
    {
        // Find the player in the scene
        player = FindFirstObjectByType<Player>();
        if (player == null)
        {
            Debug.LogWarning($"{gameObject.name}: No Player found in scene!");
        }
    }

    protected virtual void Update()
    {
        if (!isAlive) return;
        EnemyBehavior();
    }

    /// <summary>
    /// Override this method to define enemy-specific behavior
    /// </summary>
    protected abstract void EnemyBehavior();

    public virtual void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isAlive = false;
        // Add death effects here (particles, sound, etc.)
        Destroy(gameObject);
    }

    // Works with trigger colliders
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerCollision(other.GetComponent<Player>());
    }

    // Works with non-trigger colliders (for wall collision support)
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject.GetComponent<Player>());
    }

    private void HandlePlayerCollision(Player hitPlayer)
    {
        if (hitPlayer == null) return;

        if (hitPlayer.IsDashing)
        {
            // Player dashed into us - we die, show kill effect
            CollisionEffects.Instance?.PlayEnemyKill(transform.position);
            TakeDamage(999f);
        }
        else
        {
            // Player walked into us - they take damage
            OnPlayerContact(hitPlayer);
        }
    }

    protected virtual void OnPlayerContact(Player hitPlayer)
    {
        // Show damage effect and deal damage
        CollisionEffects.Instance?.PlayDamageTaken(transform.position);
        hitPlayer.TakeDamage((int)damage);
    }

    /// <summary>
    /// Gets the direction towards the player
    /// </summary>
    protected Vector2 GetDirectionToPlayer()
    {
        if (player == null) return Vector2.zero;
        return ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
    }

    /// <summary>
    /// Gets the distance to the player
    /// </summary>
    protected float GetDistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector2.Distance(transform.position, player.transform.position);
    }
}

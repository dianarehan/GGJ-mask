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

    protected Transform playerTransform;
    protected bool isAlive = true;

    protected virtual void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Start()
    {
        // Find the player in the scene by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No object with tag 'Player' found in scene!");
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
        HandlePlayerCollision(other.gameObject);
    }

    // Works with non-trigger colliders (for wall collision support)
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject);
    }

    private void HandlePlayerCollision(GameObject hitObject)
    {
        // Try getting Player component
        Player hitPlayer = hitObject.GetComponent<Player>();
        if (hitPlayer != null)
        {
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
            return;
        }

        // Try getting ShipMovement component
        ShipMovement hitShip = hitObject.GetComponent<ShipMovement>();
        if (hitShip != null)
        {
            if (hitShip.IsDashing)
            {
                // Ship dashed into us - we die
                CollisionEffects.Instance?.PlayEnemyKill(transform.position);
                TakeDamage(999f);
            }
            else
            {
                // Ship walked into us - they take damage
                CollisionEffects.Instance?.PlayDamageTaken(transform.position);
                hitShip.TakeDamage((int)damage);
            }
        }
    }

    protected virtual void OnPlayerContact(Player hitPlayer)
    {
        // Show damage effect and deal damage
        CollisionEffects.Instance?.PlayDamageTaken(transform.position);
        hitPlayer.TakeDamage((int)damage);
    }

    /// <summary>
    /// Gets the direction towards the player (supports both Player and ShipMovement)
    /// </summary>
    protected Vector2 GetDirectionToPlayer()
    {
        if (playerTransform != null) return (playerTransform.position - transform.position).normalized;
            
        return Vector2.zero;
    }

    protected float GetDistanceToPlayer()
    {
        if (playerTransform != null) return Vector2.Distance(transform.position, playerTransform.position);
        return float.MaxValue;
    }
}


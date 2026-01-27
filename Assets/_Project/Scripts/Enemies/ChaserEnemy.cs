using UnityEngine;

/// <summary>
/// Enemy Type 2: Slowly follows the player
/// Suggested sprites: Sad/Melancholic masks (mask_02, mask_03, mask_08, mask_09, mask_19 - the blue/sad ones)
/// </summary>
public class ChaserEnemy : EnemyBase
{
    [Header("Chaser Settings")]
    [SerializeField] private float moveSpeed = 1.5f;          // Slower than player
    [SerializeField] private float detectionRange = 15f;      // Range to start chasing
    [SerializeField] private float stopDistance = 0.5f;       // Don't get too close

    private Rigidbody2D rb;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    protected override void EnemyBehavior()
    {
        if (player == null) return;

        float distanceToPlayer = GetDistanceToPlayer();

        // Only chase if within detection range and not too close
        if (distanceToPlayer <= detectionRange && distanceToPlayer > stopDistance)
        {
            Vector2 direction = GetDirectionToPlayer();

            // Move towards player
            rb.linearVelocity = direction * moveSpeed;

            // Rotate to face the player
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            // Stop moving if too close or out of range
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Visualize detection range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}

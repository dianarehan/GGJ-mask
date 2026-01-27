using UnityEngine;

/// <summary>
/// Enemy Type 1: Wanders randomly around the scene
/// Suggested sprites: Confused/Neutral masks (mask_11, mask_12, mask_22, mask_33 - the sideways/neutral ones)
/// </summary>
public class WandererEnemy : EnemyBase
{
    [Header("Wanderer Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float directionChangeInterval = 2f;
    [SerializeField] private float minDirectionChangeInterval = 1f;
    [SerializeField] private float maxDirectionChangeInterval = 3f;

    private Vector2 currentDirection;
    private float directionTimer;
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

    protected override void Start()
    {
        base.Start();
        PickNewDirection();
    }

    protected override void EnemyBehavior()
    {
        // Update timer
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            PickNewDirection();
        }

        // Move in current direction
        rb.linearVelocity = currentDirection * moveSpeed;

        // Rotate to face movement direction
        if (currentDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void PickNewDirection()
    {
        // Pick a random direction
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        currentDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

        // Randomize the next direction change time
        directionTimer = Random.Range(minDirectionChangeInterval, maxDirectionChangeInterval);
    }

    // Override to handle both player collision AND wall bouncing
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // First, check for player collision (from base class)
        base.OnCollisionEnter2D(collision);

        // Then handle wall bouncing
        if (!collision.gameObject.CompareTag("Player") && collision.contacts.Length > 0)
        {
            Vector2 normal = collision.contacts[0].normal;
            currentDirection = Vector2.Reflect(currentDirection, normal);
        }
    }
}

using UnityEngine;

/// <summary>
/// Enemy Type 4: Moves towards the player AND throws projectiles
/// Suggested sprites: Evil/Intense masks (mask_21, mask_31, mask_32, mask_39, mask_40 - the evil looking ones)
/// </summary>
public class HunterEnemy : EnemyBase
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseRange = 12f;
    [SerializeField] private float preferredDistance = 5f; // Tries to keep this distance while shooting

    [Header("Shooting Settings")]
    [SerializeField] private float shootRange = 8f;
    [SerializeField] private float shootInterval = 1.5f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Behavior")]
    [SerializeField] private bool retreatWhenTooClose = true;

    private Rigidbody2D rb;
    private Transform firePoint;
    private float shootTimer;

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

        // Create fire point
        GameObject fp = new GameObject("FirePoint");
        fp.transform.SetParent(transform);
        fp.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        firePoint = fp.transform;
    }

    protected override void Start()
    {
        base.Start();
        shootTimer = shootInterval * 0.5f; // Start with a slight delay
    }

    protected override void EnemyBehavior()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = GetDistanceToPlayer();
        Vector2 direction = GetDirectionToPlayer();

        // Always face the player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Movement logic
        if (distanceToPlayer <= chaseRange)
        {
            if (distanceToPlayer > preferredDistance)
            {
                // Move towards player
                rb.linearVelocity = direction * moveSpeed;
            }
            else if (retreatWhenTooClose && distanceToPlayer < preferredDistance * 0.5f)
            {
                // Retreat if too close
                rb.linearVelocity = -direction * moveSpeed * 0.5f;
            }
            else
            {
                // At preferred distance, strafe
                Vector2 perpendicular = new Vector2(-direction.y, direction.x);
                rb.linearVelocity = perpendicular * moveSpeed * 0.5f;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Shooting logic
        if (distanceToPlayer <= shootRange)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                Shoot();
                shootTimer = shootInterval;
            }
        }
    }

    private void Shoot()
    {
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, transform.rotation);
            Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
            if (projectileRb != null)
            {
                projectileRb.linearVelocity = GetDirectionToPlayer() * projectileSpeed;
            }
            
            // Ignore collision with self
            Collider2D projectileCol = projectile.GetComponent<Collider2D>();
            Collider2D myCol = GetComponent<Collider2D>();
            if (projectileCol != null && myCol != null)
            {
                Physics2D.IgnoreCollision(projectileCol, myCol);
            }
        }
        else
        {
            CreateDefaultProjectile();
        }
    }

    private void CreateDefaultProjectile()
    {
        GameObject projectile = new GameObject("Projectile");
        projectile.transform.position = firePoint.position;

        SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(1f, 0.5f, 0f); // Orange
        projectile.transform.localScale = Vector3.one * 0.25f;

        Rigidbody2D prb = projectile.AddComponent<Rigidbody2D>();
        prb.gravityScale = 0f;
        prb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Use trigger so it doesn't push things
        CircleCollider2D col = projectile.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        // Ignore collision with self
        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol != null)
        {
            Physics2D.IgnoreCollision(col, myCol);
        }

        projectile.AddComponent<EnemyProjectile>();

        prb.linearVelocity = GetDirectionToPlayer() * projectileSpeed;
    }

    private Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance < radius ? Color.white : Color.clear);
            }
        }
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, preferredDistance);
    }
}

using UnityEngine;

/// <summary>
/// Enemy Type 3: Stationary enemy that throws projectiles at the player
/// Suggested sprites: Angry/Aggressive masks (mask_00, mask_05, mask_06, mask_17, mask_18 - the red angry ones)
/// </summary>
public class TurretEnemy : EnemyBase
{
    [Header("Turret Settings")]
    [SerializeField] private float shootRange = 10f;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Projectile Spawn")]
    [SerializeField] private Transform firePoint;

    private float shootTimer;
    private Rigidbody2D rb;

    protected override void Awake()
    {
        base.Awake();
        // Need Rigidbody2D for collision detection
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic; // Stationary
        }
    }

    protected override void Start()
    {
        base.Start();
        shootTimer = shootInterval;

        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            firePoint = fp.transform;
        }
    }

    protected override void EnemyBehavior()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = GetDistanceToPlayer();

        // Always face the player
        Vector2 direction = GetDirectionToPlayer();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Shoot if in range
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
        if (projectilePrefab == null)
        {
            CreateDefaultProjectile();
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, transform.rotation);
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.linearVelocity = GetDirectionToPlayer() * projectileSpeed;
        }
        
        // Ignore collision between turret and its own projectile
        Collider2D projectileCol = projectile.GetComponent<Collider2D>();
        Collider2D turretCol = GetComponent<Collider2D>();
        if (projectileCol != null && turretCol != null)
        {
            Physics2D.IgnoreCollision(projectileCol, turretCol);
        }
    }

    private void CreateDefaultProjectile()
    {
        // Create a simple circle projectile
        GameObject projectile = new GameObject("Projectile");
        projectile.transform.position = firePoint.position;

        // Add sprite
        SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = Color.red;
        projectile.transform.localScale = Vector3.one * 0.3f;

        // Add physics
        Rigidbody2D prb = projectile.AddComponent<Rigidbody2D>();
        prb.gravityScale = 0f;
        prb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Add collider as TRIGGER so it doesn't push things
        CircleCollider2D col = projectile.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        // Ignore collision with turret
        Collider2D turretCol = GetComponent<Collider2D>();
        if (turretCol != null)
        {
            Physics2D.IgnoreCollision(col, turretCol);
        }

        // Add projectile behavior
        projectile.AddComponent<EnemyProjectile>();

        // Set velocity
        prb.linearVelocity = GetDirectionToPlayer() * projectileSpeed;
    }

    private Sprite CreateCircleSprite()
    {
        // Create a simple white circle texture
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance < radius)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}

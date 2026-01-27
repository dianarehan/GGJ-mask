using UnityEngine;

/// <summary>
/// Projectile behavior for enemy attacks
/// Uses trigger collider to detect hits without pushing
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;

    private float timer;

    private void Start()
    {
        timer = lifetime;
        
        // Ensure collider is set as trigger (doesn't push things)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for player hit
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            if (player.IsDashing)
            {
                // Dashing through projectile - destroy it, show effect
                CollisionEffects.Instance?.PlayEnemyKill(transform.position);
                Destroy(gameObject);
                return;
            }

            // Player hit by projectile - show effect and damage
            CollisionEffects.Instance?.PlayProjectileHit(transform.position);
            player.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Destroy on hitting walls/tilemaps
        if (other.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() != null ||
            other.CompareTag("Wall") ||
            other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            // Don't destroy on enemies or other projectiles
            if (!other.CompareTag("Enemy") && other.GetComponent<EnemyProjectile>() == null)
            {
                Destroy(gameObject);
            }
        }
    }
}




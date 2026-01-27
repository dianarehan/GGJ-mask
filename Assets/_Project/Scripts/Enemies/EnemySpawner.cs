using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns waves of enemies over time within defined bounds
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        public int weight = 1; // Higher weight = more likely to spawn
    }

    [Header("Spawn Settings")]
    [SerializeField] private List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    [SerializeField] private int maxEnemies = 20;
    [SerializeField] private float minDistanceFromPlayer = 3f;

    [Header("Spawn Area")]
    [Tooltip("Drag your floor/ground Tilemap here to auto-detect bounds")]
    [SerializeField] private UnityEngine.Tilemaps.Tilemap spawnTilemap;
    
    [Tooltip("Or drag any Collider2D to use its bounds")]
    [SerializeField] private Collider2D spawnAreaCollider;
    
    [Tooltip("Or manually set spawn bounds (only used if above are empty)")]
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-8f, -4f);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(8f, 4f);
    
    [Tooltip("Shrink spawn area from edges by this amount")]
    [SerializeField] private float edgePadding = 0.5f;

    [Header("Wave Settings")]
    [SerializeField] private float timeBetweenSpawns = 2f;
    [SerializeField] private float spawnRateDecrease = 0.1f;
    [SerializeField] private float minimumSpawnTime = 0.5f;
    [SerializeField] private int enemiesPerWave = 3;
    [SerializeField] private float waveInterval = 10f;

    [Header("Difficulty Scaling")]
    [SerializeField] private bool scaleDifficulty = true;
    [SerializeField] private float difficultyIncreaseInterval = 30f;

    private Player player;
    private float spawnTimer;
    private float waveTimer;
    private float difficultyTimer;
    private int currentEnemyCount;
    private float currentSpawnRate;
    private int currentWave = 0;
    private Bounds spawnBounds;

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        currentSpawnRate = timeBetweenSpawns;
        spawnTimer = 0f;
        waveTimer = waveInterval;

        // Set up spawn bounds - priority: Tilemap > Collider > Manual
        if (spawnTilemap != null)
        {
            spawnTilemap.CompressBounds();
            spawnBounds = spawnTilemap.localBounds;
            // Convert to world space
            spawnBounds.center = spawnTilemap.transform.TransformPoint(spawnBounds.center);
            Debug.Log($"EnemySpawner: Using Tilemap bounds");
        }
        else if (spawnAreaCollider != null)
        {
            spawnBounds = spawnAreaCollider.bounds;
            Debug.Log($"EnemySpawner: Using Collider bounds");
        }
        else
        {
            Vector3 center = (Vector3)(spawnAreaMin + spawnAreaMax) / 2f;
            Vector3 size = new Vector3(spawnAreaMax.x - spawnAreaMin.x, spawnAreaMax.y - spawnAreaMin.y, 1f);
            spawnBounds = new Bounds(center, size);
            Debug.Log($"EnemySpawner: Using manual bounds");
        }

        // Apply padding
        spawnBounds.Expand(-edgePadding * 2f);

        Debug.Log($"EnemySpawner: Final spawn area Min:{spawnBounds.min} Max:{spawnBounds.max}");
    }

    private void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy) return;

        // Update spawn timer
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && currentEnemyCount < maxEnemies)
        {
            SpawnEnemy();
            spawnTimer = currentSpawnRate;
        }

        // Update wave timer
        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0f)
        {
            SpawnWave();
            waveTimer = waveInterval;
        }

        // Update difficulty
        if (scaleDifficulty)
        {
            difficultyTimer += Time.deltaTime;
            if (difficultyTimer >= difficultyIncreaseInterval)
            {
                IncreaseDifficulty();
                difficultyTimer = 0f;
            }
        }
    }

    private void SpawnEnemy()
    {
        if (enemyTypes.Count == 0) return;

        GameObject enemyPrefab = GetRandomEnemyPrefab();
        if (enemyPrefab == null) return;

        Vector2? spawnPos = GetValidSpawnPosition();
        if (!spawnPos.HasValue) return; // Couldn't find valid position

        GameObject enemy = Instantiate(enemyPrefab, spawnPos.Value, Quaternion.identity);
        enemy.tag = "Enemy";
        currentEnemyCount++;

        StartCoroutine(TrackEnemy(enemy));
    }

    private void SpawnWave()
    {
        currentWave++;
        Debug.Log($"Wave {currentWave} starting!");

        for (int i = 0; i < enemiesPerWave; i++)
        {
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    private GameObject GetRandomEnemyPrefab()
    {
        int totalWeight = 0;
        foreach (var enemy in enemyTypes)
        {
            totalWeight += enemy.weight;
        }

        int random = Random.Range(0, totalWeight);
        int current = 0;
        foreach (var enemy in enemyTypes)
        {
            current += enemy.weight;
            if (random < current)
            {
                return enemy.enemyPrefab;
            }
        }

        return enemyTypes[0].enemyPrefab;
    }

    private Vector2? GetValidSpawnPosition()
    {
        Vector2 playerPos = player.transform.position;
        const int maxAttempts = 50;

        for (int i = 0; i < maxAttempts; i++)
        {
            // Random position within spawn bounds
            float x = Random.Range(spawnBounds.min.x, spawnBounds.max.x);
            float y = Random.Range(spawnBounds.min.y, spawnBounds.max.y);
            Vector2 spawnPos = new Vector2(x, y);

            // Check if far enough from player
            if (Vector2.Distance(spawnPos, playerPos) >= minDistanceFromPlayer)
            {
                return spawnPos;
            }
        }

        // If we couldn't find a position far from player, just spawn at edge
        float edgeX = Random.value > 0.5f ? spawnBounds.min.x + 0.5f : spawnBounds.max.x - 0.5f;
        float edgeY = Random.Range(spawnBounds.min.y, spawnBounds.max.y);
        return new Vector2(edgeX, edgeY);
    }

    private System.Collections.IEnumerator TrackEnemy(GameObject enemy)
    {
        while (enemy != null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        currentEnemyCount--;
    }

    private void IncreaseDifficulty()
    {
        currentSpawnRate = Mathf.Max(minimumSpawnTime, currentSpawnRate - spawnRateDecrease);
        enemiesPerWave++;
        Debug.Log($"Difficulty increased! Spawn rate: {currentSpawnRate:F1}s, Enemies per wave: {enemiesPerWave}");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw spawn area
        Gizmos.color = Color.green;
        if (spawnAreaCollider != null)
        {
            Gizmos.DrawWireCube(spawnAreaCollider.bounds.center, spawnAreaCollider.bounds.size);
        }
        else
        {
            Vector3 center = (Vector3)(spawnAreaMin + spawnAreaMax) / 2f;
            Vector3 size = new Vector3(spawnAreaMax.x - spawnAreaMin.x, spawnAreaMax.y - spawnAreaMin.y, 0.1f);
            Gizmos.DrawWireCube(center, size);
        }
    }

    public void StopSpawning() => enabled = false;
    public void StartSpawning() => enabled = true;

    public void ClearAllEnemies()
    {
        foreach (var enemy in FindObjectsByType<EnemyBase>(FindObjectsSortMode.None))
        {
            Destroy(enemy.gameObject);
        }
        currentEnemyCount = 0;
    }

    public int CurrentWave => currentWave;
    public int EnemyCount => currentEnemyCount;
}


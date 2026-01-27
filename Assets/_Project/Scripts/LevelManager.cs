using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [System.Serializable]
    public class LevelConfig
    {
        public string levelName;
        public int killsToWin;
        public int maxActiveEnemies;
        public float spawnInterval;
        public List<EnemySpawner.EnemySpawnData> enemyTypes;
    }

    [Header("Level Configuration")]
    [SerializeField] private List<LevelConfig> levels;
    
    [Header("References")]
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private LevelUI levelUI;

    private int currentLevelIndex = 0;
    private static int savedLevelIndex = 0; // Static to persist across reloads
    private int currentKills = 0;
    private bool isLevelActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (spawner == null) spawner = FindFirstObjectByType<EnemySpawner>();
        if (levelUI == null) levelUI = FindFirstObjectByType<LevelUI>();

        // Subscribe to enemy death event
        EnemyBase.OnEnemyDeath += OnEnemyKilled;

        // Subscribe to Player/Ship death
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var playerScript = playerObj.GetComponent<Player>();
            if (playerScript != null) playerScript.OnPlayerDeath += OnPlayerDeath;

            var shipScript = playerObj.GetComponent<ShipMovement>();
            if (shipScript != null) shipScript.OnPlayerDeath += OnPlayerDeath;
        }

        // Start the saved level (defaults to 0 on first run)
        StartLevel(savedLevelIndex);
    }

    // Call this when completing a level to save progress
    private void SaveLevelProgress(int nextLevel)
    {
        savedLevelIndex = nextLevel;
    }
    
    private void OnPlayerDeath()
    {
        Debug.Log("Player Died - Game Over");
        isLevelActive = false;
        spawner.StopSpawning();
        // Don't clear enemies so they stay on screen as you die
        
        levelUI?.ShowGameOver();
        Time.timeScale = 0f;
    }

    private void OnDestroy()
    {
        EnemyBase.OnEnemyDeath -= OnEnemyKilled;
        
        // Unsubscribe? (Optional as Manager is destroyed usually on scene reload)
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isLevelActive || Time.timeScale == 0f) // Allow unpausing even if time is 0
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        bool isPaused = Time.timeScale == 0f;
        
        if (isPaused)
        {
            // Resume
            Time.timeScale = 1f;
            levelUI?.TogglePauseUI(false);
        }
        else
        {
            // Pause
            Time.timeScale = 0f;
            levelUI?.TogglePauseUI(true);
        }
    }

    public void StartLevel(int index)
    {
        if (index >= levels.Count)
        {
            Debug.Log("Game Win! No more levels.");
            return;
        }

        currentLevelIndex = index;
        savedLevelIndex = index; // Update saved index (so retry works on this level)
        
        LevelConfig config = levels[currentLevelIndex];
        
        // Reset state
        currentKills = 0;
        isLevelActive = true;
        Time.timeScale = 1f;

        // Reset Player Velocity/Inertia
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var ship = playerObj.GetComponent<ShipMovement>();
            if (ship != null) ship.ResetMovement();
            
            playerObj.transform.position = Vector3.zero; // Reset position too
        }

        // update UI
        levelUI?.HideLevelComplete();
        levelUI?.UpdateLevelText(currentLevelIndex + 1);
        levelUI?.UpdateProgress(0f);

        // Configure Spawner
        spawner.SetLevelData(config.enemyTypes, config.maxActiveEnemies, config.spawnInterval);
        spawner.StartSpawning();

        Debug.Log($"Started Level {currentLevelIndex + 1}: {config.levelName}");
    }

    private void OnEnemyKilled()
    {
        if (!isLevelActive) return;

        currentKills++;
        LevelConfig config = levels[currentLevelIndex];

        // Update UI
        float progress = (float)currentKills / config.killsToWin;
        levelUI?.UpdateProgress(progress);

        // Check Win Condition
        if (currentKills >= config.killsToWin)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        isLevelActive = false;
        spawner.StopSpawning();
        spawner.ClearAllEnemies();

        Debug.Log($"Level {currentLevelIndex + 1} Complete!");
        
        // Trigger Artifact Fill (fill piece for this level)
        levelUI?.AnimateArtifactFill(currentLevelIndex, levels.Count);

        // check if this was the last level
        if (currentLevelIndex >= levels.Count - 1)
        {
             // TODO: Show Game Win Panel
             Debug.Log("GAME COMPLETED! Trigger Win Screen here.");
             levelUI?.ShowGameWin(); // New method for Win screen
        }
        else
        {
            // Show UI and Pause
            levelUI?.ShowLevelComplete(currentLevelIndex + 1);
        }

        Time.timeScale = 0f; // Pause game
    }

    public void StartNextLevel()
    {
        StartLevel(currentLevelIndex + 1);
    }
}

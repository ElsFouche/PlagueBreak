using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Els Fouche
/// 
/// This script handles enemy logic. 
/// It spawns enemies on wave start, assigning them a random mesh
/// from the pool of available meshes, etc.
/// </summary>
public class EnemyHandler : MonoBehaviour
{
    // Designer 
    [Header("Enemy Waves")]
    [SerializeField] private int numWaves = 1;
    [SerializeField] private List<Vector3> spawnPoints  = new();
    [Header("Enemies")]
    [SerializeField] private List<F_EnemyData> enemies = new();

    [SerializeField] private RectTransform waveHealthBar;
    [SerializeField] private TMP_Text waveCount;


    [Header("Enemy Appearance")]
    [SerializeField] private List<GameObject> basicEnemies = new();
    [SerializeField] private List<GameObject> glassCannons = new();
    [SerializeField] private List<GameObject> tanks = new();
    [SerializeField] private List<GameObject> bosses = new();

    // Hidden
    [HideInInspector] public float difficultyMod = 1;
    private float currWaveHealth;
    private float maxWaveHealth;
    private int currWave = 1, enemiesInWave;
    private Dictionary<int, GameObject> spawnedEnemies = new();
    private GameBoard gameBoard;


    /// <summary>
    /// Debug gizmos to show enemy spawn locations.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        Gizmos.DrawIcon(transform.position, "EnemyHandler", true, Color.magenta);
        foreach (var spawnPoint in spawnPoints)
        {
            Gizmos.DrawWireCube(spawnPoint, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }

    private void Start()
    {
        GameObject.FindGameObjectWithTag("GameBoard").TryGetComponent<GameBoard>(out gameBoard);
        StartWave();
    }

    /// <summary>
    /// Called each time a wave is started e.g. after all enemies are defeated. 
    /// </summary>
    private void StartWave()
    {
        maxWaveHealth = WaveHealthTotal(difficultyMod);
        currWaveHealth = maxWaveHealth;
        int index = 0;

        UpdateHealthDisplay();
        UpdateWaveCount();

        // Spawn enemy in location only if enough enemies have been set.
        // Currently only spawns basic enemies. 
        foreach (var spawnPoint in spawnPoints)
        {
            if (index < basicEnemies.Count)
            {
                spawnedEnemies.Add(index,
                    Instantiate(
                    basicEnemies[Random.Range(0, basicEnemies.Count - 1)],
                    spawnPoint,
                    Quaternion.identity)
                    );
            }
            index++;
        }
        enemiesInWave = index;
    }

    private void NextWave()
    {
        difficultyMod += difficultyMod * 0.1f;
        currWave++;
        gameBoard.ResetBoard();
        UpdateWaveCount();
        StartWave();
    }
    
    /// <summary>
    /// Allows for damage to be dealt to the wave in aggregate. 
    /// Todo: display damage on a random enemy when called.
    /// Todo: destroy enemies based on total health pool 
    ///       e.g. when health percent hits a breakpoint determined by total # of enemies
    /// Todo (optional): allow more enemies to spawn when an enemy is defeated if there
    ///       are more enemies in the wave than spawn points. 
    /// </summary>
    /// <param name="damage"></param>
    public void DealDamage(float damage)
    {
        currWaveHealth = Mathf.Floor(Mathf.Clamp(currWaveHealth - damage, 0, maxWaveHealth));

        UpdateHealthDisplay();

        // If the percent of the wave health is less than the percent of remaining enemies...
        // num of spawned enemies / (enemies in wave + 1) because it offsets the breakpoints where
        // enemies disappear

        if (currWaveHealth / maxWaveHealth <= (float)(spawnedEnemies.Values.Count - 1.0f) / (float)(enemiesInWave))
        {
            // Destroy one on-screen enemy
            // Load a new list with the remaing valid keys in the spawned enemies list
            // Select one of the valid keys to eliminate from the spawned enemies list
            List<int> enemyIndices = new();
            foreach (int index in spawnedEnemies.Keys)
            {
                enemyIndices.Add(index);
            }
            int destroyEnemyAtIndex = enemyIndices[Random.Range(0, enemyIndices.Count() - 1)];

            Destroy(spawnedEnemies[destroyEnemyAtIndex]);
            spawnedEnemies.Remove(destroyEnemyAtIndex);
        }

        if (spawnedEnemies.Count == 0 && currWave < numWaves)
        {
            NextWave();
        } else
        {
            // Shop / game over / etc
        }
    }

    private float WaveHealthTotal(float modifier)
    {
        float waveHealthRunningTotal = 0.0f;
        foreach (var enemy in enemies)
        {
            waveHealthRunningTotal += enemy.baseHealth;
        }
        return waveHealthRunningTotal *= difficultyMod;
    }

    public void UpdateWaveCount(string text = "")
    {
        string waveText = "Wave: " + currWave + " / " + numWaves;
        waveCount.SetText(waveText);
    }

    private void UpdateHealthDisplay()
    {
        if (waveHealthBar)
        {
            waveHealthBar.localScale = new Vector3(currWaveHealth / maxWaveHealth, 1.0f, 1.0f);
        }
    }
}
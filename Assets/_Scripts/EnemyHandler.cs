using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShaderGraph.Drawing;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

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


    [Header("Enemy Appearance")]
    [SerializeField] private List<GameObject> basicEnemies = new();
    [SerializeField] private List<GameObject> glassCannons = new();
    [SerializeField] private List<GameObject> tanks = new();
    [SerializeField] private List<GameObject> bosses = new();

    // Hidden
    private float currWaveHealth;
    private float maxWaveHealth;
    private int currWave = 1;
    [HideInInspector] public float difficultyMod = 1;
    private Dictionary<int, GameObject> spawnedEnemies = new();

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
        currWaveHealth -= damage;
        if (waveHealthBar)
        {
            waveHealthBar.localScale = new Vector3(currWaveHealth / maxWaveHealth, 1.0f, 1.0f);
        }
    }

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
        StartWave();
    }

    /// <summary>
    /// Called each time a wave is started e.g. after all enemies are defeated. 
    /// </summary>
    private void StartWave()
    {
        maxWaveHealth = WaveHealthTotal(difficultyMod);
        currWaveHealth = maxWaveHealth;

        // Spawn enemy in location only if enough enemies have been set. 
        int index = 0;
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

    private void WaveAdvance()
    {
        difficultyMod += difficultyMod * 0.1f;
        currWave++;
    }
}
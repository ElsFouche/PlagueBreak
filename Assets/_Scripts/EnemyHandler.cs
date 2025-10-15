using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Settings = F_GameSettings;

/// <summary>
/// Els Fouche
/// 
/// This script handles enemy logic. 
/// It spawns enemies on wave start, assigning them a random mesh
/// from the pool of available meshes, etc.
/// </summary>
public class EnemyHandler : MonoBehaviour , ISaveLoad
{
    // Designer 
    [Header("Enemy Stats")]
    [Range(0.5f, 20.0f)]
    [SerializeField] private float timeBetweenAttacks;
    [Header("Enemy Waves")]
    [SerializeField] private int numWaves = 1;
    [SerializeField] private List<Vector3> spawnPoints  = new();
    [Header("Enemies")]
    [SerializeField] private List<F_EnemyData> enemies = new();

    [SerializeField] private RectTransform waveHealthBar;
    [SerializeField] private TMP_Text waveCount;
    [SerializeField] private Image timeToNextAttackUI;

    [Header("Enemy Appearance")]
    [SerializeField] private List<GameObject> basicEnemies = new();
    [SerializeField] private List<GameObject> glassCannons = new();
    [SerializeField] private List<GameObject> tanks = new();
    [SerializeField] private List<GameObject> bosses = new();

    // Hidden
    [HideInInspector] public float difficultyMod = 1;
    private float currWaveHealth;
    private float maxWaveHealth;
    private float attackDamage;
    private int currWave = 1, enemiesInWave;
    private Dictionary<int, GameObject> spawnedEnemies = new();
    private GameBoard gameBoard;
    private PlayerController playerController;
    private SaveData saveData = new();
        // Player harm loop settings
    private float updateFrequency = 0.01f;
    // Coroutines
    private Coroutine CR_HarmPlayer = null;

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
        if (!gameBoard)
        {
            // Debug.Log("Fatal: No game board found. Are you sure you set up the scene correctly?");
            Application.Quit();
        }
        GameObject.FindGameObjectWithTag("Player").TryGetComponent<PlayerController>(out playerController);
        if (!playerController)
        {
            // Debug.Log("Fatal: No player controller found. Are you sure you set up the scene correctly?");
            Application.Quit();
        }

        StartWave();
    }

    /// <summary>
    /// Called each time a wave is started e.g. after all enemies are defeated. 
    /// </summary>
    private void StartWave()
    {
        maxWaveHealth = WaveHealthTotal(difficultyMod);
        currWaveHealth = maxWaveHealth;
        attackDamage = 0;
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

        WaveDamageTotal();

        StartPlayerHarmLoop();
    }

    private void NextWave()
    {
        difficultyMod += difficultyMod * 0.1f;
        currWave++;
        gameBoard.ResetBoard();
        UpdateWaveCount();
        timeToNextAttackUI.fillAmount = 1.0f;
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
        } else if (spawnedEnemies.Count == 0 && currWave >= numWaves) {
            // Shop / game over / etc
            // Debug.Log("Wave complete.");
            saveData.completedLevels[saveData.currentLevel] = true;
            SaveData();

            SceneManager.LoadScene("LevelSelect");
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

    private void WaveDamageTotal()
    {
        foreach (var enemy in enemies)
        {
            attackDamage += enemy.baseDamage;
        }
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

    private void OnApplicationPause(bool pause)
    {
        PausePlayerHarm(pause);
    }

    public void PausePlayerHarm(bool pause)
    {
        if (pause)
        {
            if (CR_HarmPlayer != null)
            {
                StopCoroutine(CR_HarmPlayer);
                CR_HarmPlayer = null;
            }
        } else
        {
            StartPlayerHarmLoop();
        }
    }

    /// <summary>
    /// Starts or restarts the damage loop coroutine.
    /// </summary>
    private void StartPlayerHarmLoop()
    {
        if (CR_HarmPlayer != null)
        {
            StopCoroutine(CR_HarmPlayer);
            CR_HarmPlayer = StartCoroutine(HarmPlayer());
        } else
        {
            CR_HarmPlayer = StartCoroutine(HarmPlayer());
        }
    }

    private IEnumerator HarmPlayer()
    { 
        // If no countdown UI, skip decrementing the and instead wait directly. 
        if (!timeToNextAttackUI)
        {
            Debug.Log("No attack UI found. Are you sure you set up the scene correctly?");
            yield return null;
/*
            yield return new WaitForSeconds(timeBetweenAttacks);

            playerController.TakeDamage(attackDamage);

            StartCoroutine(HarmPlayer());
*/
        } 
        else
        {
            while (timeToNextAttackUI.fillAmount > 0)
            {
                // Time = timeBetweenAttacks
                // Rate = D/T = 1/timeBetweenAttacks
                // Distance = 1 (max fill amount) 
                timeToNextAttackUI.fillAmount = Mathf.Max(timeToNextAttackUI.fillAmount - (updateFrequency / timeBetweenAttacks), 0.0f);

                yield return new WaitForSeconds(updateFrequency);
            }

            playerController.TakeDamage(attackDamage);
            timeToNextAttackUI.fillAmount = 1.0f;

            // Pass control to harm paused
            StartCoroutine(HarmPausedIndicator(Settings.playerISeconds));
        }
    }

    /// <summary>
    /// If called with a value above 0: Will blink the player damage timer 
    /// for iSeconds seconds.
    /// If called with no value or values 0 or below: Will blink until 
    /// manually stopped. 
    /// </summary>
    /// <param name="iSeconds"></param>
    /// <returns></returns>
    private IEnumerator HarmPausedIndicator(float iSeconds = -1.0f)
    {
        float timePaused = 0.0f;
        int fillBlink = 0;

        if (iSeconds > 0)
        {
            while (timePaused < iSeconds)
            {
                // Blink the UI
                // Divide by expected frames per second.
                fillBlink++;
                timeToNextAttackUI.fillAmount = (fillBlink / 60) % 2;
                
                // Increment time
                yield return new WaitForEndOfFrame();
                timePaused += Time.deltaTime;
            }
        } else
        {
            while (true)
            {
                fillBlink = (fillBlink + 1) % 2;
                timeToNextAttackUI.fillAmount = fillBlink;
                yield return new WaitForEndOfFrame();
            }
        }

        // reset UI
        timeToNextAttackUI.fillAmount = 1.0f;

        // Reinitialize player harm loop?
        StartPlayerHarmLoop();
    }

    // Interfaces
      // ISaveLoad
    
    /// <summary>
    /// This method updates the save manager with data from interface members.
    /// </summary>
    public void SaveData()
    {
        // Update all subscribers with updated data. 
        foreach (var obj in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<ISaveLoad>().ToList())
        {
            obj.OnDataLoaded(saveData);
        }
    }
    /// <summary>
    /// This method is called in each interface member whenever data is loaded. 
    /// </summary>
    /// <param name="dataToLoad"></param>
    
    public void OnDataLoaded(SaveData dataToLoad)
    {
        saveData = dataToLoad;
    }
    /// <summary>
    /// This method should always return a reference to the interface member. 
    /// </summary>
    /// <returns></returns>
    public GameObject GetGameObject() { return this.gameObject; }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
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
    [SerializeField] private List<Transform> spawnPoints  = new();
    [Header("Enemy Stats")]
    [SerializeField] private List<F_EnemyData> enemies = new();

    [Header("Display Elements")]
    [SerializeField] private RectTransform waveHealthBar;
    [SerializeField] private TMP_Text waveCount;
    [SerializeField] private UnityEngine.UI.Image timeToNextAttackUI;
    [SerializeField] private UnityEngine.UI.Image attackWarningSymbol;

    [Header("Audio")]
    [Tooltip("Add audio clips here.")]
    [SerializeField] private AudioClip zombieAttack;
    [Tooltip("Add audio clips here.")]
    [SerializeField] private AudioClip zombieDamaged;
    [Tooltip("Add audio clips here.")]
    [SerializeField] private AudioClip zombieDeath;
    [Tooltip("Add audio clips here.")]
    [SerializeField] private AudioClip victory;
    [SerializeField] private AudioClip attackWarning;

    [Header("Enemy Appearance")]
    [SerializeField] private List<GameObject> basicEnemies = new();
    [SerializeField] private List<GameObject> glassCannons = new();
    [SerializeField] private List<GameObject> tanks = new();
    [SerializeField] private List<GameObject> bosses = new();

    // Hidden
    [HideInInspector] public float difficultyMod = 1;
        // Level Data
    private float currWaveHealth;
    private float maxWaveHealth;
    private float attackDamage;
    private int currWave = 1, enemiesInWave;
    private Dictionary<int, GameObject> spawnedEnemies = new();
        // References
    private GameBoard gameBoard;
    private PlayerController playerController;
    private LevelComplete levelComplete;
        // Data Management
    private SaveData saveData = new();
        // Player harm loop settings
    private float updateFrequency = 0.01f;
        // Coroutine Lockouts
    private Coroutine CR_HarmPlayer = null;
    private Coroutine CR_HarmPaused = null;

    /// <summary>
    /// Debug gizmos to show enemy spawn locations.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        Gizmos.DrawIcon(transform.position, "EnemyHandler", true, Color.magenta);
        foreach (var spawnPoint in spawnPoints)
        {
            Gizmos.DrawWireCube(spawnPoint.position, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }

    private void Start()
    {
        if (GameObject.FindGameObjectWithTag("GameBoard").TryGetComponent<GameBoard>(out GameBoard gb))
        {
            gameBoard = gb;
        }
        if (gameBoard == null)
        {
            Debug.Log("Fatal: No game board found. Are you sure you set up the scene correctly?");
            Application.Quit();
        }
        if (GameObject.FindGameObjectWithTag("Player").TryGetComponent<PlayerController>(out PlayerController pc))
        {
            playerController = pc;
        }
        if (playerController == null)
        {
            Debug.Log("Fatal: No player controller found. Are you sure you set up the scene correctly?");
            Application.Quit();
        }
        if (GameObject.FindGameObjectWithTag("LevelComplete").TryGetComponent<LevelComplete>(out LevelComplete lc))
        {
            levelComplete = lc;
        } else if (levelComplete == null)
        {
            levelComplete = Instantiate(new GameObject("LevelComplete")).AddComponent<LevelComplete>();
        }

        if (timeToNextAttackUI == null)
        {
            if (GameObject.FindGameObjectWithTag("TimeToNextAttackUI").TryGetComponent<Image>(out Image i))
            {
                timeToNextAttackUI = i;
            }
        }

        if (attackWarningSymbol == null)
        {
            if (GameObject.FindGameObjectWithTag("IncomingAttackIndicatorUI").TryGetComponent<Image>(out Image i))
            {
                attackWarningSymbol = i;
                attackWarningSymbol.gameObject.SetActive(false);
            }
        }

        saveData = SaveManager.instance.GetSaveData();

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
        int enemyCount = 0;

        UpdateHealthDisplay();
        UpdateWaveCount();

        // Spawn enemy in location only if enough enemies have been set.
        // Currently only spawns basic enemies. 
        foreach (var spawnPoint in spawnPoints)
        {
            int bossIndex = enemies.FindIndex(s => s.enemyType == E_EnemyTypes.EnemyType.Boss);
            F_EnemyData enemyToSpawn = new F_EnemyData();
            if (bossIndex < 0)
            {
                enemyToSpawn = enemies[UnityEngine.Random.Range(0, enemies.Count - 1)];
            } else
            {
                enemyToSpawn = enemies[bossIndex];
            }

            switch (enemyToSpawn.enemyType)
            {
                case E_EnemyTypes.EnemyType.None:
                    GameObject tempEnemy = new();
                    break;
                case E_EnemyTypes.EnemyType.Basic:
                    tempEnemy = Instantiate(
                        basicEnemies[UnityEngine.Random.Range(0, basicEnemies.Count - 1)],
                        spawnPoint.position,
                        spawnPoint.rotation);
                    // tempEnemy.transform.localScale = spawnPoint.localScale;
                    tempEnemy.transform.parent = transform;
                    spawnedEnemies.Add(enemyCount, tempEnemy);
                    break;
                case E_EnemyTypes.EnemyType.GlassCannon:
                    tempEnemy = Instantiate(
                        glassCannons[UnityEngine.Random.Range(0, glassCannons.Count - 1)],
                        spawnPoint.position,
                        spawnPoint.rotation);
                    // tempEnemy.transform.localScale = spawnPoint.localScale;
                    tempEnemy.transform.parent = transform;
                    spawnedEnemies.Add(enemyCount, tempEnemy);
                    break;
                case E_EnemyTypes.EnemyType.Tank:
                    tempEnemy = Instantiate(
                        tanks[UnityEngine.Random.Range(0, tanks.Count - 1)],
                        spawnPoint.position,
                        spawnPoint.rotation);
                    // tempEnemy.transform.localScale = spawnPoint.localScale;
                    tempEnemy.transform.parent = transform;
                    spawnedEnemies.Add(enemyCount, tempEnemy);
                    break;
                case E_EnemyTypes.EnemyType.Boss:
                    tempEnemy = Instantiate(
                        bosses[UnityEngine.Random.Range(0, bosses.Count - 1)],
                        spawnPoint.position,
                        spawnPoint.rotation);
                    // tempEnemy.transform.localScale = spawnPoint.localScale;
                    tempEnemy.transform.parent = transform;
                    spawnedEnemies.Add(enemyCount, tempEnemy);
                    break;
                default:
                    break;
            }
            enemyCount++;
        }
        enemiesInWave = enemyCount;

        WaveDamageTotal();

        StartPlayerHarmLoop();
    }

    private void NextWave()
    {
        difficultyMod += difficultyMod * 0.1f;
        currWave++;
        if (saveData.isHardMode)
        {
            gameBoard.ResetBoard();
        }
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

        AudioHandler.instance.PlaySFX(zombieDamaged);

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
            int destroyEnemyAtIndex = enemyIndices[UnityEngine.Random.Range(0, enemyIndices.Count() - 1)];

            Destroy(spawnedEnemies[destroyEnemyAtIndex]);
            spawnedEnemies.Remove(destroyEnemyAtIndex);

            AudioHandler.instance.PlaySFX(zombieDeath, 19);
        }

        if (spawnedEnemies.Count == 0 && currWave < numWaves)
        {
            EarnCrystals();
            NextWave();
        } else if (spawnedEnemies.Count == 0 && currWave >= numWaves) {
            EarnCrystals(0.25f);
            LevelComplete();
        }
    }

    private void LevelComplete()
    {
        AudioHandler.instance.PlaySFX(victory, 09);

        levelComplete.OnLevelComplete();

        // Debug.Log("Level Complete!");
        // Unlock next levels
        switch (saveData.currentLevel)
        {
            case "Level_01":
                if (saveData.unlockedLevels.Count > 0)
                {
                    saveData.unlockedLevels.Clear();
                }
                saveData.unlockedLevels.Add("Level_02");
                saveData.unlockedLevels.Add("Level_03");
                break;
            case "Level_02":
                if (saveData.unlockedLevels.Contains("Level_02"))
                {
                    saveData.unlockedLevels.Remove("Level_02");
                }
                // Only add the next level if it doesn't exist yet. 
                if (!saveData.unlockedLevels.Contains("Level_04"))
                {
                    saveData.unlockedLevels.Add("Level_04");
                }
                break;
            case "Level_03":
                if (saveData.unlockedLevels.Contains("Level_03"))
                {
                    saveData.unlockedLevels.Remove("Level_03");
                }
                // Only add the next level if it doesn't exist yet.
                if (!saveData.unlockedLevels.Contains("Level_04"))
                {
                    saveData.unlockedLevels.Add("Level_04");
                }
                break;
            case "Level_04":
                if (saveData.unlockedLevels.Contains("Level_04"))
                {
                    saveData.unlockedLevels.Remove("Level_04");
                }
                // Only add the next level if it doesn't exist yet.
                if (!saveData.unlockedLevels.Contains("Level_05"))
                {
                    saveData.unlockedLevels.Add("Level_05");
                }
                if (!saveData.unlockedLevels.Contains("Level_06"))
                {
                    saveData.unlockedLevels.Add("Level_06");
                }
                break;
            case "Level_05":
                if (saveData.unlockedLevels.Contains("Level_05"))
                {
                    saveData.unlockedLevels.Remove("Level_05");
                }
                // Only add the next level if both boss prereq levels have finished.
                if (saveData.unlockedLevels.Contains("BossPrereq1") && !saveData.unlockedLevels.Contains("Level_07"))
                {
                    saveData.unlockedLevels.Add("Level_07");
                } else
                {
                    saveData.unlockedLevels.Add("BossPrereq2");
                }
                break;
            case "Level_06":
                if (saveData.unlockedLevels.Contains("Level_06"))
                {
                    saveData.unlockedLevels.Remove("Level_06");
                }
                // Only add the next level if both boss prereq levels have finished.
                if (saveData.unlockedLevels.Contains("BossPrereq2") && !saveData.unlockedLevels.Contains("Level_07"))
                {
                    saveData.unlockedLevels.Add("Level_07");
                }
                else
                {
                    saveData.unlockedLevels.Add("BossPrereq1");
                }
                break;
            case "Level_07":
                if (saveData.unlockedLevels.Count > 0)
                {
                    saveData.unlockedLevels.Clear();
                }
                saveData.unlockedLevels.Add("Level_01");
                break;
            default:
                break;
        }

        SceneHandler.instance.LoadLevelFromLevelType(E_LevelType.LevelSelect, "LevelSelect");
    }

    private void EarnCrystals(float bonusChance = 0.0f)
    {
        int crystalsDropped = 0;

        for (int i = 0; i < Settings.numCrystalsDropChances; i++)
        {
            if (UnityEngine.Random.Range(0.0f, 1.0f) > (Settings.crystalDropChance + bonusChance))
            {
                crystalsDropped++;
            }
        }

        if (crystalsDropped < Settings.minCrystalsDropped)
        {
            crystalsDropped = Settings.minCrystalsDropped;
        }

        saveData.crystals += crystalsDropped;

        CurrencyDisplay currencyDisplay = (CurrencyDisplay)FindFirstObjectByType(typeof(CurrencyDisplay));
        currencyDisplay.UpdateText();
    }

    // ---------------Wave Data Calculations---------------

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

    // ---------------Updates---------------

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

    // ---------------Harming the Player---------------

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
        if (CR_HarmPaused != null)
        {
            StopCoroutine(CR_HarmPaused);
            CR_HarmPaused = null;
        }

        if (timeToNextAttackUI != null) { timeToNextAttackUI.fillAmount = 1.0f; }
        if (attackWarningSymbol != null) { attackWarningSymbol.gameObject.SetActive(false); }
        
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
        if (CR_HarmPaused != null)
        {
            StopCoroutine(CR_HarmPlayer);
        }

        // If no countdown UI, skip decrementing and instead wait directly. 
        if (!timeToNextAttackUI || !attackWarningSymbol)
        {
            Debug.Log("Missing attack UI. Are you sure you set up the scene correctly?");
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

                if (timeToNextAttackUI.fillAmount < 0.25)
                {
                    if ((int)(timeToNextAttackUI.fillAmount * 100) % 3 == 0)
                    {
                        attackWarningSymbol.gameObject.SetActive(true);
                        if (attackWarning != null)
                        {
                            AudioHandler.instance.PlaySFX(attackWarning);
                        }
                    } else
                    {
                        attackWarningSymbol.gameObject.SetActive(false);
                    }
                }

                yield return new WaitForSeconds(updateFrequency);
            }

            playerController.TakeDamage(attackDamage);

            // Audio Feedback
            AudioHandler.instance.PlaySFX(zombieAttack);

            // Touch Feedback
            if (Vibration.HasVibrator())
            {
                Vibration.Vibrate(Settings.takeDamageVibrationMilliseconds);
            }

            timeToNextAttackUI.fillAmount = 1.0f;
            attackWarningSymbol.gameObject.SetActive(false);

            // Pass control to harm paused
            CR_HarmPaused = StartCoroutine(HarmPausedIndicator(Settings.playerISeconds));
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

        CR_HarmPaused = null;

        // Reinitialize player harm loop?
        StartPlayerHarmLoop();
    }

    // ---------------Interfaces---------------

    // ---------------ISaveLoad---------------

    /// <summary>
    /// This method is called in each interface member whenever data is loaded. 
    /// </summary>
    /// <param name="dataToLoad"></param>
    public void LoadData(SaveData dataToLoad)
    {
        saveData = dataToLoad;
    }
    /// <summary>
    /// Update the save data object with local information. 
    /// </summary>
    public void SaveData(ref SaveData savedData)
    {
        // Update savedData with local info
        // savedData.whatever = whatever new
    }
}
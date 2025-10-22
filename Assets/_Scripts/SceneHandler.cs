using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Settings = F_GameSettings;

public class SceneHandler : MonoBehaviour, ISaveLoad
{
    AsyncOperation sceneLoadOp;
    Scene currentScene;
    Scene nextScene;
    Coroutine asyncLevelOp = null;
    private SaveData saveData;

    public static SceneHandler instance { get; private set; }

    private void Awake()
    {
        // Singleton garbage
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        if (!currentScene.isLoaded)
        {
            Debug.Log("No active scene, loading main menu.");
            if (asyncLevelOp != null)
            {
                StopCoroutine(asyncLevelOp);
            }
            asyncLevelOp = StartCoroutine(LoadLevelAsync("MainMenu", false));
        }

        saveData = SaveManager.instance.GetSaveData();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public Scene GetCurrentScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i) != SceneManager.GetSceneByName("PreloadScene"))
            {
                Debug.Log("Current Scene: " + SceneManager.GetSceneAt(i).name);
                return SceneManager.GetSceneAt(i);
            }
        }
        return SceneManager.GetSceneByBuildIndex(0);
    }

    public void SaveLevelID(string newLevelID)
    {
        saveData.currentLevel = newLevelID;
        Debug.Log("New scene id: " + newLevelID);
    }

    public void LoadLevelFromLevelType(E_LevelType levelType, string newLevelID)
    {
        if (asyncLevelOp != null)
        {
            Debug.Log("Async level operation: " + asyncLevelOp + "\nis currently running. Unable to load.");
            return;
        }

        switch (levelType)
        {
            case E_LevelType.None:
                break;
            case E_LevelType.MainMenu:
                SaveLevelID(newLevelID);
                asyncLevelOp = StartCoroutine(LoadLevelAsync("MainMenu"));
                break;
            case E_LevelType.Settings:
                break;
            case E_LevelType.Shop:
                break;
            case E_LevelType.SpecialShop:
                SaveLevelID(newLevelID);
                asyncLevelOp = StartCoroutine(LoadLevelAsync("CrystalShop"));
                break;
            case E_LevelType.LevelSelect:
                SaveLevelID(newLevelID);
                asyncLevelOp = StartCoroutine(LoadLevelAsync("LevelSelect"));
                break;
            case E_LevelType.Easy:
                SaveLevelID(newLevelID);
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Normal:
                SaveLevelID(newLevelID);
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Hard:
                SaveLevelID(newLevelID);
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Boss:
                SaveLevelID(newLevelID);
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            default:
                break;
        }
    }

    public void LoadLevelFromName(string name, string newLevelID)
    {
        if (asyncLevelOp != null)
        {
            Debug.Log("Async level operation: " + asyncLevelOp + "\nis currently running. Unable to load.");
            return;
        }

        SaveLevelID(newLevelID);

        asyncLevelOp = StartCoroutine(LoadLevelAsync(name));
    }

    private IEnumerator LoadLevelAsync(string levelName, bool unloadPrevious = true)
    {
        // Update our reference to the current scene.
        currentScene = GetCurrentScene();
        // Store a reference to the next scene. 
        nextScene = SceneManager.GetSceneByName(levelName);
        
        if (nextScene == null)
        {
            Debug.LogError("The next scene is invalid: failed to load.");
            asyncLevelOp = null;
            yield return null;
        } else
        {
            sceneLoadOp = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
            
            sceneLoadOp.allowSceneActivation = false;
            while (sceneLoadOp.progress <= .85f)
            {
                yield return new WaitForEndOfFrame();
            }
            if (unloadPrevious)
            {
                SceneManager.UnloadSceneAsync(currentScene);
            }
            sceneLoadOp.allowSceneActivation = true;
        }
        asyncLevelOp = null;
    }

    // Interfaces
      // ISaveLoad
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
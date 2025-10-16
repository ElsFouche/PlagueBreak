using System;
using System.Collections;
using Unity.Properties;
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

    public void SaveLevelGUID(string newLevelID)
    {
        saveData.completedLevels.Add(newLevelID, false);
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
            case E_LevelType.Shop:
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Easy:
                SaveLevelGUID(newLevelID);
                Debug.Log("Loading Level_00");
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Normal:
                SaveLevelGUID(newLevelID);
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Hard:
                SaveLevelGUID(newLevelID);
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Boss:
                SaveLevelGUID(newLevelID);
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            default:
                break;
        }
    }

    private IEnumerator LoadLevelAsync(string levelName, bool unloadPrevious = true)
    {
        Debug.Log("Loading scene: " +  levelName);
        nextScene = SceneManager.GetSceneByName(levelName);
        
        if (nextScene == null)
        {
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
            sceneLoadOp.allowSceneActivation = true;

            
            if (unloadPrevious)
            {
                SceneManager.UnloadSceneAsync(currentScene);
            }
        }
        asyncLevelOp = null;
        currentScene = GetCurrentScene();
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
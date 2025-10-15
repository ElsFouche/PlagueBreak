using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Settings = F_GameSettings;

public class SceneHandler : MonoBehaviour
{
    AsyncOperation asyncOp;
    Scene currentScene;
    Scene nextScene;
    Coroutine asyncLevelOp = null;
    private SaveManager saveManager;
    private SaveData saveData;

    private void Awake()
    {
        if (!GameObject.FindWithTag("SaveSystem").TryGetComponent<SaveManager>(out SaveManager saveManager))
        {
            Debug.Log("No save system found.");
        } else
        {
            saveData = saveManager._SaveData;
        }

    }

    private void Start()
    {
        if (!currentScene.isLoaded)
        {
            Debug.Log("No active scene, loading main menu.");
            if (asyncOp != null)
            {
                StopCoroutine(asyncLevelOp);
            }
            asyncLevelOp = StartCoroutine(LoadLevelAsync("MainMenu", false));

        }
    }

    public void ExitGame()
    {
        // save on exit
        saveManager.SaveToFile(Settings.defaultProfileName);
        StartCoroutine(Quitting());
    }
    private IEnumerator Quitting()
    {
        yield return new WaitForSeconds(0.2f);
        Application.Quit();
    }

    private Scene GetCurrentScene()
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

    private void SaveLevelGUID(Guid newSceneID)
    {
        saveData.completedLevels.Add(newSceneID, false);
        saveData.currentLevel = newSceneID;
        Debug.Log("New scene id: " + newSceneID);
    }

    public void LoadLevelFromLevelType(E_LevelType levelType, Guid newSceneID)
    {
        if (asyncLevelOp != null)
        {
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
                SaveLevelGUID(newSceneID);
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Normal:
                SaveLevelGUID(newSceneID);
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Hard:
                SaveLevelGUID(newSceneID);
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Boss:
                SaveLevelGUID(newSceneID);
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
            asyncOp = null;
            yield return null;
        } else
        {
            asyncOp = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
            
            asyncOp.allowSceneActivation = false;
            while (asyncOp.progress <= .85f)
            {
                yield return new WaitForEndOfFrame();
            }
            asyncOp.allowSceneActivation = true;

            if (unloadPrevious)
            {
                SceneManager.UnloadSceneAsync(currentScene);
            }
        }
        currentScene = GetCurrentScene();
        asyncOp = null;
    }
}
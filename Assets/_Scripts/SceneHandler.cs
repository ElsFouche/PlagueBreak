using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour , ISaveLoad
{
    AsyncOperation asyncOp;
    Scene currentScene;
    Scene nextScene;
    Coroutine asyncLevelOp = null;
    private SaveData saveData = new();

    public void ChangeScene(int newScene)
    {
        SceneManager.LoadScene(newScene);
    }

    public void ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LoadLevelFromLevelType(E_LevelType levelType, Guid newSceneID)
    {
        currentScene = SceneManager.GetActiveScene();

        if (asyncLevelOp != null)
        {
            return;
        }

        switch (levelType)
        {
            case E_LevelType.None:
                break;
            case E_LevelType.Shop:
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00", newSceneID));
                break;
            case E_LevelType.Easy:
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00", newSceneID));
                break;
            case E_LevelType.Normal:
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00", newSceneID));
                break;
            case E_LevelType.Hard:
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00", newSceneID));
                break;
            case E_LevelType.Boss:
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00", newSceneID));
                break;
            default:
                break;
        }
    }

    private IEnumerator LoadLevelAsync(string levelName, Guid newSceneID)
    {
        nextScene = SceneManager.GetSceneByName(levelName);
        if (nextScene == null)
        {
            yield return null;
        } else
        {
            saveData.completedLevels.Add(newSceneID, false);
            saveData.currentLevel = newSceneID;
            Debug.Log("New scene id: " + newSceneID);
            SaveData();

            asyncOp = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
            // asyncOp.allowSceneActivation = true;
            while (asyncOp.progress <= .9f)
            {
                yield return new WaitForEndOfFrame();
            }
            SceneManager.LoadScene(levelName);
        }
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
        Debug.Log("Data loaded in scene handler.");
        saveData = dataToLoad;
    }
    /// <summary>
    /// This method should always return a reference to the interface member. 
    /// </summary>
    /// <returns></returns>
    public GameObject GetGameObject() { return this.gameObject; }
}
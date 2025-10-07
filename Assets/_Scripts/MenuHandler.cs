using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    AsyncOperation asyncOp;
    Scene currentScene;
    Scene nextScene;
    Coroutine asyncLevelOp = null;

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

    public void LoadLevelFromLevelType(E_LevelType levelType)
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
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Easy:
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Normal:
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Hard:
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            case E_LevelType.Boss:
                asyncLevelOp = StartCoroutine(LoadLevelAsync("Level_00"));
                break;
            default:
                break;
        }
    }

    private IEnumerator LoadLevelAsync(string levelName)
    {
        nextScene = SceneManager.GetSceneByName(levelName);
        if (nextScene == null)
        {
            yield return null;
        } else
        {
            asyncOp = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
            // asyncOp.allowSceneActivation = true;
            while (asyncOp.progress <= .9f)
            {
                yield return new WaitForEndOfFrame();
            }
            SceneManager.LoadScene(levelName);
        }
    }
}
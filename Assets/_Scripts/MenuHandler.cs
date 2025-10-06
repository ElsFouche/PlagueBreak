using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    AsyncOperation asyncOp;

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

    public Scene GetLevelFromLevelType(E_LevelType levelType)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        switch (levelType)
        {
            case E_LevelType.None:
                return currentScene;
            case E_LevelType.Shop:
                SceneManager.LoadScene("Level_Shop");
                return SceneManager.GetSceneByName("Level_Shop");
            case E_LevelType.Easy:
                asyncOp = SceneManager.LoadSceneAsync("Level_00", LoadSceneMode.Additive);
                asyncOp.allowSceneActivation = false;
                return SceneManager.GetSceneByName("Level_00");
            case E_LevelType.Normal:
                SceneManager.LoadScene("Level_01");
                return SceneManager.GetSceneByName("Level_01");
            case E_LevelType.Hard:
                return SceneManager.GetSceneByName("Level_02");
            case E_LevelType.Boss:
                return SceneManager.GetSceneByName("Level_03");
            default:
                return currentScene;
        }
    }

    public AsyncOperation GetAsyncOperation()
    {
        return asyncOp;
    }
}
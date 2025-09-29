using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
/*
    public enum Levels
    {
        None,
        MainMenu,
        MainGame
    }
    [Header("Level Order")]
    [SerializeField] private Levels mainMenu = Levels.MainMenu;
    [SerializeField] private Levels mainGame = Levels.MainGame;
*/

    public void ChangeScene(int newScene)
    {
        SceneManager.LoadScene(newScene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

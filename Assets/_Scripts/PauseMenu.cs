using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
// Code for choosing putons to allow the pause menue to activate and bring you to a menue to either resume or exit game
// Jacob OShaughnessy
// 09/24/25
{
    [SerializeField] GameObject pauseMenu;

    //Allows the pause button to be pressed and bring you to the pause menu
    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    //makes it so that the pause menue disapears and you can continue with playing the game
    public void Resume() 
    { 
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
    //Used to exit the scene and move to the start of the main menu
    public void Exit() 
    { 
        //Example of the code for now, will wait for a later sprint to apply to the main game, but cod is in place for now

        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1;
    }
}

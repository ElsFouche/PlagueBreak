using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController_LevelSelect : TouchHandling
{
    // Exposed
    [SerializeField] private GameObject playerIcon;

    // Private
    private MenuHandler _menuHandlerScript;
    
    new private void Awake()
    {
        base.Awake();
        if (!playerIcon)
        {
            Debug.Log("Player icon not found.");
            playerIcon = new GameObject("PlaceholderPlayerIcon");
        }

        GameObject _menuHandlerObject = new GameObject("Menu Handler");
        _menuHandlerScript = _menuHandlerObject.AddComponent<MenuHandler>();
    }

    protected override void TouchStarted(InputAction.CallbackContext ctx)
    {
        base.TouchStarted(ctx);
        // Add functionality to TouchStarted
        // Debug.Log("Touch start position: " + touchStartPos);
        playerIcon.transform.position = playerInput.camera.WorldToScreenPoint(touchStartPos);
    }

    protected override void TouchEnded(InputAction.CallbackContext context)
    {
        base.TouchEnded(context);
        // Add functionality to TouchEnded
    }

    public void LevelIconTouched(LevelSelectButton level)
    {
        // Debug.Log("Level: " + level);
        Debug.Log("Level type: " + level.levelType);
        Debug.Log("Level name: " + _menuHandlerScript.GetLevelFromLevelType(level.levelType).name);
        Debug.Log("Scene manager Level_00 name: " + SceneManager.GetSceneByName("Level_00").name);
        Debug.Log("Scene manager current level name: " + SceneManager.GetActiveScene().name);
        StartCoroutine(WaitToStartLevel(_menuHandlerScript.GetAsyncOperation()));
/*        
        Scene levelSelected = _menuHandlerScript.GetLevelFromLevelType(level.levelType);
        if (levelSelected.IsValid())
        {
            Debug.Log("Level Selected: " + levelSelected.name);
        } else
        {
            Debug.Log("Selected level is invalid.");
        }
*/
    }

    public IEnumerator WaitToStartLevel(AsyncOperation asyncOp)
    {
        yield return new WaitForSeconds(2.0f);
        asyncOp.allowSceneActivation = true;
    }
}
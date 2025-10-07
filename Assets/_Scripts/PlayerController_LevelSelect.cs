using System.Collections;
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
        _menuHandlerScript.LoadLevelFromLevelType(level.levelType);
    }
}
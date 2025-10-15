using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController_LevelSelect : TouchHandling
{
    // Exposed
    [SerializeField] private GameObject playerIcon;

    // Private
    private SceneHandler _sceneHandlerScript;
    private List<Button> levelSelectButtons = new();
    private SaveData saveData = new();
    
    new private void Awake()
    {
        base.Awake();
        if (!playerIcon)
        {
            Debug.Log("Player icon not found.");
            playerIcon = new GameObject("PlaceholderPlayerIcon");
        }

        if (!GameObject.FindWithTag("SceneHandler").TryGetComponent<SceneHandler>(out SceneHandler _sceneHandlerScript))
        {
            Debug.Log("No scene handler found.");
        }
/*
        foreach (var button in GameObject.FindGameObjectsWithTag("LevelSelectButton"))
        {
            levelSelectButtons.Add(button.GetComponent<Button>());
        }
*/
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
        _sceneHandlerScript.LoadLevelFromLevelType(level.levelType, level.ID);
    }

    private void UpdateValidLevels()
    {
        foreach (var button in levelSelectButtons)
        {
            Guid levelID = button.GetComponent<LevelSelectButton>().ID;

            if (saveData.completedLevels.ContainsKey(levelID) && saveData.completedLevels[levelID])
            {
                button.enabled = false;
            }
        }
    }
}
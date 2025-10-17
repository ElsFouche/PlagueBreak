using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController_Menus : TouchHandling
{
    // Exposed
    [SerializeField] private GameObject playerIcon;

    // Private
    private List<Button> levelSelectButtons = new();
    private SaveData saveData;
    
    new private void Awake()
    {
        base.Awake();
        if (!playerIcon)
        {
            Debug.Log("Player icon not found.");
            playerIcon = new GameObject("PlaceholderPlayerIcon");
        }
    }

    private void Start()
    {
        saveData = SaveManager.instance.GetSaveData();
        levelSelectButtons.Clear();

        foreach (var button in GameObject.FindGameObjectsWithTag("LevelSelectButton"))
        {
            levelSelectButtons.Add(button.GetComponent<Button>());
        }

        UpdateValidLevels();
    }

    protected override void TouchStarted(InputAction.CallbackContext ctx)
    {
        base.TouchStarted(ctx);
        // Add functionality to TouchStarted
        playerIcon.transform.position = playerInput.camera.WorldToScreenPoint(touchStartPos);
    }

    protected override void TouchEnded(InputAction.CallbackContext context)
    {
        base.TouchEnded(context);
        // Add functionality to TouchEnded
    }

    public void LevelIconTouched(LevelSelectButton level)
    {
        Debug.Log("Level selected: " + level.levelType.ToString());
        SceneHandler.instance.LoadLevelFromLevelType(level.levelType, level.levelID);
    }

    public void ExitGame()
    {
        SceneHandler.instance.ExitGame();
    }

    private void UpdateValidLevels()
    {
        saveData = SaveManager.instance.GetSaveData();
        foreach (var button in levelSelectButtons)
        {
            string levelID = button.GetComponent<LevelSelectButton>().levelID;

            if (saveData.completedLevels.Contains(levelID))
            {
                button.interactable = false;
            }
        }
    }
}
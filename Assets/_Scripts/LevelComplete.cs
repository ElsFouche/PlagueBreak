using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This script is largely unnecessary but functionality from the Enemy Handler 
/// could be moved here in order to decompose it into more discrete chunks.
/// </summary>
public class LevelComplete : MonoBehaviour
{
    [Header("Level Complete Functions")]
    [Tooltip("This script is called whenever a level is completed. \n" +
             "These variables determine the effects per-level.")]
    [SerializeField] private int bonusCrystals;
    
    private SaveData saveData;

    private void Awake()
    {
        saveData = SaveManager.instance.GetSaveData();
    }

    public void OnLevelComplete()
    {

        if (bonusCrystals > 0)
        {
            saveData.crystals += bonusCrystals;
        }
    }
}
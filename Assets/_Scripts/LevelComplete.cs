using UnityEngine;


public class LevelComplete : MonoBehaviour
{
    [Header("Level Complete Functions")]
    [Tooltip("This script is called whenever a level is completed. \n" +
             "These variables determine the effects per-level.")]
    [SerializeField] 
    private bool resetMapProgress;
    private int bonusCrystals;

    [HideInInspector]
    private SaveData saveData;

    private void Awake()
    {
        saveData = SaveManager.instance.GetSaveData();
    }

    public void OnLevelComplete()
    {
        if (resetMapProgress)
        {
            saveData.completedLevels.Clear();
        }

        if (bonusCrystals > 0)
        {
            saveData.crystals += bonusCrystals;
        }
    }
}
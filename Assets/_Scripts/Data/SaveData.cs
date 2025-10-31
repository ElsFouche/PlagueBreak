using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // Settings
    public float volume = 1.0f;

    // Levels 
    public List<string> completedLevels;
    public string currentLevel;

    // Upgrade Data
    public int playerDamageBoost;
    public int playerHealthBoost;
    public int playerDamageMultiplier;

    // Currency
    public int crystals;

    // Clicked Buttons
    public List<F_Buttons> clickedButtons;

    public SaveData()
    {
        completedLevels = new List<string> { "NULL" };
        currentLevel = "";

        playerDamageBoost = 0;
        playerHealthBoost = 0;
        playerDamageMultiplier = 0;

        crystals = 0;

        clickedButtons = new List<F_Buttons> { new F_Buttons("NULL", 0)};
    }
}
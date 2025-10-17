using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public List<string> completedLevels;
    public string currentLevel;

    // Upgrade Data
    public int playerDamageBoost;
    public int playerHealthBoost;

    // Clicked Buttons
    public List<string> clickedButtons;

    public SaveData()
    {
        completedLevels = new List<string> { "NULL" };
        currentLevel = "";

        playerDamageBoost = 0;
        playerHealthBoost = 0;

        clickedButtons = new List<string> { "NULL" };
    }
}
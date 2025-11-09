using System.Collections.Generic;
using UnityEngine;

public class F_GameSettings
{
    public const int howManyInAMatch = 3;
    public const float playerHealthMax = 50.0f;
    public const float playerISeconds = 3.0f;

    public const string defaultProfileName = "Default";

    public const float crystalDropChance = 0.25f;
    public const int numCrystalsDropChances = 3;
    public const int minCrystalsDropped = 0;

    public const int touchVibrationMilliseconds = 15;
    public const int takeDamageVibrationMilliseconds = 30;

    // Level Names
    public const string mainMenu = "MainMenu";
    public const string settingsMenu = "SettingsMenu";
    public const string crystalShop = "CrystalShop";
    public const string levelSelect = "LevelSelect";
    public const string levelEasy = "Level_00";
    public const string levelNormal = "Level_04";
    public const string levelHard = "Level_00";
    public const string levelBoss = "Level_03";
    public static readonly List<string> defaultUnlockedLevels = new List<string> {
            "Level_01"
    };
}
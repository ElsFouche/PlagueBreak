using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Dictionary<string, bool> completedLevels;
    public string currentLevel;

    public SaveData()
    {
        completedLevels = new Dictionary<string, bool>();
        currentLevel = "";
    }
}
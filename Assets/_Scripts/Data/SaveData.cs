using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<string> completedLevels;
    public string currentLevel;

    public SaveData()
    {
        completedLevels = new List<string> { "NULL" };
        currentLevel = "";
    }
}
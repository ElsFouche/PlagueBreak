using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Dictionary<Guid, bool> completedLevels = new();
    public Guid currentLevel = new();
}
using UnityEngine;
using Settings = F_GameSettings;

public class SaveManager : MonoBehaviour
{
    public SaveData _SaveData = new();

    /// <summary>
    /// Finds all objects that need saved data,
    /// Determines if the event system has loaded,
    /// Determines if the main camera has loaded.
    /// </summary>
    private void Awake()
    {
        // Allow the player to select from different profiles later.
        _SaveData = LoadFromFile(Settings.defaultProfileName);
        Debug.Log("Data loaded from file.");
        Debug.Log("Current level: " + _SaveData.currentLevel);
    }

    /// <summary>
    /// If the event system and/or main camera were not found,
    /// assigns them to the player. 
    /// </summary>
    private void Start()
    {

    }

    public SaveData LoadFromFile(string filename)
    {
        if (System.IO.File.Exists(filename + ".json"))
        {
            string json = System.IO.File.ReadAllText(filename + ".json");
            return JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            Debug.Log("No save data found, creating default.");
            string json = JsonUtility.ToJson(_SaveData);
            Debug.Log(json);
            System.IO.File.WriteAllText(filename + ".json", json);
            return _SaveData;
        }
    }

    public bool SaveToFile(string filename)
    {
        if (System.IO.File.Exists(filename + ".json"))
        {
            string json = JsonUtility.ToJson(_SaveData);
            System.IO.File.WriteAllText(filename + ".json", json);
            return true;
        } else
        {
            Debug.Log("No save file found at " + filename + ".json");
            return false;
        }
    }
}
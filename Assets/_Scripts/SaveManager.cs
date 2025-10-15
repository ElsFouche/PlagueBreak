using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Settings = F_GameSettings;

public class SaveManager : MonoBehaviour , ISaveLoad
{
    private SaveData saveData = new();
    private List<ISaveLoad> ISaveLoadObjects = new();

    /// <summary>
    /// Finds all objects that need saved data,
    /// Determines if the event system has loaded,
    /// Determines if the main camera has loaded.
    /// </summary>
    private void Awake()
    {
        ISaveLoadObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<ISaveLoad>().ToList();
        foreach (var obj in ISaveLoadObjects)
        {
            /*
                Debug.Log(obj);
                Debug.Log(obj.GetGameObject().name);
            */
        }
    }

    /// <summary>
    /// If the event system and/or main camera were not found,
    /// assigns them to the player. 
    /// </summary>
    private void Start()
    {
        // Allow the player to select from different profiles later.
        saveData = LoadFromFile(Settings.defaultProfileName);
        LoadAllData();
        Debug.Log("Data loaded from file.");
        Debug.Log("Current level: " + saveData.currentLevel);
    }

    private SaveData LoadFromFile(string filename)
    {
        if (System.IO.File.Exists(filename + ".json"))
        {
            string json = System.IO.File.ReadAllText(filename + ".json");
            return JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            Debug.Log("No save data found, creating default.");
            string json = JsonUtility.ToJson(saveData);
            Debug.Log(json);
            System.IO.File.WriteAllText(filename + ".json", json);
            return saveData;
        }
    }

    /// <summary>
    /// Sends a message to all objects that implement ISaveData to
    /// update their local persistent data with this object's data.
    /// </summary>
    private void LoadAllData()
    {
        foreach (var obj in ISaveLoadObjects)
        {
            obj.OnDataLoaded(saveData);
        }
    }

    // Interfaces
        // ISaveLoad
    
    public void SaveData()
    {
        
    }

    
    public void OnDataLoaded(SaveData dataToLoad) { saveData = dataToLoad; }
    /// <summary>
    /// Returns the game object associated with this ISaveLoad instance.
    /// </summary>
    /// <returns></returns>
    public GameObject GetGameObject() { return this.gameObject; }
}
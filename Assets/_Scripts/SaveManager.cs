using UnityEngine;
using System.Linq;
using Settings = F_GameSettings;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SaveManager : MonoBehaviour
{
    [Header("File Storage Configuration")]
    [SerializeField] private string profileName;


    private SaveData saveData;
    private List<ISaveLoad> subscribers;
    private SaveDataHandler dataHandler;

    public static SaveManager instance { get; private set; }

    /// <summary>
    /// Finds all objects that need saved data,
    /// Determines if the event system has loaded,
    /// Determines if the main camera has loaded.
    /// </summary>
    private void Awake()
    {
        // Singleton garbage
        if (instance != null)
        {
            Destroy(this);
        } else
        {
            instance = this;
        }

        // Allow for multiple save slots by adjusting this section:
        if (profileName == null)
        {
            profileName = Settings.defaultProfileName;
        }
    }

    private void Start()
    {
        this.dataHandler = new SaveDataHandler(Application.persistentDataPath, profileName);
        this.subscribers = FindAllSubscribers();
        LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }


    // Accessible Methods
    public void NewGame()
    {
        this.saveData = new SaveData();
    }

    /// <summary>
    /// Call this to load the game. 
    /// </summary>
    public void LoadGame()
    {
        // Load data from file using the data handler
        this.saveData = dataHandler.LoadFromFile();

        if (this.saveData == null)
        {
            NewGame();
        }

        foreach (var sub in this.subscribers)
        {
            sub.LoadData(this.saveData);
        }
    }

    /// <summary>
    /// Call this to save the game. 
    /// </summary>
    public void SaveGame()
    {
        foreach (var sub in this.subscribers)
        {
            sub.SaveData(ref this.saveData);
        }
    }

    public ref SaveData GetSaveData()
    {
        return ref this.saveData;
    }

    private List<ISaveLoad> FindAllSubscribers()
    {
        IEnumerable<ISaveLoad> subscribersObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, 
                                                                                     FindObjectsSortMode.None)
                                                                                     .OfType<ISaveLoad>();
        return new List<ISaveLoad>(subscribersObjects);
    }
}
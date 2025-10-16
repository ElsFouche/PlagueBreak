using System;
using System.IO;
using UnityEngine;

public class SaveDataHandler
{
    private string directoryPath = "";
    private string profileName = "";

    public SaveDataHandler(string newDirectoy, string newProfileName)
    {
        this.directoryPath = newDirectoy;
        this.profileName = newProfileName;
    }

    public void SetDirectoryPath(string newDirectory)
    {
        this.directoryPath = newDirectory;
    }

    public void SetProfileName(string newProfileName)
    {
        this.profileName = newProfileName;
    }

    // Data Operations
    /// <summary>
    /// Load data from the file system. 
    /// </summary>
    /// <returns></returns>
    public SaveData LoadFromFile()
    {
        string fullPath = Path.Combine(directoryPath, profileName + ".json");
        SaveData loadedData = null;

        if (File.Exists(fullPath))
        {
            try
            {
                // Load the serialized data
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // Deserialize the loaded data
                loadedData = JsonUtility.FromJson<SaveData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load data from file: " +  fullPath + "\n" + e);
            }
        }
        return loadedData;
    }
    /// <summary>
    /// Overload for accepting different profile names. 
    /// </summary>
    /// <returns></returns>
    public SaveData LoadFromFile(string otherProfile)
    {
        string fullPath = Path.Combine(directoryPath, otherProfile + ".json");
        SaveData loadedData = null;

        if (File.Exists(fullPath))
        {
            try
            {
                // Load the serialized data
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // Deserialize the loaded data
                loadedData = JsonUtility.FromJson<SaveData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    /// <summary>
    /// Writes data to the file system. 
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="saveData"></param>
    /// <returns></returns>
    public void SaveToFile(SaveData saveData)
    {
        string fullPath = Path.Combine(directoryPath, profileName + ".json");
        Debug.Log("Saving game to: " + fullPath);

        try
        {
            // Create the directory if it doesn't exist. 
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // Serialize the data to .json
            string dataToSave = JsonUtility.ToJson(saveData, true);

            // Write the serialized data
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToSave);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred when trying to save data to file: " + fullPath + "\n" + e);
        }
    }
}
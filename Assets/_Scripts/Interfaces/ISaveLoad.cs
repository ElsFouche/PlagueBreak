using UnityEngine;

public interface ISaveLoad
{
    /// <summary>
    /// This method updates the save manager with data from interface members.
    /// </summary>
    void SaveData();
    /// <summary>
    /// This method is called in each interface member whenever data is loaded. 
    /// </summary>
    /// <param name="dataToLoad"></param>
    void OnDataLoaded(SaveData dataToLoad);
    /// <summary>
    /// This method should always return a reference to the interface member. 
    /// </summary>
    /// <returns></returns>
    GameObject GetGameObject();
}
using UnityEngine;

public interface ISaveLoad
{
    void SaveData();
    void LoadData(SaveData dataToLoad);
    GameObject GetGameObject();
}
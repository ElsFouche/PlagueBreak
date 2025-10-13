using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // The list of levels in the level select screen.
    // These are associated with the buttons based on their indices. 
    public List<E_LevelType> levelTypes = new(){
                E_LevelType.None, 
                E_LevelType.Easy, 
                E_LevelType.Easy,
                E_LevelType.Normal, 
                E_LevelType.Shop };
}
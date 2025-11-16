using UnityEngine;

public class DeleteSave : MonoBehaviour
{
    public void DeleteSavedData()
    {
        SaveManager.instance.DeleteSave();
    }
}
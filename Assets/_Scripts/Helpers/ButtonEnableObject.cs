using UnityEngine;

public class ButtonEnableObject : MonoBehaviour
{
    public void EnableObject(GameObject go)
    {
        go.SetActive(true);
    }

    public void DisableObject(GameObject go)
    {
        go.SetActive(false);
    }

    public void ToggleEnabled(GameObject go)
    {
        go.SetActive(!go.activeSelf);
    }
}
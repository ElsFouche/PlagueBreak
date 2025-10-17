using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    [SerializeField] private bool persistAcrossScenes;

    private void Awake()
    {
        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
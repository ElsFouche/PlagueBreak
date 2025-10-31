using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    private SaveData saveData;

    private void Awake()
    {
        saveData = SaveManager.instance.GetSaveData();
    }

    private void Start()
    {
        if (volumeSlider == null)
        {
            if (TryGetComponent<Slider>(out Slider vs))
            {
                volumeSlider = vs;
            }
        }

        // Load volume setting from file
        if (volumeSlider != null)
        {
            volumeSlider.value = saveData.volume;
        }
    }

    // Update save data with volume level 
    private void FixedUpdate()
    {
        saveData.volume = volumeSlider.value;
    }
}

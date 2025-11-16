using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private bool isBGMVolume;

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
            if (isBGMVolume)
            {
                volumeSlider.value = saveData.volumeBGM;
            } else
            {
                volumeSlider.value = saveData.volumeSFX;
            }
        }
    }

    // Update save data with volume level 
    private void FixedUpdate()
    {
        if (masterVolume != null)
        {
            if (masterVolume.value > volumeSlider.value)
            {
                volumeSlider.value = masterVolume.value;
            }
        }

        if (volumeSlider != null)
        {
            if (masterVolume == null)
            {
                if (isBGMVolume)
                {
                    saveData.volumeBGM = volumeSlider.value;
                } else
                {
                    saveData.volumeSFX = volumeSlider.value;
                }
            } else
            {

            }
        }
    }
}

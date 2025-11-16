using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Slider volumeSliderBGM;
    [SerializeField] private Slider volumeSliderSFX;
    [SerializeField] private Toggle difficultyToggle;
    [SerializeField] private Toggle vibrationToggle;

    private SaveData saveData;

    private void Awake()
    {
        saveData = SaveManager.instance.GetSaveData();
    }

    private void Start()
    {
        // Full null field checking omitted - set it up right the first time ya donk. 

        // Load volume setting from file
        if (volumeSliderBGM != null)
        {
            volumeSliderBGM.value = saveData.volumeBGM;
        }

        if (volumeSliderSFX != null)
        {
            volumeSliderSFX.value = saveData.volumeSFX;
        }

        if (difficultyToggle != null)
        {
            difficultyToggle.isOn = saveData.isHardMode;
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.isOn = saveData.useVibration;
        }
    }

    public void SetHardMode(Toggle toggle)
    {
        saveData.isHardMode = toggle.isOn;
    }

    public void SetUseVibration(Toggle toggle)
    {
        saveData.useVibration = toggle.isOn;
    }

    public void SetBGMVolume(Slider slider)
    {
        saveData.volumeBGM = slider.value;
        AudioHandler.instance.UpdateVolumeBGM(slider.value);
    }

    public void SetSFXVolume(Slider slider)
    {
        saveData.volumeSFX = slider.value;
    }
}
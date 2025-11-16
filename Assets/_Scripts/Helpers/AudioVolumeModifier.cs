using UnityEngine;

/// <summary>
/// This script modifies the volume of the associated audio source based on the user's saved volume preference. 
/// </summary>
public class AudioVolumeModifier : MonoBehaviour
{
    [SerializeField] private bool isBackgroundMusic = false; 
    private AudioSource m_AudioSource;

    private void Awake()
    {
        if (!gameObject.TryGetComponent<AudioSource>(out m_AudioSource))
        {
            Debug.Log("No audio source found on this game object. Killing audio helper " + this.GetInstanceID());
            Destroy(this);
        } else
        {
            if (m_AudioSource != null)
            {
                if (isBackgroundMusic)
                {
                    m_AudioSource.volume *= SaveManager.instance.GetSaveData().volumeBGM;
                } else
                {
                    m_AudioSource.volume *= SaveManager.instance.GetSaveData().volumeSFX;
                }
            }
        }
    }
}
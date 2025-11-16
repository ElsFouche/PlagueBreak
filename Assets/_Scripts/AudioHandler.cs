using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    [Header("Audio")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float volumeBGM = 1.0f;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float volumeSFX = 1.0f;
    [Range(0, 5)]
    [Tooltip("This is the maximum number of simultaneous sounds.\n" +
        "If set to 0, there will only be one sound effect allowed at a time.")]
    [SerializeField] private int maxConcurrentSFX;
    [SerializeField] private AudioSource audioSourceBGM;

    private List<AudioSource> audioSourceSFX = new();
    private Coroutine CR_AudioReset = null;
    private Coroutine CR_AudioFadeOutBGM = null;
    private SaveData saveData = new();

    // Singleton
    public static AudioHandler instance { get; private set; }

    private void Awake()
    {
        // Singleton garbage
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        saveData = SaveManager.instance.GetSaveData();
        
        if (audioSourceBGM == null)
        {
            Debug.LogError("No BGM audio source has been set. Attempting to retrieve it from the game object.");
            
            if (TryGetComponent<AudioSource>(out AudioSource _as))
            {
                audioSourceBGM = _as;
                Debug.Log("Successfully loaded audio source from game object.");
            } else
            {
                Debug.LogError("Unable to find audio source. Creating new.");
                audioSourceBGM = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSourceBGM.loop = true;
        audioSourceBGM.volume = saveData.volumeBGM * volumeBGM;
        
        for (int i = 1; i <= maxConcurrentSFX + 1; i++)
        {
            if (audioSourceSFX.Count < i)
            {
                audioSourceSFX.Add(gameObject.AddComponent<AudioSource>());
                audioSourceSFX[i - 1].volume = volumeSFX * saveData.volumeSFX;
                audioSourceSFX[i - 1].priority = 10 + ((i - 1) * 10);
            }
        }
    }

    public void PlayBGM(AudioClip bgm, float volumeModifier = 1.0f, float fadeTime = 0.5f)
    {
        Debug.Log("BGM source " + this.GetInstanceID() + " requested to play: " + bgm);
        if (audioSourceBGM.isPlaying)
        {
            Debug.Log("BGM source " + this.GetInstanceID() + " is already playing: " + audioSourceBGM.clip);
            if (audioSourceBGM.clip == bgm) { return; }

            if (CR_AudioFadeOutBGM == null)
            {
                Debug.Log("Coroutine is available.");
                CR_AudioFadeOutBGM = StartCoroutine(FadeOutBGM(fadeTime, bgm, volumeModifier));
            }
            else
            {
                // Audio Queue would go here.
                return;
            }
        }
        else
        {
            Debug.Log("Setting BGM volume to " + volumeBGM * saveData.volumeBGM * volumeModifier);
            audioSourceBGM.resource = bgm;
            audioSourceBGM.volume = volumeBGM * saveData.volumeBGM * volumeModifier;
            Debug.Log("BGM volume is now " + audioSourceBGM.volume);
            audioSourceBGM.Play();
        }
    }

    public void StopBGM(float seconds = 0.5f)
    {
        if (audioSourceBGM.isPlaying)
        {
            if (CR_AudioReset == null)
            {
                CR_AudioFadeOutBGM = StartCoroutine(FadeOutBGM(seconds));
            }
            else
            {
                // Audio Queue would go here.
                StopCoroutine(CR_AudioFadeOutBGM);
                CR_AudioFadeOutBGM = StartCoroutine(FadeOutBGM(seconds));
                return;
            }
        }
    }

    public void PlaySFX(AudioClip clip, int clipPriority = 256, float volumeModifier = 1.0f)
    {
        if (clip == null) { return; }
        if (audioSourceSFX.Count < 1) 
        {
            audioSourceSFX.Add(gameObject.AddComponent<AudioSource>());
            audioSourceSFX[0].volume = volumeSFX * saveData.volumeSFX * volumeModifier;
            audioSourceSFX[0].volume = Mathf.Clamp(audioSourceSFX[0].volume, 0.0f, 1.0f);
        }

        AudioSource clipSource = null;

        if (GetPlayingSources(audioSourceSFX).Count > maxConcurrentSFX)
        {
            foreach (var source in audioSourceSFX)
            {
                if (source.priority > clipPriority)
                {
                    clipSource = source;
                    break;
                }
            }
            
            if (clipSource == null) { return; }
        }
        else
        {
            foreach (var source in audioSourceSFX)
            {
                if (!source.isPlaying)
                {
                    clipSource = source; 
                    break;
                }
            }
        }
        float volume = volumeSFX * saveData.volumeSFX * volumeModifier;
        volume = Mathf.Clamp(volume, 0.0f, 1.0f);

        if (clipSource.isPlaying)
        {
            FadeOut(clipSource, 0.2f);
            StartCoroutine(PlayAfter(clipSource, clip, true, volume, 0.2f));
        }
        else
        {
            if (clip.loadState == AudioDataLoadState.Loaded)
            {
                clipSource.volume = volume;
                clipSource.PlayOneShot(clip);
            }
            else
            {
                clip.LoadAudioData();
                StartCoroutine(PlayAfterLoad(clipSource, clip, true, volume));
            }
        }
    }

    public void PlaySFXOverride(AudioClip clip, float volumeModifier = 1.0f)
    {
        AudioSource clipSource = null; 

        foreach (var source in audioSourceSFX)
        {
            if (!source.isPlaying || source.clip == clip)
            {
                clipSource = source;
                break;
            }
        }
            
        if (clipSource == null)
        {
            clipSource = audioSourceSFX[audioSourceSFX.Count - 1];
        }

        clipSource.Stop();
        clipSource.PlayOneShot(clip);
    }

    private List<int> GetPlayingSources(List<AudioSource> sources)
    {
        List<int> indices = new();
        int index = 0;
        foreach (AudioSource source in sources)
        {
            if (source.isPlaying)
            {
                indices.Add(index);
            }
            index++;
        }

        return indices;
    }

    private IEnumerator PlayAfter(AudioSource source, AudioClip clip, bool playeOneShot = false, float volume = 0.6f, float seconds = 0.0f)
    {
        yield return new WaitForSeconds(seconds);
        
        source.volume = volume;

        if (playeOneShot)
        {
            source.PlayOneShot(clip);
        } else
        {
        source.resource = clip;
        source.Play();
        }
    }

    private IEnumerator PlayAfterLoad(AudioSource source, AudioClip clip, bool playOneShot = false, float volume = 0.6f)
    {
        yield return new WaitUntil(() => clip.loadState == AudioDataLoadState.Loaded);

        source.volume = volume; 

        if (playOneShot)
        {
            source.PlayOneShot(clip);
        } else
        {
            source.resource = clip;
            source.Play();
        }
    }

    private IEnumerator FadeOutBGM(float seconds = 0.5f, AudioClip nextClip = null, float volumeModifier = 1.0f)
    {
        float startVolume = audioSourceBGM.volume;
        float timer = 0.0f;
        while (audioSourceBGM.volume >= 0.01f)
        {
            timer = Mathf.Min(Time.deltaTime / seconds + timer, 1.0f);
            audioSourceBGM.volume = Mathf.Lerp(startVolume, 0, timer);
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Stopping BGM following fade out.");

        audioSourceBGM.Stop();

        if (nextClip != null)
        {
            CR_AudioFadeOutBGM = null;
            PlayBGM(nextClip, volumeModifier);
        } else
        {
            CR_AudioFadeOutBGM = null;
        }
    }

    private IEnumerator FadeOut(AudioSource source, float seconds = 0.5f)
    {
        float startVolume = source.volume;
        float timer = 0.0f;
        while (source.volume >= 0.01f)
        {
            timer = Mathf.Min(Time.deltaTime / seconds + timer, 1.0f);
            source.volume = Mathf.Lerp(startVolume, 0, timer);
            yield return new WaitForEndOfFrame();
        }

        source.Stop();
        source.clip = null;
    }

}

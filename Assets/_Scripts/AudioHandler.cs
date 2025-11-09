using System.Collections;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Add the audio source on the enemy handler here.")]
    [SerializeField] private AudioSource audioSource;

    private int currAudioPriority = 0;
    private Coroutine CR_AudioReset = null;
    private SaveData saveData = new();

    private void Awake()
    {
        saveData = SaveManager.instance.GetSaveData();
        if (audioSource != null)
        {
            audioSource.volume *= saveData.volume;
        }
    }

    public void PlayAudio(int clipPriority = -1)
    {
        PlayAudio(audioSource.clip, clipPriority);
    }

    public void PlayAudio(AudioClip clip)
    {
        PlayAudio(clip, -1);
    }

    public void PlayAudio(AudioClipRef audio)
    {
        PlayAudio(audio.clip, audio.priority);
    }

    public void PlayAudio(AudioClip clip, int clipPriority = -1)
    {
        if (clip == null) { return; }
        if (audioSource == null) { return; }

        if (audioSource.isPlaying && clipPriority < currAudioPriority)
        {
            return;
        }
        else
        {
            currAudioPriority = Mathf.Max(clipPriority, 0);
            if (clip.loadState == AudioDataLoadState.Loaded)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                clip.LoadAudioData();
                PlayAudio(clip);
            }
            if (CR_AudioReset != null)
            {
                StopCoroutine(CR_AudioReset);
            }
            if (this.isActiveAndEnabled)
            {
                CR_AudioReset = StartCoroutine(ResetAudioPriority());
            }
        }
    }

    private IEnumerator ResetAudioPriority(int priority = 0)
    {
        yield return new WaitUntil(() => !audioSource.isPlaying);
        currAudioPriority = 0;
    }
}

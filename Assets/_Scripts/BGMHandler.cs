using System.Collections;
using UnityEngine;

/// <summary>
/// This script is intended to work alongside the audio system prefab.
/// It searches for the audio source tagged with AudioSourceBGM and updates
/// its background music clip to the desired clip. 
/// This script should be attached to objects that are modified on a per-level basis. 
/// </summary>
public class BGMHandler : MonoBehaviour
{
    [SerializeField] private AudioClip backgroundMusic;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float volumeModifier = 0.5f;
    [Range(0.0f, 2.0f)]
    [SerializeField] private float fadeOutTime = 0.0f;

    private void Start()
    {
        AudioHandler.instance.PlayBGM(backgroundMusic, volumeModifier, fadeOutTime);
    }
}
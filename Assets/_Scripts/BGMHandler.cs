using UnityEngine;

/// <summary>
/// This script should be attached to an object in the game world
/// and modified on a per-level basis in order to set a level's
/// background music. 
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
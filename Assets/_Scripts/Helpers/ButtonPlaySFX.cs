using UnityEngine;

public class ButtonPlaySFX : MonoBehaviour
{
    public AudioClip sFX;
    [Tooltip("Higher values mean lower priority.")]
    public int sFXPriority = 256;
    [Range(0f, 2f)]
    public float volumeMultiplier = 1.0f;

    public void PlaySFX()
    {
        AudioHandler.instance.PlaySFX(sFX,sFXPriority,volumeMultiplier);
    }
}
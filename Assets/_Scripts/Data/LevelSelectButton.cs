using UnityEngine;

/// <summary>
/// This script is a workaround for Unity's inability to accept enum values
/// as parameters for the OnClick function of UI buttons. 
/// The script should be attached to the button and passed as a parameter
/// to the target script. 
/// </summary>
public class LevelSelectButton : MonoBehaviour
{
    public E_LevelType levelType;
}
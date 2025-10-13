using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using Settings = F_GameSettings;

/// <summary>
/// Els Fouche, 10/10/2025
/// Primary game manager. 
/// Provides save/load functionality via an interface.
/// Guarantees that levels it exists in have required components to function.
/// </summary>
public class GameManager : MonoBehaviour
{
    public EventSystem _EventSystem;
    public Camera _MainCamera;
    
    private bool bEventSysFound = false, bMainCamFound = false;
    
    /// <summary>
    /// Determines if the event system has loaded,
    /// Determines if the main camera has loaded.
    /// </summary>
    private void Awake()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            Debug.Log("No event system found: Instantiating.");
            Instantiate(_EventSystem);
            bEventSysFound = false;
        }
        else bEventSysFound = true;

        if (FindFirstObjectByType<Camera>() == null)
        {
            Debug.Log("No main camera found; Instantiating.");
            Instantiate(_MainCamera);
            bMainCamFound = false;
        }
        else bMainCamFound = true;
    }

    /// <summary>
    /// If the event system and/or main camera were not found,
    /// assigns them to the player. 
    /// </summary>
    private void Start()
    {
        GameObject player;

        if ((player = GameObject.FindWithTag("Player")) != null)
        {  
            if (!bEventSysFound && _EventSystem != null)
            {
                player.GetComponent<PlayerInput>().uiInputModule = _EventSystem.GetComponent<InputSystemUIInputModule>();
            }

            if (!bMainCamFound && _MainCamera != null)
            {
                player.GetComponent<PlayerInput>().camera = _MainCamera;
            }
        }
    }
}
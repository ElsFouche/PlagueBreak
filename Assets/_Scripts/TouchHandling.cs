using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using Settings = F_GameSettings;

public class TouchHandling : MonoBehaviour
{
    // Protected
        // Touch Data
    protected Vector2 touchStartPos = new(0.0f, 0.0f), touchEndPos = new(0.0f, 0.0f);
        // Input System
    protected PlayerInput playerInput;
    protected InputAction screenTouched;
    protected InputAction touchPosition;
    private Coroutine doubleClickPrevention = null;

    protected void Awake()
    {
        if (this.gameObject.TryGetComponent<PlayerInput>(out PlayerInput pi))
        {
            playerInput = pi;
        }
        else
        {
            Debug.Log("Fatal: Player input module not found.");
            Destroy(this);
        }

        screenTouched = playerInput.actions["Main/ScreenTouched"];
        touchPosition = playerInput.actions["Main/TouchLocation"];

        StartCoroutine(PostStart());
    }

    protected void OnEnable()
    {
        screenTouched.started += TouchStarted;
        screenTouched.canceled += TouchEnded;
    }

    protected void OnDisable()
    {
        screenTouched.started -= TouchStarted;
        screenTouched.canceled -= TouchEnded;
    }
    
    private IEnumerator PostStart()
    {
        yield return new WaitForSeconds(0.5f);

        // If no camera has been set, use the main camera. 
        if (playerInput)
        {
            if (playerInput.camera == null)
            {
                playerInput.camera = Camera.main;
            }

            if (playerInput.uiInputModule == null)
            {
                if (EventSystem.current.TryGetComponent<InputSystemUIInputModule>(out InputSystemUIInputModule puii))
                {
                    this.playerInput.uiInputModule = puii;
                    // Debug.Log("Player UI input module loaded from current event system in PostStart.");
                }
            }
        }

/*
        Debug.Log("Player Input: " + playerInput.name);
        Debug.Log("Player Camera: " + playerInput.camera.name);
        Debug.Log("Player UI Input Module: " + playerInput.uiInputModule.name);
*/
    }

    /// <summary>
    /// On finger down:
    /// Retrieves the start position of the touch. 
    /// </summary>
    /// <param name="context"></param>
    protected virtual void TouchStarted(InputAction.CallbackContext context)
    {
        if (doubleClickPrevention != null)
        {
            return; 
        }

        doubleClickPrevention = StartCoroutine(ResetClickLockout());
        touchStartPos = GetFingerPosition();
    }

    /// <summary>
    /// On finger up: 
    /// Retrieves the end position of the touch. 
    /// </summary>
    /// <param name="context"></param>
    protected virtual void TouchEnded(InputAction.CallbackContext context)
    {
        touchEndPos = GetFingerPosition();
    }

    /// <summary>
    /// Returns the world position found at the touch point 
    /// based on the projection from the main camera. Dependant
    /// on the player input camera being set up. The main camera's
    /// Z value can impact this method. 
    /// </summary>
    /// <returns></returns>
    protected Vector3 GetFingerPosition()
    {
        if (playerInput == null && this.gameObject.TryGetComponent<PlayerInput>(out PlayerInput pi))
        {
            playerInput = pi;
        }

        if (playerInput.camera == null && this.gameObject.TryGetComponent<Camera>(out Camera camera))
        {
            playerInput.camera = camera;
        }

        Vector3 position = playerInput.camera.ScreenToWorldPoint(
                                            new Vector3(touchPosition.ReadValue<Vector2>().x,
                                                        touchPosition.ReadValue<Vector2>().y,
                                                        playerInput.camera.transform.position.z * -1.0f));
        position.z = transform.position.z;
        return position;
    }

    private IEnumerator ResetClickLockout()
    {
        yield return new WaitForSeconds(0.5f);

        doubleClickPrevention = null;
    }
}
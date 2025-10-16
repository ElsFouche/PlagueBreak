using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using Settings = F_GameSettings;

public class TouchHandling : MonoBehaviour
{
    // Public

    // Protected
        // Touch Data
    protected Vector2 touchStartPos = new(0.0f, 0.0f), touchEndPos = new(0.0f, 0.0f);
        // Input System
    protected PlayerInput playerInput;
    protected InputAction screenTouched;
    protected InputAction touchPosition;

    protected void Awake()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput == null)
        {
            Debug.Log("Fatal: Player input module not found.");
            Destroy(this);
        }
        screenTouched = playerInput.actions["Main/ScreenTouched"];
        touchPosition = playerInput.actions["Main/TouchLocation"];

        if (playerInput.camera == null)
        {
            playerInput.camera = Camera.main;
        }

        if (playerInput.uiInputModule == null)
        {
            if (EventSystem.current.TryGetComponent<InputSystemUIInputModule>(out InputSystemUIInputModule playerInput))
            {
                Debug.Log("Player UI input module loaded from current event system.");
            }
        }
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

    private void Start()
    {
        // If no camera has been set, use the main camera. 
        if (playerInput)
        {
            if (!playerInput.camera)
            {
                playerInput.camera = Camera.main;
            }
        }
    }

    /// <summary>
    /// On finger down:
    /// - Attempts to retrieve the game piece at the touch position. 
    /// </summary>
    /// <param name="context"></param>
    protected virtual void TouchStarted(InputAction.CallbackContext context)
    {
        touchStartPos = GetFingerPosition();
    }

    /// <summary>
    /// 
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
        Vector3 position = playerInput.camera.ScreenToWorldPoint(
                                            new Vector3(touchPosition.ReadValue<Vector2>().x,
                                                        touchPosition.ReadValue<Vector2>().y,
                                                        playerInput.camera.transform.position.z * -1.0f));
        position.z = transform.position.z;
        return position;
    }
}
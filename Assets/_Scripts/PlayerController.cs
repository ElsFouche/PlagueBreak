using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Public
    [SerializeField] private float touchRadius = 1.0f;

    // Private
    // Touch Data
    private Vector2 touchStartPos, touchEndPos;
    private float swipeAngle;
    // Input System
    private PlayerInput playerInput;
    private InputAction screenTouched;
    private InputAction touchPosition;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        screenTouched = playerInput.actions["Main/ScreenTouched"];
        touchPosition = playerInput.actions["Main/TouchLocation"];
    }
    private void OnEnable()
    {
        screenTouched.started += TouchStarted;
        screenTouched.canceled += TouchEnded;
    }
    private void OnDisable()
    {
        screenTouched.started -= TouchStarted;
        screenTouched.canceled -= TouchEnded;
    }
    private void Start()
    {

    }

    private void TouchStarted(InputAction.CallbackContext context)
    {
        Vector3 position = playerInput.camera.ScreenToWorldPoint(
                                            new Vector3(touchPosition.ReadValue<Vector2>().x,
                                                        touchPosition.ReadValue<Vector2>().y, 
                                                        playerInput.camera.transform.position.z * -1.0f));
        position.z = transform.position.z;
        Debug.Log("Touch started at " + position);
    }

    private void TouchEnded(InputAction.CallbackContext context)
    {
        Debug.Log("Touch ended.");
    }
    
    private float CalcAngleDegrees(Vector2 start, Vector2 end)
    {
        return Mathf.Atan2(end.y - start.y, end.x - start.x) * 180 / 5;
    }
}
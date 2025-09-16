using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Public
    [Header("Touch Settings")]
    [SerializeField] 
    private float touchRadius = 1.0f, validSwipeDistance = 1.0f;

    // Private
    // Touch Data
    private Vector2 touchStartPos, touchEndPos;
    // Input System
    private PlayerInput playerInput;
    private InputAction screenTouched;
    private InputAction touchPosition;
    // Game Board & Pieces
    private GameBoard board;
    private GameObject heldPiece;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.Log("Player input module not found.");
            Destroy(this);
        }
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
        board = GameObject.FindGameObjectWithTag("GameBoard").GetComponent<GameBoard>();
        if (board == null)
        {
            Debug.Log("Game board not found. Are you sure you set up the scene correctly?");
            Destroy(this);
        }
    }


    private void Update()
    {
        if (heldPiece != null && screenTouched.phase.IsInProgress())
        {
            heldPiece.transform.position = GetFingerPosition();
        }
    }

    private void TouchStarted(InputAction.CallbackContext context)
    {
        touchStartPos = GetFingerPosition();

        heldPiece = GetPieceTouched(touchStartPos);
    }

    private void TouchEnded(InputAction.CallbackContext context)
    {
        touchEndPos = GetFingerPosition();
        if (heldPiece != null && heldPiece.GetComponent<GamePiece>() != null)
        {
            StartCoroutine(heldPiece.GetComponent<GamePiece>().ReturnPiece(0.2f));
        }
        // Check piece under finger
        // if same as the held piece, lerp held piece back to start 
        // if different, check if valid match
        // if not a valid match, lerp held piece back to start
        // if valid match, lerp new piece to old piece location
        // lerp held piece to new piece location
        // perform match made logic (GameBoard)
    }

    private Vector3 GetFingerPosition()
    {
        Vector3 position = playerInput.camera.ScreenToWorldPoint(
                                            new Vector3(touchPosition.ReadValue<Vector2>().x,
                                                        touchPosition.ReadValue<Vector2>().y,
                                                        playerInput.camera.transform.position.z * -1.0f));
        position.z = transform.position.z;
        return position;
    }
    private GameObject GetPieceTouched(Vector3 position)
    {
        if (board)
        {
            GameObject temp = board.WorldPositionToGamePiece(position);
            if (temp != null)
            {
                return temp;
            }
            else return null;
        }
        else
        {
            Debug.Log("Board script invalid.");
            return null;
        }
    }


    // Debug
    private IEnumerator WigglePiece(GameObject piece)
    {
        for (int i = 0; i < 4; i++)
        {
            piece.transform.position = new Vector3(piece.transform.position.x + 0.02f, piece.transform.position.y, piece.transform.position.z);
            yield return new WaitForSeconds(0.1f);
            piece.transform.position = new Vector3(piece.transform.position.x - 0.02f, piece.transform.position.y, piece.transform.position.z);
            yield return new WaitForSeconds(0.1f);
            piece.transform.position = new Vector3(piece.transform.position.x - 0.02f, piece.transform.position.y, piece.transform.position.z);
            yield return new WaitForSeconds(0.1f);
            piece.transform.position = new Vector3(piece.transform.position.x + 0.02f, piece.transform.position.y, piece.transform.position.z);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
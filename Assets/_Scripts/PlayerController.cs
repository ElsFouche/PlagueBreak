using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using Settings = F_GameSettings;

public class PlayerController : MonoBehaviour
{
    // Public
    [Header("Player Settings")]
    [Range(0f, 10.0f)]
    [SerializeField]
    private float damagePerMatch = 3.0f;
    [SerializeField]
    private RectTransform playerHealthBar;

    // Private
    // Touch Data
    private Vector2 touchStartPos = new(0.0f, 0.0f), touchEndPos = new(0.0f, 0.0f);
    // Input System
    private PlayerInput playerInput;
    private InputAction screenTouched;
    private InputAction touchPosition;
    // Game Board & Pieces
    private GameBoard board;
    private GameObject heldPiece;
    private GamePiece heldPieceData;
    // Enemies
    private EnemyHandler enemyHandler;
    // Player
    private float playerHealth = Settings.playerHealthMax;
    private bool bIsInvincible = false;
    private Coroutine CR_InvincibleTimer = null;
    // Save Info
    private SaveManager saveManager;
    
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.Log("Fatal: Player input module not found.");
            Destroy(this);
        }
        screenTouched = playerInput.actions["Main/ScreenTouched"];
        touchPosition = playerInput.actions["Main/TouchLocation"];

        if (!playerHealthBar)
        {
            Debug.Log("No player health bar found.");
        }
        
        if (!GameObject.FindGameObjectWithTag("SaveSystem").TryGetComponent<SaveManager>(out SaveManager saveManager))
        {
            Debug.Log("No save system found. Saving will not be possible.");
        }
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
        if (!board)
        {
            Debug.Log("Fatal: Game board not found. Are you sure you set up the scene correctly?");
            Application.Quit();
        }

        enemyHandler = GameObject.FindGameObjectWithTag("EnemySystem").GetComponent<EnemyHandler>();
        if (!enemyHandler)
        {
            Debug.Log("Fatal: No enemy handler found. Are you sure you set up the scene correctly?");
            Application.Quit();
        }

        if (playerInput)
        {
            // If no camera has been set, use the main camera. 
            if (!playerInput.camera)
            {
                playerInput.camera = Camera.main;
            }

            // If no UI Input Module has been set, attempt to find it. 
            if (!playerInput.uiInputModule)
            {
                GameObject eventSystem = GameObject.FindGameObjectWithTag("EventSystem");
                if (!eventSystem)
                {
                    Debug.Log("Fatal: No event system in scene.");
                    Application.Quit();
                } else
                {
                    if (eventSystem.GetComponent<EnemyHandler>() != null)
                    {
                        playerInput.uiInputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Update currently only handles simple piece movement. 
    /// </summary>
    private void Update()
    {
        if (heldPiece != null && screenTouched.phase.IsInProgress())
        {
            heldPiece.transform.position = GetFingerPosition();
        }
    }

    /// <summary>
    /// On finger down:
    /// - Attempts to retrieve the game piece at the touch position. 
    /// </summary>
    /// <param name="context"></param>
    private void TouchStarted(InputAction.CallbackContext context)
    {
        touchStartPos = GetFingerPosition();

        heldPiece = GetPieceTouched(touchStartPos);
        if (heldPiece != null)
        {
            heldPieceData = heldPiece.GetComponent<GamePiece>();
        }
    }

    /// <summary>
    /// On finger up: 
    /// - If holding a piece: requests that the swapped pieces check for matches.
    /// - 
    /// </summary>
    /// <param name="context"></param>
    private void TouchEnded(InputAction.CallbackContext context)
    {
        if (heldPiece)
        {
            touchEndPos = GetFingerPosition();
            GameObject touchedPiece = GetPieceTouched(touchEndPos);
            GamePiece touchedPieceData;

            // Check piece under finger
            if (touchedPiece)
            {
                touchedPieceData = touchedPiece.GetComponent<GamePiece>();
            }
            else
            {
                Debug.Log("Invalid piece at end position.");
                if (heldPieceData) StartCoroutine(heldPieceData.ReturnPiece());
                return;
            }

            CheckMatchesAfterMove(touchedPieceData);
        }
    }

    /// <summary>
    /// While holding a game piece:
    /// - Attempts to swap the piece at the end touch position
    ///   with the held piece.
    /// - Checks if a valid match has been made between either
    ///   of the moved pieces. 
    /// - Scores if so, returns the pieces if not. 
    /// </summary>
    /// <param name="swappedPiece"></param>
    private void CheckMatchesAfterMove(GamePiece swappedPiece)
    {
        if (!heldPiece) return;

        bool isAdjacent = false;

        // Check if touched piece is adjacent to our current location.
        foreach (var coord in board.GetAdjacentGridCoords(touchEndPos))
        {
            if (coord == board.WorldPositionToGrid(heldPieceData.GetOriginalPosition()))
            {
                isAdjacent = true; 
                break;
            }
        }

        // If not adjacent, return the piece and exit. 
        if (!isAdjacent)
        {
            StartCoroutine(heldPiece.GetComponent<GamePiece>().ReturnPiece(0.2f));
            return;
        }
        else
        {
            int heldHorizontalMatches, heldVerticalMatches, touchedHorizontalMatches, touchedVerticalMatches;

            board.SwapPieces(heldPieceData.GetOriginalPosition(), swappedPiece.GetOriginalPosition());

            heldHorizontalMatches = heldPieceData.FindHorizontalMatches().Count;
            heldVerticalMatches = heldPieceData.FindVerticalMatches().Count;
            StartCoroutine(heldPieceData.MatchMade()); // Checks number of matches internally

            touchedHorizontalMatches = swappedPiece.FindHorizontalMatches().Count;
            touchedVerticalMatches = swappedPiece.FindVerticalMatches().Count;
            StartCoroutine(swappedPiece.MatchMade()); // Checks number of matches internally

            if (heldHorizontalMatches >= Settings.howManyInAMatch || heldVerticalMatches >= Settings.howManyInAMatch ||
                touchedHorizontalMatches >= Settings.howManyInAMatch || touchedVerticalMatches >= Settings.howManyInAMatch)
            {
                // Determine the base damage from all the matches.
                // Only count pieces beginning at the piece that made the match.
                int damage = (Mathf.Max(heldHorizontalMatches - (Settings.howManyInAMatch - 1), 0) + 
                              Mathf.Max(heldVerticalMatches - (Settings.howManyInAMatch - 1), 0) + 
                              Mathf.Max(touchedVerticalMatches - (Settings.howManyInAMatch - 1), 0) + 
                              Mathf.Max(touchedHorizontalMatches - (Settings.howManyInAMatch - 1), 0));
                HarmEnemiesFromMatchCount(damage);
            } else
            {
                board.SwapPieces(heldPieceData.GetOriginalPosition(), swappedPiece.GetOriginalPosition());
                // Debug.Log("Pieces swapped back.");
            }
        }
    }

    // Damage formula
    private void HarmEnemiesFromMatchCount(int matches)
    {
        // 5 is a magic number and should be expose to allow for designer control of the
        // game's difficulty. Per the below formula, when the player reaches 5 matches they
        // deal double damage. 
        float finalDamage = (damagePerMatch * (float)(1.0f + ((matches - 1) / 5.0f)));
        Debug.Log("Damage dealt: " + finalDamage);
        enemyHandler.DealDamage(finalDamage);
    }

    public void TakeDamage(float damage)
    {
        if (bIsInvincible)
        {
            Debug.Log("Player is invincible.");
            return;
        } else
        {
            Debug.Log("Player damaged: " + damage);
            playerHealth = Mathf.Clamp(playerHealth - damage, 0.0f, Settings.playerHealthMax);
            UpdateHealthDisplay();
            if (playerHealth <= 0.0f)
            {
                Debug.Log("Game over.");
                SceneManager.LoadScene(sceneName: "GameOver");
            } else
            {
                if (CR_InvincibleTimer != null)
                {
                    StopCoroutine(CR_InvincibleTimer);
                    CR_InvincibleTimer = StartCoroutine(InvincibleTimer(Settings.playerISeconds));
                    bIsInvincible = true;
                } else
                {
                    bIsInvincible = true;
                    CR_InvincibleTimer = StartCoroutine(InvincibleTimer(Settings.playerISeconds));
                }
            }
        }
    }

    private void UpdateHealthDisplay()
    {
        if (playerHealthBar)
        {
            playerHealthBar.localScale = new Vector3(playerHealth / Settings.playerHealthMax, 1.0f, 1.0f);
        }
    }

    /// <summary>
    /// Returns the world position found at the touch point 
    /// based on the projection from the main camera. Dependant
    /// on the player input camera being set up. The main camera's
    /// Z value can impact this method. 
    /// </summary>
    /// <returns></returns>
    private Vector3 GetFingerPosition()
    {
        Vector3 position = playerInput.camera.ScreenToWorldPoint(
                                            new Vector3(touchPosition.ReadValue<Vector2>().x,
                                                        touchPosition.ReadValue<Vector2>().y,
                                                        playerInput.camera.transform.position.z * -1.0f));
        position.z = transform.position.z;
        return position;
    }

    /// <summary>
    /// Gets the game piece at the specified position.
    /// Returns null if the game piece is invalid e.g. if it
    /// doesn't contain a GamePiece script. 
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private GameObject GetPieceTouched(Vector3 position)
    {
        if (board)
        {
            GameObject temp = board.WorldPositionToGamePiece(position);
            if (temp != null && temp.GetComponent<GamePiece>() != null)
            {
                return temp;
            } else {
                // Debug.Log("Invalid game piece.");
                return null;
            }
        } else {
            Debug.Log("Board script invalid.");
            return null;
        }
    }

    private IEnumerator InvincibleTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        bIsInvincible = false;
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
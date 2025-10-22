using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using Settings = F_GameSettings;

public class PlayerController : TouchHandling , ISaveLoad
{
    // Public
    [Header("Player Settings")]
    [Range(0f, 10.0f)]
    [SerializeField]
    private float damagePerMatch = 3.0f;
    [SerializeField]
    private RectTransform playerHealthBar;

    // Private
      // Game Board & Pieces
    private GameBoard board;
    private GameObject heldPiece;
    private GamePiece heldPieceData;
    private bool controlLockout = false; 
      // Enemies
    private EnemyHandler enemyHandler;
      // Player Data
    private float playerHealth = Settings.playerHealthMax;
    private bool bIsInvincible = false;
    private Coroutine CR_InvincibleTimer = null;
      // Save Data
    private SaveData saveData;
    
    new private void Awake()
    {
        base.Awake();

        if (!playerHealthBar)
        {
            Debug.Log("No player health bar found.");
        }

        saveData = SaveManager.instance.GetSaveData();
        damagePerMatch *= (1 + ( (float)saveData.playerDamageBoost / 100) );
        playerHealth *= (1 + ( (float)saveData.playerHealthBoost / 100) );
    }

    /// <summary>
    /// Retrieve references to the game board and the enemy handler. 
    /// </summary>
    private void Start()
    {
        if (GameObject.FindGameObjectWithTag("GameBoard").TryGetComponent(out GameBoard gb))
        {
            board = gb;
        } else
        {
            Debug.Log("Fatal: Game board not found. Are you sure you set up the scene correctly?");
            Application.Quit();
        }

        if (GameObject.FindGameObjectWithTag("EnemySystem").TryGetComponent<EnemyHandler>(out EnemyHandler eh))
        {
            enemyHandler = eh;
        } else
        {
            Debug.Log("Fatal: No enemy handler found. Are you sure you set up the scene correctly?");
            Application.Quit();
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

    // ---------------Input---------------

    /// <summary>
    /// On finger down:
    /// - Attempts to retrieve the game piece at the touch position. 
    /// </summary>
    /// <param name="context"></param>
    protected override void TouchStarted(InputAction.CallbackContext ctx)
    {
        base.TouchStarted(ctx);
        if (controlLockout)
        {
            if (heldPiece) { heldPieceData.ReturnPiece(); }
            heldPiece = null;
            return;
        }

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
    protected override void TouchEnded(InputAction.CallbackContext context)
    {
        if (controlLockout && !heldPiece)
        {
            return; 
        } else if (controlLockout)
        {
            heldPieceData.ReturnPiece();
            heldPiece = null;
            return;
        }

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

    public void SetLockout(bool locked)
    {
        controlLockout = locked;
    }

    // ---------------Combat---------------

    /// <summary>
    /// This method allows the player to deal damage to enemies.
    /// The damage dealt is adjusted based on the number of pieces
    /// in a completed match. 
    /// </summary>
    /// <param name="matches"></param>
    private void HarmEnemiesFromMatchCount(int matches)
    {
        // 5 is a magic number and should be expose to allow for designer control of the
        // game's difficulty. Per the below formula, when the player reaches 5 matches they
        // deal double damage. 
        float finalDamage = (damagePerMatch * (float)(1.0f + ((matches - 1) / 5.0f)));
        Debug.Log("Damage dealt: " + finalDamage);
        enemyHandler.DealDamage(finalDamage);
    }

    /// <summary>
    /// This method allows the player to take damage from enemies. 
    /// It includes an invulnerability check and a death check. 
    /// </summary>
    /// <param name="damage"></param>
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
                // This should be updated to use the Scene Handler and death should be
                // implemented thoughtfully. 
                SceneHandler.instance.LoadLevelFromName("GameOver", "GameOver");
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

    // ---------------Interfaces---------------

    // ISaveLoad
    /// <summary>
    /// This method is called in each interface member whenever data is loaded. 
    /// </summary>
    /// <param name="dataToLoad"></param>
    public void LoadData(SaveData dataToLoad)
    {
        saveData = dataToLoad;
    }
    /// <summary>
    /// Update the save data object with local information. 
    /// </summary>
    public void SaveData(ref SaveData savedData)
    {
        // Update savedData with local info
        // savedData.whatever = whatever new
    }

    // ---------------Debug---------------

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
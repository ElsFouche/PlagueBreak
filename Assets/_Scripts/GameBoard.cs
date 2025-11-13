using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PieceTypes = E_PieceTypes.PieceType;
using Settings = F_GameSettings;

public class GameBoard : MonoBehaviour
{
    // Public
    [HideInInspector]
    public Vector2 boardStartPosition, cellSize;
    
    // Private
    [Header("Board Settings")]
    [Range(0.1f, 1.5f)]
    [SerializeField] private float width;
    [Range(0.1f, 1.5f)]
    [SerializeField] private float height;
    [Range(2, 30)]
    [SerializeField] private int divisions;
    [Header("Game Piece Settings")]
    [Range(0.1f, 1.0f)]
    [SerializeField] private float gamePieceWidth;
    [Range(0.1f, 1.0f)]
    [SerializeField] private float gamePieceHeight;
    [SerializeField] private GameObject gamePiece;
    [SerializeField] private List<PieceTypes> potentialPieces = new();
    private List<PieceTypes> generatedPieces = new();
    private Dictionary<Vector2, GameObject> gamePieces;
    private Coroutine boardReset = null;
    private PlayerController playerController;
    // Piece Dropping
    private List<GamePiece> matchedPieces = new();
    private Coroutine CR_WaitToDrop = null;

    // Visualization 

    private void OnValidate()
    {
        divisions = (divisions / 2) * 2;
        boardStartPosition = BoardStartPosition();
        cellSize = CellSize();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        // Debug.Log("Screen to world point width,height: " + Camera.main.ScreenToWorldPoint(new Vector3(width, height, 10.0f)));

        Gizmos.color = Color.cyan;
        if (Camera.main != null)
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(Camera.main.orthographicSize * 2 * Camera.main.aspect * width,
                                                            Camera.main.orthographicSize * 2 * Camera.main.aspect * height, 0.0f));
        } 

        for (int row = 0; row < divisions; row++)
        {
            for (int col = 0; col < divisions; col++)
            {
                // From the grid start, determine our current step based on column and row and
                // the step size for the grid. Then, adjust the position to the center of the cell. 
                Gizmos.DrawCube(new Vector3(
                    boardStartPosition.x + (col * cellSize.x) + (cellSize.x / 2), 
                    boardStartPosition.y + (row * cellSize.y) + (cellSize.y / 2),
                    transform.position.z),
                    new Vector3(gamePieceWidth*cellSize.x, gamePieceHeight*cellSize.y, 0.0f));
            }
        }
    }


    // Play Mode

    private void Awake()
    {
        gamePieces = new Dictionary<Vector2, GameObject>();
        boardStartPosition = BoardStartPosition();
        cellSize = CellSize();
        GameObject tempPlayer = GameObject.FindGameObjectWithTag("Player");
        if (tempPlayer != null)
        {
            if (GameObject.FindWithTag("Player").TryGetComponent<PlayerController>(out PlayerController pc))
            {
                playerController = pc;
            } else
            {
                // Debug.LogError("Fatal: Object tagged as 'Player' does not have a player controller script.");
                // Application.Quit();
            }
        } else
        {
            // Debug.LogError("Fatal: Object tagged as 'Player' does not have a player controller script.");
            // Application.Quit();
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnGamePieces());
    }

    public PlayerController GetPlayerController()
    {
        if (playerController == null)
        {
            return null; 
        } else
        {
            return playerController;
        }
    }

    private IEnumerator SpawnGamePieces()
    {
        GameObject tempGamePiece;

        for (int row = 0; row < divisions; row++)
        {
            for (int col = 0; col < divisions; col++)
            {
                tempGamePiece = Instantiate(gamePiece, new Vector3(
                                            boardStartPosition.x + (col * cellSize.x) + (cellSize.x / 2),
                                            boardStartPosition.y + (row * cellSize.y) + (cellSize.y / 2),
                                            transform.position.z),
                                            new Quaternion(
                                                Quaternion.identity.x, 
                                                180.0f, 
                                                Quaternion.identity.z, 
                                                Quaternion.identity.w));
                tempGamePiece.transform.localScale = new Vector3(gamePieceWidth*cellSize.x, gamePieceHeight*cellSize.y, (gamePieceWidth+gamePieceHeight)/2);
                tempGamePiece.transform.parent = transform;
                
                if (tempGamePiece.GetComponent<GamePiece>() != null)
                {
                    GamePiece tempPieceData = tempGamePiece.GetComponent<GamePiece>();
                    // Initialize basic data
                        tempPieceData.SetNewPosition(tempGamePiece.transform.position);
                        tempPieceData.SetNewRotation(tempGamePiece.transform.rotation);
                        tempPieceData.SetNewScale(tempGamePiece.transform.localScale);
                        tempPieceData.SetGameBoard(this);
                        AssignPieceType(tempPieceData); 
                } else 
                { 
                    Debug.LogError("Fatal: Piece data not found on game object."); 
                    Application.Quit(); 
                }
                    

                gamePieces.Add(new Vector2(col, row), tempGamePiece);

                yield return new WaitForFixedUpdate();
            }
        }
        // DEBUG_GAMEPIECES();
        StartCoroutine(PopulateMatches());
        yield return null;
    }

    // This will need to be wrapped into some sort of difficulty setting after
    // accounting for the new piece replacement setup.
    // Currently it is called when a wave is completed. Eliminate that call unless the game is in hard mode.

    /// <summary>
    /// This function initiates a coroutine that scrambles all the pieces on
    /// the game board. 
    /// </summary>
    public void ResetBoard()
    {
        // Prevent player control until after the board has reset.
        playerController.SetLockout(true);
        
        // Check if coroutine is active to prevent double activation.
        if (boardReset == null)
        {
            boardReset = StartCoroutine(SlowBoardReset());
        }
    }

    private IEnumerator SlowBoardReset()
    {
        List<GamePiece> tempPieces = new List<GamePiece>();

        foreach (var piece in gamePieces.Values)
        {
            if (piece.TryGetComponent<GamePiece>(out GamePiece gp))
            {
                tempPieces.Add(gp);
            }
        }

        tempPieces.Shuffle();

        foreach (var piece in tempPieces)
        {
            AssignPieceType(piece);
            yield return new WaitForSeconds(0.01f);
        }

        // Return player control.
        playerController.SetLockout(false);

        // Release coroutine lockout.
        boardReset = null;
    }

    /// <summary>
    /// Randomizes the piece passed as input. If randomPiece is 
    /// set to true this will be truly random. If set false or unset
    /// the piece will be generated pseudo-randomly. 
    /// </summary>
    /// <param name="pieceData"></param>
    /// <param name="randomPiece"></param>
    private void AssignPieceType(GamePiece pieceData, bool randomPiece = false)
    {
        if (potentialPieces.Count < 1)
        {
            potentialPieces.Add(PieceTypes.Red);
            // Debug.Log("Piece types not set for the level. Remember to set up the game state completely!");
        }

        // Assign a pseudo-random piece type
        // Generate a piece using a new random value. 
        int rndIndex = UnityEngine.Random.Range(0, potentialPieces.Count);
        PieceTypes randomPieceType = potentialPieces[rndIndex];
        
        if (randomPiece)
        {
            pieceData.SetPieceType(randomPieceType);
            return;
        }

        /*
        /// The below functionality checks to ensure pieces are random
        /// based on the historic placement of them. It has been dummied
        /// out because it does not check vertical matches. However,
        /// it is likely much more performant than the implemented solution.
        /// As such, it has been left in case it proves necessary to refactor
        /// to accomdate the performance requirements of the target devices. 
                if (!generatedPieces.Contains(randomPieceType))
                {
                    generatedPieces.Clear();
                    generatedPieces.Add(randomPieceType);
                    pieceData.SetPieceType(randomPieceType);
                }
                else if (generatedPieces.LastIndexOf(randomPieceType) < 1)
                {
                    generatedPieces.Add(randomPieceType);
                    pieceData.SetPieceType(randomPieceType);
                }
                else
                {
                    PieceTypes newRandomPiece = ScrambleMatchedPiece(randomPieceType);
                    generatedPieces.Add(ScrambleMatchedPiece(newRandomPiece));
                    pieceData.SetPieceType(newRandomPiece);
                }
        */

        pieceData.SetPieceType(randomPieceType);

        for (int i = 0; i < 10; i++)
        {
            if (pieceData.FindHorizontalMatches().Count < Settings.howManyInAMatch - 1 && 
                pieceData.FindVerticalMatches().Count < Settings.howManyInAMatch - 1)
            {
                break;
            } else
            {
                PieceTypes newRandomPiece = ScrambleMatchedPiece(pieceData.GetPieceType());
                pieceData.SetPieceType(newRandomPiece);
            }
        }
    }

    private PieceTypes ScrambleMatchedPiece(PieceTypes match)
    {
        // Remove piece that would generate a match
        potentialPieces.Remove(match);
        int rndIndex = UnityEngine.Random.Range(0, potentialPieces.Count);
        PieceTypes newRandomPiece = potentialPieces[rndIndex];
        generatedPieces.Clear();
        generatedPieces.Add(newRandomPiece);
        // Re-add the piece that would have generated a match
        potentialPieces.Add(match);

        return newRandomPiece;
    }

    /// <summary>
    /// This function is used as a check to prevent generating pre-existing matches.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PopulateMatches()
    {
        List<GameObject> pieces = new List<GameObject>(gamePieces.Values); 

        foreach (GameObject gamePiece in pieces)
        {
            gamePiece.GetComponent<GamePiece>().FindHorizontalMatches();
            gamePiece.GetComponent<GamePiece>().FindVerticalMatches();
            StartCoroutine(gamePiece.GetComponent<GamePiece>().MatchMade());
            yield return new WaitForFixedUpdate();  
        }
    }
    
    // Board Data

    private Vector2 BoardStartPosition()
    {
        if (Camera.main == null)
        {
            return Vector2.zero;
        }
        return new Vector2((transform.position.x - Camera.main.orthographicSize * 2 * Camera.main.aspect * (float)width / 2), 
                           (transform.position.y - Camera.main.orthographicSize * 2 * Camera.main.aspect * (float)height / 2));
    }
    private Vector2 CellSize()
    {
        if (Camera.main == null)
        {
            return Vector2.zero;
        }
        return new Vector2(Camera.main.orthographicSize * 2 * Camera.main.aspect * (float)width / divisions, 
                           Camera.main.orthographicSize * 2 * Camera.main.aspect * (float)height / divisions);
    }

    // Conversions
    
    /// <summary>
    /// Transforms the input position into grid coordinate space.
    /// The grid coordinate is used as the key to access the corresponding 
    /// game piece. 
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector2 WorldPositionToGrid(Vector2 position)
    {
        Vector2 gridCoord;
        gridCoord = new Vector2(
                                (int)((position.x - boardStartPosition.x) / cellSize.x), 
                                (int)((position.y - boardStartPosition.y) / cellSize.y));
        return gridCoord;
    }

    /// <summary>
    /// Returns the game piece at the given grid coordinates.
    /// Returns null if no game piece is found. 
    /// </summary>
    /// <param name="gridCoord"></param>
    /// <returns></returns>
    public GameObject GridCoordToGamePiece(Vector2 gridCoord)
    {
        if (gamePieces.ContainsKey(gridCoord))
        {
            return gamePieces[gridCoord];
        }
        else return null;
    }
   
    /// <summary>
    /// Finds the game piece nearest to the input position so long as 
    /// the input is within the game board boundary. 
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject WorldPositionToGamePiece(Vector2 position)
    {
        return GridCoordToGamePiece(WorldPositionToGrid(position));
    }

    /// <summary>
    /// Returns the north, east, south, and west coordinates which
    /// are directly adjacent to the input value. The input value
    /// is automatically converted to the nearest grid coordinate. 
    /// </summary>
    /// <param name="center"></param>
    /// <returns></returns>
    public List<Vector2> GetAdjacentGridCoords(Vector2 center)
    {
        Vector2 coordCenter = WorldPositionToGrid(center);

        List<Vector2> adjacentCoords = new()
        {
            // North / Vertical Up
            new Vector2(coordCenter.x, coordCenter.y + 1),
            // East / Horizontal Right
            new Vector2(coordCenter.x + 1, coordCenter.y),
            // South / Vertical Down
            new Vector2(coordCenter.x, coordCenter.y - 1),
            // West / Horizontal Left
            new Vector2(coordCenter.x - 1, coordCenter.y)
        };
        
        return adjacentCoords;
    }

    public GamePiece GetVerticalNeighbor(Vector2 piecePosition)
    {
        GameObject tempPieceObj = GridCoordToGamePiece(new Vector2(piecePosition.x, piecePosition.y + 1));

        if (tempPieceObj == null)
        {
            return null;
        }

        GamePiece tempPieceData; 

        if (tempPieceObj.TryGetComponent<GamePiece>(out GamePiece gp))
        {
            tempPieceData = gp;
        } else
        {
            tempPieceData = null;
        }

        return tempPieceData;
    }

    public GamePiece GetVerticalNeighbor(GamePiece startPiece)
    {
        Vector2 startPieceLocation = WorldPositionToGrid(startPiece.GetOriginalPosition());

        return GetVerticalNeighbor(startPieceLocation);
    }


    /// <summary>
    /// Returns the first key of the found game piece, if it exists. 
    /// If not, returns a (-1,-1) Vector2.
    /// </summary>
    /// <param name="piece"></param>
    /// <returns></returns>
    public Vector2 GetKeyFromGamePiece(GameObject piece)
    {
        foreach (var key in gamePieces.Keys)
        {
            if (gamePieces[key] == piece)
            {
                return key;
            }
        } 
        return new Vector2(-1,-1);
    }

    // Piece Swapping

    /// <summary>
    /// Swaps the supplied game piece locations. Supplied locations must be in world space!
    /// Game pieces have their home location updated. The game board's dictionary is updated 
    /// with the new pieces based on their new home location. Returns true if successful.
    /// </summary>
    /// <param name="pieceA"></param>
    /// <param name="pieceB"></param>
    /// <param name="returnPieces"></param>
    /// <returns></returns>
    public bool SwapPieces(Vector2 pieceA, Vector2 pieceB, bool returnPieces = true)
    {
        // Convert input values to grid space. 
        pieceA = WorldPositionToGrid(pieceA);
        pieceB = WorldPositionToGrid(pieceB);
        // Attempt to store game piece data based on input coordinates.
        GamePiece pieceAData = gamePieces[pieceA].GetComponent<GamePiece>();
        GamePiece pieceBData = gamePieces[pieceB].GetComponent<GamePiece>();

        return SwapPieces(pieceAData, pieceBData, returnPieces);
    }

    /// <summary>
    /// Swaps the supplied game pieces. Game pieces have their home location
    /// updated. The game board's dictionary is updated with the new pieces based on
    /// their new home location. Returns true if successful. 
    /// </summary>
    /// <param name="pieceA"></param>
    /// <param name="pieceB"></param>
    /// <param name="returnPieces"></param>
    /// <returns></returns>
    public bool SwapPieces(GameObject pieceA, GameObject pieceB, bool returnPieces = true)
    {
        GamePiece pieceAData = pieceA.GetComponent<GamePiece>();
        GamePiece pieceBData = pieceB.GetComponent<GamePiece>();

        return SwapPieces(pieceAData, pieceBData, returnPieces);
    }

    /// <summary>
    /// Swaps the supplied game pieces. Game pieces have their home location
    /// updated. The game board's dictionary is updated with the new pieces based on
    /// their new home location. Returns true if successful. 
    /// </summary>
    /// <param name="pieceAData"></param>
    /// <param name="pieceBData"></param>
    /// <param name="returnPieces"></param>
    /// <returns></returns>
    public bool SwapPieces(GamePiece pieceAData, GamePiece pieceBData, bool returnPieces = true)
    {
        // Return valid game pieces if the other piece is not. 
        if (pieceAData == null)
        {
            if (pieceBData == null)
            {
                Debug.Log("Invalid game pieces, cannot swap.");
                return false;
            }
            else
            {
                StartCoroutine(pieceBData.ReturnPiece());
                Debug.Log("Invalid game piece A, cannot swap.");
                return false;
            }
        }
        else if (pieceBData == null)
        {
            StartCoroutine(pieceAData.ReturnPiece());
            Debug.Log("Invalid game piece B, cannot swap.");
            return false;
        }

        // ------------------Swap------------------ 

        // Update game board's knowledge of swapped pieces
        GameObject objectA = pieceAData.gameObject;
        gamePieces[WorldPositionToGrid(pieceAData.GetOriginalPosition())] = pieceBData.gameObject;
        gamePieces[WorldPositionToGrid(pieceBData.GetOriginalPosition())] = objectA;

        // Swap resting positions
        var posA = pieceAData.GetOriginalPosition();
        pieceAData.SetNewPosition(pieceBData.GetOriginalPosition());
        pieceBData.SetNewPosition(posA);

        if (returnPieces)
        {
            // Visually put pieces in new positions
            StartCoroutine(pieceAData.ReturnPiece(0.4f));
            StartCoroutine(pieceBData.ReturnPiece(0.4f));
        }

        return true;
    }

    public void AddPiecesToDrop(GamePiece pieceInMatch, int waitTime = 1)
    {
        // Add Unique (how tf is this not built in?)
        if (!matchedPieces.Contains(pieceInMatch))
        {
            matchedPieces.Add(pieceInMatch);
        }

        // Wait while pieces accumulate in the list 
        if (CR_WaitToDrop == null)
        {
            CR_WaitToDrop = StartCoroutine(WaitToDrop(waitTime));
        }
    }

    private IEnumerator WaitToDrop(int framesUntilDrop = 1)
    {
        int temp = 0;
        while (temp < framesUntilDrop)
        {
            yield return new WaitForEndOfFrame();
            temp++;
        }

        DropPieces();
        CR_WaitToDrop = null;
    }

    private void DropPieces()
    {
        List<GamePiece> nullPieces = new(matchedPieces);
        // Remove all null pieces from the list 
        foreach (var piece in nullPieces)
        {
            if (piece.GetPieceType() != PieceTypes.None)
            {
                matchedPieces.Remove(piece);
            }
        }

        // Release unnecessary memory
        matchedPieces.TrimExcess();

        List<GamePiece> swappedPieces = new();

        GamePiece vertNeighbor;
        
        // Bubble algorithm: Move all matched pieces to the top of the column
        foreach (var piece in matchedPieces)
        {
            // Add current piece to the list of pieces that will need to return home later
            if (!swappedPieces.Contains(piece))
            {
                swappedPieces.Add(piece);
            }

            // Retrieve our vertical neighbor
            vertNeighbor = GetVerticalNeighbor(piece);

            // Bubble piece to the top of the column
            while (vertNeighbor != null)
            {
                // Add the neighbor to the pieces that will need to be returned at the end. 
                if (!swappedPieces.Contains(vertNeighbor))
                {
                    swappedPieces.Add(vertNeighbor);
                }

                // Attempt to swap the pieces. 
                if (!SwapPieces(piece, vertNeighbor, false))
                {
                    Debug.LogError("Attempted to swap invalid pieces. How did this happen?");
                    break; 
                }

                // Get our new neighbor
                vertNeighbor = GetVerticalNeighbor(piece);
            }

            // Vertical neighbor is now null: We are at the top of the column.
        }

        // After bubbling all pieces to the top...
        // 1) Assign them new piece types
        // 2) Make them invisible
        // 3) Move them up in Y 
        foreach (var piece in matchedPieces)
        {
            // Assign a new random piece type. This may need to wait until after all pieces have been bubbled up
            // by calling it during the foreach to return them home? 
            AssignPieceType(piece, true);

            // Set the piece invisible 
            if (piece.gameObject.TryGetComponent<MeshRenderer>(out var renderer))
            {
                renderer.enabled = false;
            }

            // Move the piece up in Y so that it 'falls' into place when we return all the pieces.
            Vector3 tempPos = piece.GetOriginalPosition();
            piece.gameObject.transform.position = new Vector3(tempPos.x, tempPos.y + Settings.newPieceSpawnOffset, tempPos.z);
        }

        // Return all pieces home 
        foreach (var piece in swappedPieces)
        {
            if (piece.gameObject.TryGetComponent<MeshRenderer>(out var renderer))
            {
                renderer.enabled = true;
            }

            StartCoroutine(piece.ReturnPiece(1.5f, true));
        }
    }
}


// Todo:
// Establish our grid using the double for loop. 
// (The grid creation can happen at game start)
// Create an array of Vector2s which represent the game board's cells.
// Shuffle the array. 
// Iterate over the length of the array, instantiating game pieces. 

// Required methods: 
// Shift column down 
//      for every piece above a hole,
//          shift their key position down 1
//          shift their start position to the
//              new position based on the new key. 
//          run 'return piece' on the shifted pieces
// 
// 

// Matches:
// Find all pairs that exist on the board 
// Moving a piece causes a check to see if it forms a new pair with
// any of the pieces in the existing pairs.
// Any pairs that have a shared piece are marked as complete
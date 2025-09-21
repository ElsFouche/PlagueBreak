using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PieceTypes = E_PieceTypes.PieceType;

public class GameBoard : MonoBehaviour
{
    // Public
    [HideInInspector]
    public Vector2 boardStartPosition, cellSize;
    
    // Private
    [Header("Board Settings")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [Range(2, 30)]
    [SerializeField] private int divisions;
    [Header("Game Piece Settings")]
    [SerializeField] private float gamePieceWidth;
    [SerializeField] private float gamePieceHeight;
    [SerializeField] private GameObject gamePiece;

    private Dictionary<Vector2, GameObject> gamePieces;

    // Editor Only

    private void OnValidate()
    {
        divisions = (divisions / 2) * 2;
        boardStartPosition = BoardStartPosition();
        cellSize = CellSize();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0.0f));
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
                    new Vector3(gamePieceWidth, gamePieceHeight, 0.0f));
            }
        }
    }


    // Play Mode

    private void Awake()
    {
        gamePieces = new Dictionary<Vector2, GameObject>();
        boardStartPosition = BoardStartPosition();
        cellSize = CellSize();
    }

    private void Start()
    {
        StartCoroutine(SpawnGamePieces());
    }

    private IEnumerator SpawnGamePieces()
    {
        GameObject tempGamePiece;

        List<PieceTypes> generatedPieces = new();
        List<PieceTypes> potentialPieces = new();
        // potentialPieces = generatedPieces.Keys.ToList();
        foreach (PieceTypes val in Enum.GetValues(typeof(PieceTypes)))
        {
            potentialPieces.Add(val);
        }
        potentialPieces.Remove(PieceTypes.None);
        potentialPieces.Remove(PieceTypes.Powerup1);
        potentialPieces.Remove(PieceTypes.Powerup2);
        potentialPieces.Remove(PieceTypes.Powerup3);

        for (int row = 0; row < divisions; row++)
        {
            for (int col = 0; col < divisions; col++)
            {
                tempGamePiece = Instantiate(gamePiece, new Vector3(
                                            boardStartPosition.x + (col * cellSize.x) + (cellSize.x / 2),
                                            boardStartPosition.y + (row * cellSize.y) + (cellSize.y / 2),
                                            transform.position.z),
                                            Quaternion.identity);
                tempGamePiece.transform.localScale = new Vector3(gamePieceWidth, gamePieceHeight, (gamePieceWidth+gamePieceHeight)/2);
                tempGamePiece.transform.parent = transform;
                
                if (tempGamePiece.GetComponent<GamePiece>() != null)
                {
                    GamePiece tempPieceData = tempGamePiece.GetComponent<GamePiece>();
                    // Initialize basic data
                        // tempPieceData.SetNewKey(new Vector2(row, col));
                        tempPieceData.SetNewPosition(tempGamePiece.transform.position);
                        tempPieceData.SetNewRotation(tempGamePiece.transform.rotation);
                        tempPieceData.SetNewScale(tempGamePiece.transform.localScale);
                        tempPieceData.SetGameBoard(this);
                    // Assign a pseudo-random piece type
                        // Generate a piece using a new random value. 
                        int rndIndex = UnityEngine.Random.Range(0, potentialPieces.Count);
                        PieceTypes randomPieceType = potentialPieces[rndIndex];

                        if (!generatedPieces.Contains(randomPieceType))
                        {
                            generatedPieces.Clear();
                            generatedPieces.Add(randomPieceType);
                            tempPieceData.SetPieceType(randomPieceType);
                        } 
                        else if (generatedPieces.LastIndexOf(randomPieceType) < 1)
                        {
                            generatedPieces.Add(randomPieceType);
                            tempPieceData.SetPieceType(randomPieceType);
                        } else
                        {
                            // Remove piece that would generate a match
                            potentialPieces.Remove(randomPieceType);
                            rndIndex = UnityEngine.Random.Range(0, potentialPieces.Count);
                            PieceTypes newRandomPiece = potentialPieces[rndIndex];
                            generatedPieces.Clear();
                            generatedPieces.Add(newRandomPiece);
                            tempPieceData.SetPieceType(newRandomPiece);
                            // Re-add the piece that would have generated a match
                            potentialPieces.Add(randomPieceType);
                        }
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
        PopulateMatches();
        yield return null;
    }

    private void PopulateMatches()
    {
        foreach (GameObject gamePiece in gamePieces.Values)
        {
            gamePiece.GetComponent<GamePiece>().FindMatches();
        }
    }

    
    // Board Data

    private Vector2 BoardStartPosition()
    {
        return new Vector2(transform.position.x - (float)width / 2, transform.position.y - (float)height / 2);
    }
    private Vector2 CellSize()
    {
        return new Vector2((float)width / divisions, (float)height / divisions);
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
            new Vector2(coordCenter.x, coordCenter.y + 1),
            new Vector2(coordCenter.x + 1, coordCenter.y),
            new Vector2(coordCenter.x, coordCenter.y - 1),
            new Vector2(coordCenter.x - 1, coordCenter.y)
        };
        
        return adjacentCoords;
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

    /// <summary>
    /// Swaps the game pieces at the input positions, if found.
    /// Returns true if successful. 
    /// </summary>
    /// <param name="pieceA"></param>
    /// <param name="pieceB"></param>
    public bool SwapPieces(Vector2 pieceA, Vector2 pieceB)
    {
        // Convert input values to grid space. 
        pieceA = WorldPositionToGrid(pieceA);
        pieceB = WorldPositionToGrid(pieceB);
        // Attempt to store game piece data based on input coordinates.
        GamePiece pieceAData = gamePieces[pieceA].GetComponent<GamePiece>();
        GamePiece pieceBData = gamePieces[pieceB].GetComponent<GamePiece>();
        
        // Return valid game pieces if the other piece is not and exit.
        if (pieceAData == null)
        {
            if (pieceBData == null)
            {
                Debug.Log("Invalid game pieces, cannot swap.");
                return false;
            } else {
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

        // Update game board's knowledge of swapped pieces
        GameObject objectA = GridCoordToGamePiece(pieceA);
        gamePieces[pieceA] = GridCoordToGamePiece(pieceB);
        gamePieces[pieceB] = objectA;
        
        // Swap resting positions
        var posA = pieceAData.GetOriginalPosition();
        pieceAData.SetNewPosition(pieceBData.GetOriginalPosition());
        pieceBData.SetNewPosition(posA);

        // Visually put pieces in new positions
        StartCoroutine(pieceAData.ReturnPiece(0.4f));
        StartCoroutine(pieceBData.ReturnPiece(0.4f));

        return true;
    }

    /// <summary>
    /// Swaps the input game objects. Returns true if successful.
    /// </summary>
    /// <param name="pieceA"></param>
    /// <param name="pieceB"></param>
    public bool SwapPieces(GameObject pieceA, GameObject pieceB)
    {
        GamePiece pieceAData = pieceA.GetComponent<GamePiece>();
        GamePiece pieceBData = pieceB.GetComponent<GamePiece>();

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

        // Update game board's knowledge of swapped pieces
        GameObject objectA = pieceA;
        gamePieces[WorldPositionToGrid(pieceAData.GetOriginalPosition())] = pieceB;
        gamePieces[WorldPositionToGrid(pieceBData.GetOriginalPosition())] = objectA;
               
        // Swap resting positions
        var posA = pieceAData.GetOriginalPosition();
        pieceAData.SetNewPosition(pieceBData.GetOriginalPosition());
        pieceBData.SetNewPosition(posA);

        // Visually put pieces in new positions
        StartCoroutine(pieceAData.ReturnPiece(0.4f));
        StartCoroutine(pieceBData.ReturnPiece(0.4f));

        return true;
    }
    /*
        public void FindMatches()
        {
            // Check for matches
            if (!pieceAData.FindMatches() && !pieceBData.FindMatches())
            {

            }
            bool pieceAMatch = pieceAData.FindMatches();
            bool pieceBMatch = pieceBData.FindMatches();



            GameObject pieceAGameObject = gamePieces[pieceA];
            // Update key to game piece association
            gamePieces[pieceA] = gamePieces[pieceB];
            gamePieces[pieceB] = pieceAGameObject;

            // gamePieces[pieceA].

            // setnewposition
            // clearmatches
            // if findmatches pieceA and findmatches pieceB are false, return both
        }
    */
    /*
        private void DEBUG_GAMEPIECES()
        {
            foreach (var gamePiece in gamePieces)
            {
                Debug.Log("Key: " + gamePiece.Key + "    |    Location: " +  gamePiece.Value.transform.position);
            }
        }
    */
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
using NUnit.Framework.Internal.Filters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Editor Only
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
                        tempPieceData.SetNewKey(new Vector2(row, col));
                        tempPieceData.SetNewPosition(tempGamePiece.transform.position);
                        tempPieceData.SetNewRotation(tempGamePiece.transform.rotation);
                        tempPieceData.SetNewScale(tempGamePiece.transform.localScale);
                        tempPieceData.SetPieceType(E_PieceTypes.PieceType.None);
                        tempPieceData.SetNewParent(this.gameObject);
                } else { Debug.LogError("Fatal: Piece data not found on game object."); Application.Quit(); }
                    

                gamePieces.Add(new Vector2(row, col), tempGamePiece);

                yield return new WaitForFixedUpdate();
            }
        }
        // DEBUG_GAMEPIECES();
        yield return null;
    }

    
    private Vector2 BoardStartPosition()
    {
        return new Vector2(transform.position.x - (float)width / 2, transform.position.y - (float)height / 2);
    }
    private Vector2 CellSize()
    {
        return new Vector2((float)width / divisions, (float)height / divisions);
    }

    /// <summary>
    /// Finds the game piece nearest to the input position so long as 
    /// the input is within the game board boundary. 
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject WorldPositionToGamePiece(Vector2 position)
    {
        Vector2 gamePieceKey;
        gamePieceKey = new Vector2((int)((position.y - boardStartPosition.x) / cellSize.x), (int)((position.x - boardStartPosition.y) / cellSize.y));

        // Debug.Log("Game piece key: " +  gamePieceKey);

        if (gamePieces.ContainsKey(gamePieceKey))
        {
            return gamePieces[gamePieceKey];
        } else return null;
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
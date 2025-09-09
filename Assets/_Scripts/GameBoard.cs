using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
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

    private Dictionary<Vector2, GameObject> gamePieces = new();

    private void OnValidate()
    {
        divisions = (divisions / 2) * 2;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        // Determine the (lower left) start position with respect to the parent object.
        Vector2 gridStart = new Vector2(transform.position.x - (float)width / 2, 
                                        transform.position.y - (float)height / 2);
        // Determine the size of our grid cells.
        Vector2 cellSize = new Vector2((float)width / divisions, (float)height / divisions);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0.0f));
        for (int row = 0; row < divisions; row++)
        {
            for (int col = 0; col < divisions; col++)
            {
                // From the grid start, determine our current step based on column and row and
                // the step size for the grid. Then, adjust the position to the center of the cell. 
                Gizmos.DrawCube(new Vector3(
                    gridStart.x + (col * cellSize.x) + (cellSize.x / 2), 
                    gridStart.y + (row * cellSize.y) + (cellSize.y / 2),
                    transform.position.z), 
                    new Vector3(gamePieceWidth, gamePieceHeight, 0.0f));
            }
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnGamePieces());
    }

    private IEnumerator SpawnGamePieces()
    {
        Vector2 gridStart = new Vector2(transform.position.x - (float)width / 2,
                                        transform.position.y - (float)height / 2);
        Vector2 cellSize = new Vector2((float)width / divisions, (float)height / divisions);
        GameObject tempGamePiece; 

        for (int row = 0; row < divisions; row++)
        {
            for (int col = 0; col < divisions; col++)
            {
                tempGamePiece = Instantiate(gamePiece, new Vector3(
                                            gridStart.x + (col * cellSize.x) + (cellSize.x / 2),
                                            gridStart.y + (row * cellSize.y) + (cellSize.y / 2),
                                            transform.position.z),
                                            Quaternion.identity);
                tempGamePiece.transform.localScale = new Vector3(gamePieceWidth, gamePieceHeight, (gamePieceWidth+gamePieceHeight)/2);
                tempGamePiece.transform.parent = transform;
                gamePieces.Add(new Vector2(row,col), tempGamePiece);

                yield return new WaitForFixedUpdate();
            }
        }
        yield return null;
    }
}

// Establish our grid using the double for loop. 
// (The grid creation can happen at game start)
// Create an array of Vector2s which represent the game board's cells.
// Shuffle the array. 
// Iterate over the length of the array, instantiating game pieces. 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PieceTypes = E_PieceTypes.PieceType;

public class GamePiece : MonoBehaviour
{
    // private Vector2 key;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private GameBoard gameBoard;
    private PieceTypes pieceType;
    private List<Vector2> horizontalMatches = new();
    private List<Vector2> verticalMatches = new();

    // Getters

    public Vector3 GetOriginalPosition()
    {
        return originalPosition;
    }

    public Quaternion GetOriginalRotation()
    {
        return originalRotation;
    }

    public Vector3 GetOriginalScale()
    {
        return originalScale;
    }

    public PieceTypes GetPieceType()
    {
        return pieceType;
    }

    public List<Vector2> GetHorizontalMatches()
    {
        return horizontalMatches;
    }

    public List<Vector2> GetVerticalMatches()
    {
        return verticalMatches;
    }
    
    // Setters

    public void SetNewPosition(Vector3 newPosition)
    {
        originalPosition = new Vector3(newPosition.x, newPosition.y, newPosition.z);
    }

    public void SetNewRotation(Quaternion newRotation)
    {
        originalRotation = new Quaternion(newRotation.x, newRotation.y, newRotation.z, newRotation.w);
    }

    public void SetNewScale(Vector3 newScale)
    {
        originalScale = new Vector3(newScale.x, newScale.y, newScale.z);
    }

    public void SetGameBoard(GameBoard newGameBoard)
    {
        this.gameBoard = newGameBoard;
    }

    public PieceTypes SetPieceType(PieceTypes type)
    {
        pieceType = type;
        MeshRenderer pieceColor = this.GetComponent<MeshRenderer>();

        switch (type) 
        {
            case PieceTypes.Red:
                pieceColor.material.color = new Color(1.0f,0.0f,0.0f);
                break;
            case PieceTypes.Green:
                pieceColor.material.color = new Color(0.0f, 1.0f, 0.0f);
                break;
            case PieceTypes.Blue:
                pieceColor.material.color = new Color(0.0f, 0.0f, 1.0f);
                break;
            case PieceTypes.Yellow:
                pieceColor.material.color = new Color(1.0f, 1.0f, 0.0f);
                break;
        }
        return type;
    }

    public void AddHorizontalMatch(Vector2 gridCoord)
    {
        horizontalMatches.Add(gridCoord);
        if (horizontalMatches.Count > 2 )
        {
            // Match made
            Debug.Log("Horizontal match made.");
            // Debug
            foreach (var piece in horizontalMatches)
            {
                gameBoard.GridCoordToGamePiece(piece).gameObject.SetActive(false);
            }
            this.gameObject.SetActive(false);
        }
    }

    public void AddVerticalMatch(Vector2 gridCoord)
    {
        verticalMatches.Add(gridCoord);
        if (verticalMatches.Count > 2 )
        {
            // Match made
            Debug.Log("Vertical match made.");
            // Debug
            foreach (var piece in verticalMatches)
            {
                gameBoard.GridCoordToGamePiece(piece).gameObject.SetActive(false);
            }
            this.gameObject.SetActive(false);
        }
    }

    public void RemoveHorizontalMatch(Vector2 gridCoord)
    {
        horizontalMatches.Remove(gridCoord);
    }

    public void RemoveVerticalMatch(Vector2 gridCoord)
    {
        verticalMatches.Remove(gridCoord);
    }

    public void ClearMatches()
    {
        horizontalMatches.Clear();
        verticalMatches.Clear();
    }

    // Methods

    public IEnumerator ReturnPiece(float time = 1.0f)
    {
        while (Vector3.Distance(transform.position, GetOriginalPosition()) > 0.0001f)
        {
            // Debug.Log("Piece position: " + transform.position);
            // Debug.Log("Original position: " + GetOriginalPosition());
            // Debug.Log("Distance remaining: " + Vector3.Distance(transform.position, GetOriginalPosition()));

            transform.position = new Vector3(
                                     Mathf.SmoothStep(transform.position.x, GetOriginalPosition().x, time),
                                     Mathf.SmoothStep(transform.position.y, GetOriginalPosition().y, time),
                                     Mathf.SmoothStep(transform.position.z, GetOriginalPosition().z, time));


            yield return new WaitForFixedUpdate();
        }

        yield return null;
    }

    /// <summary>
    /// BUGGED
    /// </summary>
    /// <returns></returns>
    public bool FindMatches()
    {
        GamePiece temp;
        int numMatches = 0;
        Vector2 gridLocation = gameBoard.WorldPositionToGrid(originalPosition);

        foreach (var tempKey in gameBoard.GetAdjacentGridCoords(originalPosition))
        {
            if (gameBoard.GridCoordToGamePiece(tempKey))
            {
                temp = gameBoard.GridCoordToGamePiece(tempKey).GetComponent<GamePiece>();
            }
            else
            {
                Debug.Log("Adjacent piece at: " + tempKey + " not found.");

                return false;
            }

            if (temp.GetPieceType() == pieceType)
            {
                // Debug.Log("Key: " + tempKey);
                if (tempKey.y - gridLocation.y == 0)
                {
                    AddHorizontalMatch(tempKey);
                    gameBoard.GridCoordToGamePiece(tempKey).GetComponent<GamePiece>().AddHorizontalMatch(gridLocation);
                } else {
                    AddVerticalMatch(tempKey);
                    gameBoard.GridCoordToGamePiece(tempKey).GetComponent<GamePiece>().AddVerticalMatch(gridLocation);
                }

                numMatches++;
            }
        }
        
        return numMatches > F_GameSettings.PiecesInAMatch();
    }
}
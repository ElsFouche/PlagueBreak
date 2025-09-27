using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PieceTypes = E_PieceTypes.PieceType;

public class GamePiece : MonoBehaviour
{
    private GameBoard gameBoard;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
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
    public void SetGameBoard(GameBoard newGameBoard)
    {
        this.gameBoard = newGameBoard;
    }

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


    public PieceTypes SetPieceType(PieceTypes type)
    {
        pieceType = type;

        MeshRenderer pieceColor = this.GetComponent<MeshRenderer>();

        if (pieceColor == null)
        {
            return type;
        }

        switch (type) 
        {
            case PieceTypes.None:
                pieceColor.material.color = new Color(0.2f, 0.2f, 0.2f);
                break;
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
        if (!horizontalMatches.Contains(gridCoord))
        {
            horizontalMatches.Add(gridCoord);
        }
    }

    public void AddVerticalMatch(Vector2 gridCoord)
    {
        if (!verticalMatches.Contains(gridCoord))
        {
            verticalMatches.Add(gridCoord);
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
    /// Recursively searches horizontally to find matches.
    /// Will branch vertically if matches are found. 
    /// Returns the grid positions of the matches found. 
    /// </summary>
    /// <returns></returns>
    public List<Vector2> FindHorizontalMatches(GamePiece searchChainOrigin = null)
    {
        GamePiece tempData;
        Vector2 gridLocation = gameBoard.WorldPositionToGrid(originalPosition);
        Vector2 originLocation = gridLocation;
        if (searchChainOrigin)
        {
            originLocation = gameBoard.WorldPositionToGrid(searchChainOrigin.GetOriginalPosition());
        }

        horizontalMatches.Clear();

        foreach (Vector2 tempKey in gameBoard.GetAdjacentGridCoords(originalPosition))
        {
            // Debug.Log("Checking position: [" + tempKey.x + ", " + tempKey.y + "]");
            // If a valid game piece exists at the adjacent grid coord, store its data
            if (gameBoard.GridCoordToGamePiece(tempKey))
            {
                tempData = gameBoard.GridCoordToGamePiece(tempKey).GetComponent<GamePiece>();
            }
            else
            {
                // Debug.Log("Adjacent piece at: " + tempKey + " not found.");
                continue;
            }

            
            if (tempData.GetPieceType() == pieceType)
            {
                if (tempKey.y - gridLocation.y == 0)
                {
                    // Debug.Log("Piece data " + this.GetInstanceID() + " found a horizontal match.");
                    if (originLocation != tempKey)
                    {
                        // Debug.Log("Check backtrack prevented.");
                        tempData.FindHorizontalMatches(this);
                    } 

                    AddHorizontalMatch(tempKey);
                    foreach (Vector2 match in tempData.GetHorizontalMatches())
                    {
                        AddHorizontalMatch(match);
                    }
                }
            }
        }

        // Debug.Log("Piece data " + this.GetInstanceID() + " has " + horizontalMatches.Count + " horizontal matches.");
        return horizontalMatches;
    }

    /// <summary>
    /// Recursively searches vertically to find matches.
    /// Will branch horizontally if matches are found. 
    /// Returns the grid positions of the matches found. 
    /// </summary>
    /// <param name="searchChainOrigin"></param>
    /// <returns></returns>
    public List<Vector2> FindVerticalMatches(GamePiece searchChainOrigin = null)
    {
        GamePiece tempData;
        Vector2 gridLocation = gameBoard.WorldPositionToGrid(originalPosition);
        Vector2 originLocation = gridLocation;
        if (searchChainOrigin)
        {
            originLocation = gameBoard.WorldPositionToGrid(searchChainOrigin.GetOriginalPosition());
        }

        verticalMatches.Clear();

        foreach (Vector2 tempKey in gameBoard.GetAdjacentGridCoords(originalPosition))
        {
            // Debug.Log("Checking position: [" + tempKey.x + ", " + tempKey.y + "]");
            
            // If a valid game piece exists at the adjacent grid coord, store its data
            if (gameBoard.GridCoordToGamePiece(tempKey))
            {
                tempData = gameBoard.GridCoordToGamePiece(tempKey).GetComponent<GamePiece>();
            }
            else
            {
                // Debug.Log("Adjacent piece at: " + tempKey + " not found.");
                continue;
            }


            if (tempData.GetPieceType() == pieceType)
            {
                if (tempKey.y - gridLocation.y != 0)
                {
                    // Debug.Log("Piece data " + this.GetInstanceID() + " found a vertical match.");
                    if (originLocation != tempKey)
                    {
                        // Debug.Log("Check backtrack prevented.");
                        tempData.FindVerticalMatches(this);
                    } 
                    
                    AddVerticalMatch(tempKey);
                    foreach (Vector2 match in tempData.GetVerticalMatches())
                    {
                        AddVerticalMatch(match);
                    }
                }
            }
        }

        // Debug.Log("Piece data " + this.GetInstanceID() + " has " + verticalMatches.Count + " vertical matches.");
        return verticalMatches;
    }

    public IEnumerator MatchMade()
    {
        yield return new WaitForFixedUpdate();

        if (horizontalMatches.Count > 2)
        {
            // Debug
            foreach (Vector2 tempKey in horizontalMatches)
            {
                gameBoard.GridCoordToGamePiece(tempKey).GetComponent<GamePiece>().SetPieceType(PieceTypes.None);
            }
        }

        if (verticalMatches.Count > 2)
        {
            // Debug
            foreach (Vector2 tempKey in verticalMatches)
            {
                gameBoard.GridCoordToGamePiece(tempKey).GetComponent<GamePiece>().SetPieceType(PieceTypes.None);
            }
        }

        if (horizontalMatches.Count > 2 || verticalMatches.Count > 2)
        {
            SetPieceType(PieceTypes.None);
        }
    }
}
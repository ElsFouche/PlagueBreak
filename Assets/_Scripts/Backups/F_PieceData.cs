using UnityEngine;
using System.Collections;

public class F_PieceData : MonoBehaviour
{
    private Vector2 key;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private GameObject parentGameBoard;    
    private E_PieceTypes.PieceType pieceType;
/*
    // Constructors

    /// <summary>
    /// Constructor.
    /// </summary>
    public F_PieceData()
    {
        key = new Vector2(-1.0f, -1.0f);
        // originalTransform = null;
        originalPosition = Vector3.zero;
        parentGameBoard = null;
        pieceType = E_PieceTypes.PieceType.None;
    }

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="originalPosition"></param>
    /// <param name="originalRotation"></param>
    /// <param name="originalScale"></param>
    /// <param name="parentGameBoard"></param>
    /// <param name="pieceType"></param>
    public F_PieceData(Vector2 key, Transform originalTransform, GameObject parentGameBoard, E_PieceTypes.PieceType pieceType)
    {
        this.key = key;
        // this.originalTransform = originalTransform;
        originalPosition = Vector3.zero;
        this.parentGameBoard = parentGameBoard;
        this.pieceType = pieceType;
    }
*/
    // Getters

    public Vector2 GetKey()
    {
        return key;
    }

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

    public GameObject GetOriginalParent()
    {
        return parentGameBoard;
    }

    public E_PieceTypes.PieceType GetPieceType()
    {
        return pieceType;
    }

    // Setters

    public void SetNewKey(Vector2 newKey)
    {
        key = newKey;
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

    public void SetNewParent(GameObject newParent)
    {
        parentGameBoard = newParent;
    }

    public void SetPieceType(E_PieceTypes.PieceType type)
    {
        pieceType = type;
    }

    public IEnumerator ReturnPiece(float time)
    {
        while (Vector3.Distance(transform.position, GetOriginalPosition()) > 0.0001f)
        {
            Debug.Log("Piece position: " + transform.position);
            Debug.Log("Original position: " + GetOriginalPosition());

            transform.position = new Vector3(
                                                Mathf.SmoothStep(transform.position.x, GetOriginalPosition().x, time),
                                                Mathf.SmoothStep(transform.position.y, GetOriginalPosition().y, time),
                                                Mathf.SmoothStep(transform.position.z, GetOriginalPosition().z, time));

            Debug.Log("Distance remaining: " + Vector3.Distance(transform.position, GetOriginalPosition()));

            yield return new WaitForFixedUpdate();
        }
     
    yield return null;
    }
}
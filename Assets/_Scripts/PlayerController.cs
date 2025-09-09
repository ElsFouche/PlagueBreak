using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Private
    private Vector2 touchStartPos, touchEndPos;
    private float swipeAngle;



    private float CalcAngleDegrees(Vector2 start, Vector2 end)
    {
        return Mathf.Atan2(end.y - start.y, end.x - start.x) * 180 / 5;
    }
}
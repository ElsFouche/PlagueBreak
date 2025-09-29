using System;
using UnityEngine;

[Serializable]
public class E_Direction
{
    public enum EDirection4
    {
        Invalid = -1,
        North = 0,
        East = 2,
        South = 4,
        West = 6
    }

    public enum EDirection8
    {
        Invalid = -1,
        North = 0,
        Northeast = 1,
        East = 2,
        Southeast = 3,
        South = 4,
        Southwest = 5,
        West = 6,
        Northwest = 7
    }

    /// <summary>
    /// This function converts two vectors into a cardinal direction,
    /// north, east, south, or west. It accomplishes this by treating
    /// the start point (a) as the center of a unit circle and the end
    /// point (b) as a point on that unit circle. Then it finds the angle
    /// between line segment ab and an arbitrary line segment ac. In this 
    /// case, c is defined as the point on the circle that corresponds with 
    /// the northwest direction on a compass. 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public EDirection4 Direction4Way(Vector2 start, Vector2 end)
    {
        EDirection4 direction4 = EDirection4.Invalid;

        // Transform vector to local space relative to the start location.
        // Determine the angle relative to the northwest.
        int angle = (int)Vector2.SignedAngle(end - start, new Vector2(-1, 1));

        // Normalize to 360 degrees.
        angle = (angle + 360) % 360;

        // Convert to 0,1,2, or 3 based on 90 degree slices of the clock face.
        angle /= 90;

        if (Enum.IsDefined(typeof(EDirection4), angle * 2))
        {
            direction4 = (EDirection4)(angle * 2);
        }

        return direction4;
    }

    /// <summary>
    /// This function converts two vectors into a cardinal direction,
    /// north, east, south, or west. It accomplishes this by treating
    /// the start point (a) as the center of a unit circle and the end
    /// point (b) as a point on that unit circle. Then it finds the angle
    /// between line segment ab and an arbitrary line segment ac. In this 
    /// case, c is defined as the point on the circle that corresponds with 
    /// the northwest direction on a compass. 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public EDirection8 Direction8Way(Vector2 start, Vector2 end)
    {
        EDirection8 direction8 = EDirection8.Invalid;

        // Transform vector to local space relative to the start location.
        // Determine the angle relative to the northwest.
        int angle = (int)Vector2.SignedAngle(end - start, new Vector2(-1, 1));

        // Normalize to 360 degrees.
        angle = (angle + 360) % 360;

        // Convert to 0,1,2, or 3 based on 90 degree slices of the clock face.
        angle /= 45;

        if (Enum.IsDefined(typeof(EDirection8), angle))
        {
            direction8 = (EDirection8)(angle);
        }
        
        return direction8;
    }
}

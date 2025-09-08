using UnityEngine;

public static class ExitDirection
{
    /// <summary>
    /// All the byte direction to be used in the flowfield and their corressponding number.
    /// </summary>
    public static byte NotPathable = 0;
    public static byte IsExit = 123;
    public static byte North = 1;
    public static byte East = 2;
    public static byte West = 3;
    public static byte South = 4;
    public static byte NorthEast = 5;
    public static byte SouthEast = 6;
    public static byte SouthWest = 7;
    public static byte NorthWest = 8;

    /// <summary>
    /// Enum with all the Exitdirections.
    /// </summary>
    public enum ExitDirections
    {
        NoExit,
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }
    /// <summary>
    /// Function used to convert between an Exitdirections enum and the corresponding byte.
    /// </summary>
    /// <param name="dir">The Exitdirection to convert to the corresponding byte.</param>
    /// <returns>The Byte corresponding to the provided Exitdirection.</returns>
    public static byte DirectionToByte(ExitDirections dir)
    {
        if (dir == ExitDirections.North) { return North; }
        else if (dir == ExitDirections.South) { return South; }
        else if (dir == ExitDirections.West) { return West; }
        else if (dir == ExitDirections.East) { return East; }
        else if (dir == ExitDirections.NorthEast) { return NorthEast; }
        else if (dir == ExitDirections.SouthWest) { return SouthWest; }
        else if (dir == ExitDirections.SouthEast) { return SouthEast; }
        else if (dir == ExitDirections.NorthWest) { return NorthWest; }
        return NotPathable;
    }
    /// <summary>
    /// Function used to convert between an byte direction and the corresponding Exitdirection.
    /// </summary>
    /// <param name="dir">The byte to convert to the corresponding Exitdirection.</param>
    /// <returns>The Exitdirection corresponding to the provided byte.</returns>
    public static Vector2Int ByteToVector(byte byt)
    {
        if (byt == North) { return new(-1, 0); }
        else if (byt == South) { return new(1, 0); }
        else if (byt == West) { return new(0, -1); }
        else if (byt == East) { return new(0, 1); }
        else if (byt == NorthEast) { return new(-1, 1); }
        else if (byt == NorthWest) { return new(-1, -1); }
        else if (byt == SouthEast) { return new(1, 1); }
        else if (byt == SouthWest) { return new(1, -1); }
        else { return new(0, 0); }
    }
    /// <summary>
    /// Function used to convert between an Vector2Int direction and the corresponding byte.
    /// </summary>
    /// <param name="dir">The Vector2Int to convert to the corresponding byte.</param>
    /// <returns>The byte corresponding to the provided Vector2Int.</returns>
    public static byte VectorToByte(Vector2Int direction)
    {
        if (direction.x == 0)
        {
            if (direction.y > 0)
            {
                return East;
            }
            else if (direction.y < 0)
            {
                return West;
            }
            else
            {
                return NotPathable;
            }
        }
        else if (direction.y == 0)
        {
            if (direction.x > 0)
            {
                return South;
            }
            else
            {
                return North;
            }
        }
        else
        {
            if (direction.x > 0)
            {
                if (direction.y > 0)
                {
                    return SouthEast;
                }
                else
                {
                    return SouthWest;
                }
            }
            else
            {
                if (direction.y > 0)
                {
                    return NorthEast;
                }
                else
                {
                    return NorthWest;
                }
            }
        }
    }
    /// <summary>
    /// Function used to convert between an Vector2Int direction and the corresponding Exitdirection.
    /// </summary>
    /// <param name="dir">The Vector2Int to convert to the corresponding Exitdirection.</param>
    /// <returns>The Exitdirection corresponding to the provided Vector2Int.</returns>
    public static ExitDirections DirectionToExitDiretion(Vector2Int direction)
    {
        if (direction.x == 0)
        {
            if (direction.y > 0)
            {
                return ExitDirections.East;
            }
            else if (direction.y < 0)
            {
                return ExitDirections.West;
            }
            else
            {
                return ExitDirections.NoExit;
            }
        }
        else if (direction.y == 0)
        {
            if (direction.x > 0)
            {
                return ExitDirections.South;
            }
            else
            {
                return ExitDirections.North;
            }
        }
        else
        {
            if (direction.x > 0)
            {
                if (direction.y > 0)
                {
                    return ExitDirections.SouthEast;
                }
                else
                {
                    return ExitDirections.SouthWest;
                }
            }
            else
            {
                if (direction.y > 0)
                {
                    return ExitDirections.NorthEast;
                }
                else
                {
                    return ExitDirections.NorthWest;
                }
            }
        }
    }
}
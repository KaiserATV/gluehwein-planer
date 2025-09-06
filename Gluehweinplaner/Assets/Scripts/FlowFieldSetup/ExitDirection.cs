using System;
using UnityEngine;

public class ExitDirection
{
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

    public static byte DirectionToByte(ExitDirections dir)
    {
        if (dir == ExitDirections.North) { return North; }
        else if (dir == ExitDirections.South) { return South; }
        else if (dir == ExitDirections.West) { return West; }
        else if (dir == ExitDirections.East) { return East; }
        else if (dir == ExitDirections.NorthEast) { return NorthEast; }
        else if (dir == ExitDirections.SouthWest) { return SouthWest; }
        else if (dir == ExitDirections.SouthEast) { return SouthEast; }
        else if (dir == ExitDirections.NorthWest){ return NorthWest; }
        return NotPathable;
    }

    public static Vector2Int ByteToVector(byte byt)
    {
        if (byt == North) { return new(-1, 0); }
        else if (byt == South) { return new(1, 0); }
        else if (byt == West) { return new(0, -1); }
        else if (byt == East) { return new(0, 1); }
        else if (byt == NorthEast) { return new(-1, 1); }
        else if (byt == NorthWest) { return new(-1, -1); }
        else if (byt == SouthEast) { return new(1, 1); }
        else if(byt == SouthWest) { return new(1, -1); }
        else { return new(0, 0); }
    }
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
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class New_Plate
{
    public New_Plate(Vector3 pos, int[,] bcm)
    {
        Center = pos;
        BaseCostMatrix = bcm;
    }
    public Vector3 Center { get; set; }
    public Vector3 Size { get; set; }

    public bool HasNoObstacles = false;

    public bool HasOnlyObstacles = false;
    public int Rows { get; set; }
    public int Columns { get; set; }
    public int[,] BaseCostMatrix { get; set; }

    public List<New_GoalNode> AllContainedGoalNodes = new List<New_GoalNode>();

    public Dictionary<New_GoalNode, int[,]> AllGoalNodeAndDistanceFields = new Dictionary<New_GoalNode, int[,]>();

    public List<ExitDirection> CanExit = new List<ExitDirection>();


    public void AddContainedGoalNode(New_GoalNode node)
    {
        AllContainedGoalNodes.Add(node);
    }

    public List<New_GoalNode> GetAllContainedGoalNodes() { return AllContainedGoalNodes; }


    public int[,] GetDistanceFieldForGoal(New_GoalNode goalNode)
    {
        return AllGoalNodeAndDistanceFields[goalNode];
    }

    /// <summary>
    /// Adds an Distance Field to an GoalNode 
    /// </summary>
    /// <param name="goal">the Goal node</param>
    /// <param name="distance">the coressponding distancefield</param>
    public void AddGoalNodeAndDistanceField(New_GoalNode goalNode, int[,] distance)
    {
        AddContainedGoalNode(goalNode);
        AllGoalNodeAndDistanceFields.Add(goalNode, distance);
    }


    //          North (-X)
    //  West (-Z)        East (+Z)
    //          South (+X)
    //
    public void FindAllExitableDirections()
    {
        if (CanExitInDirection(ExitDirection.North))
        {
            CanExit.Add(ExitDirection.North);
        }
        if (CanExitInDirection(ExitDirection.West))
        {
            CanExit.Add(ExitDirection.West);
        }
        if (CanExitInDirection(ExitDirection.East))
        {
            CanExit.Add(ExitDirection.East);
        }
        if (CanExitInDirection(ExitDirection.South))
        {
            CanExit.Add(ExitDirection.South);
        }
        if (New_SceneManager.pathDiagonal)
        {
            if (CanExitInDirection(ExitDirection.NorthEast))
            {
                CanExit.Add(ExitDirection.NorthEast);
            }
            if (CanExitInDirection(ExitDirection.SouthEast))
            {
                CanExit.Add(ExitDirection.SouthEast);
            }
            if (CanExitInDirection(ExitDirection.SouthWest))
            {
                CanExit.Add(ExitDirection.SouthWest);
            }
            if (CanExitInDirection(ExitDirection.NorthWest))
            {
                CanExit.Add(ExitDirection.NorthWest);
            }
        }
    }


    public bool CanExitInDirection(ExitDirection exit)
    { 
        Vector2Int startPos = new Vector2Int();
        Vector2Int endPos = new Vector2Int();

        switch (exit)
        {
            case ExitDirection.North:
                startPos = new Vector2Int(0, 0);
                endPos = new Vector2Int(0, Columns - 1);
                break;
            case ExitDirection.NorthEast:
                return BaseCostMatrix[0,Columns-1] != New_GenerateMatrix.MatrixObstacleValue;
            case ExitDirection.East:
                startPos = new Vector2Int(0, Columns - 1);
                endPos = new Vector2Int(Rows - 1, Columns - 1);
                break;
            case ExitDirection.SouthEast:
                return BaseCostMatrix[Rows-1, Columns - 1] != New_GenerateMatrix.MatrixObstacleValue;
            case ExitDirection.South:
                startPos = new Vector2Int(Rows - 1, 0);
                endPos = new Vector2Int(Rows - 1, Columns - 1);
                break;
            case ExitDirection.SouthWest:
                return BaseCostMatrix[Rows-1, 0] != New_GenerateMatrix.MatrixObstacleValue;
            case ExitDirection.West:
                startPos = new Vector2Int(0, 0);
                endPos = new Vector2Int(Rows - 1, 0);
                break;
            case ExitDirection.NorthWest:
                return BaseCostMatrix[0, 0] != New_GenerateMatrix.MatrixObstacleValue;
        }


        Vector2Int diff = endPos - startPos;
        int stepsIndirection = Math.Max(diff.x, diff.y);
        int invalidTiles = stepsIndirection+1;
        diff.x = diff.x / stepsIndirection;
        diff.y = diff.y / stepsIndirection;

        while (stepsIndirection >= 0)
        {
            if (BaseCostMatrix[startPos.x, startPos.y] == New_GenerateMatrix.MatrixObstacleValue)
            {
                invalidTiles--;
            }
            stepsIndirection--;
            startPos += diff;
        }
        return !(invalidTiles == 0);
    }




    public List<Vector3> GetShortestPathToExitVector3(Vector3 exit, Vector3 start)
    {
        List<Vector2Int> steps2Int = GetShortestPathToExitVector2(GetPositionInArray(exit), start);
        List<Vector3> returnList = new List<Vector3>();
        foreach (Vector2Int step in steps2Int)
        {
            returnList.Add(GetSubTileCenterWorldCoordinates(step));
        }
        return returnList;
    }


    public List<Vector2Int> GetShortestPathToExitVector2(Vector2Int exit, Vector3 start)
    {
        if (HasNoObstacles)
        {
            if (New_SceneManager.pathDiagonal)
            {
                return New_GenerateMatrix.PathDiagonal(GetPositionInArray(start), exit);
            }
            else
            {
                return New_GenerateMatrix.InterpolateArray(GetPositionInArray(start), exit);
            }
        }
        else if (!HasNoObstacles && !HasOnlyObstacles)
        {
            //ToDo: Cache this shit
            int[,] distance = New_GenerateMatrix.GenerateDistanceField(BaseCostMatrix, Rows, Columns, exit,null);
            return New_GenerateMatrix.GetBestPathInDistanceMatrix(distance, Rows, Columns, GetPositionInArray(start));
        }
        return new List<Vector2Int>();
    }

    //needs to handle values that are negative
    public Vector3 GetSubTileCenterWorldCoordinates(Vector2Int pos)
    {
        return new Vector3((New_GenerateMatrix.TileSizeX * pos.x) + (Center.x - Size.x / 2) + (New_GenerateMatrix.TileSizeX / 2), 0, New_GenerateMatrix.TileSizeZ * pos.y + (Center.z - Size.z / 2) + New_GenerateMatrix.TileSizeZ / 2);
    }
    public Vector3 GetSubTileCenterWorldCoordinates(int rows, int cols)
    {
        return new Vector3((New_GenerateMatrix.TileSizeX * rows) + (Center.x - Size.x / 2) + (New_GenerateMatrix.TileSizeX / 2), 0, New_GenerateMatrix.TileSizeZ * cols + (Center.z - Size.z / 2) + New_GenerateMatrix.TileSizeZ / 2);
    }
    public Vector2Int GetPositionInArray(Vector3 positionVector3)
    {
        return new Vector2Int(Math.Clamp(Mathf.FloorToInt((positionVector3.x - (Center.x - (Rows * New_GenerateMatrix.TileSizeX) / 2)) / New_GenerateMatrix.TileSizeX) , 0, Rows-1), Math.Clamp(Mathf.FloorToInt((positionVector3.z - (Center.z - (Columns * New_GenerateMatrix.TileSizeZ) / 2)) / New_GenerateMatrix.TileSizeZ), 0, Columns-1));
    }


    public int GetValueAtPosition(Vector3 position)
    {
        Vector3 diff = new Vector3(Center.x - Size.x / 2, 0, Center.z - Size.z / 2) - position;
        return BaseCostMatrix[Mathf.FloorToInt(diff.x / (New_GenerateMatrix.TileSizeX)), Mathf.FloorToInt(diff.z / (New_GenerateMatrix.TileSizeZ))];
    }

    public int GetValueAtPosition(int row, int column)
    {
        return BaseCostMatrix[row, column];
    }



}

public enum ExitDirection
{
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest
}

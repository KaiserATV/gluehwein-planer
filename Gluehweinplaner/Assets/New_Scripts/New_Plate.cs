using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#nullable enable


public class New_Plate
{
    public New_Plate(Vector3 pos, int[,] bcm)
    {
        Center = pos;
        BaseCostMatrix = bcm;
    }
    public bool hasChanged = false;
    public Vector3 Center { get; set; }
    public Vector3 Size { get; set; }

    public bool HasNoObstacles = false;

    public bool HasOnlyObstacles = false;
    public int Rows { get; set; }
    public int Columns { get; set; }
    public int[,] BaseCostMatrix { get; set; }

    public List<New_GoalNode> AllContainedGoalNodes = new List<New_GoalNode>();

    public Dictionary<Vector2Int, int[,]> GoalPositionToDistanceField = new Dictionary<Vector2Int, int[,]>();

    public Dictionary<(Vector2Int, Vector2Int), List<Vector2Int>> AllTakenPaths = new Dictionary<(Vector2Int, Vector2Int), List<Vector2Int>>();

    public List<ExitDirection> CanExit = new List<ExitDirection>();

    public Dictionary<New_Bude, List<Vector2Int>> budeToOccupiedSpaces = new Dictionary<New_Bude, List<Vector2Int>>();

    public void AddContainedGoalNode(New_GoalNode node)
    {
        AllContainedGoalNodes.Add(node);
    }

    public List<New_GoalNode> GetAllContainedGoalNodes() { return AllContainedGoalNodes; }


    /// <summary>
    /// Adds an Distance Field to an GoalNode 
    /// </summary>
    /// <param name="goal">the Goal node</param>
    /// <param name="distance">the coressponding distancefield</param>
    public void AddGoalNode(New_GoalNode goalNode, bool canPathDiagonal)
    {
        if (AllContainedGoalNodes.Contains(goalNode)) { return; }
        AddContainedGoalNode(goalNode);
        if (GoalPositionToDistanceField.ContainsKey(GetPositionInArray(goalNode.Position, true))) { return; }
        GoalPositionToDistanceField.Add(GetPositionInArray(goalNode.Position,true), (HasNoObstacles || HasOnlyObstacles) ? new int[0, 0] : New_GenerateMatrix.GenerateDistanceField(this, goalNode.Position, null, canPathDiagonal));
        hasChanged = false;
    }


    //          North (-X)
    //  West (-Z)        East (+Z)
    //          South (+X)
    //
    public void FindAllExitableDirections(bool canPathDiagonal)
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
        if (canPathDiagonal)
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
                return BaseCostMatrix[0, Columns - 1] != New_GenerateMatrix.MatrixObstacleValue;
            case ExitDirection.East:
                startPos = new Vector2Int(0, Columns - 1);
                endPos = new Vector2Int(Rows - 1, Columns - 1);
                break;
            case ExitDirection.SouthEast:
                return BaseCostMatrix[Rows - 1, Columns - 1] != New_GenerateMatrix.MatrixObstacleValue;
            case ExitDirection.South:
                startPos = new Vector2Int(Rows - 1, 0);
                endPos = new Vector2Int(Rows - 1, Columns - 1);
                break;
            case ExitDirection.SouthWest:
                return BaseCostMatrix[Rows - 1, 0] != New_GenerateMatrix.MatrixObstacleValue;
            case ExitDirection.West:
                startPos = new Vector2Int(0, 0);
                endPos = new Vector2Int(Rows - 1, 0);
                break;
            case ExitDirection.NorthWest:
                return BaseCostMatrix[0, 0] != New_GenerateMatrix.MatrixObstacleValue;
        }


        Vector2Int diff = endPos - startPos;
        int stepsIndirection = Math.Max(diff.x, diff.y);
        int invalidTiles = stepsIndirection + 1;
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


    public List<Vector3> GetShortestPathToExitVector3(Vector3 exit, Vector3 start, bool canPathDiagonal)
    {
        List<Vector2Int> steps2Int = GetShortestPathToExitVector2(GetPositionInArray(exit, true), GetPositionInArray(start, true), canPathDiagonal, exit);
        List<Vector3> returnList = new List<Vector3>();
        foreach (Vector2Int step in steps2Int)
        {
            returnList.Add(GetSubTileCenterWorldCoordinates(step));
        }
        return returnList;
    }


    public List<Vector2Int> GetShortestPathToExitVector2(Vector2Int exit, Vector2Int start, bool canPathDiagonal, Vector3 worldExit)
    {
        if (AllTakenPaths.ContainsKey((start, exit))) { return AllTakenPaths[(start, exit)]; }
        if (HasNoObstacles)
        {
            if (canPathDiagonal)
            {
                List<Vector2Int> path = New_GenerateMatrix.PathDiagonal(start, exit);
                AllTakenPaths.Add((start, exit), path);
                return path;
            }
            else
            {
                List<Vector2Int> path = New_GenerateMatrix.InterpolateArray(start, exit, ((Vector2Int a, Vector2Int b) compare) => CloserPlateToGoal(compare.a, compare.b, worldExit), canPathDiagonal, Rows, Columns);
                AllTakenPaths.Add((start, exit), path);
                return path;
            }
        }
        else if (!HasNoObstacles && !HasOnlyObstacles)
        {
            int[,] distance;
            if (GoalPositionToDistanceField.ContainsKey(exit))
            {
                distance = GoalPositionToDistanceField[exit];
            }
            else
            {
                distance = New_GenerateMatrix.GenerateDistanceField(BaseCostMatrix, Rows, Columns, exit, null, canPathDiagonal);
                GoalPositionToDistanceField.Add(exit, distance);
            }
            List<Vector2Int> path = New_GenerateMatrix.GetBestPathInDistanceMatrix(distance, Rows, Columns, start, canPathDiagonal);
            AllTakenPaths.Add((start, exit), path);
            return path;
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
    public Vector2Int GetPositionInArray(Vector3 positionVector3, bool safe)
    {
        if (safe)
        {
            return new Vector2Int(Math.Clamp(Mathf.FloorToInt((positionVector3.x - (Center.x - (Rows * New_GenerateMatrix.TileSizeX) / 2)) / New_GenerateMatrix.TileSizeX), 0, Rows - 1), Math.Clamp(Mathf.FloorToInt((positionVector3.z - (Center.z - (Columns * New_GenerateMatrix.TileSizeZ) / 2)) / New_GenerateMatrix.TileSizeZ), 0, Columns - 1));
        }
        else
        {
            return new Vector2Int(Mathf.FloorToInt((positionVector3.x - (Center.x - (Rows * New_GenerateMatrix.TileSizeX) / 2)) / New_GenerateMatrix.TileSizeX), Mathf.FloorToInt((positionVector3.z - (Center.z - (Columns * New_GenerateMatrix.TileSizeZ) / 2)) / New_GenerateMatrix.TileSizeZ));
        }
    }


    public int GetValueAtPosition(Vector3 position)
    {
        return BaseCostMatrix[GetPositionInArray(position, true).x, GetPositionInArray(position, true).y];
    }

    public int GetValueAtPosition(int row, int column)
    {
        return BaseCostMatrix[row, column];
    }


    public Vector2Int OccupySpaces(New_Bude b, Vector2Int start, Vector3 exitWorld, bool canPathDiagonal)
    {
        List<Vector2Int> stepsTaken = new List<Vector2Int> { start };
        Vector2Int goalPos = GetPositionInArray(exitWorld, false);
        stepsTaken = New_GenerateMatrix.InterpolateArray(start, goalPos, ((Vector2Int a, Vector2Int b) compare) => CloserPlateToGoal(compare.a, compare.b, exitWorld), false, Rows, Columns);

        if (Contains(goalPos))
        {
            stepsTaken.Add(goalPos);
        }
        //Debug.Log("Pos: ");
        foreach (Vector2Int step in stepsTaken)
        {
            //Debug.Log(GetSubTileCenterWorldCoordinates(step));
            BaseCostMatrix[step.x, step.y] += New_GenerateMatrix.MatrixObstacleValue;
        }
        

        if (budeToOccupiedSpaces.ContainsKey(b))
        {
            budeToOccupiedSpaces[b].AddRange(stepsTaken);
        }
        else
        {
            budeToOccupiedSpaces.Add(b, stepsTaken);
        }

        hasChanged = true;
        //prob less needed to do
        RecalcWakable();
        FindAllExitableDirections(canPathDiagonal);
        GoalPositionToDistanceField = new Dictionary<Vector2Int, int[,]>();
        AllTakenPaths = new Dictionary<(Vector2Int, Vector2Int), List<Vector2Int>>();
        return stepsTaken.Last();
    }

    public Vector2Int CloserPlateToGoal (Vector2Int a, Vector2Int b, Vector3 exit)
    {
        return (Vector3.Distance(GetSubTileCenterWorldCoordinates(a), exit) < Vector3.Distance(GetSubTileCenterWorldCoordinates(b), exit)) ? a : a;
    }

    public void RecalcWakable()
    {
        int clearTiles = 0;
        int obstacleTiles = 0;
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                if (BaseCostMatrix[row, column] == New_GenerateMatrix.MatrixObstacleValue)
                {
                    obstacleTiles++;
                }
                else
                {
                    clearTiles++;
                }
            }
        }
        if (clearTiles == Rows * Columns)
        {
            HasNoObstacles = true;
        }
        else
        {
            HasNoObstacles = false;
        }
        if (obstacleTiles == Rows * Columns)
        {
            HasOnlyObstacles = true;
        }
        else
        {
            HasOnlyObstacles = false;
        }
    }

    public bool Contains(Vector2Int check)
    {
        return check.x >= 0 && check.y >= 0 && check.x <= Rows - 1 && check.y <= Columns - 1;
    }
}

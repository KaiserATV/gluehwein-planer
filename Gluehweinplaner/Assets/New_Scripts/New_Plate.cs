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

    public List<ExitDirection> InvalidExist = new List<ExitDirection> { ExitDirection.North,ExitDirection.East,ExitDirection.West,ExitDirection.South};

    public Dictionary<New_GoalNode, int[,]> AllGoalNodeAndDistanceFields = new Dictionary<New_GoalNode, int[,]>();

    public Dictionary<ExitDirection, Vector2Int> ExitAndCorespondingExitpoint = new Dictionary<ExitDirection, Vector2Int>();

    public Dictionary<ExitDirection, int[,]> AllExitsAndDistanceFields = new Dictionary<ExitDirection, int[,]>();

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

    /// <param name="exitDirection"></param>
    /// <returns>An int[,] for the corresponding Direction of exit.</returns>
    public int[,] GetDistanceFieldForExit(ExitDirection exitDirection)
    {
        return AllExitsAndDistanceFields[exitDirection];
    }

    /// <summary>
    /// Adds an Distance Field to an Exit direction
    /// </summary>
    /// <param name="exitDirection">the Exit enum should be used</param>
    /// <param name="distance">the coressponding distance field</param>
    public void AddDistanceFieldToExit(ExitDirection exitDirection, int[,] distance)
    {
        AllExitsAndDistanceFields.Add(exitDirection, distance);
    }

    public Vector3 GenerateAndAddExitPointVector3(ExitDirection exit)
    {
        return GetSubTileCenterWorldCoordinates(GenerateAndAddExitPointVector2(exit));
    }

    public Vector3 GetExitPointV3(ExitDirection exit)
    {
        return GetSubTileCenterWorldCoordinates(ExitAndCorespondingExitpoint[exit]);
    }


    //          North (-X)
    //  West (-Z)        East (+Z)
    //          South (+X)
    //
    public Vector2Int GenerateAndAddExitPointVector2(ExitDirection exit)
    {
        switch (exit)
        {
            case ExitDirection.North:
                Vector2Int exitPoint = new Vector2Int(0,Mathf.FloorToInt((Columns-1)/2));
                if (BaseCostMatrix[exitPoint.x, exitPoint.y] == New_GenerateMatrix.MatrixObstacleValue) { return new Vector2Int(-1, -1); }
                ExitAndCorespondingExitpoint.Add(ExitDirection.North, exitPoint);
                InvalidExist.Remove(ExitDirection.North);
                return ExitAndCorespondingExitpoint[ExitDirection.North];
            case ExitDirection.East:
                exitPoint = new Vector2Int(Mathf.FloorToInt((Rows-1)/2), Columns-1);
                if (BaseCostMatrix[exitPoint.x, exitPoint.y] == New_GenerateMatrix.MatrixObstacleValue) { return new Vector2Int(-1, -1); }
                ExitAndCorespondingExitpoint.Add(ExitDirection.East, exitPoint);
                InvalidExist.Remove(ExitDirection.East);
                return ExitAndCorespondingExitpoint[ExitDirection.East];
            case ExitDirection.West:
                exitPoint = new Vector2Int(Mathf.FloorToInt((Rows-1) / 2),0);
                if (BaseCostMatrix[exitPoint.x, exitPoint.y] == New_GenerateMatrix.MatrixObstacleValue) { return new Vector2Int(-1, -1); }
                ExitAndCorespondingExitpoint.Add(ExitDirection.West, exitPoint);
                InvalidExist.Remove(ExitDirection.West);
                return ExitAndCorespondingExitpoint[ExitDirection.West];
            case ExitDirection.South:
                exitPoint = new Vector2Int(Rows-1, Mathf.FloorToInt((Columns-1) / 2));
                if (BaseCostMatrix[exitPoint.x, exitPoint.y] == New_GenerateMatrix.MatrixObstacleValue) { return new Vector2Int(-1, -1); }
                ExitAndCorespondingExitpoint.Add(ExitDirection.South, exitPoint);
                InvalidExist.Remove(ExitDirection.South);
                return ExitAndCorespondingExitpoint[ExitDirection.South];
        }
        return new Vector2Int(-1, -1);
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
            return New_GenerateMatrix.InterpolateArray<int>(GetPositionInArray(start), exit);
        }
        else if (!HasNoObstacles && !HasOnlyObstacles)
        {
            //ToDo: Cache this shit
            int[,] distance = New_GenerateMatrix.GenerateDistanceField(BaseCostMatrix, Rows, Columns, exit);
            return New_GenerateMatrix.GetBestPathInDistanceMatrix(distance, Rows, Columns, GetPositionInArray(start));
        }
        return new List<Vector2Int>();
    }

    //needs to handle values that are negative
    public Vector3 GetSubTileCenterWorldCoordinates(Vector2Int pos)
    {
        return new Vector3((New_GenerateMatrix.tileSizeX * pos.x) + (Center.x - Size.x / 2) + (New_GenerateMatrix.tileSizeX / 2), 0, New_GenerateMatrix.tileSizeZ * pos.y + (Center.z - Size.z / 2) + New_GenerateMatrix.tileSizeZ / 2);
    }
    public Vector3 GetSubTileCenterWorldCoordinates(int rows, int cols)
    {
        return new Vector3((New_GenerateMatrix.tileSizeX * rows) + (Center.x - Size.x / 2) + (New_GenerateMatrix.tileSizeX / 2), 0, New_GenerateMatrix.tileSizeZ * cols + (Center.z - Size.z / 2) + New_GenerateMatrix.tileSizeZ / 2);
    }
    public Vector2Int GetPositionInArray(Vector3 positionVector3)
    {
        return new Vector2Int(Math.Clamp(Mathf.FloorToInt((positionVector3.x - (Center.x - (Rows * New_GenerateMatrix.tileSizeX) / 2)) / New_GenerateMatrix.tileSizeX) , 0, Rows-1), Math.Clamp(Mathf.FloorToInt((positionVector3.z - (Center.z - (Columns * New_GenerateMatrix.tileSizeZ) / 2)) / New_GenerateMatrix.tileSizeZ), 0, Columns-1));
    }


    public int GetValueAtPosition(Vector3 position)
    {
        Vector3 diff = new Vector3(Center.x - Size.x / 2, 0, Center.z - Size.z / 2) - position;
        return BaseCostMatrix[Mathf.FloorToInt(diff.x / (New_GenerateMatrix.tileSizeX)), Mathf.FloorToInt(diff.z / (New_GenerateMatrix.tileSizeZ))];
    }

    public int GetValueAtPosition(int row, int column)
    {
        return BaseCostMatrix[row, column];
    }



}

public enum ExitDirection
{
    North,
    East,
    West,
    South
}

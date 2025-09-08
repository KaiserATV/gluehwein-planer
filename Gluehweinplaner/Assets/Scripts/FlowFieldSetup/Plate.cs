using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#nullable enable

/// <inheritdoc cref="IPlate"/>
public class Plate : IPlate
{
    public Plate(Vector3 pos, int[,] bcm, bool d)
    {
        Center = pos;
        BaseCostMatrix = bcm;
        canPathDiagonal = d;
    }
    public bool canPathDiagonal = false;
    public bool hasChanged = false;
    private bool addedAllSides = false;
    public Vector3 Center { get; set; }
    public Vector3 Size { get; set; }

    public bool HasNoObstacles = false;

    public bool HasOnlyObstacles = false;
    public int Rows { get; set; }
    public int Columns { get; set; }
    public int[,] BaseCostMatrix { get; set; }

    private List<GoalNode> goalNodes = new List<GoalNode>();

    public Dictionary<ExitDirection.ExitDirections, List<Portal>> exitDirectionToPortals = new Dictionary<ExitDirection.ExitDirections, List<Portal>>();
    public Dictionary<(Vector2Int, Vector2Int), List<Vector3>> pathsTaken = new Dictionary<(Vector2Int, Vector2Int), List<Vector3>>();

    public Dictionary<Bude, List<Vector2Int>> budeToOccupiedSpaces = new Dictionary<Bude, List<Vector2Int>>();

    public Dictionary<(Vector2Int, Vector3), List<Vector3>> finalPaths = new Dictionary<(Vector2Int, Vector3), List<Vector3>>();
    public Dictionary<Vector3, byte[,]> finalFlowFields = new Dictionary<Vector3, byte[,]>();

    public void CheckForPortalNodes(ExitDirection.ExitDirections dir)
    {
        if (exitDirectionToPortals.ContainsKey(dir))
        {
            if (exitDirectionToPortals[dir].Count > 0) { return; }
        }
        switch (dir)
        {
            case ExitDirection.ExitDirections.North:
                CalcPortalNodesForSide(ExitDirection.ExitDirections.North, new(0, 0), new(0, Columns - 1), new(0, 1));
                return;
            case ExitDirection.ExitDirections.South:
                CalcPortalNodesForSide(ExitDirection.ExitDirections.South, new(Rows - 1, Columns - 1), new(Rows - 1, 0), new(0, -1));
                return;
            case ExitDirection.ExitDirections.West:
                CalcPortalNodesForSide(ExitDirection.ExitDirections.West, new(Rows - 1, 0), new(0, 0), new(-1, 0));
                return;
            case ExitDirection.ExitDirections.East:
                CalcPortalNodesForSide(ExitDirection.ExitDirections.East, new(0, Columns - 1), new(Rows - 1, Columns - 1), new(1, 0));
                return;
        }
        if (canPathDiagonal)
        {
            List<Vector2Int> exitCoords = new List<Vector2Int> { new(Rows - 1, Columns - 1), new(0, 0), new(0, Columns - 1), new(Rows - 1, 0) };
            Vector2Int exit;
            switch (dir)
            {
                case ExitDirection.ExitDirections.NorthEast:
                    exit = new(0, Columns - 1);
                    break;
                case ExitDirection.ExitDirections.NorthWest:
                    exit = new(0, 0);
                    break;
                case ExitDirection.ExitDirections.SouthEast:
                    exit = new(Rows - 1, Columns - 1);
                    break;
                default:
                    exit = new(Rows - 1, 0);
                    break;
            }

            if (BaseCostMatrix[exit.x, exit.y] == GenerateMatrix.MatrixIsPathableValue)
            {
                if (exitDirectionToPortals.ContainsKey(dir)) { exitDirectionToPortals.Remove(dir); }
                exitDirectionToPortals.Add(dir, new List<Portal> { new Portal(new List<Vector2Int> { exit }) });
            }
        }
    }

    public void CalculatePortalNodes()
    {
        if (canPathDiagonal)
        {
            List<Vector2Int> exitCoords = new List<Vector2Int> { new(Rows - 1, Columns - 1), new(0, 0), new(0, Columns - 1), new(Rows - 1, 0) };
            Queue<ExitDirection.ExitDirections> exitDirections = new Queue<ExitDirection.ExitDirections>();
            exitDirections.Enqueue(ExitDirection.ExitDirections.SouthEast);
            exitDirections.Enqueue(ExitDirection.ExitDirections.NorthWest);
            exitDirections.Enqueue(ExitDirection.ExitDirections.SouthWest);
            exitDirections.Enqueue(ExitDirection.ExitDirections.NorthEast);
            foreach (Vector2Int exit in exitCoords)
            {
                ExitDirection.ExitDirections exitDirection = exitDirections.Dequeue();
                if (BaseCostMatrix[exit.x, exit.y] == GenerateMatrix.MatrixIsPathableValue)
                {
                    if (exitDirectionToPortals.ContainsKey(exitDirection)) { exitDirectionToPortals.Remove(exitDirection); }
                    exitDirectionToPortals.Add(exitDirection, new List<Portal> { new Portal(new List<Vector2Int> { exit }) });
                    exitDirectionToPortals[exitDirection][0].GenerateFlowField(this);
                }
            }
        }
        List<(ExitDirection.ExitDirections, Vector2Int, Vector2Int, Vector2Int)> exits = new List<(ExitDirection.ExitDirections, Vector2Int, Vector2Int, Vector2Int)>();
        exits.Add((ExitDirection.ExitDirections.North, new(0, 0), new(0, Columns - 1), new(0, 1)));
        exits.Add((ExitDirection.ExitDirections.East, new(0, Columns - 1), new(Rows - 1, Columns - 1), new(1, 0)));
        exits.Add((ExitDirection.ExitDirections.South, new(Rows - 1, Columns - 1), new(Rows - 1, 0), new(0, -1)));
        exits.Add((ExitDirection.ExitDirections.West, new(Rows - 1, 0), new(0, 0), new(-1, 0)));
        foreach ((ExitDirection.ExitDirections, Vector2Int, Vector2Int, Vector2Int) item in exits)
        {
            CalcPortalNodesForSide(item.Item1, item.Item2, item.Item3, item.Item4);
        }
    }

    private void CalcPortalNodesForSide(ExitDirection.ExitDirections dir, Vector2Int start, Vector2Int end, Vector2Int moveDirection)
    {
        if (exitDirectionToPortals.ContainsKey(dir)) { exitDirectionToPortals.Remove(dir); }
        List<Portal> portalNodes = new List<Portal>();
        List<Vector2Int> pos = new List<Vector2Int>();
        Vector2Int curr = start;
        bool added = false;
        while (curr != end)
        {
            if (BaseCostMatrix[curr.x, curr.y] == GenerateMatrix.MatrixIsPathableValue && (curr != (end - moveDirection)))
            {
                added = false;
                pos.Add(curr);
            }
            else
            {
                if ((!added || (curr == (end - moveDirection))) && pos.Count != 0)
                {
                    portalNodes.Add(new Portal(pos));
                    portalNodes.Last().GenerateFlowField(this);
                    added = true;
                    pos = new List<Vector2Int>();
                }
                start = curr + moveDirection;
            }
            curr += moveDirection;
        }
        exitDirectionToPortals.Add(dir, portalNodes);
    }


    public void AddGoalNode(GoalNode goalNode)
    {
        if (goalNodes.Contains(goalNode)) { return; }
        goalNodes.Add(goalNode);
        hasChanged = false;
        byte[,] flowField;
        if (HasNoObstacles || HasOnlyObstacles)
        {
            flowField = new byte[0, 0];
        }
        else
        {
            (_, flowField) = GenerateMatrix.GenerateDistanceFieldAndFlowField(this, goalNode.Position, canPathDiagonal);
        }
        finalFlowFields.Add(goalNode.Position, flowField);
    }


    //          North (-X)
    //  West (-Z)        East (+Z)
    //          South (+X)
    //

    public Portal? GetClostestPortal(Vector3 goal, ExitDirection.ExitDirections exit)
    {
        CheckForPortalNodes(exit);
        if (exitDirectionToPortals[exit].Count == 0) { return null; }
        Portal? closest = null;
        float currDist = int.MaxValue;
        foreach (Portal p in exitDirectionToPortals[exit])
        {
            float dist = Vector3.Distance(GetSubTileCenterWorldCoordinates(p.Center), goal);
            if (dist < currDist)
            {
                closest = p;
                currDist = dist;
            }
        }
        if (closest == null) { return null; }
        if (!closest.HasFlowField) { closest.GenerateFlowField(this); }
        return closest;
    }

    public List<Vector3> GetShortestPathToToNextPlateV3(Portal portal, Vector3 start)
    {
        Vector2Int startArray = GetPositionInArray(start, true);
        List<Vector2Int> steps2Int = GetShortestPathToToNextPlateV2(portal, startArray);
        List<Vector3> returnList = new List<Vector3>();
        foreach (Vector2Int step in steps2Int)
        {
            returnList.Add(GetSubTileCenterWorldCoordinates(step));
        }
        return returnList;
    }

    public List<Vector2Int> GetShortestPathToToNextPlateV2(Portal portal, Vector2Int startArray)
    {
        //calculate best Portal
        if (HasNoObstacles)
        {
            Vector2Int close = portal.GetClostestToStart(startArray);
            Vector3 clostest = GetSubTileCenterWorldCoordinates(close);
            if (canPathDiagonal)
            {
                //List<Vector2Int> path = GenerateMatrix.PathDiagonal(start, exit); //no path needed, can walök stright to last point
                List<Vector2Int> path = new List<Vector2Int> { close };
                return path;
            }
            else
            {
                List<Vector2Int> path = GenerateMatrix.InterpolateArray(startArray, close, ((Vector2Int a, Vector2Int b) compare) => CloserVector2IntToGoal(compare.a, compare.b, clostest), canPathDiagonal, Rows, Columns);
                return path;
            }
        }
        else if (!HasNoObstacles && !HasOnlyObstacles)
        {
            List<Vector2Int> path = GenerateMatrix.GetBestPathInFlowField(portal.flowfield, startArray);
            return path;
        }
        return new List<Vector2Int>();
    }


    public List<Vector3> GetShortestPathToGoalWithin(Vector3 _start, Vector3 goal)
    {
        Vector2Int start = GetPositionInArray(_start, true);
        if (finalPaths.ContainsKey((start, goal))) { return finalPaths[(start, goal)]; }
        if (HasNoObstacles) { return new List<Vector3> { goal }; }
        if (HasOnlyObstacles) { return new List<Vector3>(); }
        if (!finalFlowFields.ContainsKey(goal))
        {
            byte[,] flowField;
            if (HasNoObstacles || HasOnlyObstacles)
            {
                flowField = new byte[0, 0];
            }
            else
            {
                (_, flowField) = GenerateMatrix.GenerateDistanceFieldAndFlowField(this, goal, canPathDiagonal);
            }
            finalFlowFields.Add(goal, flowField);
        }
        List<Vector2Int> steps2Int = GenerateMatrix.GetBestPathInFlowField(finalFlowFields[goal], start);
        List<Vector3> returnList = new List<Vector3>();
        foreach (Vector2Int step in steps2Int)
        {
            returnList.Add(GetSubTileCenterWorldCoordinates(step));
        }
        finalPaths.Add(((start, goal)), returnList);
        return returnList;
    }




    //needs to handle values that are negative
    public Vector3 GetSubTileCenterWorldCoordinates(Vector2Int pos)
    {
        return new Vector3((GenerateMatrix.TileSizeX * pos.x) + (Center.x - Size.x / 2) + (GenerateMatrix.TileSizeX / 2), 0, GenerateMatrix.TileSizeZ * pos.y + (Center.z - Size.z / 2) + GenerateMatrix.TileSizeZ / 2);
    }
    public Vector3 GetSubTileCenterWorldCoordinates(int rows, int cols)
    {
        return new Vector3((GenerateMatrix.TileSizeX * rows) + (Center.x - Size.x / 2) + (GenerateMatrix.TileSizeX / 2), 0, GenerateMatrix.TileSizeZ * cols + (Center.z - Size.z / 2) + GenerateMatrix.TileSizeZ / 2);
    }
    public Vector2Int GetPositionInArray(Vector3 positionVector3, bool safe)
    {
        if (safe)
        {
            return new Vector2Int(Math.Clamp(Mathf.FloorToInt((positionVector3.x - (Center.x - (Rows * GenerateMatrix.TileSizeX) / 2)) / GenerateMatrix.TileSizeX), 0, Rows - 1), Math.Clamp(Mathf.FloorToInt((positionVector3.z - (Center.z - (Columns * GenerateMatrix.TileSizeZ) / 2)) / GenerateMatrix.TileSizeZ), 0, Columns - 1));
        }
        else
        {
            return new Vector2Int(Mathf.FloorToInt((positionVector3.x - (Center.x - (Rows * GenerateMatrix.TileSizeX) / 2)) / GenerateMatrix.TileSizeX), Mathf.FloorToInt((positionVector3.z - (Center.z - (Columns * GenerateMatrix.TileSizeZ) / 2)) / GenerateMatrix.TileSizeZ));
        }
    }


    public int GetBaseValueAtPosition(Vector3 position, bool safe)
    {
        return BaseCostMatrix[GetPositionInArray(position, safe).x, GetPositionInArray(position, safe).y];
    }

    public int GetBaseValueAtPosition(int row, int column)
    {
        return BaseCostMatrix[row, column];
    }


    public Vector2Int OccupySpaces(Bude bude, Vector2Int start, Vector3 exitWorld)
    {
        List<Vector2Int> stepsTaken = new List<Vector2Int> { };
        Vector2Int goalPos = GetPositionInArray(exitWorld, false);
        stepsTaken = GenerateMatrix.InterpolateArray(start, goalPos, ((Vector2Int a, Vector2Int b) compare) => CloserVector2IntToGoal(compare.a, compare.b, exitWorld), false, Rows, Columns);
        foreach (Vector2Int step in stepsTaken)
        {
            BaseCostMatrix[step.x, step.y] += GenerateMatrix.MatrixObstacleValue;
        }

        if (budeToOccupiedSpaces.ContainsKey(bude))
        {
            budeToOccupiedSpaces[bude].AddRange(stepsTaken);
        }
        else
        {
            budeToOccupiedSpaces.Add(bude, stepsTaken);
        }

        hasChanged = true;

        //RecalcWakable();
        HasNoObstacles = false;
        ExitDirection.ExitDirections dir = ExitDirection.DirectionToExitDiretion(goalPos - start);
        if (start == new Vector2(0, 0))
        {
            if (dir == ExitDirection.ExitDirections.East)
            {
                dir = ExitDirection.ExitDirections.North;
            }
            else
            {
                dir = ExitDirection.ExitDirections.West;
            }
        }
        else if (start == new Vector2Int(0, Columns - 1))
        {
            if (dir == ExitDirection.ExitDirections.West)
            {
                dir = ExitDirection.ExitDirections.North;
            }
            else
            {
                dir = ExitDirection.ExitDirections.East;
            }
        }
        else if (start == new Vector2Int(Rows - 1, Columns - 1))
        {
            if (dir == ExitDirection.ExitDirections.West)
            {
                dir = ExitDirection.ExitDirections.South;
            }
            else
            {
                dir = ExitDirection.ExitDirections.East;
            }
        }
        else if (start == new Vector2Int(Rows - 1, 0))
        {
            if (dir == ExitDirection.ExitDirections.East)
            {
                dir = ExitDirection.ExitDirections.South;
            }
            else
            {
                dir = ExitDirection.ExitDirections.West;
            }
        }
        return stepsTaken.Last();
    }
    public void BudeRemoved(Bude bude)
    {
        List<Vector2Int> allOccupied = budeToOccupiedSpaces[bude];
        foreach (Vector2Int step in allOccupied)
        {
            BaseCostMatrix[step.x, step.y] -= GenerateMatrix.MatrixObstacleValue;
        }
        hasChanged = true;

        exitDirectionToPortals = new Dictionary<ExitDirection.ExitDirections, List<Portal>>();
        pathsTaken = new Dictionary<(Vector2Int, Vector2Int), List<Vector3>>();
        finalPaths = new Dictionary<(Vector2Int, Vector3), List<Vector3>>();
        finalFlowFields = new Dictionary<Vector3, byte[,]>();
    }
    public Vector2Int CloserVector2IntToGoal(Vector2Int a, Vector2Int b, Vector3 exit)
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
                if (BaseCostMatrix[row, column] == GenerateMatrix.MatrixObstacleValue)
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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Portal
{
    public Vector2Int Center;
    public List<Vector2Int> GoalPositions2;
    public bool HasFlowField = false;
    public byte[,] flowfield;
    public Dictionary<Vector2Int, Vector2Int> startToExit = new Dictionary<Vector2Int, Vector2Int>();

    public Portal(List<Vector2Int> pos2)
    {
        GoalPositions2 = pos2;
        Center = new(Mathf.FloorToInt((pos2.Last().x+pos2.First().x)/2), Mathf.FloorToInt(Mathf.FloorToInt((pos2.Last().y + pos2.First().y) / 2)));
    }
    public void GenerateFlowField(Plate plate)
    {
        (_, flowfield) = GenerateMatrix.GenerateDistanceFieldAndFlowField(plate.BaseCostMatrix, plate.Rows, plate.Columns, GoalPositions2, plate.canPathDiagonal);
        HasFlowField = true;
    }
    public Vector2Int GetClostestToStart(Vector2Int start)
    {
        if (startToExit.ContainsKey(start)) { return startToExit[start]; }
        Vector2Int clostest = GenerateMatrix.GetClostestV2(GoalPositions2, start);
        startToExit.Add(start, clostest);
        return clostest;
    }
}
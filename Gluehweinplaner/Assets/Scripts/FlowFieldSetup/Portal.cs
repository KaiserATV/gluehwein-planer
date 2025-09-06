using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Portal
{
    public Vector2Int Center;
    public List<Vector2Int> Positions2plate1;
    public List<Vector2Int> Positions2plate2;
    public bool HasFlowField = false;
    public byte[,] flowfield1;
    public byte[,] flowfield2;
    public Plate plate1;
    public Plate plate2;
    public Dictionary<Vector2Int, Vector2Int> startToExit = new Dictionary<Vector2Int, Vector2Int>();

    public Portal(List<Vector2Int> pos2)
    {
        Positions2plate1 = pos2;
        Center = new(Mathf.FloorToInt((pos2.Last().x+pos2.First().x)/2), Mathf.FloorToInt(Mathf.FloorToInt((pos2.Last().y + pos2.First().y) / 2)));
    }
    public void GenerateFlowField(Plate plate)
    {
        (_, flowfield1) = GenerateMatrix.GenerateDistanceFieldAndFlowField(plate.BaseCostMatrix, plate.Rows, plate.Columns, Positions2plate1, plate.canPathDiagonal);
        HasFlowField = true;
    }
    public Vector2Int GetClostestToStart(Vector2Int start)
    {
        if (startToExit.ContainsKey(start)) { return startToExit[start]; }
        Vector2Int clostest = GenerateMatrix.GetClostestV2(Positions2plate1, start);
        startToExit.Add(start, clostest);
        return clostest;
    }
}
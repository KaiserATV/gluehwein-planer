using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <inheritdoc cref="IPortal"/>
public class Portal
{
    public Dictionary<(Plate,Vector2Int), Vector2Int> startToExit = new Dictionary<(Plate, Vector2Int), Vector2Int>();
    public Dictionary<Plate,(Vector2Int Center, List<Vector2Int> goalPositions)> plateToData = new Dictionary<Plate, (Vector2Int Center,List<Vector2Int> goalpositions)>();
    public Dictionary<Plate, byte[,]> plateToFlowfield = new Dictionary<Plate, byte[,]>();
    public Portal(List<Vector2Int> pos2plate1, List<Vector2Int> pos2plate2, Plate plate1, Plate plate2)
    {
        plateToData.Add(plate1, (new(Mathf.FloorToInt((pos2plate1.Last().x + pos2plate1.First().x) / 2), Mathf.FloorToInt(Mathf.FloorToInt((pos2plate1.Last().y + pos2plate1.First().y) / 2))),pos2plate1));
        plateToData.Add(plate2, (new(Mathf.FloorToInt((pos2plate2.Last().x + pos2plate2.First().x) / 2), Mathf.FloorToInt(Mathf.FloorToInt((pos2plate2.Last().y + pos2plate2.First().y) / 2))), pos2plate2));
    }
    public void GenerateFlowFields()
    {
        if(plateToFlowfield.Count > 0) { return; }
        foreach (KeyValuePair<Plate, (Vector2Int Center, List<Vector2Int> goalPositions)> kvp in plateToData) {
            (_, byte[,] flowfield) = GenerateMatrix.GenerateDistanceFieldAndFlowField(kvp.Key.BaseCostMatrix, kvp.Key.Rows, kvp.Key.Columns, kvp.Value.goalPositions, kvp.Key.canPathDiagonal, (Vector2Int a, Vector2Int b)=>true);
            if (plateToFlowfield.ContainsKey(kvp.Key) && kvp.Key.hasChanged)
            {
                plateToFlowfield[kvp.Key] = flowfield;
            }
            else
            {
                plateToFlowfield.Add(kvp.Key, flowfield);
            }
        }
    }
    public Vector2Int GetClostestToStart(Vector2Int start,Plate plate)
    {
        if (startToExit.ContainsKey((plate,start))) { return startToExit[(plate,start)]; }
        Vector2Int clostest = GenerateMatrix.GetClostestV2(plateToData[plate].goalPositions, start);
        startToExit.Add((plate, start), clostest);
        return clostest;
    }
}
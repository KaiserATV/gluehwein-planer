using System.Collections.Generic;
using UnityEngine;

public class New_GoalNode
{
    private List<Bude> lineOfSightTo = new List<Bude>();

    public Vector3 Position;

    public Plate OnPlate { get; set; }

    Vector3 maxValues;
    Vector3 minValues;

    public void AddGoal(Bude goal) { lineOfSightTo.Add(goal); }
    public void RemoveGoal(Bude goal) { lineOfSightTo.Remove(goal); }

    public New_GoalNode(List<Bude> goals, Plate p)
    {
        OnPlate = p;
        lineOfSightTo = goals;
        maxValues = minValues = lineOfSightTo[0].GetFarestPoint();
        lineOfSightTo[0].goalNode = this;
        for (int i = 1; i < lineOfSightTo.Count; i++)
        {
            Bude goal = lineOfSightTo[i];
            goal.goalNode = this;
            if (goal.GetFarestPoint().x > maxValues.x)
            {
                maxValues.x = goal.GetFarestPoint().x;
            }
            else if (goal.GetFarestPoint().x < minValues.x)
            {
                minValues.x = goal.GetFarestPoint().x;
            }
            if (goal.GetFarestPoint().z > maxValues.z)
            {
                maxValues.z = goal.GetFarestPoint().z;
            }
            else if (goal.GetFarestPoint().z < minValues.z)
            {
                minValues.z = goal.GetFarestPoint().z;
            }
        }
        CalculatePosition();
    }

    public void CalculatePosition()
    {
        //First way of setting Position, needs to be improved upon
        //ToDo: 
        //1. Positionierung checken ob platz erreichbar ist
        //2. Positionierung verschieben wenn nicht erreichbar ist
        //3. Vergleichen ob neue Position von allen Goals direkt erreichbar ist
        
        Vector3 pot = new Vector3(minValues.x, 0, minValues.z);
        while (OnPlate.GetBaseValueAtPosition(pot, true) != New_GenerateMatrix.MatrixIsPathableValue)
        {
            if (pot.x + New_GenerateMatrix.TileSizeX < maxValues.x)
            {
                pot.x += New_GenerateMatrix.TileSizeX;
            }
            else if (pot.z + New_GenerateMatrix.TileSizeZ < maxValues.z)
            {
                pot.z += New_GenerateMatrix.TileSizeZ;
            }
            else { break; } //needs to do something}
        }
        Position = pot;
    }

    //pahting to goal should be here maybe, or you path directly

}

using System.Collections.Generic;
using UnityEngine;

public class New_GoalNode
{
    private List<New_Bude> lineOfSightTo = new List<New_Bude>();

    public Vector3 Position;

    public Vector2Int OnPlate { get; set; }

    public void AddGoal(New_Bude goal) { lineOfSightTo.Add(goal); }
    public void RemoveGoal(New_Bude goal) { lineOfSightTo.Remove(goal); }

    public New_GoalNode(List<New_Bude> goals)
    {
        lineOfSightTo = goals;
        foreach(New_Bude goal in lineOfSightTo)
        {
            goal.goalNode = this;
        }
    }

    public void CalculatePosition()
    {

        //should get correct baseMatrix to determin Position
        bool basePos = true;

        float minX = 0;
        float maxX = 0;
        float minZ = 0;
        float maxZ = 0;

        for (int i = 0; i < lineOfSightTo.Count; i++)
        {
            Vector3 Positions = lineOfSightTo[i].GetPosition();
            if (i != 0)
            {
                if (Positions.x >= minX)
                {
                    if (Positions.x > maxX)
                    {
                        maxX = Positions.x;
                    }
                }
                else
                {
                    minX = Positions.x;
                }

                if (Positions.z >= minZ)
                {
                    if (Positions.z > maxZ)
                    {
                        maxZ = Positions.z;
                    }
                }
                else
                {
                    minZ = Positions.z;
                }
                basePos = false;
            }
            else
            {
                minX = maxX = Positions.x;
                minZ = maxZ = Positions.z;

            }
        }

        //First way of setting Position, needs to be improved upon
        //ToDo: 
        //1. Positionierung checken ob platz erreichbar ist
        //2. Positionierung verschieben wenn nicht erreichbar ist
        //3. Vergleichen ob neue Position von allen Goals direkt erreichbar ist
        Position = new Vector3(minX + (maxX - minX) / 2, 0, minZ + (maxZ - minZ) / 2);

        if (basePos)
        {
            Position += new Vector3(1, 0, 1);
        }
    }

    //pahting to goal should be here maybe, or you path directly

}

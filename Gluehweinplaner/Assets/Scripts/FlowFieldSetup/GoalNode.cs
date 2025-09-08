using System.Collections.Generic;
using UnityEngine;

public class GoalNode
{
    private List<Bude> lineOfSightTo = new List<Bude>();

    public Vector3 Position;
    private SceneManager sm;
    public Plate OnPlate { get; set; }
    private List<NPC> usingGoalNode = new List<NPC>();

    Vector3 maxValues;
    Vector3 minValues;

    public void AddGoal(Bude goal) { lineOfSightTo.Add(goal);goal.goalNode = this; }
    public void RemoveGoal(Bude goal) { lineOfSightTo.Remove(goal); goal.goalNode = null; }
    public void AddOnWayToGoalNode(NPC npc) { usingGoalNode.Add(npc); }
    public void RemoveNPC(NPC npc) { usingGoalNode.Remove(npc); if (lineOfSightTo.Count == 0 && usingGoalNode.Count == 0) { sm.RemoveGoalNode(this); } }

    public GoalNode(List<Bude> goals, Plate p, SceneManager sceneManager)
    {
        sm = sceneManager;
        OnPlate = p;
        lineOfSightTo = goals;
        foreach (Bude  b in lineOfSightTo) { b.goalNode = this; }
        CalculatePosition();
    }

    public void CalculatePosition()
    {
        maxValues = minValues = lineOfSightTo[0].GetFarestPoint();
        //First way of setting Position, needs to be improved upon
        //ToDo: 
        //1. Positionierung checken ob platz erreichbar ist
        //2. Positionierung verschieben wenn nicht erreichbar ist
        //3. Vergleichen ob neue Position von allen Goals direkt erreichbar ist
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

        Vector3 pot = new Vector3(minValues.x, 0, minValues.z);
        while (OnPlate.GetBaseValueAtPosition(pot, true) != GenerateMatrix.MatrixIsPathableValue)
        {
            if (pot.x + GenerateMatrix.TileSizeX < maxValues.x)
            {
                pot.x += GenerateMatrix.TileSizeX;
            }
            else if (pot.z + GenerateMatrix.TileSizeZ < maxValues.z)
            {
                pot.z += GenerateMatrix.TileSizeZ;
            }
            else { break; } //needs to do something}
        }
        Position = pot;
    }

    public void BudeMoved(Bude bude)
    {
        sm.BudeMoved(bude);
        if (sm.WorldPositionToPlate(bude.GetFarestPoint()) != OnPlate)
        {
            lineOfSightTo.Remove(bude);
        }
        if (lineOfSightTo.Count > 0)
        {
            CalculatePosition();
        }
        else
        {
            if (usingGoalNode.Count == 0)
            {
                sm.RemoveGoalNode(this);
            }
        }
        foreach (NPC npc in usingGoalNode)
        {
            npc.BudeMoved(bude);
        }
    }

    public void BudeDestroyed(Bude bude)
    {
        sm.RemoveBude(bude);
        usingGoalNode = null;
        lineOfSightTo.Remove(bude);
        if (lineOfSightTo.Count > 0)
        {
            CalculatePosition();
        }
        foreach (NPC npc in usingGoalNode)
        {
            npc.BudeDestroyed(bude);
        }
    }
}

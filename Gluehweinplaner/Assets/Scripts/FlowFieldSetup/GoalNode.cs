using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalNode
{
    private List<Bude> lineOfSightTo = new List<Bude>();

    public Vector3 Position;
    private SceneManager sm;
    public Plate OnPlate { get; set; }
    private List<NPC_navmesh> onWayToGoalNode = new List<NPC_navmesh>();
    private List<NPC_navmesh> atGoal = new List<NPC_navmesh>();

    Vector3 maxValues;
    Vector3 minValues;

    public void AddGoal(Bude goal) { lineOfSightTo.Add(goal);goal.goalNode = this; }
    public void RemoveGoal(Bude goal) { lineOfSightTo.Remove(goal); goal.goalNode = null; }
    public void AddOnWayToGoalNode(NPC_navmesh npc) { onWayToGoalNode.Add(npc); }
    public void RemoveOnWayToGoalNode(NPC_navmesh npc) { onWayToGoalNode.Remove(npc); if (onWayToGoalNode.Count == 0 && lineOfSightTo.Count == 0) { sm.RemoveGoalNode(this); } }
    public void OnWayToWait(NPC_navmesh npc) { atGoal.Add(npc); onWayToGoalNode.Remove(npc); }
    public void RemoveWaitingAtGoal(NPC_navmesh npc) { atGoal.Remove(npc); onWayToGoalNode.Add(npc); }
    public void RemoveSafe(NPC_navmesh npc) { if (atGoal.Contains(npc)){ atGoal.Remove(npc); }else if(onWayToGoalNode.Contains(npc)){ onWayToGoalNode.Remove(npc); }}

    public GoalNode(List<Bude> goals, Plate p, SceneManager sceneManager)
    {
        sm = sceneManager;
        OnPlate = p;
        lineOfSightTo = goals;
        maxValues = minValues = lineOfSightTo[0].GetFarestPoint();
        foreach (Bude  b in lineOfSightTo) { b.goalNode = this; }
        CalculatePosition();
    }

    public void CalculatePosition()
    {
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
        if(sm.WorldPositionToPlate(bude.GetFarestPoint()) != OnPlate)
        {
            foreach(NPC_navmesh npc in onWayToGoalNode)
            {
                npc.BudeMoved(bude);
            }
            foreach (NPC_navmesh npc in atGoal)
            {
                npc.BudeMoved(bude);
            }
            lineOfSightTo.Remove(bude);
        }
        CalculatePosition();
    }

    public void BudeDestroyed(Bude bude)
    {
        foreach (NPC_navmesh npc in onWayToGoalNode)
        {
            npc.BudeMoved(bude);
        }
        foreach (NPC_navmesh npc in atGoal)
        {
            npc.BudeMoved(bude);
        }
        lineOfSightTo.Remove(bude);

        CalculatePosition();
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <inheritdoc cref="IGoalNode"/> 
public class GoalNode : IGoalNode
{
    private List<Bude> lineOfSightTo = new List<Bude>();
    public Vector3 Position;
    private SceneManager sm;
    public Plate OnPlate { get; set; }
    private List<NPC> usingGoalNode = new List<NPC>();
    Vector3 maxValues;
    Vector3 minValues;
    public void AddBude(Bude bude) { lineOfSightTo.Add(bude); bude.goalNode = this; }
    public void RemoveBude(Bude bude) { lineOfSightTo.Remove(bude); bude.goalNode = null; }
    public void UsingGoalnodeAdd(NPC npc) { usingGoalNode.Add(npc); }
    public void RemoveNPC(NPC npc) { usingGoalNode.Remove(npc); if (lineOfSightTo.Count == 0 && usingGoalNode.Count == 0) { sm.RemoveGoalNode(this); } }
    public GoalNode(List<Bude> goals, Plate p, SceneManager sceneManager)
    {
        sm = sceneManager;
        OnPlate = p;
        lineOfSightTo = goals;
        foreach (Bude b in lineOfSightTo) { b.goalNode = this; }
        CalculatePosition();
    }
    public void CalculatePosition()
    {
        maxValues = minValues = lineOfSightTo[0].GetFarestPoint();
        //First way of setting Position, needs to be improved upon
        //ToDo: 
        //1. Check if position is reachable from the goalnode
        //2. Move position if not reachable
        //3. Compare if the position is reachable from every position
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
            else { break; } //needs to do something
        }
        Position = pot;
    }
    public void BudeMoved(Bude bude)
    {
        if (sm.WorldPositionToPlate(bude.GetFarestPoint()) != OnPlate)
        {
            RemoveBude(bude);
        }
       
        sm.BudeMoved(bude);
        foreach (NPC npc in usingGoalNode)
        {
            npc.BudeMoved(bude);
        }
        if (lineOfSightTo.Count > 0)
        {
            CalculatePosition();
        }
        else
        {
            OnPlate.RemoveGoalNode(this);
            sm.RemoveGoalNode(this);
        }
    }
    public void BudeDestroyed(Bude bude)
    {
        sm.RemoveBude(bude);
        usingGoalNode = null;
        RemoveBude(bude);
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

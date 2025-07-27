using System.Collections.Generic;
using UnityEngine;

public class New_InactiveAgentsContainer : MonoBehaviour
{
    private Vector3 WorldCoords;
    private int StoredPlayerCount;
    private LinkedList<New_NPC> StoredAgents = new LinkedList<New_NPC>();
    private void Start()
    {
        WorldCoords = this.transform.position;
        StoredPlayerCount = 0;
    }

    public void AddAgent(New_NPC ac)
    {
        StoredAgents.AddFirst(ac);
        StoredPlayerCount++;
    }
    public New_NPC GetAgent()
    {
        New_NPC ac = null;
        if (StoredPlayerCount != 0)
        {
            ac = StoredAgents.First.Value;
            StoredPlayerCount--;
            StoredAgents.RemoveFirst();
        }
        return ac;
    }

    public Vector3 GetWorldCoords() { return WorldCoords; }
    public int GetStoredCount() { return StoredPlayerCount; }

}

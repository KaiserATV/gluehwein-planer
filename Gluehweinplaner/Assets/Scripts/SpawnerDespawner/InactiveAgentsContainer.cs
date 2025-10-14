using System.Collections.Generic;
using UnityEngine;

public class InactiveAgentsContainer : MonoBehaviour
{
    private Vector3 WorldCoords;
    private int StoredPlayerCount;
    private LinkedList<NPC_navmesh> StoredAgents = new LinkedList<NPC_navmesh>();
    private void Start()
    {
        WorldCoords = this.transform.position;
        StoredPlayerCount = 0;
    }

    public void AddAgent(NPC_navmesh ac)
    {
        StoredAgents.AddFirst(ac);
        StoredPlayerCount++;
    }
    public NPC_navmesh GetAgent()
    {
        NPC_navmesh ac = null;
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

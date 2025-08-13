using System.Collections.Generic;
using UnityEngine;

public class New_Bude : MonoBehaviour
{
    public float WaitTime = 1f;
    public GameObject Agent;
    public float AgentWidthX = 2f;
    public float AgentWidthZ = 2f;
    public int attraktivitaet = 5;
    public int kapazitaet;
    public float delayBeforeNotBusy = 5f;
    private float timeGoneBy = 0f;
    public bool busy = false;

    public int tilesX;
    public int tilesZ;

    private int typeIndex;

    public int attrakIncr=10;
    public int waitIncr = 5;

    private New_BitArray2D wait_L;
    private New_BitArray2D wait_R;
    private New_BitArray2D ziel;

    public New_GoalNode goalNode;

    public void Start()
    {
        transform.hasChanged = false;
        Bounds b = Agent!.GetComponentInChildren<SkinnedMeshRenderer>().bounds;
        AgentWidthX = b.size.x;
        AgentWidthZ = b.size.z;

        // !!!!IMPORTANT!!!! the number of the children specifies the position in the prefab, if changed, chang number here!!!!!!!
        // 1 - Wait_L, 2 - Wait_R, 3 - Ziel

        //Ziel Array
        Transform child = transform.GetChild(3);
        Bounds bound = child.GetComponent<MeshRenderer>().localBounds;
        ziel = new New_BitArray2D(bound, child, 0, WaitTime, AgentWidthX, AgentWidthZ);

        //Wait_L Array
        child = transform.GetChild(1);
        bound = child.GetComponent<MeshRenderer>().localBounds;
        wait_L = new New_BitArray2D(bound, child, 1, WaitTime, AgentWidthX, AgentWidthZ);

        //Wait_R Array
        child = transform.GetChild(2);
        bound = child.GetComponent<MeshRenderer>().localBounds;
        wait_R = new New_BitArray2D(bound, child, 2, WaitTime, AgentWidthX, AgentWidthZ);

        CalcKapa();
    }

    private void Update()
    {
        if (timeGoneBy < delayBeforeNotBusy)
        {
            timeGoneBy += Time.deltaTime;
        }
        else
        {
            busy = CheckAuslastung();
            timeGoneBy = 0;
        }

        if (transform.hasChanged) {
            ziel.RefreshPos();
            wait_L.RefreshPos();
            wait_R.RefreshPos();
            transform.hasChanged = false;
        }
    }

    private void CalcKapa()
    {
        kapazitaet = ziel.GetKapa() + wait_L.GetKapa() + wait_R.GetKapa();
    }

    public void Reset()
    {
        ziel.Reset();
        wait_L.Reset();
        wait_R.Reset();
    }


    public Vector3? GetNewPosition(New_NPC ac)
    {
        Vector3 cellCoord;
        if (!ziel.IsFull())
        {
            cellCoord = ziel.FindBestPositionAndAdd(ac);
        }
        else if (!wait_L.IsFull())
        {
            cellCoord = wait_L.FindBestPositionAndAdd(ac);
        }
        else if (!wait_R.IsFull())
        {
            cellCoord = wait_R.FindBestPositionAndAdd(ac);
        }else
        {
            return null;
        }

        return cellCoord;
    }

    public bool CheckAuslastung()
    {
        busy = busy || (ziel.IsFull() && wait_L.IsFull() && wait_R.IsFull());
        return ziel.IsFull() && wait_L.IsFull() && wait_R.IsFull();
    }

    public void increaseAttraktivitaet()
    {
        attraktivitaet++;

    }
    public void decreaseAttraktivitaet()
    {
        if (attraktivitaet - attrakIncr > 0)
        {
            attraktivitaet--;
        }
    }

    public void increaseWaittime()
    {
        WaitTime++;

    }
    public void decreaseWaittime()
    {
        if (WaitTime - waitIncr > 0) { 
            WaitTime--;
        }
    }

    public New_BudenJSON GetBudenJSON()
    {
        return new New_BudenJSON(transform.position.x, transform.position.z, transform.eulerAngles.y, typeIndex, attraktivitaet, WaitTime);
    }

    public void SetTypeIndex(int i)
    {
        typeIndex = i;
    }

    public void ToBeDestroyed()
    {
        ziel.Destroying();
        wait_L.Destroying();
        wait_R.Destroying();
    }
    public void RemovePlayer(New_NPC npc)
    {
        ziel.RemovePlayer(npc);
        wait_L.RemovePlayer(npc);
        wait_R.RemovePlayer(npc);
    }

    public Vector3 GetPosition() { return transform.position; }


    /// <summary>
    /// Gets all Corners of the Buden transform in order:
    /// Top left, Top right, Bottom Left, Bottom Right
    /// </summary>
    /// <returns></returns>
    public List<Vector3> GetAllCornerPoints()
    {
        Transform t = this.transform.GetChild(4);
        Bounds b = this.transform.GetChild(4).GetComponent<MeshRenderer>().localBounds;
        return new List<Vector3>
        {
            t.transform.TransformPoint(new Vector3(-b.size.x/2, 0, -b.size.z/2)),//Top Left
            t.transform.TransformPoint(new Vector3(-b.size.x/2, 0, b.size.z/2)),//Top Right
            t.transform.TransformPoint(new Vector3(b.size.x/2, 0, -b.size.z/2)),//Bottom Left
            t.transform.TransformPoint(new Vector3(b.size.x/2, 0, b.size.z/2))//Bottom Right
        };
    }



    public Vector3 GetFacingDirection() { return transform.GetChild(3).transform.position - transform.GetChild(0).transform.position; }

    public Vector3 GetFarestPoint() {
        Transform t = this.transform.GetChild(3);
        Bounds b = t.GetComponent<MeshRenderer>().localBounds;
        return t.TransformPoint(new Vector3(-b.size.x / 2, 0, 0));
    }
}

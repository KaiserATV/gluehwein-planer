using System.Collections.Generic;
using UnityEngine;
/// <inheritdoc cref="IBude"/>
public class Bude : MonoBehaviour, IBude
{
    public int WaitTime = 20;
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

    public int attrakIncr = 10;
    public int waitIncr = 5;

    private BitArray2D wait_L;
    private BitArray2D wait_R;
    private BitArray2D ziel;

    public GoalNode goalNode;
    public List<Plate> onplates;

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
        ziel = new BitArray2D(bound, child, 0, AgentWidthX, AgentWidthZ);

        //Wait_L Array
        child = transform.GetChild(1);
        bound = child.GetComponent<MeshRenderer>().localBounds;
        wait_L = new BitArray2D(bound, child, 1, AgentWidthX, AgentWidthZ);

        //Wait_R Array
        child = transform.GetChild(2);
        bound = child.GetComponent<MeshRenderer>().localBounds;
        wait_R = new BitArray2D(bound, child, 2, AgentWidthX, AgentWidthZ);

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
            busy = CheckOccupation();
            timeGoneBy = 0;
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
    public void BudeMoved()
    {
        foreach(Plate p in onplates)
        {
            p.BudeRemoved(this);
        }
        onplates = new List<Plate>();
        goalNode.BudeMoved(this);
    }
    public void BudeRemove()
    {
        goalNode.BudeDestroyed(this);
    }
    public (Vector3?, Vector3Int?) GetNewPosition()
    {
        Vector3 cellCoord;
        Vector2Int arrayPos;
        Vector3Int returnVector;
        if (!ziel.IsFull())
        {
            (cellCoord, arrayPos) = ziel.FindBestPositionAndAdd();
            returnVector = new(0, arrayPos.x, arrayPos.y);
        }
        else if (!wait_L.IsFull())
        {
            (cellCoord, arrayPos) = wait_L.FindBestPositionAndAdd();
            returnVector = new(1, arrayPos.x, arrayPos.y);
        }
        else if (!wait_R.IsFull())
        {
            (cellCoord, arrayPos) = wait_R.FindBestPositionAndAdd();
            returnVector = new(2, arrayPos.x, arrayPos.y);
        }
        else
        {
            return (null, null);
        }
        return (cellCoord, returnVector);
    }
    public void RemovePlayer(Vector3Int pos)
    {
        switch (pos.x)
        {
            case 0:
                ziel.RemovePlayer(new(pos.y, pos.z));
                break;
            case 1:
                wait_L.RemovePlayer(new(pos.y, pos.z));
                break;
            case 2:
                wait_R.RemovePlayer(new(pos.y, pos.z));
                break;
        }
    }
    public bool CheckOccupation()
    {
        busy = busy || (ziel.IsFull() && wait_L.IsFull() && wait_R.IsFull());
        return ziel.IsFull() && wait_L.IsFull() && wait_R.IsFull();
    }
    public void increaseAttractivness()
    {
        attraktivitaet++;

    }
    public void decreaseAttractivness()
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
        if (WaitTime - waitIncr > 0)
        {
            WaitTime--;
        }
    }
    public BudenJSON GetBudenJSON()
    {
        return new BudenJSON(transform.position.x, transform.position.z, transform.eulerAngles.y, typeIndex, attraktivitaet, WaitTime);
    }
    public void SetTypeIndex(int i)
    {
        typeIndex = i;
    }
    public Vector3 GetPosition() { return transform.position; }
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
    public Vector3 GetFarestPoint()
    {
        Transform t = this.transform.GetChild(3);
        Bounds b = t.GetComponent<MeshRenderer>().localBounds;
        return t.TransformPoint(new Vector3(-b.size.x / 2, 0, 0));
    }
}

using UnityEngine;

public class New_Bude : MonoBehaviour
{
    public float agentRadius = 1f;

    public float WaitTime = 1f;

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
        this.transform.hasChanged = false;

        //calculate the tiles that are blocked by the box???        


        // !!!!IMPORTANT!!!! the number of the children specifies the position in the prefab, if changed, chang number here!!!!!!!
        // 1 - Wait_L, 2 - Wait_R, 3 - Ziel

        //Ziel Array
        Transform child = this.transform.GetChild(3);
        Bounds bound = child.GetComponent<MeshRenderer>().localBounds;
        ziel = new New_BitArray2D(bound, child, agentRadius, 0, WaitTime);

        //Wait_L Array
        child = this.transform.GetChild(1);
        bound = child.GetComponent<MeshRenderer>().localBounds;
        wait_L = new New_BitArray2D(bound, child, agentRadius, 1, WaitTime);

        //Wait_R Array
        child = this.transform.GetChild(2);
        bound = child.GetComponent<MeshRenderer>().localBounds;
        wait_R = new New_BitArray2D(bound, child, agentRadius, 2, WaitTime);

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

        if (this.transform.hasChanged) {
            ziel.RefreshPos();
            wait_L.RefreshPos();
            wait_R.RefreshPos();
            this.transform.hasChanged = false;
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
        return new New_BudenJSON(this.transform.position.x, this.transform.position.z, this.transform.eulerAngles.y, typeIndex, attraktivitaet, WaitTime);
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

    public Vector3 GetPosition() { return this.transform.position; }

    public Vector3 GetFacingDirection() { return this.transform.GetChild(3).transform.position - this.transform.GetChild(0).transform.position; }
}

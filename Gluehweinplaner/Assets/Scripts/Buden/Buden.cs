using UnityEngine;

public class Buden : MonoBehaviour
{
    public float agentRadius = 1f;

    public int waitTime = 10;

    public int attraktivitaet = 5;
    public int kapazitaet;
    public float delayBeforeNotBusy = 5f;
    private float timeGoneBy = 0f;
    public bool busy = false;

    private int typeIndex;

    public int attrakIncr=10;
    public int waitIncr = 5;

    private New_New_BitArray2D wait_B;
    private New_New_BitArray2D wait_L;
    private New_New_BitArray2D wait_R;
    private New_New_BitArray2D ziel;

    public void Start()
    {
        this.transform.hasChanged = false;
        
        // !!!!IMPORTANT!!!! the number of the children specifies the position in the prefab, if changed, chang number here!!!!!!!
        // 1 - Wait_B, 2 - Wait_L, 3 - Wait_R, 4 - Ziel
        //0 -directly infront of Bode, 1 - to the left of the Bude, 2- to the right of the Bude
        //ziel Array
        Transform child = this.transform.GetChild(4);
        Bounds bound = child.GetComponent<MeshRenderer>().localBounds;
        ziel = new New_New_BitArray2D(bound, child, agentRadius, 0);

        //Wait_B Array
        child = this.transform.GetChild(1);
        bound = child.GetComponent<MeshRenderer>().localBounds;
        wait_B = new New_New_BitArray2D(bound, child, agentRadius, 0);

        //Wait_L Array
        child = this.transform.GetChild(2);
        bound = child.GetComponent<MeshRenderer>().localBounds;
        wait_L = new New_New_BitArray2D(bound, child, agentRadius, 1);

        //Wait_R Array
        child = this.transform.GetChild(3);
        bound = child.GetComponent<MeshRenderer>().localBounds;
        wait_R = new New_New_BitArray2D(bound, child, agentRadius, 2);

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
            wait_B.RefreshPos();
            wait_L.RefreshPos();
            wait_R.RefreshPos();
            this.transform.hasChanged = false;
        }
    }

    private void CalcKapa()
    {
        kapazitaet = ziel.GetKapa() + wait_B.GetKapa() + wait_L.GetKapa() + wait_R.GetKapa();
    }

    public void Reset()
    {
        ziel.Reset();
        wait_B.Reset();
        wait_L.Reset();
        wait_R.Reset();
    }


    public Vector3Int GetNewPosition(AgentController ac)
    {
        Vector2Int cellCoord;
        int zone;
        if (!ziel.IsFull())
        {
            cellCoord = ziel.FindBestPositionAndAdd(ac);
            zone = 0;
        }
        else if (!wait_B.IsFull())
        {
            cellCoord = wait_B.FindBestPositionAndAdd(ac);
            zone = 1;
        }
        else if (!wait_L.IsFull())
        {
            cellCoord = wait_L.FindBestPositionAndAdd(ac);
            zone = 2;
        }
        else if (!wait_R.IsFull())
        {
            cellCoord = wait_R.FindBestPositionAndAdd(ac);
            zone = 3;
        }else
        {
            //Needs to find another target
            return new Vector3Int(-1, -1, -1);
        }
        return new Vector3Int(cellCoord.x, cellCoord.y, zone);
    }

    public bool CheckAuslastung()
    {
        busy = busy || (ziel.IsFull() && wait_B.IsFull() && wait_L.IsFull() && wait_R.IsFull());
        return ziel.IsFull() && wait_B.IsFull() && wait_L.IsFull() && wait_R.IsFull();
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
        waitTime++;

    }
    public void decreaseWaittime()
    {
        if (waitTime - waitIncr > 0) { 
            waitTime--;
        }
    }

    public BudenJSON GetBudenJSON()
    {
        return new BudenJSON(this.transform.position.x, this.transform.position.z, this.transform.eulerAngles.y, typeIndex, attraktivitaet, waitTime);
    }

    public void SetTypeIndex(int i)
    {
        typeIndex = i;
    }

    public void ToBeDestroyed()
    {
        ziel.Destroying();
        wait_B.Destroying();
        wait_L.Destroying();
        wait_R.Destroying();
    }

}

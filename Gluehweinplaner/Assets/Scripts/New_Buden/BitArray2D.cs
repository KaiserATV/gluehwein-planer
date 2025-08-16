using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitArray2D
{

    //z=0x=0 z=0x=1
    //z=1x=0 z=1x=1
    private BitArray array;
    private bool full;
    private int cellsX;
    private int cellsZ;
    private float AgentWidthX;
    private float AgentWidthZ;
    private Dictionary<NPC, Vector2Int> registeredPlayers=new Dictionary<NPC, Vector2Int>();
    private Transform childT;
    private int positionToBude; //0 -directly infront of Bode, 1 - to the left of the Bude, 2- to the right of the Bude
    private float schiebX;
    private float schiebZ;
    private const float spacearound = 2f;

    public BitArray2D( Bounds b, Transform child, int p, float ax, float az) { 
        childT = child;

        full = false;   
        positionToBude = p;
        AgentWidthX = ax;
        AgentWidthZ = az;
       
        CalcWidthHeight();


        array = new BitArray(cellsX * cellsZ);

        schiebX = b.size.x / (child.localScale.x) * spacearound;
        schiebZ = b.size.z / (child.localScale.z) * spacearound;

    }

    public void Reset()
    {
        registeredPlayers = new Dictionary<NPC, Vector2Int>();
        array = new BitArray(cellsX * cellsZ);
    }
    public int GetKapa()
    {
        return cellsX * cellsZ;
    }


    private void CalcWidthHeight()
    {
        cellsX = Mathf.FloorToInt(childT.localScale.x / (AgentWidthX));
        cellsZ = Mathf.FloorToInt(childT.localScale.z / (AgentWidthZ));
    }

    public Vector3 GetRealWorldCords(Vector2Int cells)
    {
        float lx = (cellsX / 2) * schiebX - (cells.x * schiebX) - schiebX / 2;
        float lz = (cellsZ / 2) * schiebZ - (cells.y * schiebZ) - schiebZ / 2;

        if (positionToBude == 0)
        {
            lx += schiebX / 2;
        }
        Vector3 tV = childT.TransformPoint(new Vector3(lx, 0, lz));
        return new Vector3(tV.x, 0,tV.z);
    }


    public void AddPlayer(Vector2Int v, NPC ac)
    {
        registeredPlayers.Add(ac,v);
        ac.SetCells(v);
        // ac.SetGoal(GetRealWorldCords(v));
        array[v.y * cellsX + v.x] = true;
        if (registeredPlayers.Count == cellsX * cellsZ) { full = true; }
    }

    public void RemovePlayer(NPC ac)
    {
        if (registeredPlayers.ContainsKey(ac)) { 
            Vector2Int pos = registeredPlayers[ac];
            registeredPlayers.Remove(ac);
            array[pos.y * cellsX + pos.x] = false;
            full = false;
        };
    }

    public Vector3 FindBestPositionAndAdd(NPC ac)
    {
        Vector2Int coord = new Vector2Int(-1, -1);
        if (!full)
        {
            switch (positionToBude)
            {
                case 0:
                    coord = AddInFront(ac);
                    break;
                case 1:
                    coord = AddToLeft(ac);
                    break;
                case 2:
                    coord = AddToRight(ac);
                    break;
            }
        }
        return GetRealWorldCords(coord);
    }

    public Vector2Int AddInFront(NPC ac)
    {
        for (int x = 0; x < cellsX; x++)
        {
            for (int z = 0; z < cellsZ; z++)
            {
                if (!array[z * cellsX + x])
                {
                    AddPlayer(new Vector2Int(x, z), ac);
                    return new Vector2Int(x, z);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    public Vector2Int AddToLeft(NPC ac)
    {
        for (int x = 0; x < cellsX; x++)
        {
            for (int z = cellsZ - 1; z >= 0; z--)
            {
                if (!array[z * cellsX + x])
                {
                    AddPlayer(new Vector2Int(x, z), ac);
                    return new Vector2Int(x, z);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }


    //ToDo: Fix this shit
    public Vector2Int AddToRight(NPC ac)
    {
        for (int x = 0; x < cellsX; x++)
        {
            for (int z = 0; z < cellsZ; z++)
            {
                if (!array[z * cellsX + x])
                {
                    AddPlayer(new Vector2Int(x, z), ac);
                    return new Vector2Int(x, z);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }
    public void RefreshPos()
    {
        foreach ((NPC ac,Vector2Int pos) in registeredPlayers)
        {
            ac.InvalidatePosition(GetRealWorldCords(pos));
        }
    }

    public bool IsFull()
    {
        return full;
    }

    public void Destroying()
    {
        foreach((NPC ac, Vector2Int pos) in registeredPlayers)
        {
            ac.BudeDestroyed();
        }
    }
}

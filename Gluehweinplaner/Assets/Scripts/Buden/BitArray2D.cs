using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitArray2D
{

    //z=0x=0 z=0x=1
    //z=1x=0 z=1x=1
    private BitArray array;
    private int cellsX;
    private int cellsZ;
    private float AgentWidthX;
    private float AgentWidthZ;
    private int currentWaiting = 0;
    private Transform childT;
    private int positionToBude; //0 -directly infront of Bode, 1 - to the left of the Bude, 2- to the right of the Bude
    private float schiebX;
    private float schiebZ;
    private const float spacearound = 2f;
    private int kapa;

    public BitArray2D( Bounds b, Transform child, int p, float ax, float az) { 
        childT = child;

        positionToBude = p;
        AgentWidthX = ax;
        AgentWidthZ = az;
       
        CalcWidthHeight();
        kapa = cellsX * cellsZ;

        array = new BitArray(cellsX * cellsZ);

        schiebX = b.size.x / (child.localScale.x) * spacearound;
        schiebZ = b.size.z / (child.localScale.z) * spacearound;

    }

    public void Reset()
    {
        array = new BitArray(kapa);
    }
    public int GetKapa()
    {
        return kapa;
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


    private void AddPlayer(Vector2Int v)
    {
        array[v.y * cellsX + v.x] = true;
        currentWaiting++;
    }

    public void RemovePlayer(Vector2Int pos)
    {
        array[pos.y * cellsX + pos.x] = false;
        currentWaiting--;
    }

    public (Vector3,Vector2Int) FindBestPositionAndAdd()
    {
        Vector2Int coord = new Vector2Int(-1, -1);
        if (!IsFull())
        {
            switch (positionToBude)
            {
                case 0:
                    coord = AddInFront();
                    break;
                case 1:
                    coord = AddToLeft();
                    break;
                case 2:
                    coord = AddToRight();
                    break;
            }
        }
        return (GetRealWorldCords(coord),coord);
    }

    private Vector2Int AddInFront()
    {
        for (int x = 0; x < cellsX; x++)
        {
            for (int z = 0; z < cellsZ; z++)
            {
                if (!array[z * cellsX + x])
                {
                    AddPlayer(new Vector2Int(x, z));
                    return new Vector2Int(x, z);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    private Vector2Int AddToLeft()
    {
        for (int x = 0; x < cellsX; x++)
        {
            for (int z = cellsZ - 1; z >= 0; z--)
            {
                if (!array[z * cellsX + x])
                {
                    AddPlayer(new Vector2Int(x, z));
                    return new Vector2Int(x, z);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    private Vector2Int AddToRight()
    {
        for (int x = 0; x < cellsX; x++)
        {
            for (int z = 0; z < cellsZ; z++)
            {
                if (!array[z * cellsX + x])
                {
                    AddPlayer(new Vector2Int(x, z));
                    return new Vector2Int(x, z);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }
  
    public bool IsFull()
    {
        return currentWaiting == kapa;
    }
}

using UnityEngine;

public class Heatmap : MonoBehaviour
{
    public bool showMax = false;
    public bool showClear = true;


    public float[] properties;
    public int[] playCellCount;
    public int[] playMaxCount;
    public float[] clear;

    public Material material;

    private Bounds b;
    int cellCountX = 0;
    int cellCountZ = 0;
    float normalSizeX = 0;
    float normalSizeZ = 0;
    float randSizeX = 0;
    float randSizeZ = 0;
    int cells;
    private int statCounter = 0;

    public void InitHeatMap(int _cellCountX, int _cellCountZ, float _normalSizeX, float _normalSizeZ, float _randSizeX, float _randSizeZ)
    {
        cellCountX = _cellCountX;
        cellCountZ = _cellCountZ;
        normalSizeX = _normalSizeX;
        normalSizeZ = _normalSizeZ;
        randSizeX = _randSizeX;
        randSizeZ = _randSizeZ;

        cells = cellCountX * cellCountZ;
        b = GetComponent<MeshRenderer>().bounds;
        properties = new float[cells];
        playCellCount = new int[cells];
        playMaxCount = new int[cells];
        clear = new float[cells];

        material.SetInt("_Rows", cellCountX);
        material.SetFloat("_XDistance", normalSizeX);
        material.SetFloat("_ZDistance", normalSizeZ);
        material.SetVector("_MinVals", new Vector2(b.min.x, b.min.z));
    }

   
    public void Reset()
    {
        properties = new float[cells];
        playCellCount = new int[cells];
        playMaxCount = new int[cells];
        showMax = false;
        showClear = true;
        statCounter = 0;
        material.SetFloatArray("_Properties", clear);
    }


    public void ToggleAlphaMode()
    {

        if (statCounter == 0)
        {
            showCurrentAlpha();
            showClear = false;
            showMax = false;        }
        else if (statCounter == 1)
        {
            showMaxAlpha();
            showClear = false;
            showMax = true;
        }
        else
        {
            showClearArray();
            showClear = true;
            showMax = false;
        }

        statCounter = (statCounter+1) % 3;
    }

    private void showClearArray()
    {
        material.SetFloatArray("_Properties", clear);
    }

    private void showMaxAlpha()
    {
        for (int i = 0; i < properties.Length; i++)
        {
            properties[i] = determineAlpha(playMaxCount[i]);
        }
        material.SetFloatArray("_Properties", properties);
    }

    //Muss ausgeführt werden um wieder die aktuelle anzeige anzuzeigen
    private void showCurrentAlpha()
    {
        for (int i = 0; i < properties.Length; i++)
        {
            properties[i] = determineAlpha(playCellCount[i]);
        }
        material.SetFloatArray("_Properties", properties);
    }


    public void Spawned(Vector2Int platePos)
    {
        if(platePos.x < 0 || platePos.x >= cellCountX) { return; } 
        else if (platePos.y < 0 || platePos.y >= cellCountZ) { return; }
        int index = cellCountZ * platePos.x + platePos.y;
        if (index >= 0 && index <= cells)
        {
            playCellCount[index] += 1;
            int c = playCellCount[index];
            int cM = playMaxCount[index];
            if (c > cM) playMaxCount[index] = c;
            properties[index] = determineAlpha(showMax ? cM : c);
            if (showClear) { 
                material.SetFloatArray("_Properties", clear);
            }
            else
            {
                material.SetFloatArray("_Properties", properties);
            }
        }
    }

    public void ClearPos(Vector2Int pos)
    {
        int index1 = cellCountZ * pos.x + pos.y;
        int c = playCellCount[index1];
        if (c > 0)
        {
            playCellCount[index1]--;
            properties[index1] = determineAlpha(c - 1);
            material.SetFloatArray("_Properties", properties);
        }
    }


    public void Moved(Vector2Int from, Vector2Int to)
    {
        if(from == to) { return; }
        if (from.x >= 0 && from.x < cellCountX && from.y >= 0 && from.y < cellCountZ) {
            int index1 = cellCountZ * from.x + from.y;
            playCellCount[index1] -= 1;
            if (showMax)
            {
                properties[index1] = determineAlpha(playMaxCount[index1]);
            }
            else
            {
                properties[index1] = determineAlpha(playCellCount[index1]);
            }

        }
        else { return; }

        if (to.x >= 0 && to.x < cellCountX && to.y >= 0 && to.y < cellCountZ)
        {
            int index2 = cellCountZ * to.x + to.y;
            playCellCount[index2] += 1;

            int c = playCellCount[index2];
            int cM = playMaxCount[index2];
            if (c > cM) playMaxCount[index2] = c;
            if (showMax)
            {
                properties[index2] = determineAlpha(c);
            }
            else
            {
                properties[index2] = determineAlpha(playCellCount[index2]);
            }
        }
        else { return; }


        if (showClear)
        {
            material.SetFloatArray("_Properties", clear);
        }
        else
        {

            material.SetFloatArray("_Properties", properties);
        }
    }

    public float determineAlpha(int usage)
    {
        if(usage == 0)
        {
            return 0;
        }
        if(usage <= usageCat.low)
        {
            return alphaCat.low;
        }
        else if (usage >= usageCat.high)
        {
                return alphaCat.high;
        }
        else
        {
            if(usage < usageCat.medium)
            {
                return alphaCat.mediumLow;
            }else if(usage < usageCat.mediumHigh)
            {
                return alphaCat.medium;
            }
            else
            {
                return alphaCat.mediumHigh;
            }
        }
    }

}
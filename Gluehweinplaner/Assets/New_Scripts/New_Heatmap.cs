using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

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

    public Heatmap(int _cellCountX, int _cellCountZ, float _normalSizeX, float _normalSizeZ, float _randSizeX, float _randSizeZ)
    {
        cellCountX = _cellCountX;
        cellCountZ = _cellCountZ;
        normalSizeX = _normalSizeX;
        normalSizeZ = _normalSizeZ;
        randSizeX = _randSizeX;
        randSizeZ = _randSizeZ;
        b = GetComponent<MeshRenderer>().bounds;
        properties = new float[cells];
        playCellCount = new int[cells];
        playMaxCount = new int[cells];
        clear = new float[cells];
        cells = cellCountX * cellCountZ;

        material.SetInt("_Rows", cellCountX);
        material.SetFloat("_XDistance", normalSizeX);
        material.SetFloat("_ZDistance", normalSizeZ);
        //material.SetFloat("_XRandDistance", randSizeX);
        //material.SetFloat("_ZRandDistance", randSizeZ);
        material.SetVector("_MaxVals", new Vector2(b.max.x, b.max.z));
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
        int index = cellCountX * platePos.x + platePos.y;
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

    public void ClearPos (Vector2Int pos)
    {
        int index1 = cellCountX * pos.x + pos.y;
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
        int index1 = cellCountX * from.x + from.y;
        int index2 = cellCountX * to.x + to.y;
        if ((index1 != index2) && (index1 >= 0) && (index1 <= cells) && (index2 >= 0) && (index2 <= cells))
        {
            playCellCount[index1] -= 1;
            playCellCount[index2] += 1;

            int c = playCellCount[index2];
            int cM = playMaxCount[index2];
                
            if ( c>cM ) playMaxCount[index2] = c;
            if (showClear) {
                material.SetFloatArray("_Properties", clear);
            }
            else
            {
                if (showMax)
                {
                    properties[index1] = determineAlpha(playMaxCount[index1]);
                    properties[index2] = determineAlpha(cM);
                }
                else
                {
                    properties[index1] = determineAlpha(playCellCount[index1]);
                    properties[index2] = determineAlpha(playCellCount[index2]);
                }
                material.SetFloatArray("_Properties", properties);
            }
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
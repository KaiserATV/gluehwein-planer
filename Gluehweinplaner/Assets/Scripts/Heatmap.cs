using UnityEngine;
/// <inheritdoc cref="IHeatmap"/>
public class Heatmap : MonoBehaviour, IHeatmap
{
    public bool showMax = false;
    public bool showClear = true;
    public int Rows = 44;
    public int Cols = 44;
    public float RowHeight = 0;
    public float ColWidth = 0;
    public float[] properties;
    public int[] playCellCount;
    public int[] playMaxCount;
    public float[] clear;

    public Material material;

    private Bounds b;

    private int statCounter = 0;

    public void Start()
    {
        b = GetComponent<MeshRenderer>().bounds;
        properties = new float[2000];
        playCellCount = new int[2000];
        playMaxCount = new int[2000];
        clear = new float[2000];
        RowHeight = b.size.x / Rows;
        ColWidth = b.size.z / Cols;

        material.SetInt("_Rows", Rows);
        material.SetFloat("_XDistance", RowHeight);
        material.SetFloat("_ZDistance", ColWidth);
        material.SetVector("_MinVals", new Vector2(b.min.x, b.min.z));
    }


    public void Reset()
    {
        properties = new float[2000];
        playCellCount = new int[2000];
        playMaxCount = new int[2000];
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
            showMax = false;
        }
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

        statCounter = (statCounter + 1) % 3;
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


    public void Spawned(Vector3 spawnedPos)
    {
        if (spawnedPos.x < b.min.x || spawnedPos.x > b.max.x || spawnedPos.z < b.min.z || spawnedPos.z > b.max.z) { return; }
        Vector2Int inArray = new Vector2Int(
            Mathf.FloorToInt((spawnedPos.x - b.min.x) / RowHeight)
            ,
            Mathf.FloorToInt((spawnedPos.z - b.min.z) / ColWidth)
            );
        int index = Cols * inArray.x + inArray.y;
        if (index < 0 || index >= 2000) { return; }
        playCellCount[index] += 1;
        int c = playCellCount[index];
        int cM = playMaxCount[index];
        if (c > cM) playMaxCount[index] = c;
        properties[index] = determineAlpha(showMax ? cM : c);
        if (showClear)
        {
            material.SetFloatArray("_Properties", clear);
        }
        else
        {
            material.SetFloatArray("_Properties", properties);
        }
    }

    public void ClearPos(Vector3 pos)
    {
        if (pos.x < b.min.x || pos.x > b.max.x || pos.z < b.min.z || pos.z > b.max.z) { return; }
        Vector2Int inArray = new Vector2Int(
        Mathf.FloorToInt((pos.x - b.min.x) / RowHeight)
        ,
        Mathf.FloorToInt((pos.z - b.min.z) / ColWidth)
        );
        int index = Cols * inArray.x + inArray.y;
        if (index < 0 || index >= 2000) { return; }
        int c = playCellCount[index];
        if (c > 0)
        {
            playCellCount[index]--;
            properties[index] = determineAlpha(c - 1);
            material.SetFloatArray("_Properties", properties);
        }
    }


    public void Moved(Vector3 from, Vector3 to)
    {
        if (from == to) { return; }
        Vector2Int inArrayFrom = new Vector2Int(
            Mathf.FloorToInt((from.x - b.min.x) / RowHeight)
            ,
            Mathf.FloorToInt((from.z - b.min.z) / ColWidth)
            );
        int indexFrom = Cols * inArrayFrom.x + inArrayFrom.y;

        Vector2Int inArrayTo = new Vector2Int(
            Mathf.FloorToInt((to.x - b.min.x) / RowHeight)
            ,
            Mathf.FloorToInt((to.z - b.min.z) / ColWidth)
            );
        int indexTo = Cols * inArrayTo.x + inArrayTo.y;
        if (indexFrom == indexTo) { return; }
        if (from.x < b.min.x || from.x > b.max.x || from.z < b.min.z || from.z > b.max.z)
        {
            if (to.x < b.min.x || to.x > b.max.x || to.z < b.min.z || to.z > b.max.z) { return; }
            playCellCount[indexTo] += 1;
            if (playCellCount[indexTo] > playMaxCount[indexTo]) { playMaxCount[indexTo] = playCellCount[indexTo]; }
            if (showMax)
            {
                properties[indexTo] = determineAlpha(playMaxCount[indexTo]);
            }
            else
            {
                properties[indexTo] = determineAlpha(playCellCount[indexTo]);
            }
        }
        else
        {
            if (to.x >= b.min.x && to.x <= b.max.x && to.z >= b.min.z && to.z <= b.max.z)
            {
                playCellCount[indexTo] += 1;
                if (playCellCount[indexTo] > playMaxCount[indexTo]) { playMaxCount[indexTo] = playCellCount[indexTo]; }
                if (showMax)
                {
                    properties[indexTo] = determineAlpha(playMaxCount[indexTo]);
                }
                else
                {
                    properties[indexTo] = determineAlpha(playCellCount[indexTo]);
                }
            }
            playCellCount[indexFrom] -= 1;
            if (showMax)
            {
                properties[indexFrom] = determineAlpha(playMaxCount[indexFrom]);
            }
            else
            {
                properties[indexFrom] = determineAlpha(playCellCount[indexFrom]);
            }
        }
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
        if (usage == 0)
        {
            return 0;
        }
        if (usage <= usageCat.low)
        {
            return alphaCat.low;
        }
        else if (usage >= usageCat.high)
        {
            return alphaCat.high;
        }
        else
        {
            if (usage < usageCat.medium)
            {
                return alphaCat.mediumLow;
            }
            else if (usage < usageCat.mediumHigh)
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
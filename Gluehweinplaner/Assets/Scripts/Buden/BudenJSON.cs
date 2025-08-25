using System;

[Serializable]
public class BudenJSON
{
    public float xPos;
    public float zPos;
    public float yRot;
    public int typeIndex;
    public int attrak;
    public int waittime;

    public BudenJSON(float x, float z, float r, int i, int a, int w)
    {
        xPos = x;
        zPos = z;
        yRot = r;
        typeIndex = i;
        attrak = a;
        waittime = w;
    }
}

[Serializable]
public class ExitJSON
{
    public float xPos;
    public float zPos;
    public float yRot;

    public ExitJSON(float x, float z, float r)
    {
        xPos = x;
        zPos = z;
        yRot = r;
    }
}

[Serializable]
public class SpawnJSON
{
    public float xPos;
    public float zPos;
    public float yRot;
    public float spawnTime;

    public SpawnJSON(float x, float z, float r, float st)
    {
        xPos = x;
        zPos = z;
        yRot = r;
        spawnTime = st;
    }
}


[Serializable]
public class AlleBudenJSON
{
    public BudenJSON[] budenArray;
    public AlleBudenJSON(int s)
    {
        budenArray = new BudenJSON[s];
    }
}

[Serializable]
public class AlleExitJSON
{
    public ExitJSON[] exitArray;
    public AlleExitJSON(int s)
    {
        exitArray = new ExitJSON[s];
    }
}

[Serializable]
public class AlleSpawnJSON
{
    public SpawnJSON[] spawnArray;
    public AlleSpawnJSON(int s)
    {
        spawnArray = new SpawnJSON[s];
    }
}


[Serializable]
public class GanzeSzene
{
    public AlleBudenJSON alleBuden;
    public AlleExitJSON allExits;
    public AlleSpawnJSON alleSpawns;

    public GanzeSzene(AlleBudenJSON ab, AlleExitJSON ae, AlleSpawnJSON asp)
    {
        alleBuden = ab;
        allExits = ae;
        alleSpawns = asp;
    }
}
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
public class AlleBudenJSON
{
    public BudenJSON[] budenArray;
    public AlleBudenJSON(int s)
    {
        budenArray = new BudenJSON[s];
    }
}

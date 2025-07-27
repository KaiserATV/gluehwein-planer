using System;

[Serializable]
public class New_BudenJSON
{
    public float xPos;
    public float zPos;
    public float yRot;
    public int typeIndex;
    public int attrak;
    public float waittime;

    public New_BudenJSON(float x, float z,float r, int i, int a, float w)
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
public class New_AlleBudenJSON
{
    public New_BudenJSON[] budenArray;
    public New_AlleBudenJSON(int s)
    {
        budenArray = new New_BudenJSON[s];
    }
}


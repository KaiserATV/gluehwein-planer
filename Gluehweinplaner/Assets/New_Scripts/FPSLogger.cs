using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FPSLogger : MonoBehaviour
{
    List<(int, float, float, float)> stats = new List<(int, float, float, float)> ();
    int agentenLogIntervall = 50;
    New_SceneManager sc;



    float currentFps = 0;
    float avgFps = 0;
    int passedFrams = 0;
    float minFps = float.MaxValue;
    float maxFps = 0;
    int lastAgentCount = 0;

    private void Start()
    {
        sc = GameObject.Find("SceneManager").GetComponent<New_SceneManager>();
        currentFps = 1f / Time.deltaTime;
        avgFps += currentFps;
        passedFrams++;
    }
    
    void Update()
    {
        currentFps = 1f / Time.deltaTime;
        avgFps += currentFps;
        passedFrams++;
        if(currentFps < minFps)
        {
            minFps = currentFps;
        }
        else if(currentFps > maxFps)
        {
            maxFps = currentFps;
        }


        if (sc.playerCount % agentenLogIntervall == 0 && sc.playerCount != lastAgentCount)
        {
            stats.Add((sc.playerCount,(float)Math.Round(avgFps/passedFrams,2), (float)Math.Round(minFps,2), (float)Math.Round(maxFps, 2)));
            currentFps = 0;
            avgFps = 0;
            passedFrams = 0;
            minFps = float.MaxValue;
            maxFps = 0;
            lastAgentCount = sc.playerCount;
        }
    }

    public void OnApplicationQuit()
    {
        string path = Application.persistentDataPath + "/benchmarks.csv";
        Debug.Log("Speichere JSON nach: " + path);
        using (StreamWriter writer = new StreamWriter(path, false))
        {
            writer.Write(CreateCSV());
        }
    }

    private string CreateCSV()
    {
        string returnString = "Agents,avgFPS,minFPS,maxFPS\n";
        foreach (var item in stats)
        {
            returnString += item.Item1 + "," + item.Item2 + "," + item.Item3 + "," + item.Item4 + "\n";
        }
        return returnString;
    }

}

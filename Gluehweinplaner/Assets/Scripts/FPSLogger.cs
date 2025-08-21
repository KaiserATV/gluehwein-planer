using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;


public class FPSLogger : MonoBehaviour
{
    List<(int,float, int ,double, double, double, double)> stats = new List<(int, float, int, double, double, double, double)> ();
    SceneManager sc;
    StringBuilder advancedStats = new StringBuilder();
    StringBuilder fpsStats = new StringBuilder();
    //https://docs.unity3d.com/2020.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html
    ProfilerRecorder systemMemoryRecorder;
    ProfilerRecorder gcMemoryRecorder;
    ProfilerRecorder mainThreadTimeRecorder;
    ProfilerRecorder GPUFrameTimeRecorder;
    ProfilerRecorder CPUTotalFrameTimeRecorder;
    int passedFrames = 0;
    private void Start()
    {
        sc = GameObject.Find("SceneManager").GetComponent<SceneManager>();
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
        CPUTotalFrameTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "CPU Total Frame Time");
        GPUFrameTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "GPU Frame Time");
        passedFrames++;
    }

    void Update()
    {
        passedFrames++;
        if (passedFrames % 10 == 0)
        {
            stats.Add((passedFrames, Time.deltaTime, sc.playerCount ,Math.Round(GetRecorderFrameAverage(mainThreadTimeRecorder),3), Math.Round((double)systemMemoryRecorder.LastValue,3), Math.Round((double)CPUTotalFrameTimeRecorder.LastValue,3), Math.Round((double)GPUFrameTimeRecorder.LastValue,3)));
        }
        if (!sc.CanAddPlayer())
        {
            stats.Add((passedFrames, Time.deltaTime, sc.playerCount, Math.Round(GetRecorderFrameAverage(mainThreadTimeRecorder), 3), Math.Round((double)systemMemoryRecorder.LastValue, 3), Math.Round((double)CPUTotalFrameTimeRecorder.LastValue, 3), Math.Round((double)GPUFrameTimeRecorder.LastValue, 3)));
            Application.Quit();
            EditorApplication.isPlaying = false;
        }
    }

    public void OnApplicationQuit()
    {
        string pathTwo = Application.persistentDataPath + "/advanced-stats.csv";
        advancedStats.AppendLine("FrameNr.,FPS,agentCount,FrameTime in ms,System Memory in MB, CPU Frame Time in ms, GPU Frame Time in ms");
        foreach ((int, float, int, double, double, double, double) stat in stats)
        {
            advancedStats.AppendLine($"{stat.Item1},{Math.Round(1f / stat.Item2,3)},{stat.Item3},{Math.Round(stat.Item4 * (1e-6f),3)},{Math.Round(stat.Item5 / (1024 * 1024),3)},{Math.Round(stat.Item6 / 1000000f,3)},{Math.Round(stat.Item7 / 1000000f,3)}");
        }

        using (StreamWriter writer = new StreamWriter(pathTwo, false))
        {
            writer.Write(advancedStats.ToString());
        }
        systemMemoryRecorder.Dispose();
        gcMemoryRecorder.Dispose();
        mainThreadTimeRecorder.Dispose();
        GPUFrameTimeRecorder.Dispose();
        CPUTotalFrameTimeRecorder.Dispose();
    }
    static double GetRecorderFrameAverage(ProfilerRecorder recorder)
    {
        var samplesCount = recorder.Capacity;
        if (samplesCount == 0)
            return 0;

        double r = 0;
        unsafe
        {
            var samples = stackalloc ProfilerRecorderSample[samplesCount];
            recorder.CopyTo(samples, samplesCount);
            for (var i = 0; i < samplesCount; ++i)
                r += samples[i].Value;
            r /= samplesCount;
        }
        return r;
    }
}

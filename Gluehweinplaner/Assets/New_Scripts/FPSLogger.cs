using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;


public class FPSLogger : MonoBehaviour
{
    List<(int,int, float, float, float)> stats = new List<(int,int, float, float, float)> ();
    New_SceneManager sc;
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
        sc = GameObject.Find("SceneManager").GetComponent<New_SceneManager>();
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
        CPUTotalFrameTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "CPU Total Frame Time");
        GPUFrameTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "GPU Frame Time");
        advancedStats.AppendLine("FrameNr.,FPS,agentCount,FrameTime in ms,System Memory in MB, CPU Frame Time in ms, GPU Frame Time in ms");
    }

    void Update()
    {
        advancedStats.AppendLine($"{passedFrames},{1f / Time.deltaTime},{sc.playerCount},{GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f):F1},{systemMemoryRecorder.LastValue / (1024 * 1024)},{CPUTotalFrameTimeRecorder.LastValue},{GPUFrameTimeRecorder.LastValue}");
    }

    public void OnApplicationQuit()
    {
        string pathTwo = Application.persistentDataPath + "/advanced-stats.csv";
        Debug.Log("Speichere JSON nach: " + pathTwo);
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

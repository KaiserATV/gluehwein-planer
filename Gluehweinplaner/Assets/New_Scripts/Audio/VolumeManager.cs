using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VolumeManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    public void SetMasterVolume(float level) 
    {
        float dB = Mathf.Clamp(Mathf.Log10(level) * 20f, -80f, 0f);
        audioMixer.SetFloat("masterVolume", dB);
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadialSlider : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI valueText;
    public VolumeManager volumeManager;

    void Start()
    {
        // Initialize with saved volume
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        slider.value = savedVolume;
        UpdateVolume(savedVolume);
    }

    public void UpdateVolume(float value)
    {
        // Update text and volume
        valueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        volumeManager.SetMasterVolume(value);
        
        // Save preference
        PlayerPrefs.SetFloat("MasterVolume", value);
    }
}
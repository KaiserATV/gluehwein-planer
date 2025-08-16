// Create new script VolumeMenuCloser.cs
using UnityEngine;
using UnityEngine.UI;

public class VolumeMenuCloser : MonoBehaviour
{
    public RadicalSelection radialSelection;
    
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            radialSelection.ToggleVolumeMenu();
        });
    }
}
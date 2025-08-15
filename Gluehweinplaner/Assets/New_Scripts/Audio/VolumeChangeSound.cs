using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]

public class VolumeChangeSound : MonoBehaviour
{
    [SerializeField] private AudioClip hoverSound;
    private bool isHovering;

    [Header("Hover Settings")]
    [SerializeField] private float cooldown = 0.2f;
    private float lastPlayTime;

    void Start()
    {
        AddHoverTrigger();
    }

    void AddHoverTrigger()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        
        entry.callback.AddListener((data) => { OnPointerEnter(); });
        trigger.triggers.Add(entry);
    }

    void OnPointerEnter()
    {
        if(!isHovering)
        {
            SoundFXManager.instance.PlaySoundFXClip(hoverSound, transform, 0.8f);
            isHovering = true;
        }

        if(Time.time > lastPlayTime + cooldown)
        {
            SoundFXManager.instance.PlaySoundFXClip(hoverSound, transform, 0.8f);
            lastPlayTime = Time.time;
        }
    }

    // Optional: Add pointer exit reset
    void OnPointerExit()
    {
        isHovering = false;
    }
}



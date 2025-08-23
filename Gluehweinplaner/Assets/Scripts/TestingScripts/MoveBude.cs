using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBude : MonoBehaviour
{
    public Bude bude;
    public bool switchNow = false;
    private Vector3[] positions = { new(0, 0, 17.65f), new(0, 0, -17.65f) };
    int switches = 0;
    void Update()
    {
        if (switchNow)
        {
            SwitchPos();
            switchNow = false;
        }
    }
    
    private void SwitchPos()
    {
        this.transform.position = positions[switches%2];
        switches++;
        bude.BudeMoved();
    }

}

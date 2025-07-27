using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Bounds bound;
    private Collider col;


    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
        bound = col.bounds;
    }

    public Bounds GetBound(){
        return bound;
    }
}

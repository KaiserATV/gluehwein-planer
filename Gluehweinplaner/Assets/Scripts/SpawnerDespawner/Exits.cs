using System.Collections.Generic;
using UnityEngine;

public class Exits : MonoBehaviour
{

    private Bounds b;

    private void Start()
    {
        b = GetComponent<MeshRenderer>().bounds;
    }


    public Vector3 GetClostestPoint()
    {
        return this.transform.position;
    }

    public ExitJSON ToJSON()
    {
        return new ExitJSON(this.transform.position.x, this.transform.position.z, this.transform.rotation.y);
    }
}

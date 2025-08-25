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
        return transform.position;
    }

    public ExitJSON ToJSON()
    {
        return new ExitJSON(this.transform.position.x, this.transform.position.z, this.transform.rotation.y);
    }
}

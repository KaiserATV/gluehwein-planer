using UnityEngine;

public class Exit : MonoBehaviour
{

    private Bounds b;

    private void Start()
    {
        b = GetComponentInChildren<MeshRenderer>().bounds;
    }


    public Vector3 GetClostestPoint()
    {
        return this.transform.position;
    }

    public Vector3 GetPosition()
    {
        return this.transform.position;
    }

    public ExitJSON ToJSON()
    {
        return new ExitJSON(this.transform.position.x, this.transform.position.z, this.transform.rotation.y);
    }
}

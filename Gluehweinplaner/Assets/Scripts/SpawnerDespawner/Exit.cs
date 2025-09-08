using UnityEngine;
/// <inheritdoc cref="IExit"/>
public class Exit : MonoBehaviour, IExit
{
    private Bounds b;
    private void Start()
    {
        b = GetComponentInChildren<MeshRenderer>().bounds;
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

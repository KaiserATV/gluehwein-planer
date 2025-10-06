using UnityEngine;

public class MoveBude : MonoBehaviour
{
    private Bude bude;
    public bool switchNow = false;
    //public MeshRenderer teleportArea;
    //public Bounds teleportBounds;
    //private MeshRenderer budeRenderer;
    public float offsetTimer;
    private Vector3[] posses = new Vector3[2];
    private int changes = 1;

    private void Start()
    {
        posses[0] = new Vector3(1.42f,0,-14.98f);
        posses[1] = new Vector3(1.42f,0,15.46f);

        offsetTimer = Random.Range(-10, 60);
        bude = GetComponent<Bude>();
        //budeRenderer = GetComponentInChildren<MeshRenderer>();
        //teleportBounds = teleportArea.GetComponent<MeshRenderer>().bounds;
    }

    void FixedUpdate()
    {
        if (switchNow)
        {
            SwitchPos();
            changes ++;
            switchNow = false;
        }
    }

    private void SwitchPos()
    {
        //Vector3 newPos;
        //do
        //{
        //    newPos = new(Random.Range(teleportBounds.min.x, teleportBounds.max.x), 0, Random.Range(teleportBounds.min.x, teleportBounds.max.x));
        //}
        //while (!Physics.CheckBox(newPos, budeRenderer.bounds.extents));
        this.transform.position = posses[changes%2];
        bude.BudeMoved();
    }
}

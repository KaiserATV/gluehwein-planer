using UnityEngine;

public class MoveBude : MonoBehaviour
{
    private Bude bude;
    public bool switchNow = false;
    public MeshRenderer teleportArea;
    public Bounds teleportBounds;
    private MeshRenderer budeRenderer;
    public float offsetTimer;
    public const float changeTimer = 30;
    public float timePassed = 0;

    private void Start()
    {
        offsetTimer = Random.Range(-10, 60);
        bude = GetComponent<Bude>();
        budeRenderer = GetComponentInChildren<MeshRenderer>();
        teleportBounds = teleportArea.GetComponent<MeshRenderer>().bounds;
    }

    void FixedUpdate()
    {
        if (timePassed > (changeTimer+offsetTimer))
        {
            SwitchPos();
        }
        else
        {
            timePassed += Time.fixedDeltaTime;
        }
    }
    
    private void SwitchPos()
    {
        Vector3 newPos;
        do
        {
            newPos = new(Random.Range(teleportBounds.min.x, teleportBounds.max.x),0, Random.Range(teleportBounds.min.x, teleportBounds.max.x));
        }
        while (!Physics.CheckBox(newPos, budeRenderer.bounds.extents));

        this.transform.position = newPos;
        timePassed = 0;
        bude.BudeMoved();
    }
}

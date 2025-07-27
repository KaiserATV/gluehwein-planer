using UnityEngine;

public class New_CrowdGeneration : MonoBehaviour
{
    private GameObject prop;
    public float minWorldLimitX = 0;
    public float maxWorldLimitX = 0;
    public float minWorldLimitZ = 0;
    public float maxWorldLimitZ = 0;
    public float spawnTime = 1f;
    public float agentradius = 1f;

    private float zeitVergangen;

    private New_SceneManager sm;
    private MeshCollider col;
    private New_InactiveAgentsContainer iac;

    // Start is called before the first frame update
    void Start()
    {
        zeitVergangen = spawnTime;
        sm = GameObject.Find("SceneManager").GetComponent<New_SceneManager>();
        iac = GameObject.Find("InactiveAgentHolder").GetComponent<New_InactiveAgentsContainer>();

        prop = Resources.Load("agent") as GameObject;
        col = GetComponent<MeshCollider>();

        minWorldLimitX = col.bounds.min.x;
        maxWorldLimitX = col.bounds.max.x;
        minWorldLimitZ = col.bounds.min.z;
        maxWorldLimitZ = col.bounds.max.z;
    }

    private void FixedUpdate()
    {
        if (sm.simulating)
        {
            zeitVergangen -= Time.deltaTime;
            if (zeitVergangen > 0)
            {

            }
            else if (sm.CanAddPlayer())
            {
                Vector3 position = GenerateRandomPosition();
                Quaternion rotation = Quaternion.Euler(0, 0, 0);
                if (iac.GetStoredCount()>0)
                {
                    New_NPC ac = iac.GetAgent();
                    ac.Respawn();
                    sm.inactivePlayerCount--;
                    sm.playerCount++;
                }
                else 
                {
                    GameObject agent = Instantiate(prop, position, rotation);
                    agent.transform.parent = transform;
                    sm.playerCount++;
                }
                    zeitVergangen = spawnTime;
            }

        }
    }

    public Vector3 GenerateRandomPosition()
    {
        Vector3 position;
        do
        {
            float cellX = Random.Range(minWorldLimitX, maxWorldLimitX);
            float cellZ = Random.Range(minWorldLimitZ, maxWorldLimitZ);
            position = new Vector3(cellX, col.bounds.min.y + 1, cellZ);


        } while (Physics.CheckSphere(position,agentradius));

        return position;
    }

}
using UnityEngine;

public class CrowdGeneration : MonoBehaviour
{
    private GameObject prop;
    public float minWorldLimitX = 0;
    public float maxWorldLimitX = 0;
    public float minWorldLimitZ = 0;
    public float maxWorldLimitZ = 0;
    public float spawnTime = 1f;
    public float agentradius = 1f;

    private float zeitVergangen;

    private AgentManager am;
    private MeshCollider col;
    private InactiveAgentsContainer iac;

    // Start is called before the first frame update
    void Start()
    {
        zeitVergangen = spawnTime;
        am = GameObject.Find("AgentManager").GetComponent<AgentManager>();
        iac = GameObject.Find("InactiveAgentHolder").GetComponent<InactiveAgentsContainer>();

        prop = Resources.Load("agent") as GameObject;
        col = GetComponent<MeshCollider>();

        minWorldLimitX = col.bounds.min.x;
        maxWorldLimitX = col.bounds.max.x;
        minWorldLimitZ = col.bounds.min.z;
        maxWorldLimitZ = col.bounds.max.z;
    }

    private void FixedUpdate()
    {
        if (am.simulating)
        {
            zeitVergangen -= Time.deltaTime;
            if (zeitVergangen > 0)
            {

            }
            else if (am.CanAddPlayer())
            {
                Vector3 position = GenerateRandomPosition();
                Quaternion rotation = Quaternion.Euler(0, 0, 0);
                if (iac.GetStoredCount()>0)
                {
                    AgentController ac = iac.GetAgent();
                    ac.Respawn();
                    am.addPlayer(ac.GetComponent<AgentController>());
                    am.inactivePlayerCount--;
                    am.playerCount++;
                }
                else 
                {
                    GameObject agent = Instantiate(prop, position, rotation);
                    am.addPlayer(agent.GetComponent<AgentController>());
                    agent.transform.parent = transform;
                    am.playerCount++;
                }
                    zeitVergangen = spawnTime;
            }

        }
    }

    public Vector3 GenerateRandomPosition()
    {
        //Vector3 position;
        //do
        //{
        //    float cellX = Random.Range(minWorldLimitX, maxWorldLimitX);
        //    float cellZ = Random.Range(minWorldLimitZ, maxWorldLimitZ);
        //    position = new Vector3(cellX, col.bounds.min.y + 1, cellZ);


        //} while (Physics.CheckSphere(position,agentradius));

        return transform.position;
    }

    public SpawnJSON ToJSON()
    {
        return new SpawnJSON(this.transform.position.x, this.transform.position.z, this.transform.rotation.y, spawnTime);
    }
}
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private GameObject prop;
    public float minWorldLimitX = 0;
    public float maxWorldLimitX = 0;
    public float minWorldLimitZ = 0;
    public float maxWorldLimitZ = 0;
    public float spawnTime = 1f;
    public float agentradius = 1f;

    private float zeitVergangen;

    private SceneManager sm;
    private InactiveAgentsContainer iac;

    // Start is called before the first frame update
    void Start()
    {
        zeitVergangen = spawnTime;
        sm = GameObject.Find("SceneManager").GetComponent<SceneManager>();
        iac = GameObject.Find("InactiveAgentHolder").GetComponent<InactiveAgentsContainer>();

        prop = Resources.Load("NPC") as GameObject;
        Bounds b = GetComponentInChildren<MeshRenderer>().bounds;

        minWorldLimitX = b.min.x;
        maxWorldLimitX = b.max.x;
        minWorldLimitZ = b.min.z;
        maxWorldLimitZ = b.max.z;
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
                (Vector3 position,Vector3 egal) = GenerateRandomPosition();
                Quaternion rotation = Quaternion.Euler(0, 0, 0);
                if (iac.GetStoredCount()>0)
                {
                    NPC_navmesh ac = iac.GetAgent();
                    ac.Respawn();
                    sm.inactivePlayerCount--;
                    sm!.addPlayer(ac);
                }
                else 
                {
                    if (position != null) {
                        Debug.Log(position);
                        GameObject agent = Instantiate(prop, position, rotation);
                        agent.transform.SetParent(transform, false);
                        sm.playerCount++;
                    }
                }
                    zeitVergangen = spawnTime;
            }

        }
    }


    //public Vector3? GenerateRandomPosition() { return this.transform.position; }
    public (Vector3,Vector3) GenerateRandomPosition()
    {
        //Vector3 position = new Vector3(minWorldLimitX,1.5f,minWorldLimitZ);
        //do
        //{
        //    position.x = Random.Range(minWorldLimitX, maxWorldLimitX);
        //    position.z = Random.Range(minWorldLimitZ, maxWorldLimitZ);
        //} while (Physics.CheckSphere(position, 1f));
        //position.y = 0;
        return (transform.position + new Vector3(0, 0.5f, 0), transform.position + new Vector3(0, 0.5f, 0));
    }


    public SpawnJSON ToJSON()
    {
        return new SpawnJSON(this.transform.position.x,this.transform.position.z, this.transform.rotation.y, spawnTime);   
    }
}
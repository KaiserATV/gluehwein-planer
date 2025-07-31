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
                Vector3? position = GenerateRandomPosition();
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
                    if (position != null) {
                        GameObject agent = Instantiate(prop, position!.Value, rotation);
                        agent.transform.parent = transform;
                        sm.playerCount++;
                    }
                }
                    zeitVergangen = spawnTime;
            }

        }
    }


    public Vector3? GenerateRandomPosition() { return this.transform.position; }
    //public Vector3? GenerateRandomPosition()
    //{
    //    int maxTrys = 3;
    //    Vector3? position = null;
    //    while (maxTrys != 0)
    //    {
    //        float cellX = Random.Range(minWorldLimitX, maxWorldLimitX);
    //        float cellZ = Random.Range(minWorldLimitZ, maxWorldLimitZ);
    //        position = new Vector3(cellX, -1, cellZ);
    //        if (!Physics.Raycast(position!.Value, new Vector3(0,1,0),2, New_GenerateMatrix.ObstacleLayer))
    //        {
    //            position = null;
    //        }
    //        maxTrys--;
    //    }
    //    return position;
    //}

}
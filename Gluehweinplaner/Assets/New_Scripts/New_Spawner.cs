using UnityEngine;

public class New_Spawner : MonoBehaviour
{
    private GameObject prop;
    public float minWorldLimitX = 0;
    public float maxWorldLimitX = 0;
    public float minWorldLimitZ = 0;
    public float maxWorldLimitZ = 0;
    public float spawnTime = 1f;
    public float agentradius = 1f;
    private float zeitVergangen;
    private New_SceneManager sc;
    private MeshCollider col;
    private New_InactiveAgentsContainer iac;
    public Vector2Int onPlate { get; set; }

    void Start()
    {
        zeitVergangen = spawnTime;
        sc = GameObject.Find("SceneManager").GetComponent<New_SceneManager>();
        iac = GameObject.Find("InactiveAgentHolder").GetComponent<New_InactiveAgentsContainer>();

        prop = Resources.Load("New_agent") as GameObject;
        col = GetComponent<MeshCollider>();

        minWorldLimitX = col.bounds.min.x;
        maxWorldLimitX = col.bounds.max.x;
        minWorldLimitZ = col.bounds.min.z;
        maxWorldLimitZ = col.bounds.max.z;
    }


    private void FixedUpdate()
    {
        if (sc.simulating)
        {
            zeitVergangen -= Time.deltaTime;
            if (zeitVergangen > 0)
            {

            }
            else if (sc.CanAddPlayer())
            {
                Vector3? position = GenerateRandomPosition();
                Quaternion rotation = Quaternion.Euler(0, 0, 0);
                if (iac.GetStoredCount() > 0)
                {
                    New_NPC ac = iac.GetAgent();
                    ac.Respawn();
                    sc.inactivePlayerCount--;
                    sc.playerCount++;
                }
                else
                {
                    if (position != null)
                    {
                        GameObject agent = Instantiate(prop, position!.Value, rotation);
                        agent.transform.parent = transform;
                        sc.playerCount++;
                    }
                }
                zeitVergangen = spawnTime;
            }

        }
    }

    public Vector3? GenerateRandomPosition()
    {
        //int maxTrys = 3;
        //Vector3? position = null;
        //while (maxTrys != 0)
        //{
        //    float cellX = Random.Range(minWorldLimitX, maxWorldLimitX);
        //    float cellZ = Random.Range(minWorldLimitZ, maxWorldLimitZ);
        //    position = new Vector3(cellX, -1, cellZ);
        //    if (Physics.Raycast(position!.Value,new Vector3(0,1f,0),2, New_GenerateMatrix.ObstacleLayer))
        //    {
        //        position = null;
        //    }
        //    maxTrys--;
        //}
        return this.transform.position;
    }
    


    public Vector3 GetPosition()
    {
        return this.transform.position;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(this.transform.position,new Vector3(0.5f,0.1f,0.5f));
    }
}

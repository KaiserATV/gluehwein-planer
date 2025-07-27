using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    // Start is called before the first frame update
    private UnityEngine.AI.NavMeshAgent agent;
    
    public bool randomExitGoalNumber = true;
    public int goalsBeforeExit;
    public int goalNr;
    public const float patience = 120f;
    public float patienceLost;
    public bool inactive = false;


    public bool stopped = false;
    public bool waiting = false;
    public float timeLeftWaiting = 0.0f;

    public bool exiting = false;
    public float goalThreshhold = 1f;
    public float exitTrashhold = 1f;

    private Vector2Int bitarrayCells;
    private Vector2Int positionCells;
    private Vector2 goal;
    private New_New_BitArray2D bude;
    private AgentManager sm;
    private List<int> visitedGoalNumbers =  new List<int>();
    

    public const float updateRate = 5.0f;


    // Update is called once per frame
    void Start()
    {
        sm = GameObject.Find("AgentManager").GetComponent<AgentManager>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.autoRepath = true;
        sm.addPlayer(this);
        if (randomExitGoalNumber) { goalsBeforeExit = Random.Range(0, sm.GetBudenCount() + 1); }
        positionCells = sm.UpdatePositionInGrid(new Vector2(transform.position.x, transform.position.z));
        FindNextGoal();
        agent.destination = new Vector3(goal.x, 0, goal.y);
    }

    void Update()
    {
        if (!stopped)
        {
            if (waiting)
            {
                if(Vector3.Distance(this.transform.position, agent.destination)>goalThreshhold)
                {
                    waiting = false;
                    stopped = false;
                    agent.isStopped = false;
                }
                else
                {
                    timeLeftWaiting -= Time.deltaTime;
                    if (timeLeftWaiting < 0)
                    {
                        bude.RemovePlayer(bitarrayCells, this);
                        if (bude != null) {
                            FindNextGoal();
                        } else {
                            FindExit();
                        }
                    }
                }
            }
            else if (agent.remainingDistance < goalThreshhold && !exiting && bude != null)
            {
                timeLeftWaiting = sm.GetWaitTime(goalNr);
                waiting = true;
                agent.isStopped = true;
            }
            else if (agent.remainingDistance < exitTrashhold && exiting)
            {
                Respawn();
            }
            else
            {
                patienceLost -= Time.deltaTime;
                if (patienceLost <= 0)
                {
                    if (goalNr >= 0 && bude!=null) { bude.RemovePlayer(bitarrayCells, this); }
                    sm.LostPatience();
                    FindNextGoal();
                    patienceLost = patience;
                }
                positionCells = sm.UpdatePositionInGrid(positionCells, new Vector2(this.transform.position.x, this.transform.position.z));
            }
        }
    }
    public void SetInactive()
    {
        if (!exiting && bude!=null)
        {
            bude.RemovePlayer(bitarrayCells, this);
        }
        bude = null;
        inactive = true;
        agent.isStopped=true;
        stopped = true;
        waiting = false;
        Vector3 newp = sm.GetIACPos();
        sm.ClearPosition(positionCells);
        agent.Warp(newp);
    }


     void FindNextGoal()
    {
        waiting = false;
        stopped = false;
        agent.isStopped = false;
        timeLeftWaiting = 0.0f;
        if (goalsBeforeExit > 0 && !exiting && !inactive)
        {
            do
            {
                goalNr = sm.GetNewCoords(this, visitedGoalNumbers);
                if (goalNr == -1) { FindExit(); return; }
            } while (visitedGoalNumbers.Contains(goalNr));
            visitedGoalNumbers.Add(goalNr);
            goalsBeforeExit--;
        }
        else
        {
            FindExit();
        }
        patienceLost = patience;
    }

    void FindExit()
    {
        agent.isStopped = false;
        stopped = false;
        waiting = false;
        exiting = true;
        goal = sm.GetClostestExit(transform.position);
        agent.destination = new Vector3(goal.x, 0, goal.y);
    }

    public void InvalidatePosition(Vector3 newCoords)
    {
        if (waiting) { waiting = false; }
        if (sm.simulating) { agent.isStopped = false; stopped = false; }
        goal = newCoords;
        agent.destination = new Vector3(goal.x, 0, goal.y);
    }

    public void Respawn()
    {
        agent.Warp(sm.GetNewSpawnPoint());//to random spawner

        agent.isStopped=false;
        stopped = false;
        waiting = false;
        exiting = false;
        inactive = false;

        timeLeftWaiting = 0.0f;

        visitedGoalNumbers = new List<int>();
        bude = null;

        patienceLost = patience;

        agent.autoRepath = true;
        sm.addPlayer(this);
        if (randomExitGoalNumber) { goalsBeforeExit = Random.Range(0, sm.GetBudenCount() + 1); }
        positionCells = sm.UpdatePositionInGrid(new Vector2(transform.position.x, transform.position.z));
        FindNextGoal();
        agent.destination = new Vector3(goal.x, 0, goal.y);
    }

    public void BudeDestroyed()
    {
        bude = null;

        FindNextGoal();

        agent.destination = new Vector3(goal.x, 0, goal.y);

        waiting = false;
        if (sm.simulating)
        {
            stopped = false;
            exiting = false;
            agent.isStopped = false;
        }
        else
        {
            stopped = true;
            exiting = true;
            agent.isStopped = true;
        }
        timeLeftWaiting = 0.0f;


    }

    public void Stop()
    {
        agent.isStopped = true;
        stopped = true;
    }

    public void Resume()
    {
        agent.isStopped = false;
        stopped = false;
        agent.destination = goal;
    }

    public void SetGoal(Vector2 g)
    {
        goal = g;
    }
    public void SetBude(New_New_BitArray2D b)
    {
        bude = b;
    }
    public void SetCells(Vector2Int v)
    {
        bitarrayCells = v;
    }
    public Vector2Int GetCells()
    {
        return bitarrayCells;
    }
}

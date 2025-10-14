using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#nullable enable

public class NPC_navmesh : MonoBehaviour
{
    public GoalNode? currentGoalNode = null;
    private Vector3Int? waitingAt;
    public Vector3? waitingSpot;
    public Vector3? nextWayPoint;
    public Vector3? prevWayPoint;
    public Vector3? exit = null;
    private Bude? bude = null;
    private SceneManager? sm = null;
    private NavMeshAgent? agent;
    private string walkingName = "Walking";
    private string waitingName = "Waiting";

    public Queue<Vector3> moveList = new Queue<Vector3>();
    private Queue<Bude> budenToVisit = new Queue<Bude>();

    public const float patience = 120f;
    public const float wayPointTolerance = 0.3f;

    public float speed = 2f;
    public float patienceLost;
    public float timeLeftWaiting = 0.0f;
    public int goalsBeforeExit;

    public bool randomExitGoalNumber = true;
    public bool waiting = false;
    public bool exiting = false;
    public bool onWayToBude = false;
    public bool onWayToGoalNode = false;
    public bool onWayBackFromBude = false;
    public bool lostPatience = false;
    private bool budeChanged = false;


    void Start()
    {
        sm = GameObject.Find("SceneManager").GetComponent<SceneManager>();//prob better way to do this
        (Vector3 pos, Vector3 start) = sm!.GetNewSpawnPoint();
        this.transform.position = pos;
        agent = this.GetComponent<NavMeshAgent>();
        prevWayPoint = start;
        //animator = this.GetComponent<Animator>();
        sm!.Spawned(this.transform.position);
        patienceLost = patience;
        budenToVisit = sm!.CalcNewWeightedBuden(UnityEngine.Random.Range(0, sm!.GetGoalNoteCount() + 1));
        goalsBeforeExit = budenToVisit.Count;
        if (budenToVisit.Count == 0) 
        { 
            exiting = true;
            agent.destination = sm.GetRandomExitPosition();
        }
        else
        {
            bude = budenToVisit.Dequeue();
            currentGoalNode = bude.goalNode;
            currentGoalNode!.AddOnWayToGoalNode(this);
            agent.destination = currentGoalNode.Position;
        }

    }

    private void FixedUpdate()
    {
        if(agent!.remainingDistance < wayPointTolerance)
        {
            if (exiting)
            {
                Respawn();
            }
            else if(onWayToGoalNode)
            {
                if (onWayBackFromBude)
                {
                    onWayBackFromBude = false;
                    if (budenToVisit.Count == 0)
                    {
                        exiting = true;
                        agent.destination = sm!.GetRandomExitPosition();
                        currentGoalNode!.RemoveOnWayToGoalNode(this);
                    }
                    else
                    {
                        currentGoalNode!.RemoveOnWayToGoalNode(this);
                        bude = budenToVisit.Dequeue();
                        currentGoalNode = bude.goalNode;
                        currentGoalNode!.AddOnWayToGoalNode(this);
                        agent.destination = currentGoalNode.Position;
                    }
                }
                else
                {
                    (Vector3? waitingPosition, _) = bude!.GetNewPosition();
                    if (waitingPosition == null)
                    {
                        if (budenToVisit.Count == 0)
                        {
                            exiting = true;
                            agent.destination = sm!.GetRandomExitPosition();
                            currentGoalNode!.RemoveOnWayToGoalNode(this);
                        }
                        else
                        {
                            currentGoalNode!.RemoveOnWayToGoalNode(this);
                            bude = budenToVisit.Dequeue();
                            currentGoalNode = bude.goalNode;
                            currentGoalNode!.AddOnWayToGoalNode(this);
                            agent.destination = currentGoalNode.Position;
                        }
                    }
                    else
                    {
                        agent.destination = waitingPosition.Value;
                        onWayToBude = true;
                        timeLeftWaiting = bude!.WaitTime;
                        onWayToGoalNode = false;
                        currentGoalNode!.RemoveOnWayToGoalNode(this);
                        currentGoalNode!.OnWayToWait(this);
                    }
                }
            }
            if (waiting)
            {
                if(timeLeftWaiting > 0)
                {
                    timeLeftWaiting -= Time.fixedDeltaTime;
                }
                else
                {
                    onWayToBude = false;
                    onWayBackFromBude = true;
                    onWayToGoalNode = true;
                    agent!.destination = currentGoalNode!.Position;
                    currentGoalNode!.RemoveWaitingAtGoal(this);
                    currentGoalNode!.AddOnWayToGoalNode(this);
                }

            }
        
        }


    }


    public void BudeMoved(Bude movedBude)
    {
        if (bude == movedBude)
        {
            onWayToBude = false;
            waiting = false;
            budeChanged = true;
            if (!onWayToGoalNode)
            {
                onWayBackFromBude = true;
                onWayToBude = false;
            }
        }
    }

    public void Respawn()
    {
        (Vector3 pos, Vector3 start) = sm!.GetNewSpawnPoint();
        if (pos == null) { agent!.isStopped = true; return; }
        sm!.Moved(transform.position, pos);
        this.transform.position = pos;
        randomExitGoalNumber = true;
        waiting = false;
        exiting = false;
        onWayToBude = false;
        onWayToGoalNode = false;
        onWayBackFromBude = false;

        patienceLost = patience;
        waitingSpot = null;

        exit = null;

        goalsBeforeExit = UnityEngine.Random.Range(0, sm.GetGoalNoteCount() + 1);//In future mayy goal here
        budenToVisit = sm!.CalcNewWeightedBuden(goalsBeforeExit);
        if (budenToVisit.Count == 0) { exiting = true; }
        else
        {
            bude = budenToVisit.Dequeue();
            moveList = sm.HandlePathRequest(start, bude!.goalNode);
            currentGoalNode = bude.goalNode;
            if (moveList.Count == 0) { exiting = true; }
            else
            {
                nextWayPoint = moveList.Dequeue();
                onWayToGoalNode = true;
            }
        }
        //animator!.SetBool(walkingName, true);
    }

    private void MoveTo(Vector3 towards, float timeSinceLastMove, bool showMoved)
    {
        Vector3 dir = towards - transform.position;
        Vector3 before = this.transform.position;
        if (dir != Vector3.zero)
        {
            this.transform.SetPositionAndRotation(Vector3.MoveTowards(this.transform.position, towards, speed * timeSinceLastMove), Quaternion.LookRotation(dir));
        }
        if (showMoved)
        {
            sm!.Moved(before, transform.position);
        }
    }

    public void SetInactive(Vector3 inactivePostion)
    {
        sm!.removePlayer(this);
        agent!.isStopped = true;
        this.transform.position = inactivePostion;
        //animator!.SetBool(waitingName, false);
        //animator!.SetBool(walkingName, false);
    }

    public void BudeDestroyed()
    {
        bude = null;

        if (budenToVisit.Count == 0)
        {
            exiting = true;
            currentGoalNode = null;
        }
        else
        {
            bude = budenToVisit.Dequeue();
            currentGoalNode = bude.goalNode;
        }

        waiting = false;
        if (sm!.simulating)
        {
            agent!.isStopped = false;
        }
        else
        {
            agent!.isStopped = true;
        }
        timeLeftWaiting = 0.0f;
    }

    public void Stop()
    {
        agent!.isStopped = true;
    }

    public void Resume()
    {
        agent!.isStopped = false;
    }

    public void SetMoveList(Queue<Vector3> list)
    {
        this.moveList = list;
        nextWayPoint = moveList.Dequeue();
    }


    public Vector3 GetPosition()
    {
        return this.transform.position;
    }

}

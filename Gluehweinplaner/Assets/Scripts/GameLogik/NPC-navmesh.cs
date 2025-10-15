using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#nullable enable

public class NPC_navmesh : MonoBehaviour
{
    public GoalNode? currentGoalNode = null;
    private Vector3Int? waitingAt;
    public Vector3? waitingSpot;
    private Bude? bude = null;
    private SceneManager? sm = null;
    private NavMeshAgent? agent;
    public Vector3 destination = new Vector3(-1,-1,-1);

    private Queue<Bude> budenToVisit = new Queue<Bude>();

    public const float patience = 120f;
    public const float wayPointTolerance = 1f;

    public float speed = 2f;
    public float patienceLost = 0;
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
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        //animator = this.GetComponent<Animator>();
        sm!.Spawned(this.transform.position);
        budenToVisit = sm!.CalcNewWeightedBuden(UnityEngine.Random.Range(0, sm!.GetGoalNoteCount() + 1));
        goalsBeforeExit = budenToVisit.Count;
        patienceLost = patience;
        if (budenToVisit.Count == 0) 
        { 
            exiting = true;
            destination = sm.GetRandomExitPosition();
        }
        else
        {
            bude = budenToVisit.Dequeue();
            currentGoalNode = bude.goalNode;
            currentGoalNode!.AddOnWayToGoalNode(this);
            destination = currentGoalNode!.Position;
            onWayToGoalNode = true;
        }
        agent.destination = destination;
    }

    private void FixedUpdate()
    {
        if (!waiting)
        {
            if (Vector3.Distance(transform.position,destination) < wayPointTolerance)
            {
                if (exiting)
                {
                    Respawn();
                }
                else if (onWayToBude)
                {
                    timeLeftWaiting = bude!.WaitTime;
                    waiting = true;
                    onWayToBude = false;
                }
                else if(onWayToGoalNode)
                {
                    (waitingSpot, waitingAt) = bude!.GetNewPosition();
                    if (waitingSpot == null || budeChanged)
                    {
                        if (budenToVisit.Count > 0)
                        {
                            currentGoalNode!.RemoveOnWayToGoalNode(this);
                            bude = budenToVisit.Dequeue();
                            currentGoalNode = bude!.goalNode;
                            patienceLost = patience;
                            currentGoalNode!.AddOnWayToGoalNode(this);
                            destination = currentGoalNode!.Position;
                        }
                        else
                        {
                            exiting = true;
                            onWayToGoalNode = false;
                            destination = sm!.GetRandomExitPosition();
                        }
                    }
                    else
                    {
                        currentGoalNode!.OnWayToWait(this);
                        onWayToGoalNode = false;
                        onWayToBude = true;
                        destination = waitingSpot!.Value;
                    }
                    agent!.destination = destination;
                }
                else if(onWayBackFromBude)
                {
                    if (budenToVisit.Count == 0)
                    {
                        exiting = true;
                        destination = sm!.GetRandomExitPosition();
                    }
                    else
                    {
                        currentGoalNode!.RemoveOnWayToGoalNode(this);
                        bude = budenToVisit.Dequeue();
                        currentGoalNode = bude!.goalNode;
                        patienceLost = patience;
                        currentGoalNode!.AddOnWayToGoalNode(this);
                        onWayToGoalNode = true;
                        destination = currentGoalNode!.Position;
                    }
                    agent!.destination = destination;
                    onWayBackFromBude = false;
                }
            }
        }
        else
        {//waiting
            if (timeLeftWaiting > 0)
            {
                timeLeftWaiting -= Time.fixedDeltaTime;
            }
            else
            {
                bude!.RemovePlayer(waitingAt!.Value);
                waitingAt = null;
                currentGoalNode!.RemoveWaitingAtGoal(this);
                onWayBackFromBude = true;
                onWayToGoalNode = false;
                waiting = false;
                waitingSpot = null;
                destination = currentGoalNode!.Position;
                agent!.destination = destination;
            }
        }

        if (!waiting && !onWayToBude)
        {
            if (patienceLost > 0)
            {
                patienceLost -= Time.deltaTime;
            }
            else
            {
                sm!.LostPatience();
                if (budenToVisit.Count == 0)
                {
                    exiting = true;
                    destination = sm!.GetRandomExitPosition();
                }
                else
                {
                    bude = budenToVisit.Dequeue();
                    currentGoalNode!.RemoveSafe(this);
                    currentGoalNode = bude.goalNode;
                    currentGoalNode!.AddOnWayToGoalNode(this);
                    onWayToGoalNode = true;
                    onWayBackFromBude = true;
                    patienceLost = patience;
                    destination = currentGoalNode!.Position;
                }
                agent!.destination = destination;
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
        agent!.Warp(pos);
        randomExitGoalNumber = true;
        
        waiting = false;
        exiting = false;
        onWayToBude = false;
        onWayToGoalNode = false;
        onWayBackFromBude = false;

        
        patienceLost = patience;
        waitingSpot = null;

        goalsBeforeExit = UnityEngine.Random.Range(0, sm.GetGoalNoteCount() + 1);//In future mayy goal here
        budenToVisit = sm!.CalcNewWeightedBuden(goalsBeforeExit);
        if (budenToVisit.Count == 0)
        {
            exiting = true;
            destination = sm.GetRandomExitPosition();
        }
        else
        {
            bude = budenToVisit.Dequeue();
            currentGoalNode = bude.goalNode;
            currentGoalNode!.AddOnWayToGoalNode(this);
            destination = currentGoalNode!.Position;
            onWayToGoalNode = true;
        }
        agent.destination = destination;
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

    public Vector3 GetPosition()
    {
        return this.transform.position;
    }

}

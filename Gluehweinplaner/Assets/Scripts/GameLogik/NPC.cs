using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#nullable enable

public class NPC : MonoBehaviour
{
    public GoalNode? currentGoalNode = null;
    private Vector3Int? waitingAt;
    public Vector3? waitingSpot;
    private Vector3? currentGoal;
    public Vector3? exit = null;
    private Bude? bude = null;
    private SceneManager? sm = null;
    private Animator? animator;
    private string walkingName = "Walking";
    private string waitingName = "Waiting";

    private Queue<Bude> budenToVisit = new Queue<Bude>();
    private NavMeshAgent? agent;
    
    public const float patience = 120f;
    public const float waitingTolerance = 0.2f;
    public const float exitTolerance = 0.1f;
    public const float wayPointTolerance = 1f;
    
    public float speed = 2f;
    public float patienceLost;
    public float timeLeftWaiting = 0.0f;
    public int goalsBeforeExit;
    public float checkForChangeIntervall = 1f;

    public bool randomExitGoalNumber = true;
    public bool stopped = false;
    public bool waiting = false;
    public bool exiting = false;
    public bool onWayToBude = false;
    public bool onWayToGoalNode = false;
    public bool onWayToExit = false;
    public bool onWayBackFromBude = false;
    public bool lostPatience = false;
    private bool budeChanged = false;


    void Start()
    {
        sm = GameObject.Find("SceneManager").GetComponent<SceneManager>();//prob better way to do this
        agent = GetComponent<NavMeshAgent>();
        (Vector3 pos, Vector3 start) = sm!.GetNewSpawnPoint();   
        this.transform.position = pos;
        animator = this.GetComponent<Animator>();
        sm!.Spawned(this.transform.position);
        patienceLost = patience;
        budenToVisit = sm!.CalcNewWeightedBuden(UnityEngine.Random.Range(0, sm!.GetGoalNoteCount() + 1));
        goalsBeforeExit = budenToVisit.Count;
        if (budenToVisit.Count == 0) { 
            exiting = true;
            currentGoal = sm!.GetRandomExitPosition();
        } 
        else
        {
            bude = budenToVisit.Dequeue();
            currentGoalNode = bude.goalNode;
            currentGoal = currentGoalNode.Position;
            currentGoalNode!.AddOnWayToGoalNode(this);
            onWayToGoalNode = true;
        }
        agent!.SetDestination(currentGoal!.Value);
        animator.SetBool(walkingName,true);
    }

    private void FixedUpdate()
    {
        if (!stopped)
        {
            if (!waiting)
            {
                if (Vector3.Distance(transform.position, (currentGoal!.Value+new Vector3(0f,0.7f,0f))) < wayPointTolerance)
                {
                    if (onWayToGoalNode)
                    {
                        (waitingSpot, waitingAt) = bude!.GetNewPosition();
                        onWayToGoalNode = false;
                        if (waitingSpot == null || budeChanged)
                        {
                            if (budenToVisit.Count == 0)
                            {
                                exiting = true;
                                currentGoal = sm!.GetRandomExitPosition();
                            }
                            else
                            {
                                bude = budenToVisit.Dequeue();
                                currentGoalNode = bude.goalNode;
                                currentGoal = currentGoalNode.Position;
                                currentGoalNode!.AddOnWayToGoalNode(this);
                                onWayToGoalNode = true;
                            }
                        }
                        else
                        {
                            currentGoalNode!.OnWayToWait(this);
                            currentGoal = waitingSpot;
                            onWayToBude = true;
                        }
                        agent!.SetDestination(currentGoal!.Value);
                    }
                    else if (onWayToBude)
                    {
                        waiting = true;
                        agent!.isStopped = true;
                        onWayToBude = false;
                        timeLeftWaiting = bude!.WaitTime;
                        animator!.SetBool(waitingName, true);
                    }
                    else if (exiting)
                    {
                        Respawn();
                    }
                    else if (onWayBackFromBude)
                    {
                        onWayBackFromBude = false;
                        if (budenToVisit.Count == 0)
                        {
                            exiting = true;
                            currentGoal = sm!.GetRandomExitPosition();
                        }
                        else
                        {
                            bude = budenToVisit.Dequeue();
                            currentGoalNode!.RemoveOnWayToGoalNode(this);
                            currentGoalNode = bude.goalNode;
                            currentGoal = currentGoalNode.Position;
                            currentGoalNode!.AddOnWayToGoalNode(this);
                            onWayToGoalNode = true;
                        }
                        agent!.SetDestination(currentGoal!.Value);
                        patienceLost = patience;
                    }
                }
            }
            else
            {
                if (timeLeftWaiting > 0)
                {
                    timeLeftWaiting -= Time.fixedDeltaTime;
                }
                else
                {
                    bude!.RemovePlayer(waitingAt!.Value);
                    waitingAt = null;
                    currentGoalNode!.RemoveWaitingAtGoal(this);
                    onWayToGoalNode = false;
                    waiting = false;
                    waitingSpot = null;
                    onWayBackFromBude = true;
                    currentGoal = currentGoalNode.Position;
                    agent!.SetDestination(currentGoal!.Value);
                    agent!.isStopped = false;
                    animator!.SetBool(waitingName, false);
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
                    }
                    else
                    {
                        waitingAt = null;
                        currentGoalNode!.RemoveWaitingAtGoal(this);
                        onWayToGoalNode = false;
                        waiting = false;
                        waitingSpot = null;
                        onWayBackFromBude = true;
                        agent!.SetDestination(currentGoal!.Value);
                        animator!.SetBool(waitingName, false);
                    }
                    patienceLost = patience;
                }
            }
        }

    }


    public void BudeMoved(Bude movedBude)
    {
        if(bude == movedBude)
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
        (Vector3 pos,Vector3 start) = sm!.GetNewSpawnPoint();
        if(pos == null) { stopped = true; return;}
        sm!.Moved(transform.position, pos);
        this.transform.position = pos;
        randomExitGoalNumber = true;
        stopped = false;
        waiting = false;
        exiting = false;
        onWayToBude = false;
        onWayToGoalNode = false;
        onWayBackFromBude = false;
        onWayToExit = false;

        patienceLost = patience;
        waitingSpot = null;

        exit = null;

        goalsBeforeExit = UnityEngine.Random.Range(0, sm.GetGoalNoteCount() + 1);//In future mayy goal here
        budenToVisit = sm!.CalcNewWeightedBuden(goalsBeforeExit);
        if (budenToVisit.Count == 0)
        {
            exiting = true;
            currentGoal = sm!.GetRandomExitPosition();
        }
        else
        {
            bude = budenToVisit.Dequeue();
            currentGoalNode = bude.goalNode;
            currentGoal = currentGoalNode.Position;
            currentGoalNode!.AddOnWayToGoalNode(this);
            onWayToGoalNode = true;
        }
        agent!.SetDestination(currentGoal!.Value);

        animator!.SetBool(walkingName, true);
    }

    public void SetInactive(Vector3 inactivePostion)
    {
        sm!.removePlayer(this);
        stopped = true;
        this.transform.position = inactivePostion;
        animator!.SetBool(waitingName, false);
        animator!.SetBool(walkingName, false);
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
            stopped = false;
        }
        else
        {
            stopped = true;
        }
        timeLeftWaiting = 0.0f;
    }

    public void Stop()
    {
        stopped = true;
    }

    public void Resume()
    {
        stopped = false;
    }

    public Vector3 GetPosition()
    {
        return this.transform.position;
    }

}

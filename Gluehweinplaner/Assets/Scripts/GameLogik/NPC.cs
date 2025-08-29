using System.Collections.Generic;
using UnityEngine;
#nullable enable

public class NPC : MonoBehaviour
{
    public GoalNode? currentGoalNode = null;
    private Vector3Int? waitingAt;
    public Vector3? waitingSpot;
    public Vector3? nextWayPoint;
    public Vector3? prevWayPoint;
    public Vector3? exit = null;
    private Bude? bude = null;
    private SceneManager? sm = null;
    private Animator? animator;
    private string walkingName = "Walking";
    private string waitingName = "Waiting";

    public Queue<Vector3> moveList = new Queue<Vector3>();
    private Queue<Bude> budenToVisit = new Queue<Bude>();

    public const float patience = 120f;
    public const float waitingTolerance = 0.2f;
    public const float exitTolerance = 0.1f;
    public const float wayPointTolerance = 0.3f;

    public float speed = 2f;
    public float patienceLost;
    public float timeLeftWaiting = 0.0f;
    public int goalsBeforeExit;
    public float checkForChangeIntervall = 1f;

    public bool randomExitGoalNumber = true;
    public bool inactive = false;
    public bool stopped = false;
    public bool waiting = false;
    public bool exiting = false;
    public bool onWayToBude = false;
    public bool onWayToStart = true;
    public bool onWayToGoalNode = false;
    public bool onWayToExit = false;
    public bool onWayBackFromBude = false;
    public bool lostPatience = false;
    private bool budeChanged = false;


    void Start()
    {
        sm = GameObject.Find("SceneManager").GetComponent<SceneManager>();//prob better way to do this
        (Vector3 pos, Vector3 start) = sm!.GetNewSpawnPoint();
        this.transform.position = pos;
        prevWayPoint = start;
        animator = this.GetComponent<Animator>();
        sm!.Spawned(this.transform.position);
        patienceLost = patience;
        budenToVisit = sm!.CalcNewWeightedBuden(UnityEngine.Random.Range(0, sm!.GetGoalNoteCount() + 1));
        goalsBeforeExit = budenToVisit.Count;
        if (budenToVisit.Count == 0) { exiting = true; }
        else
        {
            bude = budenToVisit.Dequeue();
            moveList = sm.HandlePathRequest(start, bude!.goalNode!);
            currentGoalNode = bude.goalNode;
            currentGoalNode!.AddOnWayToGoalNode(this);
            if (moveList.Count == 0) { exiting = true; }
            else
            {
                nextWayPoint = moveList.Dequeue();
                onWayToGoalNode = true;
            }
        }
        animator.SetBool(walkingName, true);
    }

    private void FixedUpdate()
    {
        if (!stopped)
        {
            if (!exiting)
            {
                if (!waiting)
                {
                    if (!onWayToBude)
                    {
                        if (onWayToGoalNode || onWayToExit)
                        {
                            if (nextWayPoint != null && Vector3.Distance(transform.position, nextWayPoint!.Value) > wayPointTolerance)
                            {
                                if (onWayToStart || moveList.Count == 0)
                                {
                                    MoveTo(nextWayPoint.Value, Time.deltaTime, true);
                                }
                                else
                                {
                                    MoveTo(nextWayPoint.Value, Time.deltaTime, false);
                                }
                            }
                            else
                            {
                                if (moveList.Count > 0)
                                {
                                    if (onWayToStart)
                                    {
                                        onWayToStart = false;
                                        prevWayPoint = transform.position;
                                    }
                                    sm!.Moved(prevWayPoint!.Value, nextWayPoint!.Value);
                                    prevWayPoint = nextWayPoint;
                                    nextWayPoint = moveList.Dequeue();
                                    MoveTo(nextWayPoint.Value, Time.deltaTime, false);
                                }
                                else
                                {
                                    if (onWayToExit)
                                    {
                                        exiting = true;
                                    }
                                    else
                                    {
                                        onWayToBude = true;

                                    }
                                    onWayToGoalNode = false;
                                }
                            }
                        }
                        else if (onWayBackFromBude)
                        {
                            float dist = Vector3.Distance(currentGoalNode!.Position, this.transform.position);
                            if (dist > wayPointTolerance)
                            {
                                MoveTo(currentGoalNode!.Position, Time.deltaTime, true);
                            }
                            else
                            {
                                if (budenToVisit.Count == 0)
                                {
                                    exiting = true;
                                }
                                else
                                {
                                    currentGoalNode.RemoveOnWayToGoalNode(this);
                                    bude = budenToVisit.Dequeue();
                                    currentGoalNode = bude!.goalNode;
                                    patienceLost = patience;
                                    moveList = sm!.HandlePathRequest(this.transform.position, currentGoalNode!);
                                    currentGoalNode!.AddOnWayToGoalNode(this);
                                    nextWayPoint = moveList.Dequeue();
                                    onWayToGoalNode = true;
                                }
                                onWayBackFromBude = false;
                            }
                        }
                    }
                    else
                    {//on way to bude
                        if (waitingSpot == null)
                        {
                            if (bude == null)
                            {
                                exiting = true;
                            }
                            else
                            {
                                (waitingSpot, waitingAt) = bude!.GetNewPosition();
                                if (waitingSpot == null || budeChanged)
                                {
                                    if (budenToVisit.Count > 0)
                                    {
                                        onWayToBude = false;
                                        currentGoalNode!.RemoveOnWayToGoalNode(this);
                                        onWayBackFromBude = true;
                                    }
                                    else
                                    {
                                        exiting = true;
                                    }
                                }
                                else
                                {
                                    currentGoalNode!.OnWayToWait(this);
                                }
                            }
                        }
                        else
                        {
                            if (Vector3.Distance(transform.position, waitingSpot!.Value) > waitingTolerance)
                            {
                                MoveTo(waitingSpot!.Value, Time.fixedDeltaTime, true);
                            }
                            else
                            {
                                timeLeftWaiting = bude!.WaitTime;
                                waiting = true;
                                animator!.SetBool(waitingName, true);
                                onWayToBude = false;
                            }
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
                        animator!.SetBool(waitingName, false);
                    }
                }

            }
            else
            {//exiting
                if (!onWayToExit)
                {
                    if (exit == null)
                    {
                        exit = sm!.GetRandomExitPosition();
                        moveList = sm.HandlePathRequest(this.transform.position, exit!.Value);
                        if (moveList.Count == 0) { stopped = true; return; }
                        nextWayPoint = moveList!.Dequeue();
                        prevWayPoint = nextWayPoint;
                        onWayToGoalNode = true;
                        onWayToExit = true;
                        exiting = false;
                        onWayToBude = false;
                    }
                }
                else
                {
                    exit = null;
                    Respawn();
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
                        bude = budenToVisit.Dequeue();
                        currentGoalNode!.RemoveSafe(this);
                        currentGoalNode = bude.goalNode;
                        currentGoalNode!.AddOnWayToGoalNode(this);
                        onWayToGoalNode = true;
                        onWayBackFromBude = true;
                        nextWayPoint = null;
                        patienceLost = patience;
                    }
                }
            }
        }//stopped
        else
        {
            animator!.SetBool(walkingName, false);
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
        if (pos == null) { stopped = true; return; }
        sm!.Moved(transform.position, pos);
        this.transform.position = pos;
        randomExitGoalNumber = true;
        inactive = false;
        stopped = false;
        waiting = false;
        exiting = false;
        onWayToBude = false;
        onWayToGoalNode = false;
        onWayBackFromBude = false;
        onWayToExit = false;
        onWayToStart = true;

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
        animator!.SetBool(walkingName, true);
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

//private void FixedUpdate()
//{
//    if (!stopped)
//    {
//        if (!waiting)
//        {
//            if (Vector3.Distance(transform.position, (currentGoal!.Value+new Vector3(0f,0.7f,0f))) < wayPointTolerance)
//            {
//                if (onWayToGoalNode)
//                {
//                    (waitingSpot, waitingAt) = bude!.GetNewPosition();
//                    onWayToGoalNode = false;
//                    if (waitingSpot == null || budeChanged)
//                    {
//                        if (budenToVisit.Count == 0)
//                        {
//                            exiting = true;
//                            currentGoal = sm!.GetRandomExitPosition();
//                        }
//                        else
//                        {
//                            bude = budenToVisit.Dequeue();
//                            currentGoalNode = bude.goalNode;
//                            currentGoal = currentGoalNode.Position;
//                            currentGoalNode!.AddOnWayToGoalNode(this);
//                            onWayToGoalNode = true;
//                        }
//                    }
//                    else
//                    {
//                        currentGoalNode!.OnWayToWait(this);
//                        currentGoal = waitingSpot;
//                        onWayToBude = true;
//                    }
//                    agent!.SetDestination(currentGoal!.Value);
//                }
//                else if (onWayToBude)
//                {
//                    waiting = true;
//                    agent!.isStopped = true;
//                    onWayToBude = false;
//                    timeLeftWaiting = bude!.WaitTime;
//                    animator!.SetBool(waitingName, true);
//                }
//                else if (exiting)
//                {
//                    Respawn();
//                }
//                else if (onWayBackFromBude)
//                {
//                    onWayBackFromBude = false;
//                    if (budenToVisit.Count == 0)
//                    {
//                        exiting = true;
//                        currentGoal = sm!.GetRandomExitPosition();
//                    }
//                    else
//                    {
//                        bude = budenToVisit.Dequeue();
//                        currentGoalNode!.RemoveOnWayToGoalNode(this);
//                        currentGoalNode = bude.goalNode;
//                        currentGoal = currentGoalNode.Position;
//                        currentGoalNode!.AddOnWayToGoalNode(this);
//                        onWayToGoalNode = true;
//                    }
//                    agent!.SetDestination(currentGoal!.Value);
//                    patienceLost = patience;
//                }
//            }
//        }
//        else
//        {
//            if (timeLeftWaiting > 0)
//            {
//                timeLeftWaiting -= Time.fixedDeltaTime;
//            }
//            else
//            {
//                bude!.RemovePlayer(waitingAt!.Value);
//                waitingAt = null;
//                currentGoalNode!.RemoveWaitingAtGoal(this);
//                onWayToGoalNode = false;
//                waiting = false;
//                waitingSpot = null;
//                onWayBackFromBude = true;
//                currentGoal = currentGoalNode.Position;
//                agent!.SetDestination(currentGoal!.Value);
//                agent!.isStopped = false;
//                animator!.SetBool(waitingName, false);
//            }
//        }
//        if (!waiting && !onWayToBude)
//        {
//            if (patienceLost > 0)
//            {
//                patienceLost -= Time.deltaTime;
//            }
//            else
//            {
//                sm!.LostPatience();
//                if (budenToVisit.Count == 0)
//                {
//                    exiting = true;
//                }
//                else
//                {
//                    waitingAt = null;
//                    currentGoalNode!.RemoveWaitingAtGoal(this);
//                    onWayToGoalNode = false;
//                    waiting = false;
//                    waitingSpot = null;
//                    onWayBackFromBude = true;
//                    agent!.SetDestination(currentGoal!.Value);
//                    animator!.SetBool(waitingName, false);
//                }
//                patienceLost = patience;
//            }
//        }
//    }

//}
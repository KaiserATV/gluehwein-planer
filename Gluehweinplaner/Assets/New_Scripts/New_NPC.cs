using System.Collections.Generic;
using UnityEngine;
#nullable enable

public class New_NPC : MonoBehaviour
{
    public New_GoalNode? currentGoalNode;
    public New_GoalNode? prevGoalNode;
    public Vector3 waitingSpot;
    public Vector3 nextWayPoint;
    public Vector3 exit;
    private Vector2Int bitarrayCells;
    private New_Bude? bude;
    private New_SceneManager? sm;

    public Queue<Vector3> moveList = new Queue<Vector3>();
    private Queue<New_Bude> budenToVisit = new Queue<New_Bude>();
    
    
    public const float patience = 120f;
    public const float waitingTolerance = 0.2f;
    public const float goalDistanceTolerance = 1f;
    
    public float speed = 0.01f;
    public float patienceLost;
    public float timeLeftWaiting = 0.0f;
    public int goalsBeforeExit;
    
    public bool randomExitGoalNumber = true;
    public bool inactive = false;
    public bool stopped = false;
    public bool waiting = false;
    public bool exiting = false;
    public bool onWayToBude = false;
    public bool onWayToPrevGoalNode = false;


    
    void Start()
    {
        sm = GameObject.Find("SceneManager").GetComponent<New_SceneManager>();//prob better way to do this
        goalsBeforeExit = UnityEngine.Random.Range(0, sm.GetGoalNoteCount()+1);//In future mayy goal here
        if (goalsBeforeExit>0)
        {
            budenToVisit = sm.GetNewBuden(goalsBeforeExit);
            bude = budenToVisit.Dequeue();
            currentGoalNode = bude.goalNode;
            patienceLost = patience;
            prevGoalNode = null;
            moveList = sm.HandlePathRequest(this.transform.position, currentGoalNode);
            nextWayPoint = moveList.Dequeue();
            exit = sm.GetRandomExitPosition();
        }
        else
        {
            exit = sm.GetRandomExitPosition();
            exiting = true;
            patienceLost = patience;
            prevGoalNode = null;
            currentGoalNode = null;
            moveList = sm!.HandlePathRequest(GetPosition(), exit);
            nextWayPoint = moveList.Dequeue();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!stopped)
        {
            if (!waiting)
            {
                if (moveList.Count != 0)
                {
                    if (Vector3.Distance(this.transform.position, nextWayPoint) > goalDistanceTolerance)
                    {
                        this.transform.position = Vector3.MoveTowards(this.transform.position, nextWayPoint, speed);
                        patienceLost -= Time.deltaTime;
                        if (patienceLost <= 0)
                        {
                            sm!.LostPatience();
                            if (budenToVisit.Count == 0)
                            {
                                onWayToBude = false;
                                exiting = true;
                                currentGoalNode = null;
                                moveList = sm!.HandlePathRequest(GetPosition(), exit);
                                nextWayPoint = moveList.Dequeue();
                                this.transform.position = Vector3.MoveTowards(this.transform.position, nextWayPoint, speed);
                            }
                            else
                            {
                                prevGoalNode = currentGoalNode;
                                bude = budenToVisit.Dequeue();
                                currentGoalNode = bude.goalNode;
                            }
                            patienceLost = patience;
                        }
                    }
                    else
                    {
                        nextWayPoint = moveList.Dequeue();
                    }
                }
                else
                {
                    if (onWayToPrevGoalNode) { 
                        if(Vector3.Distance(this.transform.position, waitingSpot) > goalDistanceTolerance)
                        {
                            this.transform.position = Vector3.MoveTowards(this.transform.position, prevGoalNode!.Position, speed);
                        }
                        else
                        {
                            onWayToPrevGoalNode = false;
                            if (budenToVisit.Count == 0)
                            {
                                exiting = true;
                                currentGoalNode = null;
                                moveList = sm!.HandlePathRequest(GetPosition(), exit);
                                nextWayPoint = moveList.Dequeue();
                                this.transform.position = Vector3.MoveTowards(this.transform.position, nextWayPoint, speed);
                            }
                            else
                            {
                                prevGoalNode = currentGoalNode;
                                bude = budenToVisit.Dequeue();
                                currentGoalNode = bude.goalNode;
                            }
                        }
                    }
                    else if (!exiting && !onWayToBude)
                    {
                        Vector3? holder = bude!.GetNewPosition(this);
                        if(holder != null)
                        {
                            waitingSpot = holder!.Value;
                            onWayToBude = true;
                            timeLeftWaiting = bude.WaitTime;
                            this.transform.position = Vector3.MoveTowards(this.transform.position, waitingSpot, speed);
                        }
                        else
                        {
                            if (budenToVisit.Count == 0)
                            {
                                onWayToBude = false;
                                exiting = true;
                                currentGoalNode = null;
                                moveList = sm!.HandlePathRequest(GetPosition(), exit);
                                nextWayPoint = moveList.Dequeue();
                                this.transform.position = Vector3.MoveTowards(this.transform.position, nextWayPoint, speed);
                            }
                            else
                            {
                                prevGoalNode = currentGoalNode;
                                bude = budenToVisit.Dequeue();
                                currentGoalNode = bude.goalNode;
                            }
                        }
                    }
                    else if (onWayToBude)
                    {
                        if (Vector3.Distance(this.transform.position, waitingSpot) > waitingTolerance)
                        {
                            this.transform.position = Vector3.MoveTowards(this.transform.position, waitingSpot, speed);
                        }
                        else
                        {
                            waiting = true;
                        }
                    }
                    else if (exiting)
                    {
                        if (Vector3.Distance(this.transform.position, exit) > goalDistanceTolerance)
                        {
                            this.transform.position = Vector3.MoveTowards(this.transform.position, exit, speed);
                        }
                        else
                        {
                            sm!.removePlayer(this);
                            Respawn();
                        }
                    }
                }
            }
            else
            {
                if (timeLeftWaiting > 0)
                {
                    timeLeftWaiting -= Time.deltaTime;
                }
                else
                {
                    bude!.RemovePlayer(this);
                    onWayToPrevGoalNode = true;
                    waiting = false;
                    onWayToBude = false;
                }
            }
        }
    }

    public void InvalidatePosition(Vector3 newCoords)
    {
        if (waiting) { waiting = false; }
        if (sm!.simulating) { stopped = false; }
    }

    public void Respawn()
    {
        this.transform.position = sm!.GetNewSpawnPoint();

        randomExitGoalNumber = true;
        inactive = false;
        stopped = false;
        waiting = false;
        exiting = false;
        onWayToBude = false;
        onWayToPrevGoalNode = false;

        timeLeftWaiting = 0.0f;

        sm.addPlayer(this); 
        
        if (goalsBeforeExit > 0)
        {
            budenToVisit = sm.GetNewBuden(goalsBeforeExit);
            bude = budenToVisit.Dequeue();
            currentGoalNode = bude.goalNode;
            patienceLost = patience;
            prevGoalNode = null;
            moveList = sm.HandlePathRequest(this.transform.position, currentGoalNode);
            nextWayPoint = moveList.Dequeue();
            exit = sm.GetRandomExitPosition();
        }
        else
        {
            exit = sm.GetRandomExitPosition();
            exiting = true;
            patienceLost = patience;
            prevGoalNode = null;
            currentGoalNode = null;
            moveList = sm!.HandlePathRequest(GetPosition(), exit);
            nextWayPoint = moveList.Dequeue();
        }
    }

    public void SetInactive(Vector3 inactivePostion)
    {
        stopped = true;
        this.transform.position = inactivePostion;
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

    public void SetCells(Vector2Int v)
    {
        bitarrayCells = v;
    }
    public Vector2Int GetCells()
    {
        return bitarrayCells;
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

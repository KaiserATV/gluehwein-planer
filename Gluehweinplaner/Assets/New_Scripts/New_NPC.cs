using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
#nullable enable

public class New_NPC : MonoBehaviour
{
    //public bool showBudeDebug = true;
    //public bool showTilesDebug = true;
    public int shownTiles = -1;

    public New_GoalNode? currentGoalNode = null;
    public Vector3? waitingSpot;
    public Vector3? nextWayPoint;
    public Vector3? exit = null;
    private Vector2Int bitarrayCells;
    private New_Bude? bude = null;
    private New_SceneManager? sm = null;

    public Queue<Vector3> moveList = new Queue<Vector3>();
    private Queue<New_Bude> budenToVisit = new Queue<New_Bude>();
    
    public const float patience = 120f;
    public const float waitingTolerance = 0.2f;
    public const float exitTolerance = 0.1f;
    public const float wayPointTolerance = 0.3f;
    
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
    public bool onWayToGoalNode = false;
    public bool onWayToExit = false;
    public bool onWayBackFromBude = false;
    public bool lostPatience = false;



    void Start()
    {
        sm = GameObject.Find("SceneManager").GetComponent<New_SceneManager>();//prob better way to do this
        Vector3? pos = sm!.GetNewSpawnPoint();
        this.transform.position = pos!.Value;

        patienceLost = patience;

        goalsBeforeExit = UnityEngine.Random.Range(0, sm.GetGoalNoteCount() + 1);//In future mayy goal here
        //goalsBeforeExit = 1;
        budenToVisit = sm!.GetNewBuden(goalsBeforeExit);
        if (budenToVisit.Count == 0) { exiting = true; } else
        {
            bude = budenToVisit.Dequeue();
            moveList = sm.HandlePathRequest(this.transform.position, bude!.goalNode);
            currentGoalNode = bude.goalNode;
            if (moveList.Count == 0) { exiting = true; } else
            {
                nextWayPoint = moveList.Dequeue();
                onWayToGoalNode = true;
            }
        }
            
    }

    // Update is called once per frame
    void Update()
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
                            if (Vector3.Distance(transform.position,nextWayPoint!.Value) > wayPointTolerance)
                            {
                                MoveTo(nextWayPoint.Value, Time.deltaTime);
                            }
                            else
                            {
                                if (moveList.Count > 0)
                                {
                                    nextWayPoint = moveList.Dequeue();
                                    MoveTo(nextWayPoint.Value, Time.deltaTime);
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
                        }else if (onWayBackFromBude)
                        {
                            if (Vector3.Distance(currentGoalNode!.Position, this.transform.position) > wayPointTolerance)
                            {
                                MoveTo(currentGoalNode!.Position, Time.deltaTime);
                            }
                            else
                            {
                                if (budenToVisit.Count == 0)
                                {
                                    exiting = true;
                                    onWayToGoalNode = false;
                                }
                                else
                                {
                                    bude = budenToVisit.Dequeue();
                                    currentGoalNode = bude!.goalNode;
                                    moveList = sm!.HandlePathRequest(this.transform.position, currentGoalNode);
                                    nextWayPoint = moveList.Dequeue();
                                    MoveTo(currentGoalNode!.Position, Time.deltaTime);

                                }
                                bude!.RemovePlayer(this);
                                onWayBackFromBude = false;
                            }
                        }
                      
                    }
                    else
                    {//on way to bude
                        if (waitingSpot == null)
                        {
                            if(bude == null)
                            {
                                exiting = true;
                            }
                            else
                            {
                                waitingSpot = bude!.GetNewPosition(this);
                            }
                        }
                        else
                        {
                            if (Vector3.Distance(transform.position, waitingSpot!.Value) > waitingTolerance)
                            {
                                MoveTo(waitingSpot!.Value, Time.deltaTime);
                            }
                            else
                            {
                                timeLeftWaiting = bude!.WaitTime;
                                waiting = true;
                                onWayToBude = false;
                            }
                        }
                    }

                }
                else
                {//waiting
                    if (timeLeftWaiting > 0)
                    {
                        timeLeftWaiting -= Time.deltaTime;
                    }
                    else
                    {
                        onWayBackFromBude = true;
                        onWayToGoalNode = false;
                        waiting = false;
                    }
                }

            }
            else
                {//exiting
                    if (!onWayToExit)
                    {
                        if(exit == null)
                        {
                            exit = sm!.GetRandomExitPosition();
                            moveList = sm.HandlePathRequest(this.transform.position, exit!.Value);
                            nextWayPoint = moveList.Dequeue();
                            onWayToGoalNode = true;
                            onWayToExit = true;
                            exiting = false;
                        }
                    }
                    else
                    {
                        Respawn();
                    }
                }
        }//stopped
        if (!waiting)
        {
            if (patienceLost > 0)
            {
                patienceLost -= Time.deltaTime;
            }
            else
            {
                if (budenToVisit.Count == 0)
                {
                    exiting = true;
                }
                else
                {
                    bude = budenToVisit.Dequeue();
                    onWayToGoalNode = true;
                    onWayBackFromBude = true;
                    nextWayPoint = null;
                    patienceLost = patience;
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
        Vector3? pos = sm!.GetNewSpawnPoint();
        if(pos == null) { stopped = true; return;}
        this.transform.position = pos!.Value;


        randomExitGoalNumber = true;
        inactive = false;
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
        budenToVisit = sm!.GetNewBuden(goalsBeforeExit);
        if (budenToVisit.Count == 0) { exiting = true; }
        else
        {
            bude = budenToVisit.Dequeue();
            moveList = sm.HandlePathRequest(this.transform.position, bude!.goalNode);
            currentGoalNode = bude.goalNode;
            if (moveList.Count == 0) { exiting = true; }
            else
            {
                nextWayPoint = moveList.Dequeue();
                onWayToGoalNode = true;
            }
        }
    }

    private void MoveTo(Vector3 towards, float timeSinceLastMove)
    {
        this.transform.position = Vector3.MoveTowards(this.transform.position, towards, speed * timeSinceLastMove);
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

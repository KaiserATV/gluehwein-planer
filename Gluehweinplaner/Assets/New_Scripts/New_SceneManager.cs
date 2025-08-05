using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;


//ToDo:
// 1. Generate Distance Field for every exit


public class New_SceneManager : MonoBehaviour
{
    public bool pathDiagonal = false;

    public int maxPlayerCount = 50;
    public bool simulating = false;


    New_Plate[,] allPlateArray;
    List<New_GoalNode> allGoalNodes = new List<New_GoalNode>();
    private List<New_NPC> alleCurrentAgents = new List<New_NPC>();
    List<Bounds> allFloorBounds = new List<Bounds>();
    private LinkedList<int>leereStellen=new LinkedList<int>();
    New_Bude[] allBudenScripts;
    New_Spawner[] allSpawner;
    New_Exit[] allExits;
    New_InactiveAgentsContainer iac;
    public MeshRenderer[] floors;


    public static float maxGoalNodeDistance = 30.0f;//best case it is calculated if there are blocking pionts between the goals
    public float normalPlateX = 0;
    public float normalPlateZ = 0;
    public float randPlateX = 0;
    public float randPlateZ = 0;
    public int plateCountX = 0;
    public int plateCountZ = 0;

    public int playerCount = 0;
    public int inactivePlayerCount = 0;
    public int agentsLostPatience = 0;
    public int maxKapazitaet;
    public int allBudenWeigth;


    private Dictionary<(Vector3, Vector3), (List<New_Plate>, Queue<Vector3>)> allPositionsToGoals = new Dictionary<(Vector3, Vector3), (List<New_Plate>, Queue<Vector3>)>();

    void Start()
    {
        iac = GameObject.Find("InactiveAgentHolder").GetComponent<New_InactiveAgentsContainer>();
        //Get all Bounds of all Floors

        foreach (MeshRenderer floor in floors)
        {
            allFloorBounds.Add(floor.bounds);
        }

        allBudenScripts = GameObject.Find("BudenContainer").GetComponentsInChildren<New_Bude>();
        allSpawner = GameObject.Find("SpawnerContainer").GetComponentsInChildren<New_Spawner>();
        allExits = GameObject.Find("ExitContainer").GetComponentsInChildren<New_Exit>();


        //Generate for every floor the tiles and positions thereof
        foreach (Bounds floor in allFloorBounds)//vorerst nur ein floor, sonst problem
        {
            New_TransferType tt = New_PlateGenerator.CalculatePlatePositionsAndBaseCostMatrices(floor, plateCountX, plateCountZ);
            allPlateArray = new New_Plate[plateCountX, plateCountZ];
            allPlateArray = tt.Plates;
            normalPlateX = tt.normalPlateX;
            normalPlateZ = tt.normalPlateZ;
            randPlateX = tt.randPlateX;
            randPlateZ = tt.randPlateZ;
        }

        foreach (New_Plate plate in allPlateArray)
        {
            plate.FindAllExitableDirections(pathDiagonal);//prob needs all neighbors stored to recalc
        }

        foreach (New_Bude b in allBudenScripts)
        {
            List<Vector3> cornerPos = b.GetAllCornerPoints();/// Top left, Top right, Bottom Left, Bottom Right
            ReserveBudenPosition(b, cornerPos[0], cornerPos[1]);
            ReserveBudenPosition(b, cornerPos[1], cornerPos[3]);
            ReserveBudenPosition(b, cornerPos[3], cornerPos[2]);
            ReserveBudenPosition(b, cornerPos[2], cornerPos[0]);
        }

        

        GenerateGoalNodes();
    }


    void Update()
    {
        if (playerCount > maxPlayerCount)
        {
            DespawnUnused();
        }
    }
    private void DespawnUnused()
    {
        while (playerCount > maxPlayerCount && alleCurrentAgents.Count > 0)
        {
            New_NPC ac = alleCurrentAgents[0];
            ac.SetInactive(iac.GetWorldCoords());
            iac.AddAgent(ac);
            alleCurrentAgents.Remove(ac);
            playerCount--;
            inactivePlayerCount++;
        }
    }


   
    public void ReserveBudenPosition(New_Bude b,Vector3 _start, Vector3 _end)
    {
        Vector2Int start = WorldPositionToPlateArrayPosition(_start);
        Vector2Int end = WorldPositionToPlateArrayPosition(_end);
        Vector2Int dirEnd = end - start;
        Vector3 worldEnd = _end - _start;
        List<Vector2Int> platesToVisit = New_GenerateMatrix.InterpolateArray(start, end, ((Vector2Int a, Vector2Int b) toCompare) => (Vector3.Distance(allPlateArray[toCompare.a.x, toCompare.a.y].Center, _end) < Vector3.Distance(allPlateArray[toCompare.b.x, toCompare.b.y].Center, _end))? toCompare.a : toCompare.b , false, plateCountX, plateCountZ);
      
        Vector2Int startPos = allPlateArray[platesToVisit[0].x, platesToVisit[0].y].GetPositionInArray(_start, true);
        if (platesToVisit.Count > 1)
        {
            Vector2Int dirToNextPlate = platesToVisit[1] - platesToVisit[0];
            for (int i = 0; i < platesToVisit.Count;i++)
            {
                if (i < platesToVisit.Count - 1)
                {
                    dirEnd = end - platesToVisit[i];
                    startPos = allPlateArray[platesToVisit[i].x, platesToVisit[i].y].OccupySpaces(b, startPos, _end, pathDiagonal);
                    dirToNextPlate = platesToVisit[i + 1] - platesToVisit[i];
                }
                else
                {
                    allPlateArray[platesToVisit[i].x, platesToVisit[i].y].OccupySpaces(b, startPos, _end, pathDiagonal);
                    return;
                }
                if (dirToNextPlate.x == 0)
                {//y bigger
                    if (dirToNextPlate.y > 0)
                    {
                        startPos.y = 0;
                    }
                    else
                    {
                        startPos.y = allPlateArray[platesToVisit[i+1].x, platesToVisit[i + 1].y].Columns - 1;
                    }
                }
                else
                {
                    if (dirToNextPlate.x > 0)
                    {
                        startPos.x = 0;
                    }
                    else
                    {
                        startPos.x = allPlateArray[platesToVisit[i + 1].x, platesToVisit[i + 1].y].Rows - 1;
                    }
                }
            }
        }
        else
        {
            allPlateArray[platesToVisit.Last().x, platesToVisit.Last().y].OccupySpaces(b, startPos, _end,pathDiagonal);
        }
    }





    public void addPlayer(New_NPC npc) { if(!alleCurrentAgents.Contains(npc)){ alleCurrentAgents.Add(npc); playerCount++; } }
    public void removePlayer(New_NPC npc) { playerCount--; alleCurrentAgents.Remove(npc); }
    public bool CanAddPlayer() { return playerCount < maxPlayerCount; }
    public Vector3? GetNewSpawnPoint()
    {
        return allSpawner[UnityEngine.Random.Range(0, allSpawner.Length)].GenerateRandomPosition();
    }
    // public int GetBudenCount() { return alleBuden.Length-leereStellen.Count; }
    // public int ExitCount() { return alleExits.Length; }

    public void LostPatience() { agentsLostPatience++; }

    void GenerateGoalNodes()
    {
        //Conditions in which the should go to the same goalnode
        //1: Facing similar direction and not one infornt of the other
        //2: Facing eachother, but they are truly facing eathother

        List<List<New_Bude>> groups = new List<List<New_Bude>>();

        bool added = false;

        int j = 0;
        bool shouldBreak = false;


        for (int i = 0; i < allBudenScripts.Length; i++)
        {
            New_Bude goal = allBudenScripts[i];
            while (j < groups.Count && !shouldBreak)
            {
                if (Vector3.Distance(groups[j][0].GetFarestPoint(), goal.GetFarestPoint()) <= maxGoalNodeDistance && WorldPositionToPlateArrayPosition(goal.GetFarestPoint()) == WorldPositionToPlateArrayPosition(groups[j][0].GetFarestPoint()))
                {
                    Vector3 firstDirection = groups[j][0].GetFacingDirection();
                    Vector3 goalDirection = goal.GetFacingDirection();


                    float dot = Vector3.Dot(goalDirection.normalized, firstDirection.normalized);//dot product to get the direction the vectors are facing

                    //should divide by position
                    Vector3 displacement = groups[j][0].GetFarestPoint() - goal.GetFarestPoint();
                    float positionBudenToEachOther = Vector3.Dot(displacement.normalized, goalDirection.normalized);

                    if (positionBudenToEachOther > 0)
                    {
                        //only invalid position here are
                        //1: If it is not in front by enough
                        //2: They are pointing in the same direction and are directly infront of eathother(or to close)
                        if (positionBudenToEachOther > 0.5f) //everthing could be accetpable
                        {
                            if (positionBudenToEachOther > 0.8f)//only accetable if facing eachother
                            {
                                if (dot < -0.4f)
                                {
                                    groups[j].Add(goal);
                                    added = true;
                                    shouldBreak = true;
                                }
                            }
                            else
                            {
                                groups[j].Add(goal);
                                added = true;
                                shouldBreak = true;
                            }
                        }
                        else // only accetpable if facing same direction
                        {
                            if (dot < 0.1f)
                            {
                                groups[j].Add(goal);
                                added = true;
                                shouldBreak = true;
                            }
                        }
                    }
                    else if (positionBudenToEachOther < 0)//the other but inverted
                    {
                        if (positionBudenToEachOther < -0.5f) //everthing could be accetpable
                        {
                            if (positionBudenToEachOther < -0.8f)//only accetable if facing eachother
                            {
                                if (dot < -0.4f)
                                {
                                    groups[j].Add(goal);
                                    added = true;
                                    shouldBreak = true;
                                }
                            }
                            else
                            {
                                groups[j].Add(goal);
                                added = true;
                                shouldBreak = true;
                            }
                        }
                        else // only accetpable if facing same direction
                        {
                            if (dot > 0.9f)
                            {
                                groups[j].Add(goal);
                                added = true;
                                shouldBreak = true;
                            }
                        }
                    }
                }
                j++;
            }

            if (!added)
            {
                groups.Add(new List<New_Bude>());
                groups[^1].Add(goal);
            }

            shouldBreak = false;
            j = 0;
            added = false;
        }

        foreach (List<New_Bude> goalGroup in groups)
        {
            New_GoalNode gn = new New_GoalNode(goalGroup, WorldPositionToPlate(goalGroup[0].GetFarestPoint()));
            allGoalNodes.Add(gn);
            gn.OnPlate.AddGoalNode(gn, pathDiagonal);
        }
    }

    List<Vector3> debugTiles = new List<Vector3>();
    List<New_Plate> debugPlates = new List<New_Plate>();

    public Queue<Vector3> HandlePathRequest(Vector3 start, New_GoalNode goalNode)
    {
        return HandlePathRequest(start, goalNode.Position);
    }

    int[,] DebugDistance;
    bool debugingDistance = false;
    public Queue<Vector3> HandlePathRequest(Vector3 start, Vector3 goal)
    {
        if (allPositionsToGoals.ContainsKey((start,goal))) {
            return new Queue<Vector3>(allPositionsToGoals[(start, goal)].Item2);
        }

        Vector2Int arrayStart = WorldPositionToPlateArrayPosition(start);
        Vector2Int arrayGoal = WorldPositionToPlateArrayPosition(goal);

        int[,] baseCostPlates = New_GenerateMatrix.GenerateBaseCostMatrix(plateCountX, plateCountZ, (int row, int column) => !allPlateArray[row, column].HasOnlyObstacles, out bool onlyObstacles, out bool noObstacles);
        int[,] distanceMatrix = New_GenerateMatrix.GenerateDistanceField(baseCostPlates, plateCountX, plateCountZ, arrayGoal, (Vector2Int currentPlate, Vector2Int direction) => canPathTo(currentPlate,direction), pathDiagonal);

        List<Vector2Int> platePosToVisit = New_GenerateMatrix.GetBestPathInDistanceMatrix(distanceMatrix, plateCountX, plateCountZ, arrayStart, pathDiagonal, (Vector2Int currentPlate, Vector2Int direction) => canPathTo(currentPlate, direction));
        List<New_Plate> platesToVisit = new List<New_Plate>();

        foreach (Vector2Int pos in platePosToVisit)
        {
            platesToVisit.Add(allPlateArray[pos.x, pos.y]);
        }
        debugPlates = platesToVisit;
        debugingDistance = true;
        DebugDistance = distanceMatrix;

        Queue<Vector3> wayPoints = New_GenerateMatrix.GeneratePath(platesToVisit, start, goal, pathDiagonal);

        allPositionsToGoals.Add((start, goal), (platesToVisit, new Queue<Vector3>(wayPoints)));

        debugTiles = wayPoints.ToList();

        return wayPoints;
    }

    bool canPathTo(Vector2Int currentPlate, Vector2Int direction)
    {
        ExitDirection exit;
        ExitDirection inverse;
        if (direction.x == 0)
        {
            if (direction.y > 0)
            {
                exit = ExitDirection.East;
                inverse = ExitDirection.West;
            }
            else
            {
                exit = ExitDirection.West;
                inverse = ExitDirection.East;
            }
        }
        else if (direction.y == 0)
        {
            if (direction.x > 0)
            {
                exit = ExitDirection.South;
                inverse = ExitDirection.North;
            }
            else
            {
                exit = ExitDirection.North;
                inverse = ExitDirection.South;
            }
        }
        else
        {
            if (direction.x > 0)
            {
                if (direction.y > 0)
                {
                    exit = ExitDirection.SouthEast;
                    inverse = ExitDirection.NorthWest;
                }
                else
                {
                    exit = ExitDirection.SouthWest;
                    inverse = ExitDirection.NorthEast;
                }
            }
            else
            {
                if (direction.y > 0)
                {
                    exit = ExitDirection.NorthEast;
                    inverse = ExitDirection.SouthWest;
                }
                else
                {
                    exit = ExitDirection.NorthWest;
                    inverse = ExitDirection.SouthEast;
                }
            }
        }
        return !allPlateArray[currentPlate.x, currentPlate.y].HasOnlyObstacles && !allPlateArray[currentPlate.x + direction.x, currentPlate.y + direction.y].HasOnlyObstacles && allPlateArray[currentPlate.x, currentPlate.y].CanExit.Contains(exit) && allPlateArray[currentPlate.x + direction.x, currentPlate.y + direction.y].CanExit.Contains(inverse);
    }



    New_Plate WorldPositionToPlate(Vector3 pos)
    {
        int plateNumberX = Mathf.FloorToInt((pos.x - allFloorBounds[0].min.x) / normalPlateX);
        int plateNumberZ = Mathf.FloorToInt((pos.z - allFloorBounds[0].min.z) / normalPlateZ);
        plateNumberX = Math.Clamp(plateNumberX, 0, plateCountX - 1);
        plateNumberZ = Math.Clamp(plateNumberZ, 0, plateCountZ - 1);
        return allPlateArray[plateNumberX,plateNumberZ];
    }


    Vector2Int WorldPositionToPlateArrayPosition(Vector3 pos)
    {
        int plateNumberX = Mathf.FloorToInt((pos.x - allFloorBounds[0].min.x) / normalPlateX);
        int plateNumberZ = Mathf.FloorToInt((pos.z - allFloorBounds[0].min.z) / normalPlateZ);
        plateNumberX = Math.Clamp(plateNumberX, 0, plateCountX - 1);
        plateNumberZ = Math.Clamp(plateNumberZ, 0, plateCountZ - 1);
        return new Vector2Int(plateNumberX, plateNumberZ);
    }   

    public Queue<New_Bude> GetNewBuden(int goalsBeforeExit)
    {
        Queue<New_Bude> returnBuden = new Queue<New_Bude>();
        for (int i = 0; i < goalsBeforeExit; i++)
        {
            int random = UnityEngine.Random.Range(0, allBudenScripts.Length);
            New_Bude newBude = allBudenScripts[random];
            while (returnBuden.Contains(newBude))
            {
                newBude = allBudenScripts[UnityEngine.Random.Range(0, allBudenScripts.Length)];
            }
            returnBuden.Enqueue(newBude);
        }
        return returnBuden;
    }

    public Vector3 GetRandomExitPosition()
    {
        return allExits[UnityEngine.Random.Range(0, allExits.Length)].GetPosition();
    }
    public int GetGoalNoteCount()
    {
        return allGoalNodes.Count;
    }

    public void AddBude(New_Bude neueBude)
    {
        if (leereStellen.Count > 0)
        {
            neueBude.Start();
            allBudenScripts[leereStellen.First.Value] = neueBude;
            leereStellen.RemoveFirst();
        }
        else
        {
            neueBude.Start();
            List<New_Bude> tempList = allBudenScripts.ToList();
            tempList.Add(neueBude);
            allBudenScripts = tempList.ToArray();
        }
        CalcAllBudenWeight();
    }

    public void RemoveBude(Buden wegBude)
    {
        // SoundFXManager.instance.PlaySoundFXClip(deleteSoundClip, transform, 1f);

        for(int i = 0; i <allBudenScripts.Length; i++)
        {
            if(allBudenScripts[i]== wegBude)
            {
                allBudenScripts[i] = null;
                leereStellen.AddFirst(i);
            }
        }
    }

 private int CalcNewWeightedBude(List<int> besuchteBudenNr)
    {
        int rand = UnityEngine.Random.Range(0, allBudenWeigth+1);
        int bNr=-1;
        int tmpCount=0;
        for (int i = 0; i < allBudenScripts.Length; i++)
        {
            if (!besuchteBudenNr.Contains(i) && allBudenScripts[i]!=null)
            {
                bNr = i;
                tmpCount += allBudenScripts[i].attraktivitaet;
                if (tmpCount > rand) { return bNr; }
            }
        }
        return bNr;
    }

    public void CalcAllBudenWeight()
    {
        maxKapazitaet = 0;
        allBudenWeigth = 0;
        foreach(New_Bude b in allBudenScripts)
        {
            if(b != null)
            {
                maxKapazitaet += b.kapazitaet;
                allBudenWeigth += b.attraktivitaet;
            }
        }
    }

    public bool showDebugPlates = false;
    public int shownPlate = -1;
    //16,29 sus
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) { return; }
        if (debugingDistance)
        {
            for (int i = 0; i < plateCountX; i++)
            {
                for (int j = 0; j < plateCountZ; j++)
                {
                    if (debugingDistance)
                    {
                        Handles.Label(allPlateArray[i, j].Center + new Vector3(0, 1, 0), DebugDistance[i, j].ToString());
                    }
                    ;
                }
            }
        }
        if (showDebugPlates)
        {
            for (int i = 0; i < shownPlate; i++)
            {
                New_Plate p = debugPlates[i];
                Gizmos.color = (i % 2 == 0) ? Color.red : Color.black;
                Gizmos.DrawCube(p.Center, new Vector3(p.Size.x, 0.1f, p.Size.z));
            }
        }
        foreach (New_GoalNode n in allGoalNodes)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(n.Position, new Vector3(1, 0.1f, 1));
        }
        foreach (New_Plate p in allPlateArray)
        {
            for (int row = 0; row < p.Rows; row++)
            {
                for (int col = 0; col < p.Columns; col++)
                {
                    if (p.BaseCostMatrix[row, col] != New_GenerateMatrix.MatrixIsPathableValue)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(p.GetSubTileCenterWorldCoordinates(row, col), new Vector3(New_GenerateMatrix.TileSizeX - 0.01f, 0.01f, New_GenerateMatrix.TileSizeZ - 0.01f));
                    }
                }
            }
        }
    }

}



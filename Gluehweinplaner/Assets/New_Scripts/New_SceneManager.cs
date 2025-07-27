using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;


//ToDo:
// 1. Generate Distance Field for every exit


public class New_SceneManager : MonoBehaviour
{
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


    private bool geCalct = false;



    private Dictionary<(Vector3, Vector3), (List<New_Plate>, Queue<Vector3>)> allPositionsToGoals = new Dictionary<(Vector3, Vector3), (List<New_Plate>, Queue<Vector3>)>();

    void Start()
    {
        iac = GameObject.Find("InactiveAgentHolder").GetComponent<New_InactiveAgentsContainer>();
        //Get all Bounds of all Floors
        MeshCollider[] allFloors = new MeshCollider[1];
        allFloors[0] = GameObject.Find("Leipzig").transform.GetChild(1).GetComponent<MeshCollider>();
        foreach (MeshCollider floor in allFloors)
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
            allPlateArray = new New_Plate[tt.plateCountX, tt.plateCountZ];
            allPlateArray = tt.Plates;
            plateCountX = tt.plateCountX;
            plateCountZ = tt.plateCountZ;
            normalPlateX = tt.normalPlateX;
            normalPlateZ = tt.normalPlateZ;
            randPlateX = tt.randPlateX;
            randPlateZ = tt.randPlateZ;
        }


        GenerateGoalNodes();


        foreach (New_Plate plate in allPlateArray)
        {
            if (!plate.HasOnlyObstacles && !plate.HasNoObstacles)
            {
                Vector2Int vect = plate.GenerateAndAddExitPointVector2(ExitDirection.North);
                if (vect != new Vector2Int(-1, -1)) { plate.AddDistanceFieldToExit(ExitDirection.North, New_GenerateMatrix.GenerateDistanceField(plate.BaseCostMatrix, plate.Rows, plate.Columns, vect)); }
                vect = plate.GenerateAndAddExitPointVector2(ExitDirection.West);
                if (vect != new Vector2Int(-1, -1)) { plate.AddDistanceFieldToExit(ExitDirection.West, New_GenerateMatrix.GenerateDistanceField(plate.BaseCostMatrix, plate.Rows, plate.Columns, vect)); }
                vect = plate.GenerateAndAddExitPointVector2(ExitDirection.East);
                if (vect != new Vector2Int(-1, -1)) { plate.AddDistanceFieldToExit(ExitDirection.East, New_GenerateMatrix.GenerateDistanceField(plate.BaseCostMatrix, plate.Rows, plate.Columns, vect)); }
                vect = plate.GenerateAndAddExitPointVector2(ExitDirection.South);
                if (vect != new Vector2Int(-1, -1)) { plate.AddDistanceFieldToExit(ExitDirection.South, New_GenerateMatrix.GenerateDistanceField(plate.BaseCostMatrix, plate.Rows, plate.Columns, vect)); }
            }
            else
            {
                plate.GenerateAndAddExitPointVector2(ExitDirection.North);
                plate.GenerateAndAddExitPointVector2(ExitDirection.South);
                plate.GenerateAndAddExitPointVector2(ExitDirection.West);
                plate.GenerateAndAddExitPointVector2(ExitDirection.East);
            }
        }


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

    public void addPlayer(New_NPC npc) { if(!alleCurrentAgents.Contains(npc)){ alleCurrentAgents.Add(npc); playerCount++; } }
    public void removePlayer(New_NPC npc) { playerCount--; alleCurrentAgents.Remove(npc); }
    public bool CanAddPlayer() { return playerCount < maxPlayerCount; }
    public Vector3 GetNewSpawnPoint()
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
                if (Vector3.Distance(groups[j][0].GetPosition(), goal.GetPosition()) <= maxGoalNodeDistance)
                {
                    Vector3 firstDirection = groups[j][0].GetFacingDirection();
                    Vector3 goalDirection = goal.GetFacingDirection();


                    float dot = Vector3.Dot(goalDirection.normalized, firstDirection.normalized);//dot product to get the direction the vectors are facing

                    //should divide by position
                    Vector3 displacement = groups[j][0].GetPosition() - goal.GetPosition();
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
            New_GoalNode gn = new New_GoalNode(goalGroup);
            allGoalNodes.Add(gn);
            gn.CalculatePosition();
            AddGoalNodeToCorrespondingPlateAndGenerateDistanceField(gn);
        }
    }

    void AddGoalNodeToCorrespondingPlateAndGenerateDistanceField(New_GoalNode goal)
    {
        New_Plate plate = allPlateArray[goal.OnPlate.x, goal.OnPlate.y];
        plate.AddGoalNodeAndDistanceField(goal, (!plate.HasNoObstacles && !plate.HasOnlyObstacles) ? new int[0, 0] : New_GenerateMatrix.GenerateDistanceField(plate, goal.Position));
    }

    List<Vector3> debugTiles = new List<Vector3>();
    List<Vector2Int> debugPlates = new List<Vector2Int>();

    public Queue<Vector3> HandlePathRequest(Vector3 start, New_GoalNode goalNode)
    {
        return HandlePathRequest(start, goalNode.Position);
    }

    public Queue<Vector3> HandlePathRequest(Vector3 start, Vector3 goal)
    {
        if (allPositionsToGoals.ContainsKey((start,goal))) {
            return allPositionsToGoals[(start, goal)].Item2;
        }
        List<Vector3> steps = new List<Vector3> { goal };

        Vector2Int arrayStart = WorldPositionToPlateArrayPosition(start);
        Vector2Int arrayGoal = WorldPositionToPlateArrayPosition(goal);

        int[,] baseCostPlates = New_GenerateMatrix.GenerateBaseCostMatrix(plateCountX, plateCountZ, (int row, int column) => !allPlateArray[row, column].HasOnlyObstacles);
        int[,] distanceMatrix = New_GenerateMatrix.GenerateDistanceField(baseCostPlates, plateCountX, plateCountZ, arrayStart);
        List<Vector2Int> platePosToVisit = New_GenerateMatrix.GetBestPathInDistanceMatrix(distanceMatrix, plateCountX, plateCountZ, arrayGoal);

        debugPlates = platePosToVisit;

        List<New_Plate> platesToVisit = new List<New_Plate>();

        foreach(Vector2Int pos in platePosToVisit)
        {
            platesToVisit.Add(allPlateArray[pos.x, pos.y]);
        }

        geCalct = true;
        

        Queue<Vector3> wayPoints = New_GenerateMatrix.GeneratePath(platesToVisit, start, goal);

        allPositionsToGoals.Add((start, goal), (platesToVisit, wayPoints));

        debugTiles = wayPoints.ToList();

        return wayPoints;
    }


    Vector2Int WorldPositionToPlateArrayPosition(New_Bude goal)
    {
        int plateNumberX = Mathf.FloorToInt((goal.GetPosition().x - allFloorBounds[0].min.x) / normalPlateX);
        int plateNumberZ = Mathf.FloorToInt((goal.GetPosition().z - allFloorBounds[0].min.z) / normalPlateZ);
        plateNumberX = Math.Clamp(plateNumberX, 0, plateCountX - 1);
        plateNumberZ = Math.Clamp(plateNumberZ, 0, plateCountZ - 1);
        return new Vector2Int(plateNumberX, plateNumberZ);
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



    public bool showPlatesDebug = false;
    public bool showTilesDebug = false;
    public int shownTiles = -1;

    void OnDrawGizmos()
    {
        Color[] colorsExit = new Color[] { Color.black, Color.red };
        Color[] colorsPlate = new Color[] { Color.blue, Color.yellow };
        if (geCalct)
        {
            if (showPlatesDebug)
            {
                for (int i = 0; i < debugPlates.Count; i++)
                {
                    Vector3 pos = allPlateArray[debugPlates[i].x, debugPlates[i].y].Center;
                    Gizmos.color = colorsPlate[i % 2];
                    Gizmos.DrawCube(pos, new Vector3(allPlateArray[debugPlates[i].x, debugPlates[i].y].Rows * New_GenerateMatrix.tileSizeX, 0.01f, allPlateArray[debugPlates[i].x, debugPlates[i].y].Columns * New_GenerateMatrix.tileSizeZ));
                }
            }
            if (showTilesDebug)
            {
                int count = shownTiles;
                if (shownTiles == -1) { count = debugTiles.Count; }
                for (int i = 0; i < count; i++)
                {
                    Vector3 step = debugTiles[i];
                    Gizmos.color = colorsExit[i % 2];
                    Gizmos.DrawCube(step, new Vector3(New_GenerateMatrix.tileSizeX, 0.01f, New_GenerateMatrix.tileSizeZ));
                }
            }
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(allGoalNodes[0].Position, new Vector3(1, 1, 1));
        }
    }
}

    

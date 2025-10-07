using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#nullable enable
/// <inheritdoc cref="ISceneManager"/>
public class SceneManager : MonoBehaviour, ISceneManager
{
    public static float maxGoalNodeDistance = 30.0f;//best case it is calculated if there are blocking pionts between the goals

    public bool pathDiagonal = false;
    public bool simulating = false;

    public int plateCountX = 0;
    public int plateCountZ = 0;

    public int maxPlayerCount = 1;
    public int playerCount = 0;
    public int inactivePlayerCount = 0;
    public int agentsLostPatience = 0;

    public int maxKapazitaet;
    public int allBudenWeigth;

    public string budenContainerName = "BudenContainer";
    public string exitContainerName = "ExitContainer";
    public string spawnerContainerName = "SpawnerContainer";
    public string heatmapname = "HeatMap";

    public MeshRenderer[]? floors = null;
    public List<Bude>? allBudenScripts = null;


    private Plate[,]? allPlateArray = null;
    private Spawner[]? allSpawner = null;
    private Exit[]? allExits = null;
    private InactiveAgentsContainer? iac = null;
    private Heatmap? hm;

    private Dictionary<Vector3, (int[,], byte[,])> goalPositionToFlowField = new Dictionary<Vector3, (int[,], byte[,])>();
    private Dictionary<(Vector3, Vector3), (List<Plate>, Queue<Vector3>)> allPositionsToGoals = new Dictionary<(Vector3, Vector3), (List<Plate>, Queue<Vector3>)>();

    private List<GoalNode> allGoalNodes = new List<GoalNode>();
    private List<Bounds> allFloorBounds = new List<Bounds>();
    private List<NPC> alleCurrentAgents = new List<NPC>();

    public float normalPlateX = 0;
    public float normalPlateZ = 0;

    [SerializeField] private AudioClip? deleteSoundClip;
    [SerializeField] private AudioClip? saveSoundClip;
    [SerializeField] private AudioClip? loadSoundClip;

    void Start()
    {
        iac = GameObject.Find("InactiveAgentHolder").GetComponent<InactiveAgentsContainer>();
        //Get all Bounds of all Floors

        foreach (MeshRenderer floor in floors!)
        {
            allFloorBounds.Add(floor.bounds);
        }

        allBudenScripts = GameObject.Find(budenContainerName).GetComponentsInChildren<Bude>().ToList();
        allSpawner = GameObject.Find(spawnerContainerName).GetComponentsInChildren<Spawner>();
        allExits = GameObject.Find(exitContainerName).GetComponentsInChildren<Exit>();
        hm = GameObject.Find(heatmapname).GetComponent<Heatmap>();


        CalcAllBudenWeight();

        //Generate for every floor the tiles and positions thereof
        foreach (Bounds floor in allFloorBounds)//vorerst nur ein floor, sonst problem
        {
            PlateGeneratorDto tt = PlateGenerator.CalculatePlatePositionsAndBaseCostMatrices(floor, plateCountX, plateCountZ, pathDiagonal);
            allPlateArray = new Plate[plateCountX, plateCountZ];
            allPlateArray = tt.Plates;
            normalPlateX = tt.normalPlateX;
            normalPlateZ = tt.normalPlateZ;
            plateCountX = tt.plateCountX;
            plateCountZ = tt.plateCountZ;
        }
        show = true;

        foreach (Bude b in allBudenScripts)
        {
            List<Vector3> cornerPos = b.GetAllCornerPoints();/// Top left, Top right, Bottom Left, Bottom Right
            List<Plate> affectedPlates = new List<Plate>();
            affectedPlates.AddRange(ReserveBudenPosition(b, cornerPos[0], cornerPos[1]));
            affectedPlates.AddRange(ReserveBudenPosition(b, cornerPos[1], cornerPos[3]));
            affectedPlates.AddRange(ReserveBudenPosition(b, cornerPos[3], cornerPos[2]));
            affectedPlates.AddRange(ReserveBudenPosition(b, cornerPos[2], cornerPos[0]));
            b.onplates = affectedPlates;
        }
        GenerateGoalNodes();
    }
    public bool show = false;
    public bool laden = false;
    public bool speichern = false;
    void Update()
    {
        if (laden)
        {
            LoadSceneFromJSON();
            laden = false;
        }
        if (speichern)
        {
            SaveJSON();
            speichern = false;
        }
        if (playerCount > maxPlayerCount)
        {
            DespawnUnused();
        }
    }
    private void DespawnUnused()
    {
        while (playerCount > maxPlayerCount && alleCurrentAgents.Count > 0)
        {
            NPC ac = alleCurrentAgents[0];
            ac.SetInactive(iac!.GetWorldCoords());
            iac.AddAgent(ac);
            alleCurrentAgents.Remove(ac);
            playerCount--;
            inactivePlayerCount++;
            hm!.ClearPos(ac.GetPosition());
        }
    }
    public void StartSimulation() { simulating = true; }
    public void ResumeSimulation() { simulating = true; foreach (NPC ac in alleCurrentAgents) { ac.Resume(); } CalcAllBudenWeight(); }
    public void StopSimulation() { simulating = false; foreach (NPC ac in alleCurrentAgents) { ac.Stop(); } }
    public void AddPlayer(NPC npc) { if (!alleCurrentAgents.Contains(npc)) { alleCurrentAgents.Add(npc); playerCount++; } }
    public void removePlayer(NPC npc) { playerCount--; alleCurrentAgents.Remove(npc); }
    public bool CanAddPlayer() { return playerCount < maxPlayerCount; }
    public (Vector3, Vector3) GetNewSpawnPoint()
    {
        return allSpawner![UnityEngine.Random.Range(0, allSpawner.Length)].GenerateRandomPosition();
    }
    public void LostPatience() { agentsLostPatience++; }
    public void ResetSimulation()
    {
        foreach (NPC ac in alleCurrentAgents)
        {
            ac.SetInactive(iac!.GetWorldCoords());
            iac!.AddAgent(ac);
        }
        foreach (Bude b in allBudenScripts!) { if (b != null) { b.Reset(); } }
        inactivePlayerCount = playerCount;
        playerCount = 0;
        agentsLostPatience = 0;
        simulating = false;
        alleCurrentAgents = new List<NPC>();
        hm!.Reset();
        CalcAllBudenWeight();
    }

    public void AddBude(Bude neueBude)
    {
        allBudenScripts!.Add(neueBude);
        List<Vector3> cornerPos = neueBude.GetAllCornerPoints();/// Top left, Top right, Bottom Left, Bottom Right
        List<Plate> affectedPlates = new List<Plate>();
        affectedPlates.AddRange(ReserveBudenPosition(neueBude, cornerPos[0], cornerPos[1]));
        affectedPlates.AddRange(ReserveBudenPosition(neueBude, cornerPos[1], cornerPos[3]));
        affectedPlates.AddRange(ReserveBudenPosition(neueBude, cornerPos[3], cornerPos[2]));
        affectedPlates.AddRange(ReserveBudenPosition(neueBude, cornerPos[2], cornerPos[0]));
        neueBude.onplates = affectedPlates;
        FindOrCreateNewGoalNode(neueBude);
        CalcAllBudenWeight();
    }

    public void RemoveBude(Bude wegBude)
    {
        foreach (Plate plate in wegBude.onplates)
        {
            plate.BudeRemoved(wegBude);
        }
        SoundFXManager.instance.PlaySoundFXClip(deleteSoundClip, transform, 1f);
        wegBude.BudeRemove();
        allBudenScripts!.Add(wegBude);
    }
    public void BudeMoved(Bude bude)
    {
        List<Plate> onplates = bude.onplates;
        foreach (Plate plate in onplates)
        {
            plate.BudeRemoved(bude);
        }
        List<Vector3> cornerPos = bude.GetAllCornerPoints();/// Top left, Top right, Bottom Left, Bottom Right
        List<Plate> affectedPlates = new List<Plate>();
        affectedPlates.AddRange(ReserveBudenPosition(bude, cornerPos[0], cornerPos[1]));
        affectedPlates.AddRange(ReserveBudenPosition(bude, cornerPos[1], cornerPos[3]));
        affectedPlates.AddRange(ReserveBudenPosition(bude, cornerPos[3], cornerPos[2]));
        affectedPlates.AddRange(ReserveBudenPosition(bude, cornerPos[2], cornerPos[0]));
        bude.onplates = affectedPlates;
        FindOrCreateNewGoalNode(bude);
    }

    public void Pause()
    {
        if (simulating)
        {
            StopSimulation();
        }
        else
        {
            ResumeSimulation();
        }
    }

    public void ToggleSimulation()
    {
        if (playerCount == 0)
        {
            StartSimulation();
        }
        else
        {
            ResetSimulation();
        }
    }
    public void IncreaseMaxPlayerCount()
    {
        maxPlayerCount += 50;
    }
    public void DecreaseMaxPlayerCount()
    {
        if (maxPlayerCount >= 50)
        {
            maxPlayerCount -= 50;
        }
    }
    /// <summary>
    /// This function creates the json string from all relevant objects in the scene.
    /// </summary>
    /// <returns>THe json-string representing the scene.</returns>
    private string CreateJSON()
    {
        AlleBudenJSON aB = new AlleBudenJSON(allBudenScripts!.Count);
        AlleExitJSON aE = new AlleExitJSON(allExits!.Length);
        AlleSpawnJSON aS = new AlleSpawnJSON(allSpawner!.Length);
        int j = 0;
        for (int i = 0; i < allBudenScripts!.Count; i++)
        {
            if (allBudenScripts[i] != null)
            {
                aB.budenArray[j] = allBudenScripts[i].GetBudenJSON();
                j++;
            }
        }
        for (int i = 0; i < allSpawner.Length; i++)
        {
            aS.spawnArray[i] = allSpawner[i].ToJSON();
        }
        for (int i = 0; i < allExits!.Length; i++)
        {
            aE.exitArray[i] = allExits![i].ToJSON();
        }
        return JsonUtility.ToJson(new GanzeSzene(aB, aE, aS));
    }

    public void SaveJSON()
    {
        Debug.Log("Speichere JSON");
        string path = Application.persistentDataPath + "/Position.json";
        Debug.Log("Speichere JSON nach: " + path);
        using (StreamWriter writer = new StreamWriter(path, false))
        {
            writer.Write(CreateJSON());
        }
        SoundFXManager.instance.PlaySoundFXClip(saveSoundClip, transform, 1f);
    }

    private GanzeSzene? ReadJSON()
    {
        string path = Application.persistentDataPath + "/Position.json";
        GanzeSzene? a = null;

        if (!File.Exists(path))
        {
            Debug.LogWarning("Datei existiert nicht: " + path);
            return null;
        }
        try
        {
            Debug.Log("Lese Datei von Pfad: " + path);
            using (StreamReader reader = new StreamReader(path))
            {
                string jsonContent = reader.ReadToEnd();

                a = JsonUtility.FromJson<GanzeSzene>(jsonContent);

                if (a == null)
                {
                    Debug.LogWarning("Fehler beim Parsen der JSON-Datei.");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Fehler beim Lesen der Datei: " + e);
        }

        return a;
    }


    public void LoadSceneFromJSON()
    {
        GanzeSzene? gS = ReadJSON();
        GameObject? stand = Resources.Load("New_Stand") as GameObject;
        GameObject? exit = Resources.Load("Exit") as GameObject;
        GameObject? spawner = Resources.Load("Spawner") as GameObject;
        if (gS != null)
        {
            GameObject budenContainer = GameObject.Find(budenContainerName);
            foreach (BudenJSON b in gS.alleBuden.budenArray)
            {
                Vector3 pos = new(b.xPos, 0, b.zPos);
                Quaternion orien = Quaternion.Euler(0, b.yRot, 0);
                if (stand != null && !Physics.CheckSphere(new Vector3(pos.x, 10, pos.z), 6.25f))
                {
                    GameObject newObj = Instantiate(stand, pos, orien);
                    Bude bd = newObj.GetComponent<Bude>();
                    newObj.transform.parent = budenContainer.transform;
                    bd.attraktivitaet = b.attrak;
                    bd.WaitTime = b.waittime;
                    bd.SetTypeIndex(1);
                    AddBude(bd);
                }
            }
            foreach (ExitJSON e in gS.allExits.exitArray)
            {
                Vector3 pos = new Vector3(e.xPos, 0, e.zPos);
                Quaternion orien = Quaternion.Euler(0, e.yRot, 0);
                Transform exitContainer = GameObject.Find(exitContainerName).transform;
                if (exit != null)
                {
                    GameObject newObj = Instantiate(exit, pos, orien);
                    newObj.transform.parent = exitContainer;
                }
            }
            foreach (SpawnJSON s in gS.alleSpawns.spawnArray)
            {
                Vector3 pos = new Vector3(s.xPos, 0, s.zPos);
                Quaternion orien = Quaternion.Euler(0, s.yRot, 0);
                Transform spawnerContainer = GameObject.Find(spawnerContainerName).transform;
                if (spawner != null)
                {
                    GameObject newObj = Instantiate(spawner, pos, orien);
                    newObj.transform.parent = spawnerContainer;
                }
            }
            SoundFXManager.instance.PlaySoundFXClip(loadSoundClip, transform, 1f);
        }
        else
        {
            Debug.LogWarning("Konnte keine Datei lesen von pfad: " + Application.persistentDataPath + "/Position.json");
        }
    }
    public List<Plate> ReserveBudenPosition(Bude b, Vector3 _start, Vector3 _end)
    {
        Vector2Int start = WorldPositionToPlateArrayPosition(_start);
        Vector2Int end = WorldPositionToPlateArrayPosition(_end);
        Vector2Int dirEnd = end - start;
        Vector3 worldEnd = _end - _start;
        List<Vector2Int> platePosToVisit = GenerateMatrix.InterpolateArray(start, end, ((Vector2Int a, Vector2Int b) toCompare) => (Vector3.Distance(allPlateArray![toCompare.a.x, toCompare.a.y].Center, _end) < Vector3.Distance(allPlateArray[toCompare.b.x, toCompare.b.y].Center, _end)) ? toCompare.a : toCompare.b, false, plateCountX, plateCountZ);
        List<Plate> platesToVisit = new List<Plate>();
        foreach (Vector2Int pos in platePosToVisit)
        {
            platesToVisit.Add(allPlateArray![pos.x, pos.y]);
        }
        Vector2Int startPos = allPlateArray![platePosToVisit[0].x, platePosToVisit[0].y].GetPositionInArray(_start, true);
        if (platePosToVisit.Count > 1)
        {
            Vector2Int dirToNextPlate = platePosToVisit[1] - platePosToVisit[0];
            for (int i = 0; i < platePosToVisit.Count; i++)
            {
                if (i < platePosToVisit.Count - 1)
                {
                    dirEnd = end - platePosToVisit[i];
                    startPos = allPlateArray[platePosToVisit[i].x, platePosToVisit[i].y].OccupySpaces(b, startPos, _end);
                    dirToNextPlate = platePosToVisit[i + 1] - platePosToVisit[i];
                }
                else
                {
                    allPlateArray[platePosToVisit[i].x, platePosToVisit[i].y].OccupySpaces(b, startPos, _end);
                    return platesToVisit;
                }
                if (dirToNextPlate.x == 0)
                {//y bigger
                    if (dirToNextPlate.y > 0)
                    {
                        startPos.y = 0;
                    }
                    else
                    {
                        startPos.y = allPlateArray[platePosToVisit[i + 1].x, platePosToVisit[i + 1].y].Columns - 1;
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
                        startPos.x = allPlateArray[platePosToVisit[i + 1].x, platePosToVisit[i + 1].y].Rows - 1;
                    }
                }
            }
        }
        else
        {
            allPlateArray[platePosToVisit.Last().x, platePosToVisit.Last().y].OccupySpaces(b, startPos, _end);
        }
        return platesToVisit;
    }
    /// <summary>
    /// This function generates all goalnodes. For that it takes a list of position from buden an groups then according to their distance and position. Furthermore this scripts checks wether or not the orientation
    /// of the bude that would be added is correct or not, so that a common goalnode can be generated.
    /// </summary>
    void GenerateGoalNodes()
    {
        List<List<Bude>> groups = new List<List<Bude>>();
        bool added = false;
        int j = 0;
        bool shouldBreak = false;
        for (int i = 0; i < allBudenScripts!.Count; i++)
        {
            Bude goal = allBudenScripts[i];
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
                    if (positionBudenToEachOther > 0)//checks if the bude has the matching facing diretion
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
                groups.Add(new List<Bude>());
                groups[^1].Add(goal);
            }
            shouldBreak = false;
            j = 0;
            added = false;
        }
        foreach (List<Bude> goalGroup in groups)
        {
            GoalNode gn = new GoalNode(goalGroup, WorldPositionToPlate(goalGroup[0].GetFarestPoint()), this);
            allGoalNodes.Add(gn);
            gn.OnPlate.AddGoalNode(gn);
        }
    }
    public void FindOrCreateNewGoalNode(Bude bude)
    {
        Plate on = WorldPositionToPlate(bude.GetFarestPoint());
        foreach (GoalNode gn1 in allGoalNodes)
        {
            if (gn1.OnPlate == on)
            {
                gn1.AddBude(bude);
                gn1.CalculatePosition();
                return;
            }
        }
        GoalNode gn = new GoalNode(new List<Bude> { bude }, WorldPositionToPlate(bude.GetFarestPoint()), this);
        allGoalNodes.Add(gn);
        gn.OnPlate.AddGoalNode(gn);
    }
    public void RemoveGoalNode(GoalNode gn)
    {
        allGoalNodes.Remove(gn);
    }
    public Queue<Vector3> HandlePathRequest(Vector3 start, GoalNode goalNode)
    {
        return HandlePathRequest(start, goalNode.Position);
    }
    public Queue<Vector3> HandlePathRequest(Vector3 start, Vector3 goal)
    {
        if (allPositionsToGoals.ContainsKey((start, goal)))
        {
            if (!PlatePathChanged(allPositionsToGoals[(start, goal)].Item1))
            {
                return new Queue<Vector3>(allPositionsToGoals[(start, goal)].Item2);
            }
            else
            {
                allPositionsToGoals.Remove((start, goal));
            }
        }
        Vector2Int arrayStart = WorldPositionToPlateArrayPosition(start);
        Vector2Int arrayGoal = WorldPositionToPlateArrayPosition(goal);
        List<Vector2Int> platePosToVisit;
        byte[,] flowField;
        int[,] baseCostPlates;
        List<Plate> platesToVisit;

        if (goalPositionToFlowField.ContainsKey(start))
        {
            (baseCostPlates,flowField) = goalPositionToFlowField[start];
        }
        else
        {
            baseCostPlates = GenerateMatrix.GenerateBaseCostMatrix(plateCountX, plateCountZ, (int row, int column) => !allPlateArray![row, column].HasOnlyObstacles, out bool onlyObstacles, out bool noObstacles);
            (_, flowField) = GenerateMatrix.GenerateDistanceFieldAndFlowField(baseCostPlates, plateCountX, plateCountZ, new List<Vector2Int> { arrayGoal }, pathDiagonal, canPathTo);
        }

        platePosToVisit = GenerateMatrix.GetBestPathInFlowFieldFull(flowField, arrayStart);
        if (platePosToVisit.Count == 0) { return new Queue<Vector3>(); }//there is no way to path to the goal from the given position
        platesToVisit = new List<Plate>();
        foreach (Vector2Int pos in platePosToVisit)
        {
            platesToVisit.Add(allPlateArray![pos.x, pos.y]);
        }
        (Queue<Vector3> wayPoints, Plate? lastVisitedPlate) = GenerateMatrix.GeneratePath(platesToVisit, start, goal);
        if (lastVisitedPlate == null)
        {
            if (!goalPositionToFlowField.ContainsKey(goal))
            {
                goalPositionToFlowField.Add(goal, (baseCostPlates, flowField));
            }
            if (!allPositionsToGoals.ContainsKey((start, goal)))
            {
                allPositionsToGoals.Add((start, goal), (platesToVisit, new Queue<Vector3>(wayPoints)));
            }
            
            return wayPoints;
        }
        else // if the current plates to visit contain a plate that is not passable a new path needs to be generated
        {
            //should theoretacly only be reached if within a plate no path to an portal an be found
            int tries = 0;//amount of tries taken to generate a path
            Plate goalPlate = allPlateArray![arrayGoal.x, arrayGoal.y];
            do
            {
                Vector2Int platePos = WorldPositionToPlateArrayPosition(lastVisitedPlate!.Center);
                baseCostPlates[platePos.x, platePos.y] = GenerateMatrix.MatrixObstacleValue;
                (_, flowField) = GenerateMatrix.GenerateDistanceFieldAndFlowField(baseCostPlates, plateCountX, plateCountZ, new List<Vector2Int> { arrayGoal }, pathDiagonal, canPathTo);
                platePosToVisit = GenerateMatrix.GetBestPathInFlowFieldFull(flowField, arrayStart);
                if (platePosToVisit.Count == 0) { return new Queue<Vector3>(); }//there is no way to path to the goal from the given position
                platesToVisit = new List<Plate>();
                foreach (Vector2Int pos in platePosToVisit)
                {
                    platesToVisit.Add(allPlateArray![pos.x, pos.y]);
                }
                (Queue<Vector3> newWayPoints, Plate? lastPlate) = GenerateMatrix.GeneratePath(platesToVisit, start, goal);
                lastVisitedPlate = lastPlate;
                if (lastPlate == null)
                {
                    if (allPositionsToGoals.ContainsKey((start, goal)))
                    {
                        allPositionsToGoals.Remove((start, goal));
                    }
                    if (goalPositionToFlowField.ContainsKey(goal))
                    {
                        goalPositionToFlowField.Remove(goal);
                    }
                    allPositionsToGoals.Add((start, goal), (platesToVisit, new Queue<Vector3>(newWayPoints)));
                    goalPositionToFlowField.Add(goal, (baseCostPlates, flowField));
                    return newWayPoints;
                }
                tries++;
            } while (tries < Mathf.Max(plateCountX, plateCountZ));
            return new Queue<Vector3>();//no path could be found
        }
    }
    public bool isCloser(Vector2Int currMin, Vector2Int toCheck, Vector2Int goal)
    {
        return Vector2Int.Distance(currMin, goal) > Vector2Int.Distance(toCheck, goal);
    }
    public bool canPathTo(Vector2Int startPlate, Vector2Int neighborPlate)
    {
        return allPlateArray![startPlate.x, startPlate.y].HasPortalInDirection(ExitDirection.DirectionToExitDirection(neighborPlate - startPlate), allPlateArray[neighborPlate.x, neighborPlate.y]);
    }

    /// <summary>
    /// This function checks wether or not the path of plates contains a plate whichs path has been changed
    /// </summary>
    /// <param name="np"></param>
    /// <returns>True if the path has changed.</returns>
    private bool PlatePathChanged(List<Plate> np)
    {
        bool changed = false;
        foreach (Plate p in np)
        {
            changed |= p.hasChanged;
        }
        return changed;
    }
    public void ClearPos(Vector3 pos)
    {
        hm!.ClearPos(pos);
    }
    public Plate WorldPositionToPlate(Vector3 pos)
    {
        int plateNumberX = Mathf.FloorToInt((pos.x - allFloorBounds[0].min.x) / normalPlateX);
        int plateNumberZ = Mathf.FloorToInt((pos.z - allFloorBounds[0].min.z) / normalPlateZ);
        plateNumberX = Math.Clamp(plateNumberX, 0, plateCountX - 1);
        plateNumberZ = Math.Clamp(plateNumberZ, 0, plateCountZ - 1);
        return allPlateArray![plateNumberX, plateNumberZ];
    }
    Vector2Int WorldPositionToPlateArrayPosition(Vector3 pos)
    {
        int plateNumberX = Mathf.FloorToInt((pos.x - allFloorBounds[0].min.x) / normalPlateX);
        int plateNumberZ = Mathf.FloorToInt((pos.z - allFloorBounds[0].min.z) / normalPlateZ);
        plateNumberX = Math.Clamp(plateNumberX, 0, plateCountX - 1);
        plateNumberZ = Math.Clamp(plateNumberZ, 0, plateCountZ - 1);
        return new Vector2Int(plateNumberX, plateNumberZ);
    }
    public Vector3 GetRandomExitPosition()
    {
        return allExits![UnityEngine.Random.Range(0, allExits.Length)].GetPosition();
    }
    public int GetGoalNodeCount()
    {
        return allGoalNodes.Count;
    }

    public void Moved(Vector3 from, Vector3 to)
    {
        hm!.Moved(from, to);
    }
    public void Spawned(Vector3 pos)
    {
        hm!.Spawned(pos);
    }
    public Queue<Bude> CalcNewWeightedBuden(int goalsBeforeExit)
    {
        Queue<Bude> returnBuden = new Queue<Bude>();
        List<Bude> allBuden = allBudenScripts!.ToList();
        int tmpWeight = allBudenWeigth;
        for (int i = 0; i < goalsBeforeExit; i++)
        {
            int rand = UnityEngine.Random.Range(0, allBudenWeigth);
            foreach (Bude bude in allBuden)
            {
                if (!returnBuden.Contains(bude))
                {
                    rand -= bude.attraktivitaet;
                    if (rand < 0) { allBuden.Remove(bude); tmpWeight -= bude.attraktivitaet; returnBuden.Enqueue(bude); break; }
                }
            }
        }
        return returnBuden;
    }
    public void CalcAllBudenWeight()
    {
        maxKapazitaet = 0;
        allBudenWeigth = 0;
        foreach (Bude b in allBudenScripts!)
        {
            if (b != null)
            {
                maxKapazitaet += b.kapazitaet;
                allBudenWeigth += b.attraktivitaet;
            }
        }
    }
}

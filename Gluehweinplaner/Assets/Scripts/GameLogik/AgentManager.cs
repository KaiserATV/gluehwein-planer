using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class AgentManager : MonoBehaviour
{
    public int playerCount = 0;
    public int inactivePlayerCount = 0;
    public int maxPlayerCount = 50;
    public int agentsLostPatience = 0;
    public int maxKapazitaet;

    public bool simulating = false;

    public string budenContainerName = "BudenContainer";
    public string exitContainerName = "ExitContainer";
    public string spawnerContainerName = "SpawnerContainer";
    public string heatmapname = "HeatMap";

    public int allBudenWeigth;

    private List<AgentController> alleCurrentAgents = new List<AgentController>();
    private LinkedList<int>leereStellen=new LinkedList<int>();
    Heatmap hm;
    InactiveAgentsContainer iac;

    public Vector2 cellsizes;

    public Buden[] alleBuden;
    Exits[] alleExits;
    CrowdGeneration[] spawner;

    [SerializeField] private AudioClip deleteSoundClip;
    [SerializeField] private AudioClip saveSoundClip;
    [SerializeField] private AudioClip loadSoundClip;


    // Start is called before the first frame update
    void Start()
    {
        alleBuden = GameObject.Find(budenContainerName).GetComponentsInChildren<Buden>();
        alleExits = GameObject.Find(exitContainerName).GetComponentsInChildren<Exits>();
        spawner = GameObject.Find(spawnerContainerName).GetComponentsInChildren<CrowdGeneration>();
        hm = GameObject.Find(heatmapname).GetComponentInChildren<Heatmap>();
        iac = GameObject.Find("InactiveAgentHolder").GetComponent<InactiveAgentsContainer>();

        CalcAllBudenWeight();

        cellsizes.x = hm.cellsizeX;
        cellsizes.y = hm.cellsizeZ;

    }
    public bool laden = false;
    public bool speichern = false;
    private void Update()
    {
        if (laden)
        {
            LoadBudenFromJSON();
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
            AgentController ac = alleCurrentAgents[0];
            ac.SetInactive();
            iac.AddAgent(ac);
            alleCurrentAgents.Remove(ac);
            playerCount--;
            inactivePlayerCount++;
        }
    }


    public int GetNewCoords(AgentController ac, List<int> besuchteBudenNr)
    {
        if (leereStellen.Count > 0) { besuchteBudenNr.AddRange(leereStellen);}
        int budenNummer;
        if (besuchteBudenNr.Count == alleBuden.Length) {
            return -1;
        }else{
            budenNummer = CalcNewWeightedBude(besuchteBudenNr);
        }
        if(budenNummer == -1)
        {
            return -1;
        }
        if (!alleBuden[budenNummer].CheckAuslastung())
        {
            alleBuden[budenNummer].GetNewPosition(ac);
            return budenNummer;
        }

        return -1;
    }

    public Vector3 GetIACPos() { return iac.GetWorldCoords(); }

    private int CalcNewWeightedBude(List<int> besuchteBudenNr)
    {
        int rand = Random.Range(0, allBudenWeigth+1);
        int bNr=-1;
        int tmpCount=0;
        for (int i = 0; i < alleBuden.Length; i++)
        {
            if (!besuchteBudenNr.Contains(i) && alleBuden[i]!=null)
            {
                bNr = i;
                tmpCount += alleBuden[i].attraktivitaet;
                if (tmpCount > rand) { return bNr; }
            }
        }
        return bNr;
    }

    public void CalcAllBudenWeight()
    {
        maxKapazitaet = 0;
        allBudenWeigth = 0;
        foreach(Buden b in alleBuden)
        {
            if(b != null)
            {
                maxKapazitaet += b.kapazität;
                allBudenWeigth += b.attraktivitaet;
            }
        }
    }

    public Vector3 GetClostestExit(Vector3 position)
    {
        Vector3 exitCoords = Vector3.zero;
        if(alleExits.Length > 0)
        {
            exitCoords = alleExits[0].GetClostestPoint();
            float currClostestDistance = Vector3.Distance(position,exitCoords);
            for (int i = 1; i < alleExits.Length; i++)
            {
                if (Vector3.Distance(position, alleExits[i].GetClostestPoint()) < currClostestDistance)
                {
                    exitCoords = alleExits[i].GetClostestPoint();
                    currClostestDistance = Vector3.Distance(position, exitCoords);
                }
            }
        }
        return new Vector3(exitCoords.x,exitCoords.z,0);
    }

    public float GetWaitTime(int budenNr)
    {
        return alleBuden[budenNr].waitTime;
    }

    public void addPlayer(AgentController ac){alleCurrentAgents.Add(ac); }
    public void removePlayer(AgentController ac){ playerCount--; alleCurrentAgents.Remove(ac); }

    public bool CanAddPlayer() { return (playerCount < maxPlayerCount); }
    public void StartSimulation() { simulating = true; }
    public void ResumeSimulation() {  simulating = true; foreach (AgentController ac in alleCurrentAgents) { ac.Resume(); } CalcAllBudenWeight(); }
    public void StopSimulation() { simulating = false; foreach (AgentController ac in alleCurrentAgents) { ac.Stop(); } }

    public void ResetSimulation()
    {
        foreach (AgentController ac in alleCurrentAgents)
        {
            ac.SetInactive();
            iac.AddAgent(ac);
        }
        foreach (Buden b in alleBuden) { if(b!=null){ b.Reset(); }}
        inactivePlayerCount = playerCount;
        playerCount = 0;
        agentsLostPatience = 0;
        simulating = false;
        leereStellen = new LinkedList<int>();
        alleCurrentAgents = new List<AgentController>();
        hm.Reset();
        CalcAllBudenWeight();
    }

    public void AddBude(Buden neueBude)
    {
        if (leereStellen.Count > 0)
        {
            neueBude.Start();
            alleBuden[leereStellen.First.Value] = neueBude;
            leereStellen.RemoveFirst();
        }
        else
        {
            neueBude.Start();
            List<Buden> tempList = alleBuden.ToList();
            tempList.Add(neueBude);
            alleBuden = tempList.ToArray();
        }
        CalcAllBudenWeight();
    }

    public void RemoveBude(Buden wegBude)
    {
        SoundFXManager.instance.PlaySoundFXClip(deleteSoundClip, transform, 1f);

        for(int i = 0; i <alleBuden.Length; i++)
        {
            if(alleBuden[i]== wegBude)
            {
                alleBuden[i] = null;
                leereStellen.AddFirst(i);
            }
        }
    }
    public void Pausieren()
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
        if(playerCount == 0)
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

    public Vector3 GetNewSpawnPoint()
    {
        return spawner[Random.Range(0, spawner.Length)].GenerateRandomPosition();
    }
    public int GetBudenCount() { return alleBuden.Length-leereStellen.Count; }
    public int ExitCount() { return alleExits.Length; }

    public Vector2Int UpdatePositionInGrid(Vector2Int from, Vector2 to)
    {
        return hm.Moved(from, to);
    }
    public Vector2Int UpdatePositionInGrid(Vector2 from)
    {
        return hm.Spawned(from);
    }
    public void ClearPosition(Vector2Int pos)
    {
        hm.ClearPos(pos);
    }

    public void LostPatience()
    {
        agentsLostPatience++;
    }

    private string CreateJSON()
    {
        AlleBudenJSON aB = new AlleBudenJSON(alleBuden.Length);
        AlleExitJSON aE = new AlleExitJSON(alleExits.Length);
        AlleSpawnJSON aS = new AlleSpawnJSON(spawner.Length);
        int j = 0;
        for (int i = 0; i < alleBuden.Length; i++)
        {
            if (alleBuden[i] != null)
            {
                aB.budenArray[j] = alleBuden[i].GetBudenJSON();
                j++;
            }
        }
        for (int i = 0; i < spawner.Length; i++)
        {
            aS.spawnArray[i] = spawner[i].ToJSON();
        }
        for (int i = 0; i < alleExits!.Length; i++)
        {
            aE.exitArray[i] = alleExits![i].ToJSON();
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

    private GanzeSzene ReadJSON()
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


    public void LoadBudenFromJSON()
    {
        GanzeSzene? gS = ReadJSON();
        GameObject? stand = Resources.Load("Stand") as GameObject;
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
                    Buden bd = newObj.GetComponent<Buden>();
                    newObj.transform.parent = budenContainer.transform;
                    bd.attraktivitaet = b.attrak;
                    bd.waitTime = b.waittime;
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


            //SoundFXManager.instance.PlaySoundFXClip(loadSoundClip, transform, 1f);
        }
        else
        {
            Debug.LogWarning("Konnte keine Datei lesen von pfad: " + Application.persistentDataPath + "/Position.json");
        }
    }
}
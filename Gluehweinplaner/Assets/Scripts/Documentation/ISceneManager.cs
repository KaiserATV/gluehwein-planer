using System.Collections.Generic;
using UnityEngine;

public interface ISceneManager
{
    /// <summary>
    /// This function adds a new Bude to the scene. And also calculates the goalnode as well as the occupied spaces.
    /// </summary>
    /// <param name="neueBude">The bude that has been added.</param>
    void AddBude(Bude neueBude);
    /// <summary>
    /// This function adds a to the allcontained player list and counts the playercount up.
    /// </summary>
    /// <param name="npc">The NPC added.</param>
    void AddPlayer(NPC npc);
    /// <summary>
    /// The Function is called when a Bude is beeing removed.
    /// </summary>
    /// <seealso cref="GoalNode.BudeMoved(Bude)"/>
    /// <seealso cref="Bude.BudeMoved"/>
    /// <param name="bude"></param>
    void BudeMoved(Bude bude);
    /// <summary>
    /// This function calculates the weight of all Buden contained in the scene. This weight is used to calculate the visited Bude after.
    /// </summary>
    void CalcAllBudenWeight();
    /// <summary>
    /// This Function calculates a List of Buden to visit based off of the weight of the individual bude aswell as the supplied amount of goals before exit.
    /// </summary>
    /// <param name="goalsBeforeExit">Amount of Buden to visit before exit.</param>
    /// <returns>The list of Buden to visit.</returns>
    Queue<Bude> CalcNewWeightedBuden(int goalsBeforeExit);
    /// <summary>
    /// This function checks if an Player can be added to the scene.
    /// </summary>
    /// <returns>True if a player can be added, false else.</returns>
    bool CanAddPlayer();
    /// <summary>
    /// Clears a specific position in the heatmap.
    /// </summary>
    /// <param name="pos">The position to be cleared.</param>
    void ClearPos(Vector3 pos);
    /// <summary>
    /// Decreases the maximum playercount.
    /// </summary>
    void DecreaseMaxPlayerCount();
    /// <summary>
    /// Finds an exisiting goalnode or creates a new goalnode for the supplied bude.
    /// </summary>
    /// <param name="bude">Bude for which the goalnode needs to be found.</param>
    void FindOrCreateNewGoalNode(Bude bude);
    /// <summary>
    /// Gets the amount of current goalNodes.
    /// </summary>
    /// <returns></returns>
    int GetGoalNodeCount();
    /// <summary>
    /// Gets a new spawnposition.
    /// </summary>
    /// <seealso cref="Spawner.GenerateRandomPosition"/>
    /// <returns>The new position as well as the center of the spawner.</returns>
    (Vector3, Vector3) GetNewSpawnPoint();
    /// <summary>
    /// Generates an random exit position. An random exit is choosen.
    /// </summary>
    /// <returns>The position of an randomly choosen exit.</returns>
    Vector3 GetRandomExitPosition();
    /// <summary>
    /// Function used to calculate the Path for a certain start and Goalnode. Firstly the plates to visit are calculated and thereafter the path through each one.
    /// </summary>
    /// <seealso cref="HandlePathRequest(Vector3, Vector3)"/>
    /// <param name="start">The Starting position.</param>
    /// <param name="goalNode">The ending position.</param>
    /// <returns>Returns a Queue of steps to take. This queue is empty if there is no path to be taken.</returns>
    Queue<Vector3> HandlePathRequest(Vector3 start, GoalNode goalNode);
    /// <summary>
    /// Function used to calculate the Path for a certain start and ending potiion. Firstly the plates to visit are calculated and thereafter the path through each one.
    /// </summary>
    /// <seealso cref="HandlePathRequest(Vector3, GoalNode)"/>
    /// <param name="start">The Starting position.</param>
    /// <param name="goal">The ending position.</param>
    /// <returns>Returns a Queue of steps to take. This queue is empty if there is no path to be taken.</returns>
    Queue<Vector3> HandlePathRequest(Vector3 start, Vector3 goal);
    /// <summary>
    /// This function incresese the maximum playercount.
    /// </summary>
    void IncreaseMaxPlayerCount();
    /// <summary>
    /// This function checks wether the first Vector is closer to the goal then the second Vector.
    /// </summary>
    /// <param name="currMin">The Vector, the second one is compared to.</param>
    /// <param name="toCheck">The second Vector2.</param>
    /// <param name="goal">The goal to which the distance is taken.</param>
    /// <returns>True if the first Vector is closer then the second.</returns>
    bool isCloser(Vector2Int currMin, Vector2Int toCheck, Vector2Int goal);
    /// <summary>
    /// This function loads the whole scene from a supplied json.
    /// </summary>
    void LoadSceneFromJSON();
    /// <summary>
    /// This function is called if an npc losses patience and searches for a new goal.
    /// </summary>
    void LostPatience();
    /// <summary>
    /// This function is called when a npc moves from one tile to another. This is propagated to the heatmap.
    /// </summary>
    /// <seealso cref="Heatmap.Moved(Vector3, Vector3)"/>
    /// <param name="from">The position the npc moved from.</param>
    /// <param name="to">The position the npc moved to.</param>
    void Moved(Vector3 from, Vector3 to);
    /// <summary>
    /// The function used to pause the simulation.
    /// </summary>
    /// <seealso cref="NPC.Stop()"/>
    void Pause();
    /// <summary>
    /// This function removes a bude from all buden in the scene. Also this function clears up all occupied spaces. 
    /// </summary>
    /// <seealso cref="Bude.BudeRemove()"/>
    /// <seealso cref="Plate.BudeRemoved(Bude)"/>
    /// <param name="wegBude">The bude to remove.</param>
    void RemoveBude(Bude wegBude);
    /// <summary>
    /// This function removes a goalnode from the scene if is not beeing needed anymore.
    /// </summary>
    /// <param name="gn">The goalnode to remove.</param>
    void RemoveGoalNode(GoalNode gn);
    /// <summary>
    /// This function removes a player from the scene.
    /// </summary>
    /// <param name="npc">The npc to remove.</param>
    void removePlayer(NPC npc);
    /// <summary>
    /// This function removes the spaces that the bude occupies, so that the agents don't path through it anymore.
    /// </summary>
    /// <param name="bude">The Bude for which the space should be occupied.</param>
    /// <param name="_start">The first corner from which the positon should occupy.</param>
    /// <param name="_end">The ending corner of the side from which the space should be occupied.</param>
    /// <returns>The List of plates in which this side of a bude occupies space.</returns>
    List<Plate> ReserveBudenPosition(Bude bude, Vector3 _start, Vector3 _end);
    /// <summary>
    /// Resets the simulation and all contained Buden as well as all agents.
    /// </summary>
    void ResetSimulation();
    /// <summary>
    /// Resumes the simulation if it was stopped before.
    /// </summary>
    void ResumeSimulation();
    /// <summary>
    /// Safes the current state in the scene into json
    /// </summary>
    void SaveJSON();
    /// <summary>
    /// Propagates the spawned position of an agent to the heatmap.
    /// </summary>
    /// <seealso cref="Heatmap.Spawned(Vector3)"/>
    /// <param name="pos">The position on which an agent spawned.</param>
    void Spawned(Vector3 pos);
    /// <summary>
    /// Starts the simulation.
    /// </summary>
    void StartSimulation();
    /// <summary>
    /// Stops the simulation.
    /// </summary>
    void StopSimulation();
    /// <summary>
    /// Toggles the Simulation between running and stopped.
    /// </summary>
    void ToggleSimulation();
    /// <summary>
    /// Converts a worldposition position to an specific plate.
    /// </summary>
    /// <param name="pos">The worldposition to chekc where it is.</param>
    /// <returns>The plate in which the worldcoordinates are.</returns>
    Plate WorldPositionToPlate(Vector3 pos);
}
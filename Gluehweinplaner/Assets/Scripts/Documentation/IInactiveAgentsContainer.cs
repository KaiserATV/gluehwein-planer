using UnityEngine;
/// <summary>
/// This Script is called when there are to many agents in the scene and need to be stored in the container. As well as when new players are spawned and there might be some 
/// left over in the in active container to be spawned.
/// </summary>
/// <seealso cref="ISpawner"/>
public interface IInactiveAgentsContainer
{
    /// <summary>
    /// Adds an Agent to the Inactiveagentscontainer.
    /// </summary>
    /// <param name="npc">The Agent to add</param>
    void AddAgent(NPC npc);
    /// <summary>
    /// Gets the first stored agent.
    /// </summary>
    /// <returns>The first stored agent(npc).</returns>
    NPC GetAgent();
    /// <summary>
    /// This function gets the amount of stored agents.
    /// </summary>
    /// <returns>Number of stored agents.</returns>
    int GetStoredCount();
    /// <summary>
    /// Gets the worldcoordinates of the inactive agentcontainer.
    /// </summary>
    /// <returns>The worldcoordinates of the iac.</returns>
    Vector3 GetWorldCoords();
}
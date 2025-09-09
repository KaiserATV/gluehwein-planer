using UnityEngine;
/// <summary>
/// This script manages the movement of the npc.
/// </summary>
/// <seealso cref="ISpawner"/>
/// <seealso cref="ISceneManager"/>
public interface INPC
{
    /// <summary>
    /// The function gets called if a bude to whichs goalnode the agent currently is pathing is destroyed. Causeing the agent to repath to a new goal.
    /// <param name="movedBude">The Bude that moved.</param>
    void BudeDestroyed(Bude movedBude);
    /// <summary>
    /// The function gets called if a bude to whichs goalnode the agent currently is pathing is moved. Causeing the agent to repath to the new position. 
    /// </summary>
    /// <param name="movedBude">The bude beeng moved.</param>
    void BudeMoved(Bude movedBude);
    /// <summary>
    /// The function to get the position of the agent.
    /// </summary>
    /// <returns>The Position of the agent.</returns>
    Vector3 GetPosition();
    /// <summary>
    /// The function beeing called if the agent is beeing respawned. Resetting all values and spawning from an new positio
    /// </summary>
    void Respawn();
    /// <summary>
    /// The function resumes the movement of the agent, if the agent was paused bevore
    /// </summary>
    void Resume();
    /// <summary>
    /// Sets the agents as inactive and moves it to the inactive container. Agent no more moves
    /// </summary>
    /// <param name="inactivePostion"></param>
    void SetInactive(Vector3 inactivePostion);
    /// <summary>
    /// Stops the agent. If the scene is paused.
    /// </summary>
    void Stop();
}
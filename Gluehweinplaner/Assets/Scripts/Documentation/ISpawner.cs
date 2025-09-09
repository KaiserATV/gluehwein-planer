using UnityEngine;
/// <summary>
/// This script represents the state of an spawner.
/// </summary>
/// <seealso cref="IInactiveAgentsContainer"/>
/// <seealso cref="INPC"/>
/// <seealso cref="ISceneManager"/>
public interface ISpawner
{
    /// <summary>
    /// Generates a new random spawnposition. - currently only the center of the of the spawner because of a bug with Physics.CheckSphere
    /// </summary>
    /// <returns>The spawnposition and the center of the spawner.</returns>
    (Vector3, Vector3) GenerateRandomPosition();
    /// <summary>
    /// Converts the current state of the spawner to the spawnerJSON object.
    /// </summary>
    /// <returns>The representative SpawnJSON object..</returns>
    SpawnJSON ToJSON();
}
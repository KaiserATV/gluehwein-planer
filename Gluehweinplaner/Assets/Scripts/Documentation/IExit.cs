using UnityEngine;
/// <summary>
/// This is the class that is used to represent the state of an exit internally.
/// </summary>
/// <seealso cref="ISceneManager"/>
/// <seealso cref="ISpawner"/>
public interface IExit
{
    /// <summary>
    /// Gets the position of the exit.
    /// </summary>
    /// <returns>The Vector3 position of the exit.</returns>
    Vector3 GetPosition();
    /// <summary>
    /// Converts the current state of the exit to the ExitJSON object.
    /// </summary>
    /// <returns>Exitjson object.</returns>
    ExitJSON ToJSON();
}
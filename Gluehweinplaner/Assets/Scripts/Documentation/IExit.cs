using UnityEngine;

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
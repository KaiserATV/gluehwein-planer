using UnityEngine;

public interface IHeatmap
{
    /// <summary>
    /// Subtracts one from a position in the current list of positions.
    /// </summary>
    /// <param name="pos">The position where it should be subtracted.</param>
    void ClearPos(Vector3 pos);
    /// <summary>
    /// Determines the Color, or alpha, determined by he usage- anount of agents at a position.
    /// </summary>
    /// <param name="usage">The amount of agents at a position.</param>
    /// <returns>The alpha value corresponding to the agentcount.</returns>
    float determineAlpha(int usage);
    /// <summary>
    /// Moves the value from one position to another. This happens when an agents moves.
    /// </summary>
    /// <param name="from">The position from where an agent moved.</param>
    /// <param name="to">The position to which an agent moved.</param>
    void Moved(Vector3 from, Vector3 to);
    /// <summary>
    /// Resets all values in the heatmap.
    /// </summary>
    void Reset();
    /// <summary>
    /// Thats into account when an agent spawns at an poition in the heatmap.
    /// </summary>
    /// <param name="spawnedPos">The position at which the agent spawned.</param>
    void Spawned(Vector3 spawnedPos);
    /// <summary>
    /// Initializes all fields.
    /// </summary>
    void Start();
    /// <summary>
    /// Toggles the AlphaMode between Max, Current and Clear.
    /// </summary>
    void ToggleAlphaMode();
}
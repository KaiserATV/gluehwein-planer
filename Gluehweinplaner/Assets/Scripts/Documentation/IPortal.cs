using UnityEngine;
/// <summary>
/// This script represents a Portal and its state with all flowfields and plates.
/// </summary>
public interface IPortal
{
    /// <summary>
    /// Function to initialize the generation of the flowfield to a certain Exit/Portal.
    /// </summary>
    /// <param name="plate">The Plate of whichs basecostmatrix the Flowfield is generated.</param>
    void GenerateFlowField(Plate plate);
    /// <summary>
    /// Function to get the closest Point of exitpoints in the portal to a start point.
    /// </summary>
    /// <param name="start">The start point from which the closest point is calculated.</param>
    /// <returns>The closest point.</returns>
    Vector2Int GetClostestToStart(Vector2Int start);
}
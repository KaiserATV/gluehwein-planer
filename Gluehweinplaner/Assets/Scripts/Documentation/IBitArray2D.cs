using UnityEngine;
/// <summary>
/// This Array is a bit array representing the waitingarea. This waitingarea manages the occupied positions and can return the waitingposition in realworld coordinates.
/// </summary>
/// <seealso cref="IBude"/>
public interface IBitArray2D
{
    /// <summary>
    /// Finds the best poition to add an player and adds it there.
    /// </summary>
    /// <returns>The worldcoordinates of the position and the Waiting spot in the array.</returns>
    (Vector3, Vector2Int) FindBestPositionAndAdd();
    /// <summary>
    /// Gets the total capacity of the waiting area.
    /// </summary>
    /// <returns>The total capacity of the waiting Area.</returns>
    int GetKapa();
    /// <summary>
    /// This Function returns the worldcoordinates of an provided waiting spot provided by the waiting area in the grid.
    /// </summary>
    /// <param name="cells">The waiting position within the grid.</param>
    /// <returns>The realdworld coordinates.</returns>
    Vector3 GetRealWorldCords(Vector2Int cells);
    /// <summary>
    /// This function checks if there is no more space to wait at in the waiting area.
    /// </summary>
    /// <returns>True if the waiting area is full and no spot to wait are left.</returns>
    bool IsFull();
    /// <summary>
    /// Function to remove a player from the waiting area and free up the spot.
    /// </summary>
    /// <param name="pos">Position to free up.</param>
    void RemovePlayer(Vector2Int pos);
    /// <summary>
    /// Resets all values to default.
    /// </summary>
    void Reset();
}
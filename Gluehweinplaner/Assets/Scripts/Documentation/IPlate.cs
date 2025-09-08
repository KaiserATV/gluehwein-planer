using System.Collections.Generic;
using UnityEngine;

public interface IPlate
{
    /// <summary>
    /// The Basecostmatrix of the plate.
    /// </summary>
    /// <seealso cref="GenerateMatrix.GenerateBaseCostMatrix(int, int, System.Func{int, int, bool}, out bool, out bool)"/>
    int[,] BaseCostMatrix { get; set; }
    /// <summary>
    /// The Center of the plate in realworld coordinates.
    /// </summary>
    Vector3 Center { get; set; }
    /// <summary>
    /// The number of columns the plate has.
    /// </summary>
    int Columns { get; set; }
    /// <summary>
    /// The number of rows the plate has.
    /// </summary>
    int Rows { get; set; }
    /// <summary>
    /// The Size of the plate in each direction.
    /// </summary>
    Vector3 Size { get; set; }
    /// <summary>
    /// Function used to add an goalnode to the plate which is on the plate. Also adds the flowfield corresponding goal node to the dictonary.
    /// </summary>
    /// <param name="goalNode"></param>
    void AddGoalNode(GoalNode goalNode);
    /// <summary>
    /// Function to remove an Bude from the plate.
    /// </summary>
    /// <param name="bude">The bude to be removed.</param>
    void BudeRemoved(Bude bude);
    /// <summary>
    /// Function used to Calulate ever Portal on the Plate.
    /// </summary>
    void CalculatePortalNodes();
    /// <summary>
    /// Function to Check for a Portal on specific side of an plate specified by the provided direction.
    /// </summary>
    /// <param name="dir">Direction on which the sides to be calculated.</param>
    void CheckForPortalNodes(ExitDirection.ExitDirections dir);
    /// <summary>
    /// Returns the Vector2Int which is closer to the goal.
    /// </summary>
    /// <param name="a">The first Vector2Int to compare.</param>
    /// <param name="b">The second Vector2Int to compare.</param>
    /// <param name="exit">The Vector from which the distance is calculated.</param>
    /// <returns>The closer Vector2Int.</returns>
    Vector2Int CloserVector2IntToGoal(Vector2Int a, Vector2Int b, Vector3 exit);
    /// <summary>
    /// Checks if a Vector2Int is contained on an plate.
    /// </summary>
    /// <param name="check">The Vector2Int to check.</param>
    /// <returns>true if contained, false else</returns>
    bool Contains(Vector2Int check);
    /// <summary>
    /// Returns the Value in the Basecostmatrix at the specific position.
    /// </summary>
    /// <seealso cref="GetBaseValueAtPosition(Vector3, bool)"/>
    /// <param name="row">The row in which the tile is.</param>
    /// <param name="column">The column in which the tile is.</param>
    /// <returns>The value of the Basecostmatrix at the position.</returns>
    int GetBaseValueAtPosition(int row, int column);
    /// <summary>
    /// Returns the Value in the Basecostmatrix at the specific position.
    /// </summary>
    /// <seealso cref="GetBaseValueAtPosition(int, int)"/>
    /// <param name="position">The position to be checked.</param>
    /// <param name="safe">Wether or not the Value is only considered within the bounds.</param>
    /// <returns>The value of the Basecostmatrix at the position.</returns>
    int GetBaseValueAtPosition(Vector3 position, bool safe);
    /// <summary>
    /// Returns the closest Portal to an specific goal in an certain direction.
    /// </summary>
    /// <param name="goal">The goal to which the Portalposition is compared.</param>
    /// <param name="exit">The exitdirection of which the portals are considered.</param>
    /// <returns>The Closest Portal. Null if there is no Portal in that direction.</returns>
    Portal GetClostestPortal(Vector3 goal, ExitDirection.ExitDirections exit);
    /// <summary>
    /// Converts an Vector3 to an Vector2Int that represents the tile within the plate.
    /// </summary>
    /// <param name="positionVector3">The position to convert.</param>
    /// <param name="safe">Wether the values should be clamped to values that are contained in the plate.</param>
    /// <returns>Tile/Array position of the vector3.</returns>
    Vector2Int GetPositionInArray(Vector3 positionVector3, bool safe);
    /// <summary>
    /// Returns a Path of Vector3 which represent a shortest Path from a starting position within the plate to a goal within a plate.
    /// </summary>
    /// <param name="_start">The starting position.</param>
    /// <param name="goal">The end position.</param>
    /// <returns>The path taken in realworld coordinates.</returns>
    List<Vector3> GetShortestPathToGoalWithin(Vector3 _start, Vector3 goal);
    /// <summary>
    /// Retunrs the shortest Path to an exitpoint.
    /// </summary>
    /// <seealso cref="GetShortestPathToToNextPlateV3(Portal, Vector3)"/>
    /// <param name="portal">The portal to take to the next plate.</param>
    /// <param name="startArray">The starting position within the array.</param>
    /// <returns>The steps to be taken in Vector2Int.</returns>
    List<Vector2Int> GetShortestPathToToNextPlateV2(Portal portal, Vector2Int startArray);
    /// <summary>
    /// Retunrs the shortest Path to an exitpoint.
    /// </summary>
    /// <seealso cref="GetShortestPathToToNextPlateV3(Portal, Vector3)"/>
    /// <param name="portal">The portal to take to the next plate.</param>
    /// <param name="startArray">The starting position within the array.</param>
    /// <returns>The steps to be taken in realworldcoordinates.</returns>
    List<Vector3> GetShortestPathToToNextPlateV3(Portal portal, Vector3 start);
    /// <summary>
    /// The function converts an position specifided by rowposition and columnposition within the plate to realworldcoordinates in relation to the plate.
    /// </summary>
    /// <seealso cref="GetSubTileCenterWorldCoordinates(Vector2Int)"/>
    /// <param name="rows">The row position of the value.</param>
    /// <param name="cols">The column position of the value.</param>
    /// <returns>The realworld coordinates of the specified position in relation to the plate.</returns>
    Vector3 GetSubTileCenterWorldCoordinates(int rows, int cols);
    /// <summary>
    /// The function converts an position of Vector2Int within the plate to realworldcoordinates in relation to the plate.
    /// </summary>
    /// <seealso cref="GetSubTileCenterWorldCoordinates(int, int)"/>
    /// <param name="rows">The row position of the value.</param>
    /// <param name="cols">The column position of the value.</param>
    /// <returns>The realworld coordinates of the specified position in relation to the plate.</returns>
    Vector3 GetSubTileCenterWorldCoordinates(Vector2Int pos);
    /// <summary>
    /// The function to change the positions occupied by a bude on the plate in the basecostmatrix to not pathable.
    /// </summary>
    /// <param name="bude">The bude from which the positions are taken.</param>
    /// <param name="start">The Starting position of the corner in the tiles.</param>
    /// <param name="exitWorld">The exitposition in realworldposition.</param>
    /// <returns>The position which is the last occupied space on the plate.</returns>
    Vector2Int OccupySpaces(Bude bude, Vector2Int start, Vector3 exitWorld);
    /// <summary>
    /// Function to recalculate wether an plate can be walked on. Specificly recalculates HasOnlyObstacles and HasNoObstcles.
    /// </summary>
    void RecalcWakable();
}
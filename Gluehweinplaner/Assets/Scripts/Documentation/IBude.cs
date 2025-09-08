using System.Collections.Generic;
using UnityEngine;

public interface IBude
{
    /// <summary>
    /// The Function called if the Bude is beeing moved.
    /// </summary>
    /// <seealso cref="GoalNode.BudeMoved(Bude)"/>
    void BudeMoved();
    /// <summary>
    /// The Function called if the Bude is beeing removed.
    /// </summary>
    /// <seealso cref="GoalNode.RemoveBude(Bude)"/>
    void BudeRemove();
    /// <summary>
    /// This function checks if the Bude is currently completly occupied and there are no more places to for agents to visit.
    /// </summary>
    /// <returns>True if Bude is completly occupied, else false.</returns>
    bool CheckOccupation();
    /// <summary>
    /// Decreases the attractiveness of the bude. Resulting in lessend likelihood of beeing visited by an agent.
    /// </summary>
    void decreaseAttractivness();
    /// <summary>
    /// Decreases the waittime. Agents wait less infront of the Bude.
    /// </summary>
    void decreaseWaittime();
    /// <summary>
    /// Gets all Corners of the Buden transform in order:
    /// Top left, Top right, Bottom Left, Bottom Right
    /// </summary>
    /// <returns>List with the locations of all corners</returns>
    List<Vector3> GetAllCornerPoints();
    /// <summary>
    /// Function used to create the JSON File representing the Bude.
    /// </summary>
    /// <returns>An BudenJSON object representing the current state of the Bude.</returns>
    BudenJSON GetBudenJSON();
    /// <summary>
    /// Function to get the Factiong Direction of the Bude.
    /// </summary>
    /// <returns>An Vector3 with the Direction in which the Bude is facing.</returns>
    Vector3 GetFacingDirection();
    /// <summary>
    /// The point which is opposite to the main bude at the end of the "ziel" area. This point is located in the mid of the ziel area.
    /// </summary>
    /// <returns>Returns the point which is is furthes from the Bude.</returns>
    Vector3 GetFarestPoint();
    /// <summary>
    /// This function gets a new waiting position in on of the waiting areas.
    /// </summary>
    /// <returns>The waiting Position. An Vector3 specifying the waitingarea and the position in the grid within. Null if there is no space left to wait at.</returns>
    (Vector3?, Vector3Int?) GetNewPosition();
    /// <summary>
    /// This Function gets the location of the Bude.
    /// </summary>
    /// <returns>The position of the Bude.</returns>
    Vector3 GetPosition();
    /// <summary>
    /// Increases the attractiveness of the bude. Resulting in higher likelihood of beeing visited by an agent. 
    /// </summary>
    void increaseAttractivness();
    /// <summary>
    /// Increases the waittime. Agents wait longer infront of the Bude.
    /// </summary>
    void increaseWaittime();
    /// <summary>
    /// Removes the player form the specific position at the waiting area.
    /// </summary>
    /// <param name="pos"></param>
    void RemovePlayer(Vector3Int pos);
    /// <summary>
    /// Resets the values of the Bude
    /// </summary>
    void Reset();
    /// <summary>
    /// Sets the Type of bude. Currently only one Model of Bude is in use
    /// </summary>
    /// <param name="i"></param>
    void SetTypeIndex(int i);
    /// <summary>
    /// Start function called to determine the position of the Bude.
    /// </summary>
    void Start();
}
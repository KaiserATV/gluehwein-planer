public interface IGoalNode
{
    /// <summary>
    /// The Plate on which the goalnode currently is placed. 
    /// </summary>
    Plate OnPlate { get; set; }
    /// <summary>
    /// Function to add a Goal to an GoalNode.
    /// </summary>
    /// <param name="bude"> The Bude to be added to the line of sight of a goalnode</param>
    void AddBude(Bude bude);
    /// <summary>
    /// The function used to register a NPC with the goalnode. The NPC must be registered with the 
    /// goalnode they are visiting to warrent notification if a Bude contained in the goalnode moves.
    /// </summary>
    /// <param name="npc">The NPC to be registered.</param>
    void UsingGoalnodeAdd(NPC npc);
    /// <summary>
    /// The Function gets called when a Bude is beeing destroyed. This Function notifies all NPCs currently
    /// registered with a goalnode that they should repath.
    /// </summary>
    /// <param name="bude">The Bude beeing destroyed.</param>
    void BudeDestroyed(Bude bude);
    /// <summary>
    /// The Function beeing called when a Bude gets moved. This Function notifies all NPCs currently
    /// registered with a goalnode that they should repath.
    /// </summary>
    /// <param name="bude">The Bude beeing moved.</param>
    void BudeMoved(Bude bude);
    /// <summary>
    /// The Function to calculate the Position of the goalnode. This function takes into account every position
    /// of the contained Bude.It tries to find a point in the middle, where is assumed that the point has line
    /// of sight to a Bude.
    /// </summary>
    void CalculatePosition();
    /// <summary>
    /// The function used to remove a Bude from the list with lineofsight to. Should only be used in certain
    /// situations. Use BudeMoved() or BudeDestroyed() instead.
    /// </summary>
    /// <param name="bude">The Bude to be Removed</param>
    void RemoveBude(Bude bude);
    /// <summary>
    /// Function used to unregister a npc with the goalnode. Should be called, if they are exiting or on the way
    /// to a new goalnode.
    /// </summary>
    /// <param name="npc"></param>
    void RemoveNPC(NPC npc);
}
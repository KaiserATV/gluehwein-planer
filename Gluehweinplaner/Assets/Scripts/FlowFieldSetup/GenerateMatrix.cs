using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#nullable enable

public static class GenerateMatrix
{
    /// <summary>
    /// The height of the row.
    /// </summary>
    public static float TileSizeX = 1f;
    /// <summary>
    /// The width of the column.
    /// </summary>
    public static float TileSizeZ = 1f;
    /// <summary>
    /// Value that is used to represent an obstacle in the Basecostmatrix.
    /// </summary>
    public static int MatrixObstacleValue = 999;
    /// <summary>
    /// Value that is used to represent an wakable path in the Basecostmatrix.
    /// </summary>
    public static int MatrixIsPathableValue = 1001;
    /// <summary>
    /// The Layer representing the not wakable path.
    /// </summary>
    public static int ObstacleLayer = LayerMask.GetMask("nichtWakable");


    //The base cost matrix contains, the base values of the costs to travel to a certain square in the tile. it is at first only boolean, walkable or not, where 1 represents wakable and 256 not wakable
    /// <summary>
    /// Function generating the Basecostmatrix. The Function loops over all the tiles provided and calls the "isPathable()" function to determine which value the tile should have.
    /// </summary>
    /// <param name="rowCount">The amount of rows the Basecostmatrix should have.</param>
    /// <param name="colCount">The amount of columns the Basecostmatrix should have.</param>
    /// <param name="isPathable">The function used to determine wether or not an tile is wakable.</param>
    /// <param name="onlyObstacles">True if there are only obstacles in the resulting Basecostmatrix.</param>
    /// <param name="noObstacles">True if there are no obstacles in the resulting Basecostmatrix.</param>
    /// <returns>The Basecostmatrix with the specified Rows and Columns.</returns>
    public static int[,] GenerateBaseCostMatrix(int rowCount, int colCount, Func<int, int, bool> isPathable, out bool onlyObstacles, out bool noObstacles)
    {
        int[,] baseCostHolder = new int[rowCount, colCount];

        int clearTiles = 0;
        int obstacleTiles = 0;

        //Static/Clear Cost Field mit nur einsen hier einbiden für bessere Memory
        for (int row = 0; row < rowCount; row++)
        {
            for (int column = 0; column < colCount; column++)
            {
                if (isPathable(row, column))
                {
                    baseCostHolder[row, column] = MatrixIsPathableValue;
                    clearTiles++;
                }
                else
                {
                    baseCostHolder[row, column] = MatrixObstacleValue;
                    obstacleTiles++;
                }
            }
        }
        if (clearTiles == rowCount * colCount)
        {
            noObstacles = true;
        }
        else
        {
            noObstacles = false;
        }
        if (obstacleTiles == rowCount * colCount)
        {
            onlyObstacles = true;
        }
        else
        {
            onlyObstacles = false;
        }
        return baseCostHolder;
    }
    /// <summary>
    /// Function used to determine the resulting distancefield and flowfield.
    /// </summary>
    /// <seealso cref="GenerateDistanceFieldAndFlowField(int[,], int, int, List{Vector2Int}, bool)"/>
    /// <param name="plate">The Plate for which the fields are to be calculated.</param>
    /// <param name="start">The starting point so calulate the distance from.</param>
    /// <param name="canPathDiagonal">Wether or not the agents can path diagonal.</param>
    /// <returns>The distancefield and the flowfield.</returns>
    public static (int[,], byte[,]) GenerateDistanceFieldAndFlowField(Plate plate, Vector3 start, bool canPathDiagonal)
    {
        List<Vector2Int> startPositions = new List<Vector2Int> { plate.GetPositionInArray(start, false) };
        return GenerateDistanceFieldAndFlowField(plate.BaseCostMatrix, plate.Rows, plate.Columns, startPositions, canPathDiagonal);
    }
    /// <summary>
    /// The function works like a wavefront moving away from the starting position(s). Counting up the distance from the starting field for the distancematrix.
    /// It stores the byte corresponding to the direction it moved from the last position to the new position for the flowfield.
    /// </summary>
    /// <seealso cref="GenerateDistanceFieldAndFlowField(Plate, Vector3, bool)"/>
    /// <param name="baseCost">The Basecostmatrix used to calculate the resulting distance and flowfield.</param>
    /// <param name="rows">The number of rows.</param>
    /// <param name="cols">The number of columns.</param>
    /// <param name="startPositions">The List of starting positions from which the fields are calculated.</param>
    /// <param name="canPathDiagonal">Wether or not the agent can path diagonal.</param>
    /// <returns>The distancefield and the flowfield.</returns>
    public static (int[,], byte[,]) GenerateDistanceFieldAndFlowField(int[,] baseCost, int rows, int cols, List<Vector2Int> startPositions, bool canPathDiagonal)
    {
        int[,] distanceMatrix = (int[,])baseCost.Clone();
        byte[,] returnField = new byte[rows, cols];
        Queue<Vector2Int> nextNodeToBeExpanded = new Queue<Vector2Int>();
        foreach (Vector2Int pos in startPositions)
        {
            nextNodeToBeExpanded.Enqueue(pos);
            distanceMatrix[pos.x, pos.y] = 0;
            returnField[pos.x, pos.y] = ExitDirection.IsExit;
        }
        do
        {
            Vector2Int node = nextNodeToBeExpanded.Dequeue();
            if (rows - node.x > 1)
            {
                Vector2Int nodeToBeChecked = new Vector2Int(node.x + 1, node.y);
                if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                {
                    distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                    nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                    returnField[nodeToBeChecked.x, nodeToBeChecked.y] = ExitDirection.VectorToByte(new(-1, 0));
                }
            }
            if (node.x != 0)
            {
                Vector2Int nodeToBeChecked = new Vector2Int(node.x - 1, node.y);
                if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                {
                    distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                    nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                    returnField[nodeToBeChecked.x, nodeToBeChecked.y] = ExitDirection.VectorToByte(new(+1, 0));
                }
            }

            if (node.y != 0)
            {
                Vector2Int nodeToBeChecked = new Vector2Int(node.x, node.y - 1);
                if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                {
                    distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                    nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                    returnField[nodeToBeChecked.x, nodeToBeChecked.y] = ExitDirection.VectorToByte(new(0, 1));
                }
                if (canPathDiagonal)
                {
                    if (node.x > 0)
                    {
                        nodeToBeChecked = new Vector2Int(node.x - 1, node.y - 1);
                        if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                        {
                            distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                            nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                            returnField[nodeToBeChecked.x, nodeToBeChecked.y] = ExitDirection.VectorToByte(new(1, 1));
                        }
                    }
                    if (rows - node.x > 1)
                    {
                        nodeToBeChecked = new Vector2Int(node.x + 1, node.y - 1);
                        if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                        {
                            distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                            nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                            returnField[nodeToBeChecked.x, nodeToBeChecked.y] = ExitDirection.VectorToByte(new(-1, 1));
                        }
                    }
                }
            }
            if (cols - node.y > 1)
            {

                Vector2Int nodeToBeChecked = new Vector2Int(node.x, node.y + 1);
                if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                {
                    distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                    nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                    returnField[nodeToBeChecked.x, nodeToBeChecked.y] = ExitDirection.VectorToByte(new(0, -1));
                }
                if (canPathDiagonal)
                {
                    if (node.x > 0)
                    {
                        nodeToBeChecked = new Vector2Int(node.x - 1, node.y + 1);
                        if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                        {
                            distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                            nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                            returnField[nodeToBeChecked.x, nodeToBeChecked.y] = ExitDirection.VectorToByte(new(1, -1));
                        }
                    }
                    if (rows - node.x > 1)
                    {
                        nodeToBeChecked = new Vector2Int(node.x + 1, node.y + 1);
                        if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                        {
                            distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                            nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                            returnField[nodeToBeChecked.x, nodeToBeChecked.y] = ExitDirection.VectorToByte(new(-1, -1));
                        }
                    }
                }
            }
        } while (nextNodeToBeExpanded.Count != 0);
        return (distanceMatrix, returnField);
    }
    /// <summary>
    /// Function used to determine the clostest Vector2Int from a list of positions to a provieded goal Vector2Int. 
    /// </summary>
    /// <param name="list">Amount of positions to consider.</param>
    /// <param name="point">Point to which the closest position needs to be found.</param>
    /// <returns>Return the closest Vector2Int. to the provided point.</returns>
    public static Vector2Int GetClostestV2(List<Vector2Int> list, Vector2Int point)
    {
        float distance = float.MaxValue;
        Vector2Int clostestPoint = new(-1, -1);
        foreach (Vector2Int potential in list)
        {
            if (Vector2Int.Distance(potential, point) < distance)
            {
                clostestPoint = potential;
                distance = Vector2Int.Distance(potential, point);
            }
        }
        return clostestPoint;
    }
    /// <summary>
    /// Function used to interpolate between the two points within a grid. The Algortihm used is a line-drawing algoithm provided by https://www.redblobgames.com/grids/line-drawing/ .
    /// </summary>
    /// <param name="start">The starting point of the interpolation.</param>
    /// <param name="goal">The goalpoint of the interpolation</param>
    /// <param name="decideNext">Function to decide bewtween two points which should be used to interpolate the next step.</param>
    /// <param name="canPathDiagonal">Wether or not the agents can path diagonal.</param>
    /// <param name="rows">The number of rows.</param>
    /// <param name="cols">The number of columns.</param>
    /// <returns>A List of Vecto2Ints which represent the steps taken to interpolate between the two points inside the grid.</returns>
    public static List<Vector2Int> InterpolateArray(Vector2Int start, Vector2Int goal, Func<(Vector2Int, Vector2Int), Vector2Int> decideNext, bool canPathDiagonal, int rows, int cols)
    {
        if (start == goal)
        {
            return new List<Vector2Int> { start };
        }
        int dx = goal.x - start.x, dy = goal.y - start.y;
        int nx = Math.Abs(dx), ny = Math.Abs(dy);
        int sign_x = dx > 0 ? 1 : -1, sign_y = dy > 0 ? 1 : -1;
        List<Vector2Int> points = new List<Vector2Int> { };
        if (canPathDiagonal)
        {

            for (int ix = 0, iy = 0; ix < nx || iy < ny;)
            {
                points.Add(start);
                float decision = (1 + 2 * ix) * ny - (1 + 2 * iy) * nx;
                if (decision == 0)
                {
                    // next step is canPathDiagonal
                    if (start.x + sign_x >= 0 && start.x + sign_x < rows && start.y + sign_y >= 0 && start.y + sign_y < cols)
                    {
                        start.x += sign_x;
                        start.y += sign_y;
                    }
                    else
                    {
                        points.Add(goal);
                        return points;
                    }
                    ix++;
                    iy++;
                }
                else if (decision < 0)
                {
                    // next step is horizontal
                    if (start.x + sign_x >= 0 && start.x + sign_x < rows)
                    {
                        start.x += sign_x;
                    }
                    else
                    {
                        points.Add(goal);
                        return points;
                    }
                    ix++;
                }
                else
                {
                    // next step is vertical
                    if (start.y + sign_y >= 0 && start.y + sign_y < cols)
                    {
                        start.y += sign_y;
                    }
                    else
                    {
                        points.Add(goal);
                        return points;
                    }
                    iy++;
                }
            }
            points.Add(goal);
            return points;
        }
        else
        {
            Vector2Int diffToBorder;
            if (dx > 0)
            {
                if (dy > 0)
                {
                    diffToBorder = new Vector2Int(rows - 1, cols - 1) - start;
                }
                else
                {
                    diffToBorder = new Vector2Int(rows - 1, 0) - start;
                }
            }
            else
            {
                if (dy > 0)
                {
                    diffToBorder = new Vector2Int(0, cols - 1) - start;
                }
                else
                {
                    diffToBorder = new Vector2Int(0, 0) - start;
                }
            }
            diffToBorder.x = Math.Abs(diffToBorder.x);
            diffToBorder.y = Math.Abs(diffToBorder.y);
            points.Add(start);
            Vector2 p = new Vector2(start.x, start.y);
            for (int ix = 0, iy = 0; ix < nx || iy < ny;)
            {
                if ((0.5 + ix) / nx < (0.5 + iy) / ny)
                {
                    // next step is horizontal
                    if (diffToBorder.x > 0)
                    {
                        p.x += sign_x;
                        ix++;
                        diffToBorder.x--;
                    }
                    else
                    {
                        return points;
                    }
                }
                else if ((0.5 + ix) / nx == (0.5 + iy) / ny)
                {
                    Vector2Int next = decideNext((new Vector2Int(Mathf.FloorToInt(p.x + sign_x), Mathf.FloorToInt(p.y)), new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y + sign_y))));
                    if (next == new Vector2Int(Mathf.FloorToInt(p.x + sign_x), Mathf.FloorToInt(p.y)))
                    {
                        if (diffToBorder.x > 0)
                        {
                            p.x += sign_x;
                            ix++;
                            diffToBorder.x--;
                        }
                        else
                        {
                            return points;
                        }
                    }
                    else
                    {
                        if (diffToBorder.y > 0)
                        {
                            // next step is vertical
                            p.y += sign_y;
                            iy++;
                            diffToBorder.y--;
                        }
                        else
                        {
                            return points;
                        }
                    }
                }
                else
                {
                    if (diffToBorder.y > 0)
                    {
                        // next step is vertical
                        p.y += sign_y;
                        iy++;
                        diffToBorder.y--;
                    }
                    else
                    {
                        return points;
                    }

                }
                points.Add(new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y)));
            }
            return points;
        }
    }
    /// <summary>
    /// The function used to find the best Vector3 point to an next plate within the provieded exitdirection.
    /// </summary>
    /// <seealso cref="FindBestPointToNextArrayAndGoal(Vector3, ExitDirection.ExitDirections, Plate, Plate)"/>
    /// <param name="goal">The position to find the best exit to.</param>
    /// <param name="exitDirection">The direction in which the exit position are to be considered.</param>
    /// <param name="homePlate">The Plate on which the exit positions are to be considered.</param>
    /// <param name="neighborPlate">The plate neighboring the plate on which the exits are considered.</param>
    /// <returns>Null if there is no valid exit. A Vector3 if there is an valid exit.</returns>
    public static Vector3? FindBestPointToNextArrayAndGoalV3(Vector3 goal, ExitDirection.ExitDirections exitDirection, Plate homePlate, Plate neighborPlate)
    {
        Vector2Int? ret = null;
        ret = FindBestPointToNextArrayAndGoal(goal, exitDirection, homePlate, neighborPlate);
        if (ret == null) { return null; }
        ;
        return homePlate.GetSubTileCenterWorldCoordinates(ret!.Value);
    }
    /// <summary>
    /// The function used to find the best Vector3 point to an next plate within the provieded exitdirection.
    /// </summary>
    /// <seealso cref="FindBestPointToNextArrayAndGoalV3(Vector3, ExitDirection.ExitDirections, Plate, Plate)"/>
    /// <param name="goal">The position to find the best exit to.</param>
    /// <param name="exitDirection">The direction in which the exit position are to be considered.</param>
    /// <param name="homePlate">The Plate on which the exit positions are to be considered.</param>
    /// <param name="neighborPlate">The plate neighboring the plate on which the exits are considered.</param>
    /// <returns>Null if there is no valid exit. A Vector3 if there is an valid exit.</returns>
    public static Vector2Int? FindBestPointToNextArrayAndGoal(Vector3 goal, ExitDirection.ExitDirections exitDirection, Plate homePlate, Plate neighborPlate)
    {
        Portal? clostestPortal = homePlate.GetClostestPortal(goal, exitDirection);
        if (clostestPortal == null) { return null; }
        ;
        Vector2Int? clostestPoint = null;
        float currentSmallestDistance = float.MaxValue;
        foreach (Vector2Int posToBeChecked in clostestPortal.GoalPositions2)
        {
            if (homePlate.BaseCostMatrix[posToBeChecked.x, posToBeChecked.y] == MatrixIsPathableValue)
            {
                float distance = Vector3.Distance(goal, homePlate.GetSubTileCenterWorldCoordinates(posToBeChecked));
                if (distance < currentSmallestDistance)
                {
                    if (exitDirection == ExitDirection.ExitDirections.North || exitDirection == ExitDirection.ExitDirections.South)
                    {
                        if (neighborPlate.BaseCostMatrix[neighborPlate.Rows - (posToBeChecked.x + 1), posToBeChecked.y] == MatrixIsPathableValue)
                        {
                            clostestPoint = posToBeChecked;
                            currentSmallestDistance = distance;
                        }
                    }
                    else
                    {
                        if (neighborPlate.BaseCostMatrix[posToBeChecked.x, neighborPlate.Columns - (posToBeChecked.y + 1)] == MatrixIsPathableValue)
                        {
                            clostestPoint = posToBeChecked;
                            currentSmallestDistance = distance;
                        }
                    }
                }
                else { break; }
            }
        }
        return clostestPoint;
    }

    /// <summary>
    /// Function to Generate the path for an agent from the provided plates to be visited. The path starts at the provided start and ends at the provided goal.
    /// </summary>
    /// <param name="platesToVisit">The List of plates through which the agents paths to the provided goal.</param>
    /// <param name="start">The starting point of the path.</param>
    /// <param name="goal">The ending point of the path.</param>
    /// <param name="canPathDiagonal">Wether or not the agent can path diagonal.</param>
    /// <returns>
    /// The Queue of Vector3, the waypoints which the agent is ought to take. This list is null, when there couldn't be a path found with the provided plates could be found. 
    /// In that case the last Plate is returned.
    /// </returns>
    public static (Queue<Vector3>, Plate?) GeneratePath(List<Plate> platesToVisit, Vector3 start, Vector3 goal, bool canPathDiagonal)
    {
        List<Vector3> steps = new List<Vector3> { start };
        for (int i = 0; i < platesToVisit.Count - 1; i++)
        {
            Plate currentPlate = platesToVisit[i];
            Plate nextPlate = platesToVisit[i + 1];
            Vector3 diff = nextPlate.Center - currentPlate.Center;
            Vector3? closestPoint = null;
            Vector3 checkDirection;
            Vector2Int nextDir = new(0, 0);
            if (canPathDiagonal && diff.x != 0 && diff.z != 0)
            {
                if (diff.x > 0)
                {
                    if (diff.z > 0)
                    {
                        closestPoint = (nextPlate.BaseCostMatrix[0, 0] == MatrixIsPathableValue && currentPlate.BaseCostMatrix[currentPlate.Rows - 1, currentPlate.Columns - 1] == MatrixIsPathableValue) ? currentPlate.GetSubTileCenterWorldCoordinates(currentPlate.Rows - 1, currentPlate.Columns - 1) : null;
                        checkDirection = new Vector3(GenerateMatrix.TileSizeX, 0, GenerateMatrix.TileSizeZ);
                        nextDir = new(1, 1);
                    }
                    else
                    {
                        closestPoint = (nextPlate.BaseCostMatrix[0, nextPlate.Columns - 1] == MatrixIsPathableValue && currentPlate.BaseCostMatrix[currentPlate.Rows - 1, 0] == MatrixIsPathableValue) ? currentPlate.GetSubTileCenterWorldCoordinates(currentPlate.Rows - 1, 0) : null;
                        checkDirection = new Vector3(GenerateMatrix.TileSizeX, 0, -GenerateMatrix.TileSizeZ);
                        nextDir = new(1, -1);
                    }
                }
                else
                {
                    if (diff.z > 0)
                    {
                        closestPoint = (nextPlate.BaseCostMatrix[nextPlate.Rows - 1, 0] == MatrixIsPathableValue && currentPlate.BaseCostMatrix[0, currentPlate.Columns - 1] == MatrixIsPathableValue) ? currentPlate.GetSubTileCenterWorldCoordinates(0, currentPlate.Columns - 1) : null;
                        checkDirection = new Vector3(-GenerateMatrix.TileSizeX, 0, GenerateMatrix.TileSizeZ);
                        nextDir = new(-1, 1);
                    }
                    else
                    {
                        closestPoint = (nextPlate.BaseCostMatrix[nextPlate.Rows - 1, nextPlate.Columns - 1] == MatrixIsPathableValue && currentPlate.BaseCostMatrix[0, 0] == MatrixIsPathableValue) ? currentPlate.GetSubTileCenterWorldCoordinates(0, 0) : null;
                        checkDirection = new Vector3(-GenerateMatrix.TileSizeX, 0, -GenerateMatrix.TileSizeZ);
                        nextDir = new(-1, -1);
                    }
                }
                if (closestPoint != null)
                {
                    Portal? p = currentPlate.GetClostestPortal(steps.Last(), ExitDirection.DirectionToExitDiretion(nextDir));
                    if (p == null)
                    {
                        return (new Queue<Vector3>(), currentPlate);
                    }
                    List<Vector3> subSteps = currentPlate.GetShortestPathToToNextPlateV3(p, steps.Last<Vector3>());
                    if (subSteps.Count > 0)
                    {
                        steps.AddRange(subSteps);
                        steps.Add(steps.Last() + checkDirection);
                    }
                    else
                    {
                        return (new Queue<Vector3>(), currentPlate);
                    }
                }
                else
                {
                    return (new Queue<Vector3>(), currentPlate);
                }
            }
            else
            {
                if (diff.x != 0)
                {
                    if (diff.x > 0)
                    {
                        closestPoint = GenerateMatrix.FindBestPointToNextArrayAndGoalV3(start, ExitDirection.ExitDirections.South, currentPlate, nextPlate);
                        checkDirection = new Vector3(GenerateMatrix.TileSizeX, 0, 0);
                        nextDir = new(1, 0);
                    }
                    else
                    {
                        closestPoint = GenerateMatrix.FindBestPointToNextArrayAndGoalV3(start, ExitDirection.ExitDirections.North, currentPlate, nextPlate);
                        checkDirection = new Vector3(-GenerateMatrix.TileSizeX, 0, 0);
                        nextDir = new(-1, 0);
                    }
                }
                else
                {
                    if (diff.z > 0)
                    {
                        closestPoint = GenerateMatrix.FindBestPointToNextArrayAndGoalV3(start, ExitDirection.ExitDirections.East, currentPlate, nextPlate);
                        checkDirection = new Vector3(0, 0, GenerateMatrix.TileSizeZ);
                        nextDir = new(0, 1);
                    }
                    else
                    {
                        closestPoint = GenerateMatrix.FindBestPointToNextArrayAndGoalV3(start, ExitDirection.ExitDirections.West, currentPlate, nextPlate);
                        checkDirection = new Vector3(0, 0, -GenerateMatrix.TileSizeZ);
                        nextDir = new(0, -1);
                    }
                }
                if (closestPoint != null)
                {
                    Portal? p = currentPlate.GetClostestPortal(steps.Last(), ExitDirection.DirectionToExitDiretion(nextDir));
                    if (p == null)
                    {
                        return (new Queue<Vector3>(), currentPlate);
                    }
                    List<Vector3> subSteps = currentPlate.GetShortestPathToToNextPlateV3(p, steps.Last<Vector3>());
                    if (subSteps.Count > 0)
                    {
                        steps.AddRange(subSteps);
                        steps.Add(steps.Last() + checkDirection);
                    }
                    else
                    {
                        return (new Queue<Vector3>(), currentPlate);
                    }
                }
                else
                {
                    return (new Queue<Vector3>(), currentPlate);
                }
            }
        }
        steps.AddRange(platesToVisit.Last<Plate>().GetShortestPathToGoalWithin(steps.Last(), goal));
        return (new Queue<Vector3>(steps), null);
    }
    /// <summary>
    /// The function finds the path between an start and the goal in the provided flowfield. Only returning the points at which the agent changes the direction.
    /// </summary>
    /// <seealso cref="GetBestPathInFlowFieldFull(byte[,], Vector2Int)"/>
    /// <param name="flowfield">The flowfield in which the path should be found.</param>
    /// <param name="start">The starting point of the path.</param>
    /// <returns>The List of Vector2Int of tiles to be visited in the flowfield to an goal position. The function only returns the points in the path on which the agent must chang their position.</returns>
    public static List<Vector2Int> GetBestPathInFlowField(byte[,] flowfield, Vector2Int start)
    {
        List<Vector2Int> bestPath = new List<Vector2Int>();
        byte lastDir = 111;
        int schutz = 100;
        while (lastDir != ExitDirection.IsExit && schutz > 0)
        {
            byte currByte = flowfield[start.x, start.y];
            if (currByte != lastDir)
            {
                lastDir = currByte;
                bestPath.Add(start);
            }
            lastDir = currByte;
            start += ExitDirection.ByteToVector(currByte);
            schutz--;
        }
        //Debug.Log(bestPath.Count);
        return bestPath;
    }
    /// <summary>
    /// The function returns the full path in the provided flowfield.
    /// </summary>
    /// <seealso cref="GetBestPathInFlowFieldFull(byte[,], Vector2Int)"/>
    /// <param name="flowfield">The flowfield in which the path should be found.</param>
    /// <param name="start">The starting point of the path.</param>
    /// <returns>Return the full path taken by the agent.</returns>
    public static List<Vector2Int> GetBestPathInFlowFieldFull(byte[,] flowfield, Vector2Int start)
    {
        List<Vector2Int> bestPath = new List<Vector2Int>();
        byte currByte = 0;
        while (currByte != ExitDirection.IsExit)
        {
            currByte = flowfield[start.x, start.y];
            bestPath.Add(start);
            start += ExitDirection.ByteToVector(currByte);
        }
        //Debug.Log(bestPath.Count);
        return bestPath;
    }

    /// <summary>
    /// The Function Debugs the Value of an array to the console.
    /// </summary>
    /// <typeparam name="T">The type of values in the array.</typeparam>
    /// <param name="array">The array to be debuged.</param>
    /// <param name="rows">The amount of rows.</param>
    /// <param name="cols">The amount of columns.</param>
    /// <param name="plate">The plate from which the points are.</param>
    public static void DebugArray<T>(T[,] array, int rows, int cols, Plate? plate)
    {
        Debug.Log("===========================================================================================");
        Debug.Log("Debugging Array     Rows: " + rows + " Cols: " + cols);
        Debug.Log("===========================================================================================");
        for (int i = 0; i < rows; i++)
        {
            string rowString = "";
            for (int j = 0; j < cols; j++)
            {
                string extra = (plate != null) ? ("Realworld: " + plate!.GetSubTileCenterWorldCoordinates(i, j)) : "";

                rowString = "Array: (" + i + ", " + j + ")" + extra + " Value: " + array[i, j];
            }
            Debug.Log(rowString);
        }
    }
    /// <summary>
    /// The function logs the array to the console in the array format.
    /// </summary>
    /// <typeparam name="T">Type if values in the array.</typeparam>
    /// <param name="array">The array to be debuged</param>
    /// <param name="rows">The amount of rows.</param>
    /// <param name="cols">The amount of columns.</param>
    public static void DebugArrayOnlyValues<T>(T[,] array, int rows, int cols)
    {
        Debug.Log("===========================================================================================");
        Debug.Log("Debugging Array     Rows: " + rows + " Cols: " + cols);
        Debug.Log("===========================================================================================");
        for (int i = 0; i < rows; i++)
        {
            string rowString = "";
            for (int j = 0; j < cols; j++)
            {
                rowString += "    " + array[i, j] + "    ";
            }
            Debug.Log(rowString);
        }
    }
}

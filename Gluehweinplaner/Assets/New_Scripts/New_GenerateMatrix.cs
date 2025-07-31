using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#nullable enable

public static class New_GenerateMatrix
{

    public static float TileSizeX = 1f;//breite der spalte
    public static float TileSizeZ = 1f;//höhe der zeile
    public static int MatrixObstacleValue = 999;
    public static int MatrixIsPathableValue = 1001;
    public static int ObstacleLayer = LayerMask.GetMask("nichtWakable");


    //The base cost matrix contains, the base values of the costs to travel to a certain square in the tile. it is at first only boolean, walkable or not, where 1 represents wakable and 256 not wakable
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

    public static int[,] GenerateDistanceField(New_Plate plate, Vector3 start,Func<Vector2Int, Vector2Int, bool>? canGoToNext)
    {
        Vector2Int startPosition = plate.GetPositionInArray(start);
        return GenerateDistanceField(plate.BaseCostMatrix, plate.Rows, plate.Columns, startPosition,canGoToNext);
    }

    public static int[,] GenerateDistanceField(int[,] baseCost, int rows, int cols, Vector2Int startPosition,Func<Vector2Int, Vector2Int, bool>? canGoToNext)
    {
        int[,] distanceMatrix = (int[,])baseCost.Clone();
        
        bool checkDirection = canGoToNext != null;

        Queue<Vector2Int> nextNodeToBeExpanded = new Queue<Vector2Int>();
        nextNodeToBeExpanded.Enqueue(startPosition);

        distanceMatrix[startPosition.x, startPosition.y] = 0;
        do
        {
            Vector2Int node = nextNodeToBeExpanded.Dequeue();
            if (rows - node.x > 1)
            {
                Vector2Int nodeToBeChecked = new Vector2Int(node.x + 1, node.y);
                if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                {
                    if (checkDirection)
                    {
                        if (canGoToNext!(node, nodeToBeChecked - node))
                        {
                            distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                            nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                        }
                    }
                    else
                    {
                        distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                        nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                    }
                }
            }
            if (node.x != 0)
            {
                Vector2Int nodeToBeChecked = new Vector2Int(node.x - 1, node.y);
                if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                {
                    if (checkDirection)
                    {
                        if (canGoToNext!(node, nodeToBeChecked - node))
                        {
                            distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                            nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                        }
                    }
                    else
                    {
                        distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                        nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                    }
                }
            }

            if(node.y != 0)
            {

                Vector2Int nodeToBeChecked = new Vector2Int(node.x, node.y - 1);
                if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                {
                    if (checkDirection)
                    {
                        if (canGoToNext!(node, nodeToBeChecked - node))
                        {
                            distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                            nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                        }
                    }
                    else
                    {
                        distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                        nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                    }
                }


                if (New_SceneManager.pathDiagonal)
                {
                    if (node.x > 0)
                    {
                        nodeToBeChecked = new Vector2Int(node.x - 1, node.y - 1);
                        if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                        {
                            if (checkDirection)
                            {
                                if (canGoToNext!(node, nodeToBeChecked - node))
                                {
                                    distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                                    nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                                }
                            }
                            else
                            {
                                distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                                nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                            }
                        }
                    }
                    if (rows - node.x > 1)
                    {
                        nodeToBeChecked = new Vector2Int(node.x + 1, node.y - 1);
                        if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                        {
                            if (checkDirection)
                            {
                                if (canGoToNext!(node, nodeToBeChecked - node))
                                {
                                    distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                                    nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                                }
                            }
                            else
                            {
                                distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                                nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                            }
                        }
                    }
                }
            }



            if(cols - node.y > 1)
            {

                Vector2Int nodeToBeChecked = new Vector2Int(node.x, node.y + 1);
                if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                {
                    if (checkDirection)
                    {
                        if (canGoToNext!(node, nodeToBeChecked - node))
                        {
                            distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                            nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                        }
                    }
                    else
                    {
                        distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                        nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                    }
                }


                if (New_SceneManager.pathDiagonal)
                {
                    if (node.x > 0)
                    {
                        nodeToBeChecked = new Vector2Int(node.x - 1, node.y + 1);
                        if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                        {
                            if (checkDirection)
                            {
                                if (canGoToNext!(node, nodeToBeChecked - node))
                                {
                                    distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                                    nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                                }
                            }
                            else
                            {
                                distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                                nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                            }
                        }
                    }
                    if (rows - node.x > 1)
                    {
                        nodeToBeChecked = new Vector2Int(node.x + 1, node.y + 1);
                        if (distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] == MatrixIsPathableValue)
                        {
                            if (checkDirection)
                            {
                                if (canGoToNext!(node, nodeToBeChecked - node))
                                {
                                    distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                                    nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                                }
                            }
                            else
                            {
                                distanceMatrix[nodeToBeChecked.x, nodeToBeChecked.y] = distanceMatrix[node.x, node.y] + 1;
                                nextNodeToBeExpanded.Enqueue(nodeToBeChecked);
                            }
                        }
                    }
                }
            }


        } while (nextNodeToBeExpanded.Count != 0);

        return distanceMatrix;
    }

    public static List<Vector3> GetBestPathInDistanceMatrix(New_Plate plate, int[,] distanceMatrix, int rows, int cols, Vector2Int start) {
        List<Vector2Int> stepsV2 = GetBestPathInDistanceMatrix(distanceMatrix, rows, cols, start);
        List<Vector3> stepsV3 = new List<Vector3>();
        foreach (Vector2Int step in stepsV2)
        {
            stepsV3.Add(plate.GetSubTileCenterWorldCoordinates(step));
        }
        return stepsV3;
    }

    public static List<Vector2Int> GetBestPathInDistanceMatrix(int[,] distanceMatrix, int rows, int cols, Vector2Int start) {
        List<Vector2Int> steps = new List<Vector2Int> { start };
        Vector2Int curr = start;
        Vector2Int next = curr;
        bool plate = start == new Vector2Int(2, 4);

        int schutz = 0;
        while (distanceMatrix[curr.x, curr.y] > 0 && schutz < 100)
        {
            int currMinValue = distanceMatrix[curr.x, curr.y];
            Vector2Int plateToCheck;
            if (curr.x < rows - 1)
            {
                plateToCheck = new Vector2Int(curr.x + 1, curr.y);
                if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue-1 && !steps.Contains(plateToCheck))
                {
                    next = plateToCheck;
                    currMinValue = distanceMatrix[next.x, next.y];
                }

                if (New_SceneManager.pathDiagonal)
                {
                    if (curr.y < cols - 1)
                    {
                        plateToCheck = new Vector2Int(curr.x + 1, curr.y +1);
                        if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue - 1 && !steps.Contains(plateToCheck))
                        {
                            next = plateToCheck;
                            currMinValue = distanceMatrix[next.x, next.y];
                        }
                    }


                    if (curr.y > 0)
                    {
                        plateToCheck = new Vector2Int(curr.x + 1, curr.y - 1);
                        if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue - 1 && !steps.Contains(plateToCheck))
                        {
                            next = plateToCheck;
                            currMinValue = distanceMatrix[next.x, next.y];
                        }
                    }
                }
            }
            if (curr.x > 0)
            {
                plateToCheck = new Vector2Int(curr.x - 1, curr.y);
                if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue-1 && !steps.Contains(plateToCheck))
                {
                    next = plateToCheck;
                    currMinValue = distanceMatrix[next.x, next.y];
                }


                if (New_SceneManager.pathDiagonal)
                {
                    if (curr.y < cols - 1)
                    {
                        plateToCheck = new Vector2Int(curr.x - 1, curr.y + 1);
                        if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue - 1 && !steps.Contains(plateToCheck))
                        {
                            next = plateToCheck;
                            currMinValue = distanceMatrix[next.x, next.y];
                        }
                    }


                    if (curr.y > 0)
                    {
                        plateToCheck = new Vector2Int(curr.x - 1, curr.y - 1);
                        if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue - 1 && !steps.Contains(plateToCheck))
                        {
                            next = plateToCheck;
                            currMinValue = distanceMatrix[next.x, next.y];
                        }
                    }
                }
            }
            if (curr.y < cols - 1)
            {
                plateToCheck = new Vector2Int(curr.x, curr.y + 1);
                if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue-1 && !steps.Contains(plateToCheck))
                {
                    next = plateToCheck;
                    currMinValue = distanceMatrix[next.x, next.y];
                }
            }
            if (curr.y > 0)
            {
                plateToCheck = new Vector2Int(curr.x, curr.y - 1);
                if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue-1 && !steps.Contains(plateToCheck))
                {
                    next = plateToCheck;
                    currMinValue = distanceMatrix[next.x, next.y];
                }
            }
        
            curr = next;
            steps.Add(curr);
            schutz++;
        }
        return steps;
    }

    public static List<Vector2Int> InterpolateArray(Vector2Int start, Vector2Int goal, Func<Vector2Int,bool> isInArray)
    {
        if(start == goal) { return  new List<Vector2Int> { start }; }
        Vector2Int ratioDirection = goal - start;
        Vector2Int maxStep;
        Vector2Int minStep;
        float ratioStep;
        List<Vector2Int> steps = new List<Vector2Int>();


        if (ratioDirection.x == 0)
        {
            if (ratioDirection.y == 0) { return steps; }
            maxStep = new Vector2Int(0, Math.Clamp(ratioDirection.y, -1, 1));
            minStep = new Vector2Int(0, 0);
            ratioStep = int.MaxValue;
        }
        else if (ratioDirection.y == 0)
        {
            if (ratioDirection.x == 0) { return steps; }
            maxStep = new Vector2Int(Math.Clamp(ratioDirection.x, -1, 1), 0);
            minStep = new Vector2Int(0, 0);
            ratioStep = int.MaxValue;
        }
        else
        {
            if (Mathf.Abs(ratioDirection.x) > Math.Abs(ratioDirection.y ))
            {
                maxStep = new Vector2Int(Math.Clamp(ratioDirection.x, -1, 1), 0);
                minStep = new Vector2Int(0, Math.Clamp(ratioDirection.y, -1, 1));
                ratioStep = (float)ratioDirection.x /(float) ratioDirection.y;
            }
            else
            {
                maxStep = new Vector2Int(0, Math.Clamp(ratioDirection.y, -1, 1));
                minStep = new Vector2Int(Math.Clamp(ratioDirection.x, -1, 1), 0);
                ratioStep = (float)ratioDirection.y / (float)ratioDirection.x;
            }
        }
        int i = 0;
        int schutz = 0;
        steps.Add(start);
        if(ratioStep > 0)
        {
            while (start != goal && schutz < 10)
            {
                i++;
                Debug.Log(start);
                if (i <= ratioStep)
                {
                    start += maxStep;
                    if (!isInArray(start))
                    {
                        start -= maxStep;
                    }
                }
                else
                {
                    i = 0;
                    start += minStep;
                    if (!isInArray(start))
                    {
                        start -= minStep;
                    }
                }
                steps.Add(start);
                schutz++;
            }
        }
        else
        {
            while (start != goal && schutz < 10)
            {
                Debug.Log(start);
                i--;
                if (i >= ratioStep)
                {
                    start += minStep;
                    if (!isInArray(start))
                    {
                        start -= minStep;
                    }
                }
                else
                {
                    i = 0;
                    start += maxStep;
                    if (!isInArray(start))
                    {
                        start -= maxStep;
                    }
                }
                steps.Add(start);
                schutz++;
            }
        }

            return steps;
    }

    public static List<Vector2Int> InterpolateArrayWithEndCondition(Vector2Int start, Vector2Int ratioDirection, Func<Vector2Int, bool> endCondition)
    {
        Vector2Int maxStep;
        Vector2Int minStep;
        float ratioStep;
        List<Vector2Int> steps = new List<Vector2Int>();


        if (ratioDirection.x == 0)
        {
            if (ratioDirection.y == 0) { return steps; }
            maxStep = new Vector2Int(0, Math.Clamp(ratioDirection.y, -1, 1));
            minStep = new Vector2Int(0, 0);
            ratioStep = int.MaxValue;
        }
        else if (ratioDirection.y == 0)
        {
            if (ratioDirection.x == 0) { return steps; }
            maxStep = new Vector2Int(Math.Clamp(start.x, -1, 1), 0);
            minStep = new Vector2Int(0, 0);
            ratioStep = int.MaxValue;
        }
        else
        {
            if (ratioDirection.x > ratioDirection.y)
            {
                maxStep = new Vector2Int(Math.Clamp(start.x, -1, 1), 0);
                minStep = new Vector2Int(0, Math.Clamp(ratioDirection.y, -1, 1));
                ratioStep = Mathf.Abs((float)ratioDirection.x / (float)ratioDirection.y);
            }
            else
            {
                maxStep = new Vector2Int(0, Math.Clamp(ratioDirection.y, -1, 1));
                minStep = new Vector2Int(Math.Clamp(ratioDirection.x, -1, 1), 0);
                ratioStep = Mathf.Abs((float)ratioDirection.y / (float)ratioDirection.x);
            }
        }
        int i = 0;
        int schutz = 0;
        steps.Add(start);
        while (endCondition(start))
        {
            i++;
            if (i <= ratioStep)
            {
                start += maxStep;
            }
            else
            {
                i = 0;
                start += minStep;
            }
            steps.Add(start);
            schutz++;
        }
        return steps;
    }

    public static Vector3? FindClostesPointInArrayV3(Vector3 plateArrayGoal, New_Plate plate)
    {
        Vector2Int calcPos = FindClostesPointInArrayV2(plateArrayGoal, plate);
        if (calcPos != new Vector2Int(-1, -1))
        {
            return plate.GetSubTileCenterWorldCoordinates(calcPos);
        }
        return null;
    }

    public static Vector2Int FindClostesPointInArrayV2(Vector3 plateArrayGoal, New_Plate plate)
    {
        List<(Vector2Int, float)> positionToDistance = new List<(Vector2Int, float)>
        {
            ( new Vector2Int(0, 0), Vector3.Distance(plateArrayGoal, plate.GetSubTileCenterWorldCoordinates(new Vector2Int(0, 0)))),
            ( new Vector2Int(plate.Rows-1, 0), Vector3.Distance(plateArrayGoal, plate.GetSubTileCenterWorldCoordinates(new Vector2Int(plate.Rows - 1, 0)))),
            ( new Vector2Int(0, plate.Columns-1) ,Vector3.Distance(plateArrayGoal, plate.GetSubTileCenterWorldCoordinates(new Vector2Int(0, plate.Columns - 1)))),
            ( new Vector2Int(plate.Rows - 1, plate.Columns - 1), Vector3.Distance(plateArrayGoal, plate.GetSubTileCenterWorldCoordinates(new Vector2Int(plate.Rows - 1, plate.Columns - 1))))
        };
        positionToDistance.Sort((o1, o2) => o1.Item2.CompareTo(o2.Item2));

        Vector2Int? bestPoint = FindClostestPoint(positionToDistance[0].Item1, positionToDistance[1].Item1, plateArrayGoal, plate);

        if (bestPoint != null)
        {
            return bestPoint!.Value;
        }
        else
        {
            bestPoint = FindClostestPoint(positionToDistance[0].Item1, positionToDistance[2].Item1, plateArrayGoal, plate);
            if (bestPoint != null)
            {
                return bestPoint!.Value;
            }
        }
        return new Vector2Int(-1, -1);

    }

    public static Vector2Int? FindClostestPoint(Vector2Int clostestGoal, Vector2Int secondClostesGoal, Vector3 plateArrayGoal, New_Plate plate) {

        Vector2Int goalDirection = secondClostesGoal - clostestGoal;
        int spacesBetween = Math.Max(Math.Abs(goalDirection.x), Math.Abs(goalDirection.y));
        goalDirection.x = goalDirection.x / spacesBetween;
        goalDirection.y = goalDirection.y / spacesBetween;//norming the vector

        float clostestCurrentDistance = int.MaxValue;

        Vector2Int? closestValidPoint = null;
        Vector2Int nextClosestPoint;
        for (int i = 0; i < spacesBetween; i++)
        {
            nextClosestPoint = clostestGoal + i * goalDirection;

            float nextDistance = Vector3.Distance(plateArrayGoal, plate.GetSubTileCenterWorldCoordinates(nextClosestPoint));

            if (nextDistance < clostestCurrentDistance)
            {
                if (plate.BaseCostMatrix[nextClosestPoint.x, nextClosestPoint.y] != MatrixObstacleValue)
                {
                    if (Physics.CheckSphere(plate.GetSubTileCenterWorldCoordinates(nextClosestPoint) + new Vector3(goalDirection.x, 0, goalDirection.y), 0.01f))
                    {
                        closestValidPoint = nextClosestPoint;
                        clostestCurrentDistance = nextDistance;
                    }
                }

            }
            else if (nextDistance > clostestCurrentDistance)
            {
                return closestValidPoint;
            }
        }
        return closestValidPoint;
    }

    public static Vector3? FindBestPointToNextArrayAndGoalV3(Vector3 goal, ExitDirection exitDirection, New_Plate homePlate, New_Plate neighborPlate)
    {
        Vector2Int? ret = null;
        ret = FindBestPointToNextArrayAndGoal(goal, exitDirection, homePlate, neighborPlate);
        if(ret == null){ return null; };
        return homePlate.GetSubTileCenterWorldCoordinates(ret!.Value);
    }


    public static Vector2Int? FindBestPointToNextArrayAndGoal(Vector3 goal, ExitDirection exitDirection, New_Plate homePlate, New_Plate neighborPlate)
    {
        Vector2Int startPos = new Vector2Int();
        Vector2Int endPos = new Vector2Int();
        switch (exitDirection)
        {
            case ExitDirection.North:
                startPos = new Vector2Int(0, 0);
                endPos = new Vector2Int(0, homePlate.Columns - 1);
                break;
            case ExitDirection.East:
                startPos = new Vector2Int(0, homePlate.Columns - 1);
                endPos = new Vector2Int(homePlate.Rows - 1, homePlate.Columns - 1);
                break;
            case ExitDirection.West:
                startPos = new Vector2Int(homePlate.Rows - 1, 0);
                endPos = new Vector2Int(0, 0);
                break;
            case ExitDirection.South:
                startPos = new Vector2Int(homePlate.Rows - 1, 0);
                endPos = new Vector2Int(homePlate.Rows - 1, homePlate.Columns - 1);
                break;
        }

        Vector2Int direction = endPos - startPos;
        int maxSteps = Math.Max(Math.Abs(direction.x), Math.Abs(direction.y));

        direction.x = Math.Clamp(direction.x, -1, 1);
        direction.y = Math.Clamp(direction.y, -1, 1);

        Vector2Int posToBeChecked = new Vector2Int();
        float currentSmallestDistance = float.MaxValue;

        Vector2Int? clostestPoint = null;
        for (int i = 0; i < maxSteps + 1; i++)
        {
            posToBeChecked = startPos + i * direction;
            if (homePlate.BaseCostMatrix[posToBeChecked.x, posToBeChecked.y] != MatrixObstacleValue)
            {
                float distance = Vector3.Distance(goal, homePlate.GetSubTileCenterWorldCoordinates(posToBeChecked));
                if (distance < currentSmallestDistance)
                {
                    if (exitDirection == ExitDirection.North || exitDirection == ExitDirection.South)
                    {
                        if(neighborPlate.BaseCostMatrix[neighborPlate.Rows-(posToBeChecked.x+1),posToBeChecked.y] != MatrixObstacleValue)
                        {
                            clostestPoint = posToBeChecked;
                            currentSmallestDistance = distance;
                        }
                    }
                    else
                    {
                        if(neighborPlate.BaseCostMatrix[posToBeChecked.x,neighborPlate.Columns-(posToBeChecked.y+1)] != MatrixObstacleValue)
                        {
                            clostestPoint = posToBeChecked;
                            currentSmallestDistance = distance;
                        }
                    }
                }
            }
        }
        return clostestPoint;
    }


    public static Queue<Vector3> GeneratePath(List<New_Plate> platesToVisit, Vector3 start, Vector3 goal)
    {
        List<Vector3> steps = new List<Vector3> { start };
        for (int i = 0; i < platesToVisit.Count - 1; i++)
        {
            New_Plate currentPlate = platesToVisit[i];
            New_Plate nextPlate = platesToVisit[i + 1];
            Vector3 diff = nextPlate.Center - currentPlate.Center;
            if (diff.x != 0)
            {
                if (diff.x > 0)
                {
                    Vector3? closestPoint = New_GenerateMatrix.FindBestPointToNextArrayAndGoalV3(start, ExitDirection.South, currentPlate, nextPlate);
                    if (closestPoint != null)
                    {

                        steps.AddRange(currentPlate.GetShortestPathToExitVector3(closestPoint!.Value, steps.Last<Vector3>()));
                        steps.Add(steps.Last() + new Vector3(New_GenerateMatrix.TileSizeX, 0, 0));
                    }
                }
                else
                {
                    Vector3? closestPoint = New_GenerateMatrix.FindBestPointToNextArrayAndGoalV3(start, ExitDirection.North, currentPlate, nextPlate);
                    if (closestPoint != null)
                    {
                        steps.AddRange(currentPlate.GetShortestPathToExitVector3(closestPoint!.Value, steps.Last<Vector3>()));
                        steps.Add(steps.Last() + new Vector3(-New_GenerateMatrix.TileSizeX, 0, 0));
                    }
                }
            }
            else
            {
                if (diff.z > 0)
                {
                    Vector3? closestPoint = New_GenerateMatrix.FindBestPointToNextArrayAndGoalV3(start, ExitDirection.East, currentPlate, nextPlate);
                    if (closestPoint != null)
                    {
                        steps.AddRange(currentPlate.GetShortestPathToExitVector3(closestPoint!.Value, steps.Last<Vector3>()));
                        steps.Add(steps.Last() + new Vector3(0, 0, New_GenerateMatrix.TileSizeX));
                    }
                }
                else
                {
                    Vector3? closestPoint = New_GenerateMatrix.FindBestPointToNextArrayAndGoalV3(start, ExitDirection.West, currentPlate, nextPlate);
                    if (closestPoint != null)
                    {
                        steps.AddRange(currentPlate.GetShortestPathToExitVector3(closestPoint!.Value, steps.Last<Vector3>()));
                        steps.Add(steps.Last() + new Vector3(0, 0, -New_GenerateMatrix.TileSizeX));
                    }
                }
            }
        }
        steps.Add(goal);
        return new Queue<Vector3>(steps);
    }

    public static List<Vector2Int> PathDiagonal(Vector2Int start, Vector2Int exit)
    {
        List<Vector2Int> steps = new List<Vector2Int>();
        do
        {
            steps.Add(start);
            Vector2Int diff = exit - start;
            if (diff.y > 0)
            {
                start.y += 1;
            }
            else if (diff.y < 0)
            {
                start.y -= 1;
            }
            
            
            if (diff.x > 0)
            {
                start.x += 1;
            }
            else if(diff.x < 0)
            {
                start.x -= 1;
            }

        } while (start != exit);

        return steps;
    }



    public static void DebugArray<T>(T[,] array, int rows, int cols, New_Plate? plate)
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
                rowString += "    "+array[i, j] + "    ";
            }
            Debug.Log(rowString);
        }
    }

   
}

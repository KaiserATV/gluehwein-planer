using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem.XInput;
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

    public static int[,] GenerateDistanceField(New_Plate plate, Vector3 start,Func<Vector2Int, Vector2Int, bool>? canGoToNext, bool canPathDiagonal)
    {
        Vector2Int startPosition = plate.GetPositionInArray(start, false);
        return GenerateDistanceField(plate.BaseCostMatrix, plate.Rows, plate.Columns, startPosition,canGoToNext, canPathDiagonal);
    }

    public static int[,] GenerateDistanceField(int[,] baseCost, int rows, int cols, Vector2Int startPosition, Func<Vector2Int, Vector2Int, bool>? canGoToNext, bool canPathDiagonal)
    {
        int[,] distanceMatrix = (int[,])baseCost.Clone();
        
        bool checkDirection = (canGoToNext != null);
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


                if (canPathDiagonal)
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


                if (canPathDiagonal)
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

    public static List<Vector3> GetBestPathInDistanceMatrix(New_Plate plate, int[,] distanceMatrix, int rows, int cols, Vector2Int start, bool canPathDiagonal, Func<Vector2Int, Vector2Int, bool>? canPathTo) {
        List<Vector2Int> stepsV2 = GetBestPathInDistanceMatrix(distanceMatrix, rows, cols, start, canPathDiagonal, canPathTo);
        List<Vector3> stepsV3 = new List<Vector3>();
        foreach (Vector2Int step in stepsV2)
        {
            stepsV3.Add(plate.GetSubTileCenterWorldCoordinates(step));
        }
        return stepsV3;
    }

    public static List<Vector2Int> GetBestPathInDistanceMatrix(int[,] distanceMatrix, int rows, int cols, Vector2Int start, bool canPathDiagonal, Func<Vector2Int, Vector2Int, bool>? canPathTo) {
        List<Vector2Int> steps = new List<Vector2Int> { start };
        Vector2Int curr = start;
        Vector2Int next = curr;
        bool plate = start == new Vector2Int(2, 4);
        bool checkDirection = (canPathTo != null);
        int schutz = 0;
        if (distanceMatrix[curr.x, curr.y] == MatrixIsPathableValue)
        {
            return new List<Vector2Int>();//there is no way to the goal from the start in the matrix
        }
        while (distanceMatrix[curr.x, curr.y] > 0 && schutz < 100)
        {
            int currMinValue = distanceMatrix[curr.x, curr.y];
            Vector2Int plateToCheck;
            if (curr.x < rows - 1)
            {
                plateToCheck = new Vector2Int(curr.x + 1, curr.y);
                if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue-1 && !steps.Contains(plateToCheck))
                {
                    if (checkDirection)
                    {
                        if (canPathTo!(curr, plateToCheck-curr))
                        {
                            next = plateToCheck;
                            currMinValue = distanceMatrix[next.x, next.y];
                        }
                    }
                    else
                    {
                        next = plateToCheck;
                        currMinValue = distanceMatrix[next.x, next.y];
                    }
                }

                if (canPathDiagonal)
                {
                    if (curr.y < cols - 1)
                    {
                        plateToCheck = new Vector2Int(curr.x + 1, curr.y +1);
                        if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue - 1 && !steps.Contains(plateToCheck))
                        {
                            if (checkDirection)
                            {
                                if (canPathTo!(curr, plateToCheck-curr))
                                {
                                    next = plateToCheck;
                                    currMinValue = distanceMatrix[next.x, next.y];
                                }
                            }
                            else
                            {
                                next = plateToCheck;
                                currMinValue = distanceMatrix[next.x, next.y];
                            }
                        }
                    }


                    if (curr.y > 0)
                    {
                        plateToCheck = new Vector2Int(curr.x + 1, curr.y - 1);
                        if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue - 1 && !steps.Contains(plateToCheck))
                        {
                            if (checkDirection)
                            {
                                if (canPathTo!(curr, plateToCheck - curr))
                                {
                                    next = plateToCheck;
                                    currMinValue = distanceMatrix[next.x, next.y];
                                }
                            }
                            else
                            {
                                next = plateToCheck;
                                currMinValue = distanceMatrix[next.x, next.y];
                            }
                        }
                    }
                }
            }
            if (curr.x > 0)
            {
                plateToCheck = new Vector2Int(curr.x - 1, curr.y);
                if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue-1 && !steps.Contains(plateToCheck))
                {
                    if (checkDirection)
                    {
                        if (canPathTo!(curr, plateToCheck - curr))
                        {
                            next = plateToCheck;
                            currMinValue = distanceMatrix[next.x, next.y];
                        }
                    }
                    else
                    {
                        next = plateToCheck;
                        currMinValue = distanceMatrix[next.x, next.y];
                    }
                }


                if (canPathDiagonal)
                {
                    if (curr.y < cols - 1)
                    {
                        plateToCheck = new Vector2Int(curr.x - 1, curr.y + 1);
                        if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue - 1 && !steps.Contains(plateToCheck))
                        {
                            if (checkDirection)
                            {
                                if (canPathTo!(curr, plateToCheck - curr))
                                {
                                    next = plateToCheck;
                                    currMinValue = distanceMatrix[next.x, next.y];
                                }
                            }
                            else
                            {
                                next = plateToCheck;
                                currMinValue = distanceMatrix[next.x, next.y];
                            }
                        }
                    }


                    if (curr.y > 0)
                    {
                        plateToCheck = new Vector2Int(curr.x - 1, curr.y - 1);
                        if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue - 1 && !steps.Contains(plateToCheck))
                        {
                            if (checkDirection)
                            {
                                if (canPathTo!(curr, plateToCheck - curr))
                                {
                                    next = plateToCheck;
                                    currMinValue = distanceMatrix[next.x, next.y];
                                }
                            }
                            else
                            {
                                next = plateToCheck;
                                currMinValue = distanceMatrix[next.x, next.y];
                            }
                        }
                    }
                }
            }
            if (curr.y < cols - 1)
            {
                plateToCheck = new Vector2Int(curr.x, curr.y + 1);
                if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue-1 && !steps.Contains(plateToCheck))
                {
                    if (checkDirection)
                    {
                        if (canPathTo!(curr, plateToCheck - curr))
                        {
                            next = plateToCheck;
                            currMinValue = distanceMatrix[next.x, next.y];
                        }
                    }
                    else
                    {
                        next = plateToCheck;
                        currMinValue = distanceMatrix[next.x, next.y];
                    }
                }
            }
            if (curr.y > 0)
            {
                plateToCheck = new Vector2Int(curr.x, curr.y - 1);
                if (distanceMatrix[plateToCheck.x, plateToCheck.y] == currMinValue-1 && !steps.Contains(plateToCheck))
                {
                    if (checkDirection)
                    {
                        if (canPathTo!(curr, plateToCheck - curr))
                        {
                            next = plateToCheck;
                            currMinValue = distanceMatrix[next.x, next.y];
                        }
                    }
                    else
                    {
                        next = plateToCheck;
                        currMinValue = distanceMatrix[next.x, next.y];
                    }
                }
            }
        
            curr = next;
            steps.Add(curr);
            schutz++;
        }
        return steps;
    }
    //https://www.redblobgames.com/grids/line-drawing/
    public static List<Vector2Int> InterpolateArray(Vector2Int start, Vector2Int goal, Func<(Vector2Int,Vector2Int),Vector2Int> decideNext, bool canPathDiagonal, int rows, int cols)
    {
        if (start == goal)
        {
            return new List<Vector2Int> { start };            
        }
        int dx = goal.x - start.x, dy = goal.y - start.y;
        int nx = Math.Abs(dx), ny = Math.Abs(dy);
        int sign_x = dx > 0 ? 1 : -1, sign_y = dy > 0 ? 1 : -1;

        List<Vector2Int> points = new List<Vector2Int> {  };
        if (canPathDiagonal)
        {
           
            for (int ix = 0, iy = 0; ix < nx || iy < ny;)
            {
                points.Add(start);
                float decision = (1 + 2 * ix) * ny - (1 + 2 * iy) * nx;
                if (decision == 0)
                {
                    // next step is canPathDiagonal
                    if (start.x+sign_x >=0 && start.x+sign_x < rows && start.y + sign_y >= 0 && start.y + sign_y < cols)
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
                    if(start.y + sign_y >= 0 && start.y + sign_y < cols)
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
                    diffToBorder = new Vector2Int(rows-1, cols-1)-start;
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
                    if (diffToBorder.x > 0 )
                    {
                        p.x += sign_x;
                        ix++;
                        diffToBorder.x--;
                    }
                    else
                    {
                        return points;
                    }
                }else if ((0.5 + ix) / nx == (0.5 + iy) / ny)
                {
                    Vector2Int next = decideNext((new Vector2Int(Mathf.FloorToInt(p.x + sign_x), Mathf.FloorToInt(p.y)), new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y + sign_y))));
                    if(next == new Vector2Int(Mathf.FloorToInt(p.x + sign_x), Mathf.FloorToInt(p.y)))
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

    public static Vector3 FindClostestPointInArrayV3(Vector3 plateArrayGoal, New_Plate plate)
    {
        return plate.GetSubTileCenterWorldCoordinates(FindClostestPointInArrayV2(plateArrayGoal, plate));
    }

    public static Vector2Int FindClostestPointInArrayV2(Vector3 plateArrayGoal, New_Plate plate)
    {
        List<(Vector2Int, float)> positionToDistance = new List<(Vector2Int, float)>
        {
            ( new Vector2Int(0, 0), Vector3.Distance(plateArrayGoal, plate.GetSubTileCenterWorldCoordinates(new Vector2Int(0, 0)))),
            ( new Vector2Int(plate.Rows-1, 0), Vector3.Distance(plateArrayGoal, plate.GetSubTileCenterWorldCoordinates(new Vector2Int(plate.Rows - 1, 0)))),
            ( new Vector2Int(0, plate.Columns-1) ,Vector3.Distance(plateArrayGoal, plate.GetSubTileCenterWorldCoordinates(new Vector2Int(0, plate.Columns - 1)))),
            ( new Vector2Int(plate.Rows - 1, plate.Columns - 1), Vector3.Distance(plateArrayGoal, plate.GetSubTileCenterWorldCoordinates(new Vector2Int(plate.Rows - 1, plate.Columns - 1))))
        };
        positionToDistance.Sort((o1, o2) => o1.Item2.CompareTo(o2.Item2));

        (Vector2Int bestFirstPoint, float distance1)  = FindClostestPoint(positionToDistance[0].Item1, positionToDistance[1].Item1, plateArrayGoal, plate);
        (Vector2Int bestSecondPoint, float distance2) = FindClostestPoint(positionToDistance[0].Item1, positionToDistance[2].Item1, plateArrayGoal, plate);
        
        if(bestFirstPoint == null || bestSecondPoint == null)
        {
            return (bestFirstPoint == null)?bestSecondPoint:bestFirstPoint;
        }
        else
        {
            return (distance1 < distance2)? bestFirstPoint : bestSecondPoint;
        }

    }

    public static (Vector2Int,float) FindClostestPoint(Vector2Int clostestGoal, Vector2Int secondClostesGoal, Vector3 plateArrayGoal, New_Plate plate) {

        Vector2Int goalDirection = secondClostesGoal - clostestGoal;

        int spacesBetween = Math.Max(Math.Abs(goalDirection.x), Math.Abs(goalDirection.y));
        goalDirection.x /= spacesBetween;
        goalDirection.y /= spacesBetween;//norming the vector

        Vector3 dir = plateArrayGoal - plate.GetSubTileCenterWorldCoordinates(clostestGoal);
        dir.x = (dir.x > 0) ? 1 - goalDirection.x  : -1 - goalDirection.x;
        dir.z = (dir.z > 0) ? 1 - goalDirection.y : -1 - goalDirection.y;
        dir.y = 0;
        dir.x *= TileSizeX;
        dir.z *= TileSizeZ;

        float clostestCurrentDistance = Vector3.Distance(plateArrayGoal, plate.GetSubTileCenterWorldCoordinates(clostestGoal));
        Vector2Int closestValidPoint=clostestGoal;
        Vector2Int nextClosestPoint;
        for (int i = 1; i < spacesBetween; i++)
        {
            nextClosestPoint = clostestGoal + i * goalDirection;

            float nextDistance = Vector3.Distance(plateArrayGoal, plate.GetSubTileCenterWorldCoordinates(nextClosestPoint));

            if (nextDistance < clostestCurrentDistance)
            {
                closestValidPoint = nextClosestPoint;
                clostestCurrentDistance = nextDistance;
            }
            else if (nextDistance > clostestCurrentDistance)
            {
                return (closestValidPoint,clostestCurrentDistance);
            }
        }
        return (closestValidPoint, clostestCurrentDistance);
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


    public static (Queue<Vector3>,New_Plate?) GeneratePath(List<New_Plate> platesToVisit, Vector3 start, Vector3 goal, bool canPathDiagonal)
    {
        List<Vector3> steps = new List<Vector3> { start };
        for (int i = 0; i < platesToVisit.Count - 1; i++)
        {
            New_Plate currentPlate = platesToVisit[i];
            New_Plate nextPlate = platesToVisit[i + 1];
            Vector3 diff = nextPlate.Center - currentPlate.Center;
            Vector3? closestPoint;
            Vector3 checkDirection;
            if (diff.x != 0)
            {
                if (diff.x > 0)
                {
                    closestPoint = New_GenerateMatrix.FindBestPointToNextArrayAndGoalV3(start, ExitDirection.South, currentPlate, nextPlate);
                    checkDirection = new Vector3(New_GenerateMatrix.TileSizeX, 0, 0);
                }
                else
                {
                    closestPoint = New_GenerateMatrix.FindBestPointToNextArrayAndGoalV3(start, ExitDirection.North, currentPlate, nextPlate);
                    checkDirection = new Vector3(-New_GenerateMatrix.TileSizeX, 0, 0);

                }
            }
            else
            {
                if (diff.z > 0)
                {
                    closestPoint = New_GenerateMatrix.FindBestPointToNextArrayAndGoalV3(start, ExitDirection.East, currentPlate, nextPlate);
                    checkDirection = new Vector3(0, 0, New_GenerateMatrix.TileSizeZ);
                }
                else
                {
                    closestPoint = New_GenerateMatrix.FindBestPointToNextArrayAndGoalV3(start, ExitDirection.West, currentPlate, nextPlate);
                    checkDirection = new Vector3(0, 0, -New_GenerateMatrix.TileSizeZ);
                }
            }
            if (closestPoint != null)
            {
                List<Vector3> subSteps = currentPlate.GetShortestPathToExitVector3(closestPoint!.Value, steps.Last<Vector3>(), canPathDiagonal);
                if (subSteps.Count > 0)
                {
                    steps.AddRange(subSteps);
                    steps.Add(steps.Last() + checkDirection);
                    //Debug.Log(nextPlate.GetBaseValueAtPosition(steps.Last<Vector3>(), false));
                }
                else
                {
                    return (new Queue<Vector3>(), currentPlate);
                }
            }
        }
        steps.Add(goal);
        return (new Queue<Vector3>(steps), null);
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

    public static ExitDirection DirectionToExitDiretion(Vector2Int direction)
    {
        if(direction.x == 0)
        {
            if(direction.y > 0)
            {
                return ExitDirection.East;
            }
            else if(direction.y < 0)
            {
                return ExitDirection.West;
            }
            else
            {
                return ExitDirection.NoExit;
            }
        }else if(direction.y == 0)
        {
            if (direction.x > 0)
            {
                return ExitDirection.South;
            }
            else
            {
                return ExitDirection.North;
            }
        }
        else
        {
            if(direction.x > 0)
            {
                if(direction.y > 0)
                {
                    return ExitDirection.SouthWest;
                }
                else
                {
                    return ExitDirection.SouthEast;
                }
            }
            else
            {
                if( direction.y > 0)
                {
                    return ExitDirection.NorthEast;
                }
                else
                {
                    return ExitDirection.NorthWest;
                }
            }
        }
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

public enum ExitDirection
{
    NoExit,
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest
}

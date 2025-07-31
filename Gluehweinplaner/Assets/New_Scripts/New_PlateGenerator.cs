using System;
using UnityEngine;

public static class New_PlateGenerator
{
    public static New_TransferType CalculatePlatePositionsAndBaseCostMatrices(Bounds b, int plateCountX, int plateCountZ)
    {
        Vector3 size = b.size;
        New_TransferType tt = new New_TransferType();
       
        int totalTileCountX = Mathf.FloorToInt(size.x / New_GenerateMatrix.TileSizeX);
        int totalTileCountZ = Mathf.FloorToInt(size.z / New_GenerateMatrix.TileSizeX);

        int tileCountX = Mathf.FloorToInt((float) totalTileCountX / plateCountX);
        int tileCountZ = Mathf.FloorToInt((float) totalTileCountZ / plateCountZ);

        int randTileCountX = totalTileCountX - (tileCountX * (plateCountX-1));
        int randTileCountZ = totalTileCountZ - (tileCountZ * (plateCountZ-1));

        New_Plate[,] allPlates = new New_Plate[plateCountX,plateCountZ];

        for (int i = 0; i < plateCountX; i++)
        {
            for (int j = 0; j < plateCountZ; j++)
            {
                if (i == plateCountX - 1)
                {
                    if (j == plateCountZ - 1)
                    {
                        Vector3 pos = new Vector3(b.min.x + i * (tileCountX * New_GenerateMatrix.TileSizeX) + (tileCountX * New_GenerateMatrix.TileSizeX) / 2,0f, b.min.z + j * (tileCountZ * New_GenerateMatrix.TileSizeX) + (tileCountZ * New_GenerateMatrix.TileSizeX) / 2 );
                        allPlates[i, j] = new New_Plate(pos, New_GenerateMatrix.GenerateBaseCostMatrix(randTileCountX, randTileCountZ, (int row, int column) => CheckIfPlacable((row * New_GenerateMatrix.TileSizeX) + (pos.x - ((randTileCountX * New_GenerateMatrix.TileSizeX) / 2)) + (New_GenerateMatrix.TileSizeX / 2), (column * New_GenerateMatrix.TileSizeZ) + (pos.z - ((randTileCountZ * New_GenerateMatrix.TileSizeZ) / 2)) + (New_GenerateMatrix.TileSizeZ / 2)), out bool onlyObstacles, out bool noObstacles));
                        allPlates[i, j].Center = pos;
                        allPlates[i, j].Rows = randTileCountX;
                        allPlates[i, j].Columns = randTileCountZ;
                        allPlates[i, j].HasNoObstacles = noObstacles;
                        allPlates[i, j].HasOnlyObstacles = onlyObstacles;
                        allPlates[i, j].Size = new Vector3(allPlates[i, j].Rows * New_GenerateMatrix.TileSizeX, 0, allPlates[i, j].Columns * New_GenerateMatrix.TileSizeZ);
                    }
                    else
                    {
                        Vector3 pos = new Vector3(b.min.x + i * (tileCountX * New_GenerateMatrix.TileSizeX) + (tileCountX * New_GenerateMatrix.TileSizeX) / 2,0f, b.min.z + j * (tileCountZ * New_GenerateMatrix.TileSizeX) + (tileCountZ * New_GenerateMatrix.TileSizeX) / 2);
                        allPlates[i, j] = new New_Plate(pos, New_GenerateMatrix.GenerateBaseCostMatrix(randTileCountX, tileCountZ, (int row, int column) => CheckIfPlacable((row * New_GenerateMatrix.TileSizeX) + (pos.x - ((randTileCountX * New_GenerateMatrix.TileSizeX) / 2)) + (New_GenerateMatrix.TileSizeX / 2), (column * New_GenerateMatrix.TileSizeZ) + (pos.z - ((tileCountZ * New_GenerateMatrix.TileSizeZ) / 2)) + (New_GenerateMatrix.TileSizeZ / 2)), out bool onlyObstacles, out bool noObstacles));
                        allPlates[i, j].Center = pos;
                        allPlates[i, j].Rows = randTileCountX;
                        allPlates[i, j].Columns = tileCountZ;
                        allPlates[i, j].Size = new Vector3(allPlates[i, j].Rows * New_GenerateMatrix.TileSizeX, 0, allPlates[i, j].Columns * New_GenerateMatrix.TileSizeZ);
                    }

                }
                else if (j == plateCountZ - 1)
                {
                    Vector3 pos = new Vector3(b.min.x + i * (tileCountX * New_GenerateMatrix.TileSizeX) + (tileCountX * New_GenerateMatrix.TileSizeX) / 2,0f, b.min.z + j * (tileCountZ * New_GenerateMatrix.TileSizeX) + (tileCountZ * New_GenerateMatrix.TileSizeX) / 2);    
                    allPlates[i, j] = new New_Plate(pos, New_GenerateMatrix.GenerateBaseCostMatrix(tileCountX, randTileCountZ, (int row, int column) => CheckIfPlacable((row * New_GenerateMatrix.TileSizeX) + (pos.x - ((tileCountX * New_GenerateMatrix.TileSizeX) / 2)) + (New_GenerateMatrix.TileSizeX / 2), (column * New_GenerateMatrix.TileSizeZ) + (pos.z - ((randTileCountZ * New_GenerateMatrix.TileSizeZ) / 2)) + (New_GenerateMatrix.TileSizeZ / 2)), out bool onlyObstacles, out bool noObstacles));
                    allPlates[i, j].Center = pos;   
                    allPlates[i, j].Rows = tileCountX;
                    allPlates[i, j].Columns = randTileCountZ;
                    allPlates[i, j].HasNoObstacles = noObstacles;
                    allPlates[i, j].HasOnlyObstacles = onlyObstacles; 
                    allPlates[i, j].Size = new Vector3(allPlates[i, j].Rows * New_GenerateMatrix.TileSizeX, 0, allPlates[i, j].Columns * New_GenerateMatrix.TileSizeZ);
                }
                else
                {
                    
                    Vector3 pos = new Vector3(b.min.x + i * (tileCountX * New_GenerateMatrix.TileSizeX)+(tileCountX * New_GenerateMatrix.TileSizeX)/2, 0, b.min.z + j * (tileCountZ * New_GenerateMatrix.TileSizeX) + (tileCountZ * New_GenerateMatrix.TileSizeX) / 2);
                    allPlates[i, j] = new New_Plate(pos, New_GenerateMatrix.GenerateBaseCostMatrix(tileCountX, tileCountZ, (int row, int column) => CheckIfPlacable((row * New_GenerateMatrix.TileSizeX) + (pos.x - ((tileCountX * New_GenerateMatrix.TileSizeX)/2)) + (New_GenerateMatrix.TileSizeX / 2), (column * New_GenerateMatrix.TileSizeZ) + (pos.z - ((tileCountZ * New_GenerateMatrix.TileSizeZ)/2)) + (New_GenerateMatrix.TileSizeZ / 2)), out bool onlyObstacles, out bool noObstacles));
                    allPlates[i, j].Center = pos;
                    allPlates[i, j].Rows = tileCountX;
                    allPlates[i, j].Columns = tileCountZ;
                    allPlates[i, j].HasNoObstacles = noObstacles;
                    allPlates[i, j].HasOnlyObstacles = onlyObstacles; 
                    allPlates[i, j].Size = new Vector3(allPlates[i, j].Rows * New_GenerateMatrix.TileSizeX, 0, allPlates[i, j].Columns * New_GenerateMatrix.TileSizeZ);
                }
            }
        }

        tt.Plates = allPlates;
        tt.normalPlateX = tileCountX * New_GenerateMatrix.TileSizeX;
        tt.normalPlateZ = tileCountZ * New_GenerateMatrix.TileSizeZ;
        tt.randPlateX = randTileCountX * New_GenerateMatrix.TileSizeX;
        tt.randPlateZ = randTileCountZ * New_GenerateMatrix.TileSizeZ;
        return tt;
    }

    static bool CheckIfPlacable(float posX, float posY)
    {
        bool isPlacable = !Physics.Raycast(
                            new Vector3(posX, -10f, posY),
                            new Vector3(0, 1f, 0), 11f, 
                            New_GenerateMatrix.ObstacleLayer
                            );
        return isPlacable;
    }

}

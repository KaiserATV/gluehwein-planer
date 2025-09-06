using UnityEngine;

public static class PlateGenerator
{
    public static TransferType CalculatePlatePositionsAndBaseCostMatrices(Bounds b, int plateCountX, int plateCountZ, bool pathDiagonal)
    {
        Vector3 size = b.size;
        TransferType tt = new TransferType();
       
        int totalTileCountX = Mathf.FloorToInt(size.x / GenerateMatrix.TileSizeX);
        int totalTileCountZ = Mathf.FloorToInt(size.z / GenerateMatrix.TileSizeX);

        int tileCountX = Mathf.FloorToInt((float) totalTileCountX / plateCountX);
        int tileCountZ = Mathf.FloorToInt((float)totalTileCountZ / plateCountZ);

        int randTileCountX = totalTileCountX - (tileCountX * (plateCountX - 1));
        int randTileCountZ = totalTileCountZ - (tileCountZ * (plateCountZ-1));

        if (tileCountX < 1)
        {
            tileCountX = 1;
            randTileCountX = 1;
            plateCountX = totalTileCountX;
        }
        if (tileCountZ < 1)
        {
            tileCountZ = 1;
            randTileCountZ = 1;
            plateCountZ = totalTileCountZ;
        }


        Plate[,] allPlates = new Plate[plateCountX,plateCountZ];

        for (int i = 0; i < plateCountX; i++)
        {
            for (int j = 0; j < plateCountZ; j++)
            {
                if (i == plateCountX - 1)
                {
                    if (j == plateCountZ - 1)
                    {
                        Vector3 pos = new Vector3(b.min.x + i * (tileCountX * GenerateMatrix.TileSizeX) + (tileCountX * GenerateMatrix.TileSizeX) / 2,0f, b.min.z + j * (tileCountZ * GenerateMatrix.TileSizeX) + (tileCountZ * GenerateMatrix.TileSizeX) / 2 );
                        allPlates[i, j] = new Plate(pos, GenerateMatrix.GenerateBaseCostMatrix(randTileCountX, randTileCountZ, (int row, int column) => CheckIfPlacable((row * GenerateMatrix.TileSizeX) + (pos.x - ((randTileCountX * GenerateMatrix.TileSizeX) / 2)) + (GenerateMatrix.TileSizeX / 2), (column * GenerateMatrix.TileSizeZ) + (pos.z - ((randTileCountZ * GenerateMatrix.TileSizeZ) / 2)) + (GenerateMatrix.TileSizeZ / 2)), out bool onlyObstacles, out bool noObstacles), pathDiagonal);
                        allPlates[i, j].Center = pos;
                        allPlates[i, j].Rows = randTileCountX;
                        allPlates[i, j].Columns = randTileCountZ;
                        allPlates[i, j].HasNoObstacles = noObstacles;
                        allPlates[i, j].HasOnlyObstacles = onlyObstacles;
                        allPlates[i, j].Size = new Vector3(allPlates[i, j].Rows * GenerateMatrix.TileSizeX, 0, allPlates[i, j].Columns * GenerateMatrix.TileSizeZ);

                    }
                    else
                    {
                        Vector3 pos = new Vector3(b.min.x + i * (tileCountX * GenerateMatrix.TileSizeX) + (tileCountX * GenerateMatrix.TileSizeX) / 2,0f, b.min.z + j * (tileCountZ * GenerateMatrix.TileSizeX) + (tileCountZ * GenerateMatrix.TileSizeX) / 2);
                        allPlates[i, j] = new Plate(pos, GenerateMatrix.GenerateBaseCostMatrix(randTileCountX, tileCountZ, (int row, int column) => CheckIfPlacable((row * GenerateMatrix.TileSizeX) + (pos.x - ((randTileCountX * GenerateMatrix.TileSizeX) / 2)) + (GenerateMatrix.TileSizeX / 2), (column * GenerateMatrix.TileSizeZ) + (pos.z - ((tileCountZ * GenerateMatrix.TileSizeZ) / 2)) + (GenerateMatrix.TileSizeZ / 2)), out bool onlyObstacles, out bool noObstacles), pathDiagonal);
                        allPlates[i, j].Center = pos;
                        allPlates[i, j].Rows = randTileCountX;
                        allPlates[i, j].Columns = tileCountZ;
                        allPlates[i, j].Size = new Vector3(allPlates[i, j].Rows * GenerateMatrix.TileSizeX, 0, allPlates[i, j].Columns * GenerateMatrix.TileSizeZ);
                    }

                }
                else if (j == plateCountZ - 1)
                {
                    Vector3 pos = new Vector3(b.min.x + i * (tileCountX * GenerateMatrix.TileSizeX) + (tileCountX * GenerateMatrix.TileSizeX) / 2,0f, b.min.z + j * (tileCountZ * GenerateMatrix.TileSizeX) + (tileCountZ * GenerateMatrix.TileSizeX) / 2);    
                    allPlates[i, j] = new Plate(pos, GenerateMatrix.GenerateBaseCostMatrix(tileCountX, randTileCountZ, (int row, int column) => CheckIfPlacable((row * GenerateMatrix.TileSizeX) + (pos.x - ((tileCountX * GenerateMatrix.TileSizeX) / 2)) + (GenerateMatrix.TileSizeX / 2), (column * GenerateMatrix.TileSizeZ) + (pos.z - ((randTileCountZ * GenerateMatrix.TileSizeZ) / 2)) + (GenerateMatrix.TileSizeZ / 2)), out bool onlyObstacles, out bool noObstacles), pathDiagonal);
                    allPlates[i, j].Center = pos;   
                    allPlates[i, j].Rows = tileCountX;
                    allPlates[i, j].Columns = randTileCountZ;
                    allPlates[i, j].HasNoObstacles = noObstacles;
                    allPlates[i, j].HasOnlyObstacles = onlyObstacles; 
                    allPlates[i, j].Size = new Vector3(allPlates[i, j].Rows * GenerateMatrix.TileSizeX, 0, allPlates[i, j].Columns * GenerateMatrix.TileSizeZ);
                }
                else
                {
                    
                    Vector3 pos = new Vector3(b.min.x + i * (tileCountX * GenerateMatrix.TileSizeX)+(tileCountX * GenerateMatrix.TileSizeX)/2, 0, b.min.z + j * (tileCountZ * GenerateMatrix.TileSizeX) + (tileCountZ * GenerateMatrix.TileSizeX) / 2);
                    allPlates[i, j] = new Plate(pos, GenerateMatrix.GenerateBaseCostMatrix(tileCountX, tileCountZ, (int row, int column) => CheckIfPlacable((row * GenerateMatrix.TileSizeX) + (pos.x - ((tileCountX * GenerateMatrix.TileSizeX)/2)) + (GenerateMatrix.TileSizeX / 2), (column * GenerateMatrix.TileSizeZ) + (pos.z - ((tileCountZ * GenerateMatrix.TileSizeZ)/2)) + (GenerateMatrix.TileSizeZ / 2)), out bool onlyObstacles, out bool noObstacles), pathDiagonal);
                    allPlates[i, j].Center = pos;
                    allPlates[i, j].Rows = tileCountX;
                    allPlates[i, j].Columns = tileCountZ;
                    allPlates[i, j].HasNoObstacles = noObstacles;
                    allPlates[i, j].HasOnlyObstacles = onlyObstacles; 
                    allPlates[i, j].Size = new Vector3(allPlates[i, j].Rows * GenerateMatrix.TileSizeX, 0, allPlates[i, j].Columns * GenerateMatrix.TileSizeZ);
                }
            }
        }

        tt.Plates = allPlates;
        tt.normalPlateX = tileCountX;
        tt.normalPlateZ = tileCountZ;
        tt.randPlateX = randTileCountX;
        tt.randPlateZ = randTileCountZ;
        tt.plateCountX = plateCountX;
        tt.plateCountZ = plateCountZ;
        return tt;
    }

    static bool CheckIfPlacable(float posX, float posY)
    {
        bool isPlacable = !Physics.CheckSphere(
                            new Vector3(posX, 0, posY),
                            1f,
                            GenerateMatrix.ObstacleLayer
                            );
        return isPlacable;
    }

}

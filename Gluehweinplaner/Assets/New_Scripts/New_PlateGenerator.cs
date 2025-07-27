using UnityEngine;

public static class New_PlateGenerator
{
    public static New_TransferType CalculatePlatePositionsAndBaseCostMatrices(Bounds b, int plateCountX, int plateCountZ)
    {
        Vector3 size = b.size;
        New_TransferType tt = new New_TransferType();
       
        int totalTileCountX = Mathf.FloorToInt(size.x / New_GenerateMatrix.tileSizeX);
        int totalTileCountZ = Mathf.FloorToInt(size.z / New_GenerateMatrix.tileSizeX);

        int tileCountX = Mathf.RoundToInt((float) totalTileCountX / plateCountX);
        int tileCountZ = Mathf.RoundToInt((float) totalTileCountZ / plateCountZ);

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
                        Vector3 pos = new Vector3(b.min.x + i * (tileCountX * New_GenerateMatrix.tileSizeX)+(randTileCountX * New_GenerateMatrix.tileSizeX)/2,0f, b.min.z + j * (tileCountZ * New_GenerateMatrix.tileSizeZ)+(randTileCountZ * New_GenerateMatrix.tileSizeZ)/2 );
                        allPlates[i, j] = new New_Plate(pos, New_GenerateMatrix.GenerateBaseCostMatrix(randTileCountX, randTileCountZ, (int row, int column) => Physics.CheckSphere(new Vector3((row * New_GenerateMatrix.tileSizeX) + (pos.x - ((randTileCountX * New_GenerateMatrix.tileSizeX) / 2)) + (New_GenerateMatrix.tileSizeX / 2), 0, (column * New_GenerateMatrix.tileSizeZ) + (pos.z - ((randTileCountZ * New_GenerateMatrix.tileSizeZ) / 2)) + (New_GenerateMatrix.tileSizeZ / 2)), 0.01f,LayerMask.NameToLayer("nichtWakable"))));
                        allPlates[i, j].Center = pos;
                        allPlates[i, j].Rows = randTileCountX;
                        allPlates[i, j].Columns = randTileCountZ;
                        allPlates[i, j].Size = new Vector3(allPlates[i, j].Rows * New_GenerateMatrix.tileSizeX, 0, allPlates[i, j].Columns * New_GenerateMatrix.tileSizeZ);
                    }
                    else
                    {
                        Vector3 pos = new Vector3(b.min.x + i * (tileCountX * New_GenerateMatrix.tileSizeX)+(randTileCountX * New_GenerateMatrix.tileSizeX)/2,0f, b.min.z + j * (tileCountZ * New_GenerateMatrix.tileSizeZ)+(tileCountZ * New_GenerateMatrix.tileSizeZ)/2);
                        allPlates[i, j] = new New_Plate(pos, New_GenerateMatrix.GenerateBaseCostMatrix(randTileCountX, tileCountZ, (int row, int column) => Physics.CheckSphere(new Vector3((row * New_GenerateMatrix.tileSizeX) + (pos.x - ((randTileCountX * New_GenerateMatrix.tileSizeX) / 2)) + (New_GenerateMatrix.tileSizeX / 2), 0, (column * New_GenerateMatrix.tileSizeZ) + (pos.z - ((tileCountZ * New_GenerateMatrix.tileSizeZ) / 2)) + (New_GenerateMatrix.tileSizeZ / 2)), 0.01f,LayerMask.NameToLayer("nichtWakable"))));
                        allPlates[i, j].Center = pos;
                        allPlates[i, j].Rows = randTileCountX;
                        allPlates[i, j].Columns = tileCountZ;
                        allPlates[i, j].Size = new Vector3(allPlates[i, j].Rows * New_GenerateMatrix.tileSizeX, 0, allPlates[i, j].Columns * New_GenerateMatrix.tileSizeZ);
                    }

                }
                else if (j == plateCountZ - 1)
                {
                    Vector3 pos = new Vector3(b.min.x + i * (tileCountX * New_GenerateMatrix.tileSizeX)+(tileCountX * New_GenerateMatrix.tileSizeX)/2,0f, b.min.z + j * (tileCountZ * New_GenerateMatrix.tileSizeZ)+(randTileCountZ * New_GenerateMatrix.tileSizeZ)/2);    
                    allPlates[i, j] = new New_Plate(pos, New_GenerateMatrix.GenerateBaseCostMatrix(tileCountX, randTileCountZ, (int row, int column) => Physics.CheckSphere(new Vector3((row * New_GenerateMatrix.tileSizeX) + (pos.x - ((tileCountX * New_GenerateMatrix.tileSizeX) / 2)) + (New_GenerateMatrix.tileSizeX / 2), 0, (column * New_GenerateMatrix.tileSizeZ) + (pos.z - ((randTileCountZ * New_GenerateMatrix.tileSizeZ) / 2)) + (New_GenerateMatrix.tileSizeZ / 2)), 0.01f,LayerMask.NameToLayer("nichtWakable"))));
                    allPlates[i, j].Center = pos;   
                    allPlates[i, j].Rows = tileCountX;
                    allPlates[i, j].Columns = randTileCountZ;
                    allPlates[i, j].Size = new Vector3(allPlates[i, j].Rows * New_GenerateMatrix.tileSizeX, 0, allPlates[i, j].Columns * New_GenerateMatrix.tileSizeZ);
                }
                else
                {
                    
                    Vector3 pos = new Vector3(b.min.x + i * (tileCountX * New_GenerateMatrix.tileSizeX)+(tileCountX * New_GenerateMatrix.tileSizeX)/2, 0, b.min.z + j * (tileCountZ * New_GenerateMatrix.tileSizeZ)+(tileCountZ * New_GenerateMatrix.tileSizeZ)/2);
                    allPlates[i, j] = new New_Plate(pos, New_GenerateMatrix.GenerateBaseCostMatrix(tileCountX, tileCountZ, (int row, int column) => Physics.CheckSphere(new Vector3((row * New_GenerateMatrix.tileSizeX) + (pos.x - ((tileCountX * New_GenerateMatrix.tileSizeX)/2)) + (New_GenerateMatrix.tileSizeX / 2),0, (column * New_GenerateMatrix.tileSizeZ) + (pos.z - ((tileCountZ * New_GenerateMatrix.tileSizeZ)/2)) + (New_GenerateMatrix.tileSizeZ / 2)), 0.01f,LayerMask.NameToLayer("nichtWakable"))));
                    allPlates[i, j].Center = pos;
                    allPlates[i, j].Rows = tileCountX;
                    allPlates[i, j].Columns = tileCountZ;
                    allPlates[i, j].Size = new Vector3(allPlates[i, j].Rows * New_GenerateMatrix.tileSizeX, 0, allPlates[i, j].Columns * New_GenerateMatrix.tileSizeZ);
                }

                if (allPlates[i, j].BaseCostMatrix[allPlates[i, j].Rows - 1, allPlates[i, j].Columns - 1] == New_GenerateMatrix.MatrixHasNoObstacles)
                {
                    allPlates[i, j].HasNoObstacles = true;
                }
                if (allPlates[i, j].BaseCostMatrix[allPlates[i, j].Rows - 1, allPlates[i, j].Columns - 1] == New_GenerateMatrix.MatrixHasOnlyObstacles)
                {
                    allPlates[i, j].HasOnlyObstacles = true;
                }
            }
        }

        tt.Plates = allPlates;
        tt.plateCountX = plateCountX;
        tt.plateCountZ = plateCountZ;
        tt.normalPlateX = tileCountX * New_GenerateMatrix.tileSizeX;
        tt.normalPlateZ = tileCountZ * New_GenerateMatrix.tileSizeZ;
        tt.randPlateX = randTileCountX * New_GenerateMatrix.tileSizeX;
        tt.randPlateZ = randTileCountZ * New_GenerateMatrix.tileSizeZ;
        return tt;
    }

}

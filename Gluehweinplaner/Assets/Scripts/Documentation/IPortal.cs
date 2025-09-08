using UnityEngine;

public interface IPortal
{
    void GenerateFlowField(Plate plate);
    Vector2Int GetClostestToStart(Vector2Int start);
}
using NUnit.Framework;
using UnityEngine;

public class TileLogicalInstance
{
    public TileLogicalInstance(int numWalls, Vector2Int coord)
    {
        NumWalls = numWalls;
        this.coord = coord;
        WallActivate = new bool[NumWalls];
        for (int i = 0; i < numWalls; i++)
        {
            WallActivate[i] = true;
        }
    }

    public bool IsVisited {  get; private set; } = false;
    public bool[] WallActivate { get; private set; }
    public int NumWalls;
    public Vector2Int coord { get; private set; }

    public void Visit ()
    {
        IsVisited = true;
    }

    public void SetWallActive (int wallNum, bool active)
    {
        WallActivate [wallNum] = active;
    }

}

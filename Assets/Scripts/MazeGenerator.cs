using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;


public enum MazeGenAlgorithm
{
    RecursiveBacktracking,
    RandomizedDepthFirstSearch,
    HuntAndKill
    //magari altri in futuro
}

public enum MazeType
{
    Squared,
    Hex
    //magari altri
}

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private GameObject TilePrefab;

    protected TileLogicalInstance[,] tiles;
    protected MazeGenAlgorithm algorithm;
    protected MazeType type;
    protected Vector2 size;
    protected GameObject[,] tilesGrid;
    protected System.Random random;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public IEnumerator Start()
    {
        random = new();
        /*int width = 10; int height = 10; MazeGenAlgorithm alg = MazeGenAlgorithm.RecursiveBacktracking; MazeType type = MazeType.Hex;

        tiles = new TileLogicalInstance[width, height];
        tilesGrid = new GameObject[width, height];
        this.type = type;
        this.algorithm = alg;
        size = new Vector2(width, height);

        SpawnAndInstantiate(width, height, type);

        //ClearWallsBetween(tiles[2, 2], tiles[2, 3]);

        TileLogicalInstance t1 = tiles[4, 4];
        
        foreach (var c in GetNearUnvisitedCells(t1).ToArray())
        {
            Debug.Log($"Coord {c.coord.x}, {c.coord.y}");
            VisitTile(c);
        }

        */

        yield return Generate(10, 10, MazeGenAlgorithm.RecursiveBacktracking, MazeType.Hex);
    }

    public IEnumerator Generate(int width, int height, MazeGenAlgorithm alg, MazeType type)
    {
        tiles = new TileLogicalInstance[width, height];
        tilesGrid = new GameObject[width, height];
        this.type = type;
        this.algorithm = alg;
        size = new Vector2(width, height);

        SpawnAndInstantiate(width, height, type);

        yield return RecursiveGenerationStep(tiles[0, 0], null);
        //yield return this;

    }

    protected void VisitTile(TileLogicalInstance tile)
    {
        tile.Visit();
        FindTileGameObject(tile).GetComponent<MazeTileBehavior>().Visit();
    }

    protected IEnumerator RecursiveGenerationStep (TileLogicalInstance current, TileLogicalInstance prev)
    {
        if (algorithm == MazeGenAlgorithm.RecursiveBacktracking)
        {
            VisitTile(current);

            ClearWallsBetween(current, prev);

            yield return new WaitForSeconds(0.05f);

            TileLogicalInstance next;

            do
            {
                next = GetRandomNearUnvisitedCell(current);
                if (next != null)
                    yield return RecursiveGenerationStep(next, current);

            } while (next != null);
        }

        //fine

    }

    protected void SpawnAndInstantiate (int width, int height, MazeType type)
    {
        switch (type)
        {
            case MazeType.Squared:
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        tilesGrid[x, y] = Instantiate(TilePrefab, new Vector3((x), 0, (y)), Quaternion.identity);
                        tiles[x, y] = new TileLogicalInstance(4, new Vector2Int(x, y));
                    }
                }
                break;
            case MazeType.Hex:
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        tilesGrid[x, y] = Instantiate(TilePrefab, new Vector3((x*3f + (y%2==0 ? 0f : 3f/2f)), 0, (y*Mathf.Sqrt(3)/2)), Quaternion.identity);
                        tiles[x, y] = new TileLogicalInstance(6, new Vector2Int(x, y));
                    }
                }
                break;

        }
    }

    protected void ClearWallsBetween (TileLogicalInstance currentCell, TileLogicalInstance prevCell)
    {
        if (prevCell == null)
            return;

        switch (type)
        {
            case MazeType.Squared:
                if (currentCell.coord.x < prevCell.coord.x)
                {
                    DeactivateWall(currentCell, 2);
                    DeactivateWall(prevCell, 0);
                }
                else if (currentCell.coord.x > prevCell.coord.x)
                {
                    DeactivateWall(currentCell, 0);
                    DeactivateWall(prevCell, 2);
                }
                else if (currentCell.coord.y < prevCell.coord.y)
                {
                    DeactivateWall(currentCell, 1);
                    DeactivateWall(prevCell, 3);
                }
                else if (currentCell.coord.y > prevCell.coord.y)
                {
                    DeactivateWall(currentCell, 3);
                    DeactivateWall(prevCell, 1);
                }
                break;
            case MazeType.Hex:
                {
                    int x = currentCell.coord.x;
                    int y = currentCell.coord.y;
                    int px = prevCell.coord.x;
                    int py = prevCell.coord.y;

                    bool rigaAttualeDisp = (y % 2) == 1;

                    
                    if (py == y + 2 && px == x) //prec sopra
                    {
                        DeactivateWall(currentCell, 3);
                        DeactivateWall(prevCell, 0);
                    }
                    else if (py == y - 2 && px == x)
                    {
                        DeactivateWall(currentCell, 0);
                        DeactivateWall(prevCell, 3);
                    }

                    // diagonali
                    else if (py == y + 1) //prec � sopra
                    {
                        //Debug.Log("prec � sopra");

                        if ((px == x + 1 && rigaAttualeDisp) || (px == x && !rigaAttualeDisp)) //prec � a dx
                        {
                            DeactivateWall(currentCell, 2);
                            DeactivateWall(prevCell, 5);
                            //Debug.Log("prec � sopra a dx");
                        }
                        if ((px == x - 1 && !rigaAttualeDisp) || (px == x && rigaAttualeDisp)) //prec � a sx
                        {
                            DeactivateWall(currentCell, 4);
                            DeactivateWall(prevCell, 1);
                            //Debug.Log("prec � sopra a sx");
                        }
                    }
                    else if (py == y - 1) //prec � sotto
                    {
                        //Debug.Log("prec � sotto");

                        if ((px == x + 1 && rigaAttualeDisp) || (px == x && !rigaAttualeDisp)) //prec � a dx
                        {
                            DeactivateWall(currentCell, 1);
                            DeactivateWall(prevCell, 4);
                            //Debug.Log("prec � sotto a dx");
                        }
                        if ((px == x - 1 && !rigaAttualeDisp) || (px == x && rigaAttualeDisp)) //prec � a sx
                        {
                            DeactivateWall(currentCell, 5);
                            DeactivateWall(prevCell, 2);
                            //Debug.Log("prec � sotto a sx");
                        }
                    }
                }
                break;

        }
    }

    protected void DeactivateWall(TileLogicalInstance tile, int num)
    {
        tile.SetWallActive(num, false);
        FindTileGameObject(tile).GetComponent<MazeTileBehavior>().SetWallActive(num, false);
    }

    protected IEnumerable<TileLogicalInstance> GetNearCells (TileLogicalInstance current)
    {
        switch (type)
        {
            case MazeType.Squared:
                if (current.coord.x > 0)
                    yield return tiles[current.coord.x - 1, current.coord.y];
                if (current.coord.x < size.x-1)
                    yield return tiles[current.coord.x + 1, current.coord.y];
                if (current.coord.y > 0)
                    yield return tiles[current.coord.x, current.coord.y - 1];
                if (current.coord.y < size.y - 1)
                    yield return tiles[current.coord.x, current.coord.y + 1];
                break;
            case MazeType.Hex:
                {
                    int x = current.coord.x;
                    int y = current.coord.y;
                    bool oddRow = (y % 2) == 1;

                    // diagonali 1
                    if (y < size.y - 1)
                        yield return tiles[x, y + 1];
                    if (y > 0)
                        yield return tiles[x, y - 1];

                    // su giu
                    if (y > 1)
                        yield return tiles[x, y - 2];
                    if (y < (size.y - 2))
                        yield return tiles[x, y + 2];

                    // diagonali 2
                    if (oddRow)
                    {
                        if (x < size.x - 1 && y < size.y - 1)
                            yield return tiles[x + 1, y + 1];
                        if (x < size.x - 1 && y > 0)
                            yield return tiles[x + 1, y - 1];
                    }
                    else
                    {
                        if (x > 0 && y < size.y - 1)
                            yield return tiles[x - 1, y + 1];
                        if (x > 0 && y > 0)
                            yield return tiles[x - 1, y - 1];
                    }
                }
                break;

        }
    }

    protected IEnumerable<TileLogicalInstance> GetNearUnvisitedCells (TileLogicalInstance current)
    {
        foreach (var c in GetNearCells(current))
        {
            if (!c.IsVisited)
                yield return c;
        }
    }

    protected TileLogicalInstance GetRandomNearUnvisitedCell (TileLogicalInstance current)
    {
        TileLogicalInstance[] array = GetNearUnvisitedCells(current).ToArray();

        return array.Length == 0 ? null : array[random.Next(0, array.Length)];
    }

    protected GameObject FindTileGameObject(TileLogicalInstance tile)
    {
        return tilesGrid[tile.coord.x, tile.coord.y];
    }

    public void Awake()
    {
        //tile = new TileLogicalInstance(Walls.Length);
    }


}

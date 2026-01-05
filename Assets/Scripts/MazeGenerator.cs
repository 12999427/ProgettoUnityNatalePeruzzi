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
    [SerializeField] private GameObject TilePrefabSquared;
    [SerializeField] private GameObject TilePrefabHex;
    public GameObject TilePrefab
    {
        get
        {
            switch (type)
            {
                case MazeType.Squared:
                    return TilePrefabSquared;
                case MazeType.Hex:
                    return TilePrefabHex;
                default:
                    return null;
            }
        }
    }
    public TileLogicalInstance[,] tiles;
    protected MazeGenAlgorithm algorithm;
    protected MazeType type;
    public Vector2Int size;
    protected GameObject[,] tilesGrid;

    public IEnumerator Generate(int width, int height, MazeGenAlgorithm alg, MazeType type, bool anim=false)
    {   
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        tiles = new TileLogicalInstance[width, height];
        tilesGrid = new GameObject[width, height];
        this.type = type;
        this.algorithm = alg;
        size = new Vector2Int(width, height);

        SpawnAndInstantiate(width, height, type);

        yield return RecursiveGenerationStep(tiles[0, 0], null, anim);

    }

    protected void VisitTile(TileLogicalInstance tile)
    {
        tile.Visit();
        FindTileGameObject(tile).GetComponent<MazeTileBehavior>().Visit();
    }

    protected IEnumerator RecursiveGenerationStep (TileLogicalInstance current, TileLogicalInstance prev, bool anim=false)
    {
        if (algorithm == MazeGenAlgorithm.RecursiveBacktracking)
        {
            VisitTile(current);

            ClearWallsBetween(current, prev);

            if (anim)
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

    public void GenerateInstant(int width, int height, MazeGenAlgorithm alg, MazeType type)
    {
        IEnumerator gen = Generate(width, height, alg, type, false); //Crea una instnza della coroutine

        // ripete finché non finisce
        while (gen.MoveNext()) { } //è bloccante
    }

    public void GenerateAnimatedAsynch(int width, int height, MazeGenAlgorithm alg, MazeType type)
    {
        StartCoroutine(Generate(width, height, alg, type, true));
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
                        tilesGrid[x, y] = Instantiate(TilePrefab, transform);
                        tilesGrid[x, y].transform.localScale = new Vector3(1f, 1f, 1f);
                        tilesGrid[x, y].transform.localPosition = new Vector3((x), 0, (y));
                        tilesGrid[x, y].transform.rotation = Quaternion.identity;
                        tiles[x, y] = new TileLogicalInstance(4, new Vector2Int(x, y));
                    }
                }
                break;
            case MazeType.Hex:
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        tilesGrid[x, y] = Instantiate(TilePrefab, transform);
                        tilesGrid[x, y].transform.localScale = new Vector3(1f, 1f, 1f);
                        tilesGrid[x, y].transform.localPosition = new Vector3((x*3f + (y%2==0 ? 0f : 3f/2f)), 0, (y*Mathf.Sqrt(3)/2));
                        tilesGrid[x, y].transform.rotation = Quaternion.identity;
                        tiles[x, y] = new TileLogicalInstance(6, new Vector2Int(x, y));
                    }
                }
                break;

        }
    }

    public void ClearWallsBetween (TileLogicalInstance currentCell, TileLogicalInstance prevCell)
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

        return array.Length == 0 ? null : array[Random.Range(0, array.Length)];
    }

    public GameObject FindTileGameObject(TileLogicalInstance tile)
    {
        return tilesGrid[tile.coord.x, tile.coord.y];
    }

    public void Awake()
    {
    }

    public TileLogicalInstance GetStartTile()
    {
        return tiles[0, 0];
    }

    public TileLogicalInstance GetEndTile()
    {
        List<TileLogicalInstance> borderTiles = new List<TileLogicalInstance>();
        for (int x = (int)(size.x*0.4f); x < size.x; x++)
        {
            borderTiles.Add(tiles[x, 0]);
            borderTiles.Add(tiles[x, size.y - 1]);
        }
        for (int y = 0; y < size.y; y++)
        {
            borderTiles.Add(tiles[size.x - 1, y]);
        }

        return borderTiles[Random.Range(0, borderTiles.Count)];
    }

    public TileLogicalInstance GetRandomTile()
    {
        int x = Random.Range(0, size.x);
        int y = Random.Range(0, size.y);
        return tiles[x, y];
    }

    public TileLogicalInstance GetNeighborTile(TileLogicalInstance tile, int wallIndex)
    {
        int x = tile.coord.x;
        int y = tile.coord.y;
        bool oddRow = (y % 2) == 1;

        if (type == MazeType.Squared)
        {
            switch (wallIndex)
            {
                case 0: return tiles[x + 1, y]; // muro destra
                case 1: return tiles[x, y - 1]; // muro sotto
                case 2: return tiles[x - 1, y]; // muro sinistra
                case 3: return tiles[x, y + 1]; // muro sopra
                default: return null;
            }
        }
        else if (type == MazeType.Hex)
        {
            switch (wallIndex)
            {
                case 0: return tiles[x, y - 2];       // sopra
                case 1: return tiles[x + (oddRow? 1:0), y - 1]; // sopra-dx
                case 2: return tiles[x + (oddRow? 1:0), y + 1]; // sotto-dx
                case 3: return tiles[x, y + 2];       // sotto
                case 4: return tiles[x - (oddRow? 0:1), y + 1]; // sotto-sx
                case 5: return tiles[x - (oddRow? 0:1), y - 1]; // sopra-sx
                default: return null;
            }
        }
        return null;
    }
}

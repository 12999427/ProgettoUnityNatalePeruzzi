
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct LevelDescription
{
    public int width;
    public int height;
    public MazeType mazeType;

    public int numCollectibles;
    public int numHideWalls;
    public int numLaunchPads;
    public int numArrows;
    public int numTimeBonuses;

    public LevelDescription(int w, int h, MazeType type, int collectibles = 0, int hideWalls = 0, int launchPads = 0, int arrows = 0, int timeBonuses = 0)
    {
        width = w;
        height = h;
        mazeType = type;
        numCollectibles = collectibles;
        numHideWalls = hideWalls;
        numLaunchPads = launchPads;
        numArrows = arrows;
        numTimeBonuses = timeBonuses;
    }
}

public class GameManagerBehavior : MonoBehaviour, I_PlayerInteractions
{
    [SerializeField] private GameObject mazeGeneratorObject;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject EndLevelPrefab;
    [SerializeField] private GameObject CollectiblePrefab;
    [SerializeField] private GameObject HideWallPrefab;
    [SerializeField] private GameObject LaunchPadPrefab;
    [SerializeField] private GameObject ArrowPrefab;
    [SerializeField] private GameObject TimeBonusPrefab;
    [SerializeField] private GameObject Canvas;
    [SerializeField] private GameObject ArrowIndicatorPrefab;

    protected AudioSource bonusAudioSource;
    [SerializeField] public AudioClip bonusCollectSound;


    MazeGenerator mazeGenerator;
    HUDMessage hudMessage;
    TileLogicalInstance endTile;

    Dictionary<TileLogicalInstance, GameObject> objects = new();

    public int currentLevel = -1;
    public int maxLevel = 5; //numero tot livelli

    protected int numTesoriRaccolti = 0;
    protected float startedTime;
    protected float avaiableTime = 300f;

    protected LevelDescription[] levels = new LevelDescription[]
    {
        new LevelDescription(5, 5, MazeType.Squared, 5, 4, 3, 2, 1),
        new LevelDescription(7, 7, MazeType.Squared, 4, 4, 2, 2, 1),
        new LevelDescription(12, 12, MazeType.Squared, 6, 5, 4, 3, 2),
        new LevelDescription(8, 14, MazeType.Hex, 5, 5, 3, 3, 2),
        new LevelDescription(12, 20, MazeType.Hex, 5, 4, 2, 2, 1),
    };

    void Awake()
    {
        mazeGenerator = mazeGeneratorObject.GetComponent<MazeGenerator>();
        hudMessage = Canvas.GetComponent<HUDMessage>();
        bonusAudioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        HandleMazeGeneration();
        startedTime = Time.time;
    }

    void Update()
    {
        float elapsedTime = Time.time - startedTime;
        if (elapsedTime > avaiableTime)
        {
            hudMessage.ShowMessage("Tempo scaduto!\nHai collezionato " + numTesoriRaccolti + " tesori.\nTorni al menu principale.");
            Invoke("GoBackToMainMenu", 5f);
        }
        else
        {
            hudMessage.UpdateTimeMessage("Rimangono: " + Mathf.FloorToInt(avaiableTime - elapsedTime) + " s");
        }
    }

    void HandleMazeGeneration()
    {
        currentLevel += 1;

        foreach (var obj in objects.Values)
        {
            if (obj != null)
                Destroy(obj);
        }

        Debug.Log("Generating level " + currentLevel);
        mazeGenerator.GenerateAnimatedAsynch(levels[currentLevel].width, levels[currentLevel].height, MazeGenAlgorithm.RecursiveBacktracking, levels[currentLevel].mazeType);

        playerObject.transform.position = mazeGenerator.FindTileGameObject(mazeGenerator.tiles[0, 0]).transform.position + new Vector3(0, 1, 0);

        endTile = mazeGenerator.GetEndTile();
        GameObject endLevelGameObject = Instantiate(EndLevelPrefab, mazeGenerator.FindTileGameObject(endTile).transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
        endLevelGameObject.GetComponentInChildren<EndLevelBehavior>().playerInteractions = this;
        objects.Add(endTile, endLevelGameObject);

        for (int i = 0; i < levels[currentLevel].numCollectibles; i++)
        {
            TileLogicalInstance tile = GetRandomEmptyTile();
            GameObject go = Instantiate(CollectiblePrefab, mazeGenerator.FindTileGameObject(tile).transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
            go.GetComponentInChildren<TreasureBehavior>().playerInteractions = this;
            objects.Add(tile, go);
        }

        for (int i = 0; i < levels[currentLevel].numHideWalls; i++)
        {
            TileLogicalInstance tile = GetRandomEmptyTile(true, true);
            GameObject go = Instantiate(HideWallPrefab, mazeGenerator.FindTileGameObject(tile).transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
            go.GetComponentInChildren<HideWallBehavior>().playerInteractions = this;
            objects.Add(tile, go);
        }

        for (int i = 0; i < levels[currentLevel].numLaunchPads; i++)
        {
            TileLogicalInstance tile = GetRandomEmptyTile();
            GameObject go = Instantiate(LaunchPadPrefab, mazeGenerator.FindTileGameObject(tile).transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
            objects.Add(tile, go);
        }

        for (int i = 0; i < levels[currentLevel].numArrows; i++)
        {
            TileLogicalInstance tile = GetRandomEmptyTile();
            GameObject go = Instantiate(ArrowPrefab, mazeGenerator.FindTileGameObject(tile).transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
            go.GetComponentInChildren<ArrowBehavior>().playerInteractions = this;
            objects.Add(tile, go);
        }

        for (int i = 0; i < levels[currentLevel].numTimeBonuses; i++)
        {
            TileLogicalInstance tile = GetRandomEmptyTile();
            GameObject go = Instantiate(TimeBonusPrefab, mazeGenerator.FindTileGameObject(tile).transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
            go.GetComponentInChildren<ClockBehavior>().playerInteractions = this;
            objects.Add(tile, go);
        }
    }

    public TileLogicalInstance GetRandomEmptyTile(bool avoidStart = true, bool avoidEdges = false)
    {
        TileLogicalInstance tile = null;
        do
        {
            tile = mazeGenerator.GetRandomTile();
        } while (objects.ContainsKey(tile) || (avoidStart &&tile.coord.x == 0 && tile.coord.y == 0) || (avoidEdges && (tile.coord.x == 0 || tile.coord.y == 0 || tile.coord.x == mazeGenerator.size.x - 1 || tile.coord.y == mazeGenerator.size.y - 1)));

        return tile;
    }

    public TileLogicalInstance GetTileFromGameObject(GameObject go)
    {
        foreach (var pair in objects)
        {
            if (pair.Value == go)
            {
                return pair.Key;
            }
        }
        return null;
    }

    // Robe dell'interfaccia d'interazione

    public void EndLevelInteraction(GameObject other)
    {
        if (currentLevel < maxLevel - 1)
        {
            HandleMazeGeneration();

            hudMessage.ShowMessage("Congratulazioni! Sei ora al livello " + (currentLevel + 1));
            bonusAudioSource.PlayOneShot(bonusCollectSound);
        }
        else
        {
            hudMessage.ShowMessage("Congratulazioni!\nHai vinto il gioco!\nHai collezionato " + numTesoriRaccolti + " tesori.");
            bonusAudioSource.PlayOneShot(bonusCollectSound);

            Invoke("GoBackToMainMenu", 5f);

            //schermata di vittoria
        }
    }

    void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void HideWallInteraction(GameObject other)
    {
        var tile = GetTileFromGameObject(other);
        var tileBehavior = mazeGenerator.FindTileGameObject(tile).GetComponent<MazeTileBehavior>();
        List<int> activeWalls = new List<int>();
        for (int i = 0; i < tile.WallActivate.Length; i++)
        {
            if (tile.WallActivate[i])
                activeWalls.Add(i);
        }
        int wallIndex = activeWalls[Random.Range(0, activeWalls.Count)];

        mazeGenerator.ClearWallsBetween(tile, mazeGenerator.GetNeighborTile(tile, wallIndex)); //entrambi

        Debug.Log("Cleared wall " + wallIndex + " between " + tile.coord + " and " + mazeGenerator.GetNeighborTile(tile, wallIndex).coord);

        hudMessage.ShowMessage("Muro svanito!");
        bonusAudioSource.PlayOneShot(bonusCollectSound);
    }
    public void TimeBonusInteraction(GameObject other)
    {
        hudMessage.ShowMessage("Hai guadagnato 20 secondi!");
        avaiableTime += 20f;
        bonusAudioSource.PlayOneShot(bonusCollectSound);
    }

    public void ArrowBonusInteraction(GameObject other)
    {
        var tile = GetTileFromGameObject(other);
        var tileGameObject = mazeGenerator.FindTileGameObject(tile);
        Vector3 from = tileGameObject.transform.position;
        Vector3 to = objects[endTile].transform.position;

        Vector3 deltaGlobalPosition = (to - from).normalized;

        GameObject arrowIndicator = Instantiate(ArrowIndicatorPrefab, tileGameObject.transform.position, Quaternion.LookRotation(deltaGlobalPosition, Vector3.up));
        objects[tile] = arrowIndicator;

        hudMessage.ShowMessage("La frecccia ti mostrerà la posizione dell'uscita! (non la strada da seguire)");

        bonusAudioSource.PlayOneShot(bonusCollectSound);
        
    }

    public void CollectibleTreasureInteraction(GameObject other)
    {
        hudMessage.ShowMessage("Tesoro raccolto!");
        numTesoriRaccolti += 1;
        bonusAudioSource.PlayOneShot(bonusCollectSound);
    }
}

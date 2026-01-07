
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

    private AudioSource bonusAudioSource;
    [SerializeField] public AudioClip bonusCollectSound;


    MazeGenerator mazeGenerator;
    HUDMessage hudMessage;
    TileLogicalInstance endTile;

    Dictionary<TileLogicalInstance, GameObject> objects = new();

    private int currentLevel = -1;
    private int maxLevel = 6; //numero tot livelli

    private int numTesoriRaccolti = 0;
    private float startedTime;
    private float avaiableTime;

    private bool isCustomLevel;
    private bool isLevelEnded = false;



    private LevelDescription[] levels = new LevelDescription[]
    {
        new LevelDescription(5, 5, MazeType.Squared, 3, 1, 1, 3, 1),
        new LevelDescription(4, 7, MazeType.Hex, 4, 3, 3, 3, 2),
        new LevelDescription(9, 9, MazeType.Squared, 4, 4, 3, 4, 1),
        new LevelDescription(11, 11, MazeType.Squared, 5, 5, 2, 4, 2),
        new LevelDescription(8, 14, MazeType.Hex, 5, 5, 2, 4, 2),
        new LevelDescription(9, 18, MazeType.Hex, 6, 6, 2, 4, 1),
    };

    void Awake()
    {
        mazeGenerator = mazeGeneratorObject.GetComponent<MazeGenerator>();
        hudMessage = Canvas.GetComponent<HUDMessage>();
        bonusAudioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        isCustomLevel = RunDataContainer.Instance != null && RunDataContainer.Instance.isCustomLevel;
        if (isCustomLevel)
        {
            HandleCustomMazeGeneration();
            avaiableTime = 200f;
        }
        else
        {
            HandleMazeGeneration();
            avaiableTime = 500f;
        }
        startedTime = Time.time;
        Cursor.visible = false;

    }

    void Update()
    {
        float elapsedTime = Time.time - startedTime;
        if (elapsedTime > avaiableTime && !isLevelEnded)
        {
            isLevelEnded = true;
            hudMessage.ShowMessage("Tempo scaduto!\nHai collezionato " + numTesoriRaccolti + " tesori.\nTorni al menu principale.");
            Invoke("GoBackToMainMenu", 5f);
        }
        else if (!isLevelEnded)
        {
            hudMessage.UpdateTimeMessage("Rimangono: " + Mathf.FloorToInt(avaiableTime - elapsedTime) + " s");
        }
    }

    void HandleCustomMazeGeneration()
    {
        Debug.Log("Generating custom level");
        int size = RunDataContainer.Instance.mazeSize;
        
        int width = (RunDataContainer.Instance.mazeType == MazeType.Squared ? size : (int)(size*0.6f));

        mazeGenerator.GenerateInstant(width, size, MazeGenAlgorithm.RecursiveBacktracking, RunDataContainer.Instance.mazeType);

        int area = size*width;

        float bonusesQuantity = RunDataContainer.Instance.mazeBonus/100f;

        int bonusFactor = (int) (bonusesQuantity*area/7f); //semplice calcolo per avere un numero sensato di bonus non spropositato

        playerObject.transform.position = mazeGenerator.FindTileGameObject(mazeGenerator.tiles[0, 0]).transform.position + new Vector3(0, 1, 0);

        List<TileLogicalInstance> endTiles = mazeGenerator.GetEndTiles();
        endTile = endTiles[Random.Range(0, endTiles.Count)];

        GameObject endLevelGameObject = Instantiate(EndLevelPrefab, mazeGenerator.FindTileGameObject(endTile).transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
        endLevelGameObject.GetComponentInChildren<BaseObjectBehavior>().playerInteractions = this;
        objects.Add(endTile, endLevelGameObject);

        SpawnObjects(bonusFactor, bonusFactor, bonusFactor, bonusFactor, bonusFactor);

    }

    void HandleMazeGeneration()
    {
        currentLevel += 1;

        foreach (var obj in objects.Values)
        {
            if (obj != null)
                Destroy(obj);
        }

        objects.Clear();

        Debug.Log("Generating level " + currentLevel);
        mazeGenerator.GenerateInstant(levels[currentLevel].width, levels[currentLevel].height, MazeGenAlgorithm.RecursiveBacktracking, levels[currentLevel].mazeType);

        playerObject.transform.position = mazeGenerator.FindTileGameObject(mazeGenerator.tiles[0, 0]).transform.position + new Vector3(0, 1, 0);

        List<TileLogicalInstance> endTiles = mazeGenerator.GetEndTiles();
        endTile = endTiles[Random.Range(0, endTiles.Count)];

        GameObject endLevelGameObject = Instantiate(EndLevelPrefab, mazeGenerator.FindTileGameObject(endTile).transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
        endLevelGameObject.GetComponentInChildren<BaseObjectBehavior>().playerInteractions = this;
        objects.Add(endTile, endLevelGameObject);

        SpawnObjects(levels[currentLevel].numHideWalls,
                    levels[currentLevel].numCollectibles,
                    levels[currentLevel].numLaunchPads,
                    levels[currentLevel].numArrows,
                    levels[currentLevel].numTimeBonuses
        );
    }

    void SpawnObjects(int numHideWalls, int numCollectibles, int numLaunchPads, int numArrows, int numTimeBonuses)
    {
        for (int i = 0; i < numHideWalls; i++)
        {
            TileLogicalInstance tile = GetRandomEmptyTile(true, 1);
            GameObject go = Instantiate(HideWallPrefab, mazeGenerator.FindTileGameObject(tile).transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
            go.GetComponentInChildren<BaseObjectBehavior>().playerInteractions = this;
            objects.Add(tile, go);
        }

        for (int i = 0; i < numCollectibles; i++)
        {
            TileLogicalInstance tile = GetRandomEmptyTile();
            GameObject go = Instantiate(CollectiblePrefab, mazeGenerator.FindTileGameObject(tile).transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
            go.GetComponentInChildren<BaseObjectBehavior>().playerInteractions = this;
            objects.Add(tile, go);
        }

        for (int i = 0; i < numLaunchPads; i++)
        {
            TileLogicalInstance tile = GetRandomEmptyTile();
            GameObject go = Instantiate(LaunchPadPrefab, mazeGenerator.FindTileGameObject(tile).transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
            objects.Add(tile, go);
        }

        for (int i = 0; i < numArrows; i++)
        {
            TileLogicalInstance tile = GetRandomEmptyTile();
            GameObject go = Instantiate(ArrowPrefab, mazeGenerator.FindTileGameObject(tile).transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
            go.GetComponentInChildren<BaseObjectBehavior>().playerInteractions = this;
            objects.Add(tile, go);
        }

        for (int i = 0; i < numTimeBonuses; i++)
        {
            TileLogicalInstance tile = GetRandomEmptyTile();
            GameObject go = Instantiate(TimeBonusPrefab, mazeGenerator.FindTileGameObject(tile).transform.position + new Vector3(0, 0f, 0), Quaternion.identity);
            go.GetComponentInChildren<BaseObjectBehavior>().playerInteractions = this;
            objects.Add(tile, go);
        }
    }

    public TileLogicalInstance GetRandomEmptyTile(bool avoidStart = true, int avoidEdgesNumTiles = 0)
    {
        TileLogicalInstance tile = null;
        do
        {
            tile = mazeGenerator.GetRandomTile();
        } while (objects.ContainsKey(tile) || (avoidStart &&tile.coord.x == 0 && tile.coord.y == 0) || (tile.coord.x < avoidEdgesNumTiles || tile.coord.y < avoidEdgesNumTiles || tile.coord.x >= mazeGenerator.size.x - avoidEdgesNumTiles || tile.coord.y >= mazeGenerator.size.y - avoidEdgesNumTiles));

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
        if (currentLevel < maxLevel - 1  && !isCustomLevel)
        {
            HandleMazeGeneration();

            hudMessage.ShowMessage("Congratulazioni! Sei ora al livello " + (currentLevel + 1));
            Debug.Log("Passed to level " + (currentLevel + 1));
            bonusAudioSource.PlayOneShot(bonusCollectSound);
        }
        else
        {

            if (isLevelEnded)
                return;

            isLevelEnded = true;


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
            try
            {
                mazeGenerator.GetNeighborTile(tile, i);
            }
            catch
            {
                continue;
            }
            if (tile.WallActivate[i])
                activeWalls.Add(i);
        }

        if (activeWalls.Count == 0)
            return;
        
        int wallIndex = activeWalls[Random.Range(0, activeWalls.Count)];

        mazeGenerator.ClearWallsBetween(tile, mazeGenerator.GetNeighborTile(tile, wallIndex)); //entrambi

        Debug.Log("Cleared wall " + wallIndex + " between " + tile.coord + " and " + mazeGenerator.GetNeighborTile(tile, wallIndex).coord);
        //Debug.Log(string.Join(",", tile.WallActivate));

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

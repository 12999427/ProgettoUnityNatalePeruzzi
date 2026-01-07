using UnityEngine;

public class RunDataContainer : MonoBehaviour
{
    private static RunDataContainer instance;
    public static RunDataContainer Instance
    {
        get
        {
            return instance;
        }
        private set
        {
            instance = value;
        }
    }

    public int mazeSize;
    public MazeType mazeType;
    public int mazeBonus;
    public bool isCustomLevel = false;

    void Awake()
    {
        if (Instance != null)
        {
            //non dovrebbe succedere perchè è creato manualmente dall'editor
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

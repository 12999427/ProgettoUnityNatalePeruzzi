using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider volumeSlider;
    public TextMeshProUGUI VolumeValueText;

    [Header("Maze")]
    public Slider mazeSizeSlider;
    public TextMeshProUGUI MazeSizeValueText;

    public Slider mazeBonusSlider;
    public TextMeshProUGUI MazeBonusValueText;

    public TMP_Dropdown mazeTypeDropdown;

    [Header("Camera")]
    public Slider fovSlider;
    public TextMeshProUGUI FOVValueText;

    // ---- PlayerPrefs keys ----
    private const string PREF_VOLUME = "Volume";
    private const string PREF_FOV = "FOV";

    void Start()
    {
        Cursor.visible = true;

        LoadSettings();
        ForceUIRefresh();
    }


    public void Play()
    {
        RunDataContainer.Instance.isCustomLevel = false;
        SceneManager.LoadScene("Game");
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    public void LaunchCustomLevel()
    {
        RunDataContainer.Instance.isCustomLevel = true;
        SceneManager.LoadScene("Game");
    }


    public void SetVolume(float value)
    {
        float dB = ((value/100f)-1f) * 80f;
        audioMixer.SetFloat("Volume", dB);

        VolumeValueText.text = Mathf.RoundToInt(value) + "%";
        PlayerPrefs.SetFloat(PREF_VOLUME, value/100f);
    }


    public void SetFOV(float fov)
    {
        Camera.main.fieldOfView = fov;
        //Debug.Log("asdjasldj");
        FOVValueText.text = "asdjasldj";
        FOVValueText.text = Mathf.RoundToInt(fov).ToString();


        PlayerPrefs.SetFloat(PREF_FOV, fov);
    }


    public void SetCustomMazeSize(float size)
    {
        int customMazeSize = Mathf.RoundToInt(size);
        MazeSizeValueText.text = customMazeSize.ToString();

        RunDataContainer.Instance.mazeSize = customMazeSize;

    }

    public void SetCustomMazeBonuses(float bonuses)
    {
        int customMazeBonuses = Mathf.RoundToInt(bonuses);
        MazeBonusValueText.text = customMazeBonuses.ToString();

        RunDataContainer.Instance.mazeBonus = customMazeBonuses;

    }

    public void SetCustomMazeType(int type)
    {
        MazeType customMazeType = (MazeType)type;

        RunDataContainer.Instance.mazeType = customMazeType;
    }


    private void LoadSettings()
    {
        volumeSlider.value = PlayerPrefs.GetFloat(PREF_VOLUME, 1f)*100f;
        fovSlider.value = PlayerPrefs.GetFloat(PREF_FOV, 60f);
    }

    private void ForceUIRefresh()
    {
        SetVolume(volumeSlider.value);
        SetFOV(fovSlider.value);

        SetCustomMazeSize(mazeSizeSlider.value);
        SetCustomMazeBonuses(mazeBonusSlider.value);
        SetCustomMazeType(mazeTypeDropdown.value);
    }
}

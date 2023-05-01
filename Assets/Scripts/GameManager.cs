using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PersistentData
{
    public static string PlayerName
    {
        get => PlayerPrefs.GetString("PlayerName", "Default");
        set => PlayerPrefs.SetString("PlayerName", value);
    }
    public static int PlayerId
    {
        get{
            if (PlayerPrefs.HasKey("PlayerId"))
                return PlayerPrefs.GetInt("PlayerId");
            int id = Random.Range(0, int.MaxValue);
            PlayerPrefs.SetInt("PlayerId", id);
            return id;
        }
    }
    public static int ResourceCount
    {
        get => PlayerPrefs.GetInt("Resources", 0);
        set => PlayerPrefs.SetInt("Resources", value);
    }

    public static string LevelPath = null;
    public static LevelMeta? LevelMeta = null;
    public static Serializer.LevelData? OverrideLevel = null;
    public static bool Validating = false;

}

public class GameManager : MonoBehaviour
{
    private void OnGUI()
    {
        string[] levels =
        {
            "MainMenu",
            "Level Select",
            "Build Level",
            "Play Level"
        };
        bool gotoMain =     GUI.Button(new Rect(Screen.width - 120, 10, 100, 35),   levels[0]);
        bool gotoSelect =   GUI.Button(new Rect(Screen.width - 120, 50, 100, 35), levels[1]);
        bool gotoBuild =    GUI.Button(new Rect(Screen.width - 120, 90, 100, 35),  levels[2]);
        bool gotoPlay =     GUI.Button(new Rect(Screen.width - 120, 130, 100, 35),  levels[3]);
        GUI.color = Color.black;
        GUI.Label(new Rect(Screen.width - 120, 170, 300, 35), $"Current: {levels[SceneManager.GetActiveScene().buildIndex]}");

        if (gotoMain)
        {
            SceneManager.LoadScene(0);
            PersistentData.Validating = false;
            PersistentData.OverrideLevel = null;
        }

        if (gotoSelect)
        {
            SceneManager.LoadScene(1);
            PersistentData.Validating = false;
            PersistentData.OverrideLevel = null;
        }

        if (gotoBuild)
        {
            SceneManager.LoadScene(2);
            PersistentData.Validating = false;
            PersistentData.OverrideLevel = null;
        }

        if (gotoPlay)
        {
            SceneManager.LoadScene(3);
            PersistentData.Validating = false;
            PersistentData.OverrideLevel = null;
        }
    }

    
}

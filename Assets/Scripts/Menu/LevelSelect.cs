using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    private List<string> levels;
    private List<string> currentLevelsOnDisplay;

    public int levelsOnDisplay = 6;

    public int y_min, y_max, x_min, x_max;


    public GameObject prefab;

    public void Start()
    {
        levels = new List<string>();
        levels.Add("space cowboy 1");
        levels.Add("space cowboy 2");
        levels.Add("space cowboy 3");
        levels.Add("space cowboy 4");
        levels.Add("space cowboy 5");
        levels.Add("space cowboy 6");
        levels.Add("space cowboy 7");
        levels.Add("space cowboy 8");
        levels.Add("space cowboy 9");
        levels.Add("space cowboy 10");
        levels.Add("space cowboy 11");
        levels.Add("space cowboy 12");
        levels.Add("space cowboy 13");

        SpawnNewLevel();
    }

    private void SpawnNewLevel()
    {
        var g_1 = Instantiate(prefab, this.transform.position + new Vector3(x_min, y_min, 0), Quaternion.identity, this.transform);
        var g_2 = Instantiate(prefab, this.transform.position + new Vector3(x_min, y_max, 0), Quaternion.identity, this.transform);

        var g_3 = Instantiate(prefab, this.transform.position + new Vector3(x_max, y_min, 0), Quaternion.identity, this.transform);
        var g_4 = Instantiate(prefab, this.transform.position + new Vector3(x_max, y_max, 0), Quaternion.identity, this.transform);
    }

    private void MoveLevels()
    {

    }

    private void MoveSelection()
    {

    }

    private void DespawnLevel()
    {

    }
}

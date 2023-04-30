using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    private List<string> levels;
    private List<(GameObject, float)> currentLevelsOnDisplay;

    public int levelsOnDisplay = 6;

    public int y_min, y_max, x_min, x_max;
    public int y_offset_between_astroids;

    public GameObject prefab;

    [SerializeField]
    float moveSpeed, radius;
    float angle;

    void Update()
    {
        List<GameObject> toRemove = new List<GameObject>();
        foreach((GameObject gmo, float ang) in currentLevelsOnDisplay)
        {
            //gmo.transform.RotateAround(this.transform.position - new Vector3(radius, 0, 0), new Vector3(0, 0, 1), ang * moveSpeed * Time.deltaTime);
            gmo.transform.position -= new Vector3(0, 10, 0);
        }
  


    }

    public void Start()
    {
        currentLevelsOnDisplay = new List<(GameObject, float)>();
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

        Enter();
    }

    private void Enter()
    {

        for(int i = 0; i < levels.Count; i++)
        {
            var x = Instantiate(prefab, this.transform.position + new Vector3((x_min + x_max) / 2, y_max + y_offset_between_astroids * i, 0), Quaternion.identity, this.transform);
            currentLevelsOnDisplay.Add((x, 0.15f * i));
        }


    }

    private void MoveSelection()
    {

    }

    private void DespawnLevel(GameObject gameObject)
    {
        GameObject.Destroy(gameObject);
    }
}

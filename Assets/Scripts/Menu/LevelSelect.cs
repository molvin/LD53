using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    private List<string> levels;
    private List<Astroid_data_holder> currentLevelsOnDisplay;

    public int levelsOnDisplay = 6;

    public int y_min, y_max, x_min, x_max;
    public int y_offset_between_astroids;

    public GameObject prefab;

    [SerializeField]
    float moveSpeed, radius, wheelSpeed;
    float angle;

    private GameObject selected_level;

    void Update()
    {

        var speedmod = Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;

        for (int i = 0; i < currentLevelsOnDisplay.Count; i++)
        {
            currentLevelsOnDisplay[i].current_angle += Time.deltaTime * -moveSpeed * (1 + speedmod);
            currentLevelsOnDisplay[i].game_object.transform.position =
                new Vector3(Mathf.Cos(currentLevelsOnDisplay[i].current_angle), Mathf.Sin(currentLevelsOnDisplay[i].current_angle), 0)
                * (radius + currentLevelsOnDisplay[i].personal_offset)
                + (this.transform.position - new Vector3(radius, 0, 0))
                ;

        }




    }


    public void OnLevelClick(GameObject x)
    {
        for (int i = 0; i < currentLevelsOnDisplay.Count; i++)
        {
            currentLevelsOnDisplay[i].game_object.GetComponentInChildren<Animator>(true).gameObject.SetActive(false);
        }

        x.GetComponentInChildren<Animator>(true).gameObject.SetActive(true);
        selected_level = x;

    }

    private class Astroid_data_holder
    {
        public Astroid_data_holder(GameObject game_object, float current_angle, float personal_offset)
        {
            this.game_object = game_object;
            this.current_angle = current_angle;
            this.personal_offset = personal_offset;
        }

        public GameObject game_object;
        public float current_angle;
        public float personal_offset;
    }

    public void Start()
    {
        currentLevelsOnDisplay = new List<Astroid_data_holder>();
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

            currentLevelsOnDisplay.Add(new Astroid_data_holder(x, 0.15f * i, Random.Range(0, 250)));
            x.GetComponent<Button>().onClick.AddListener(() => OnLevelClick(x)); 
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

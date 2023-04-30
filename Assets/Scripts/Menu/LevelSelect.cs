using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    private List<LevelMeta> levels;
    private List<Astroid_data_holder> currentLevelsOnDisplay;

    public int levelsOnDisplay = 6;

    public int y_min, y_max, x_min, x_max;
    public int y_offset_between_astroids;

    public GameObject prefab;

    [SerializeField]
    float moveSpeed, radius, wheelSpeed;
    float angle;

    private Astroid_data_holder selected_level;

    public LevelInforBoxController level_info_object;

    public void BackToMainMenu()
    {
        level_info_object.gameObject.SetActive(false);
        selected_level = null;
        levels = null;
        
        currentLevelsOnDisplay.ForEach(x => GameObject.Destroy(x.game_object));
        currentLevelsOnDisplay = null;
        //Disable level select
        FindObjectOfType<MainMenu>(true).BackToMainMenu();
        this.gameObject.SetActive(false);
    }

    void Update()
    {

        if (currentLevelsOnDisplay == null)
            return;
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


    private void OnLevelClick(Astroid_data_holder x)
    {
        for (int i = 0; i < currentLevelsOnDisplay.Count; i++)
        {
            currentLevelsOnDisplay[i].game_object.GetComponentInChildren<Animator>(true).gameObject.SetActive(false);
        }

        x.game_object.GetComponentInChildren<Animator>(true).gameObject.SetActive(true);
        selected_level = x;
        level_info_object.SetData(x.data);

    }

    private class Astroid_data_holder
    {
        public Astroid_data_holder(GameObject game_object, float current_angle, float personal_offset, LevelMeta data)
        {
            this.game_object = game_object;
            this.current_angle = current_angle;
            this.personal_offset = personal_offset;
            this.data = data;
        }

        public GameObject game_object;
        public float current_angle;
        public float personal_offset;
        public LevelMeta data;
    }

    public void Start()
    {
      

        Enter();
    }

    private void Enter()
    {
        currentLevelsOnDisplay = new List<Astroid_data_holder>();
        levels = new List<LevelMeta>();
        levels.Add(new LevelMeta() { Attempts = 8, Creator = "Niklas", ID = 0, Resource = 1, Time = 67, Wins = 7 });
        levels.Add(new LevelMeta() { Attempts = 8, Creator = "Daniel A", ID = 0, Resource = 1, Time = 67, Wins = 7 });
        levels.Add(new LevelMeta() { Attempts = 8, Creator = "Daniel F", ID = 0, Resource = 1, Time = 67, Wins = 7 });
        levels.Add(new LevelMeta() { Attempts = 8, Creator = "Mans", ID = 0, Resource = 1, Time = 67, Wins = 7 });
        levels.Add(new LevelMeta() { Attempts = 8, Creator = "Per", ID = 0, Resource = 1, Time = 67, Wins = 7 });
        levels.Add(new LevelMeta() { Attempts = 8, Creator = "Emma", ID = 0, Resource = 1, Time = 67, Wins = 7 });
        levels.Add(new LevelMeta() { Attempts = 8, Creator = "Niklas11", ID = 0, Resource = 1, Time = 67, Wins = 7 });


        level_info_object.gameObject.SetActive(true);

        for (int i = 0; i < levels.Count; i++)
        {
            var x = Instantiate(prefab, this.transform.position + new Vector3((x_min + x_max) / 2, y_max + y_offset_between_astroids * i, 0), Quaternion.identity, this.transform);
            var data = new Astroid_data_holder(x, 0.15f * i, Random.Range(0, 250), levels[i]);
            currentLevelsOnDisplay.Add(data);
            x.GetComponent<Button>().onClick.AddListener(() => OnLevelClick(data)); 
        }

        OnLevelClick(currentLevelsOnDisplay[0]);

    }
}

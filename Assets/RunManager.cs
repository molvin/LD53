using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class RunManager : MonoBehaviour
{
    public Serializer serializer;
    public ServiceTalker Service;
    public GameObject TruckPrefab;
    public GameObject CameraPrefab;
    public GameObject WinPrefab;
    public TextMeshProUGUI Timer;
    public GameObject GameOverUi;
    public Button Retry;
    public Button ToMenu;

    private LevelMeta currentLevel;
    private float startTime;
    GameObject truck;
    new GameObject camera;
    Vector3 origin;

    bool won;

    private void Start()
    {
        StartRun();
    }
    public void StartRun()
    {
        IEnumerator Coroutine()
        {
            GameOverUi.SetActive(false);
            if (Timer != null)
                Timer.enabled = false;
            yield return new WaitForSeconds(0.5f);

            string data;
            if(PersistentData.Validating && PersistentData.OverrideLevel != null)
            {
                data = JsonUtility.ToJson(PersistentData.OverrideLevel); //TODO: fix this
            }
            else if(PersistentData.LevelMeta != null)
            {
                Debug.Log("Reading from service");

                currentLevel = PersistentData.LevelMeta.Value;
                Serializer.LevelData level = Service.DownloadLevel(currentLevel);
                data = JsonUtility.ToJson(level); //TODO: fix this
            }
            else if (PersistentData.LevelPath != null)
            {
                data = System.IO.File.ReadAllText(PersistentData.LevelPath);
                Debug.Log("Reading from file");
            }
            else
            {
                Debug.LogError("No Level Path Set");
                yield break;
            }
            
            var generateIter = serializer.Generate(data);
            Debug.Log($"Generating {data}");
            while (generateIter.MoveNext())
            {
                float progress = (float)generateIter.Current;
                yield return null;
            }

            PersistentData.LevelMeta = PersistentData.LevelMeta = null;

            yield return new WaitForSeconds(0.5f);

            DestroyImmediate(Camera.main.gameObject);
            origin = SplineNoise3D.SplineLine[0].pos;
            truck = Instantiate(TruckPrefab, origin, Quaternion.identity);
            camera = Instantiate(CameraPrefab, origin, Quaternion.identity);
            var lastSpline = SplineNoise3D.SplineLine[SplineNoise3D.SplineLine.Count - 1];
            GameObject win = Instantiate(WinPrefab, lastSpline.pos, Quaternion.identity);
            var winPoint = win.GetComponent<WinPoint>();
            winPoint.Radius = lastSpline.radius;
            winPoint.Manager = this;

            startTime = Time.time;

            if (currentLevel.Time == 0)
            {
                currentLevel.Time = 30;
                Debug.Log("No time set in level, defaulting to 30s");
            }

            yield return RunGame();
        }

        StartCoroutine(Coroutine());
    }

    private IEnumerator RunGame()
    {
        GameOverUi.SetActive(false);

        float t = 0.0f;
        while (t < currentLevel.Time || PersistentData.Validating)
        {
            t = Time.time - startTime;
            if (Timer)
            {
                Timer.enabled = true;
                Timer.text = $"{(!PersistentData.Validating ? (currentLevel.Time - t) : t):0}";
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }

            yield return null;
        }

        if(!won)
            Lose();
    }

    public void Win()
    {
        void FinishLevel()
        {
            if (PersistentData.Validating)
            {
                string data = serializer.SerializeLevelToJson();
                var service = Service;
                LevelMeta meta = new LevelMeta
                {
                    Wins = 0,
                    Attempts = 0,
                    Time = Mathf.CeilToInt((Time.time - startTime) * 1.1f),
                    ID = PersistentData.PlayerId,
                    Creator = PersistentData.PlayerName,
                    Resource = 100
                };
                Debug.Log($"Uploading {JsonUtility.ToJson(meta)}");
                service.UploadLevel(meta, data);
                Debug.Log("Done Uploading");
            }
            else
            {
                try
                {
                    if (currentLevel.ID != 0)
                        Service.SendLevelComplete(currentLevel, 1);
                    else
                        Debug.Log("Current level not set, cant update attempts");
                }
                catch { }

                PersistentData.ResourceCount += currentLevel.Resource;
            }
            PersistentData.Validating = false;
            PersistentData.OverrideLevel = null;
            SceneManager.LoadScene(0);
        }


        won = true;

        truck.GetComponent<HoverController>().enabled = false;
        GameOverUi.SetActive(true);
        Retry.onClick.RemoveAllListeners();
        Retry.onClick.AddListener(() => { Restart(); won = false; StartCoroutine(RunGame()); });
        ToMenu.onClick.RemoveAllListeners();
        ToMenu.onClick.AddListener(() => FinishLevel());
        StopAllCoroutines();
    }

    public void Lose()
    {
        truck.GetComponent<HoverController>().enabled = false;
        GameOverUi.SetActive(true);
        Retry.onClick.RemoveAllListeners();
        Retry.onClick.AddListener(() => { Restart(); StartCoroutine(RunGame()); });
        ToMenu.onClick.RemoveAllListeners();
        ToMenu.onClick.AddListener(() => SceneManager.LoadScene(0));    //TODO: correct scene
        StopAllCoroutines();
    }

    private void Restart()
    {
        try
        {
            if (currentLevel.ID != 0)
            {
                Service.SendLevelComplete(currentLevel, 0);
                Debug.Log($"Updated attempts {currentLevel.ID} {currentLevel.Creator}");
                Service.GetMetaList();
            }
            else
                Debug.Log("Current level not set, cant update attempts");
        }
        catch { }

        truck.GetComponent<HoverController>().enabled = true;
        truck.transform.position = camera.transform.position = origin;
        truck.transform.rotation = camera.transform.rotation = Quaternion.identity;
        truck.GetComponent<Rigidbody>().velocity = Vector3.zero;
        truck.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        startTime = Time.time;
        won = false;
    }
}

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
    public GameObject DeliveryBoxPrefab;
    public GameObject CameraPrefab;
    public GameObject WinPrefab;
    public TextMeshProUGUI Timer;
    public GameMenu GameMenu;
    public LoadingScreen LoadingScreen;

    private LevelMeta currentLevel;
    private float startTime;
    GameObject truck;
    BoxScript deliveryBox;
    new GameObject camera;
    Vector3 origin;
    private float timeAtFinish;

    bool won;
    private bool paused;

    private void Start()
    {
        StartRun();
    }
    public void StartRun()
    {
        IEnumerator Coroutine()
        {
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
                currentLevel = new LevelMeta
                {
                    ID = PersistentData.PlayerId,
                    Creator = PersistentData.PlayerName
                };
                Serializer.LevelData level = Service.DownloadLevel(currentLevel);
                data = JsonUtility.ToJson(level); //TODO: fix this
            }
            
            var generateIter = serializer.Generate(data);
            Debug.Log($"Generating {data}");
            while (generateIter.MoveNext())
            {
                float progress = (float)generateIter.Current;
                LoadingScreen.setProgress(progress);
                yield return null;
            }
            Camera.main.transform.position = SplineNoise3D.SplineLine[0].pos;
            bool wait = true;
            LoadingScreen.finished_fade_out = () => wait = false;
            LoadingScreen.fadeOut();
            while (wait)
                yield return null;

            yield return new WaitForSeconds(0.5f);

            DestroyImmediate(Camera.main.gameObject);
            origin = SplineNoise3D.SplineLine[0].pos;
            truck = Instantiate(TruckPrefab, origin, Quaternion.identity);
            deliveryBox = Instantiate(DeliveryBoxPrefab, origin, Quaternion.identity).GetComponent<BoxScript>();
            deliveryBox.transform.position += deliveryBox.Offset;
            deliveryBox.Owner = truck.GetComponent<HoverController>();
            camera = Instantiate(CameraPrefab, origin, Quaternion.identity);
            var lastSpline = SplineNoise3D.SplineLine[SplineNoise3D.SplineLine.Count - 1];
            GameObject win = Instantiate(WinPrefab, lastSpline.pos, Quaternion.identity);
            var winPoint = win.GetComponent<WinPoint>();
            winPoint.Radius = lastSpline.radius;
            winPoint.Manager = this;

            startTime = Time.time;

            if (currentLevel.AuthorTime == 0)
            {
                currentLevel.AuthorTime = 30;
                Debug.Log("No time set in level, defaulting to 30s");
            }

            yield return RunGame();
        }

        StartCoroutine(Coroutine());
    }

    private IEnumerator RunGame()
    {
        float t = 0.0f;
        while (t < currentLevel.AuthorTime || PersistentData.Validating)
        {
            t = Time.time - startTime;
            if (Timer)
            {
                Timer.enabled = true;
                Timer.text = $"{(!PersistentData.Validating ? (currentLevel.AuthorTime - t) : t):0}";
            }

            //TODO: remove when pause works
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }

            if (Input.GetButtonDown("Pause"))
            {
                Pause();
            }

            yield return null;
        }

        if(!won)
            Lose();
    }

    public void Win()
    {
        if (won)
            return;

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
                    AuthorTime = timeAtFinish * 1.1f,
                    RecordTime = timeAtFinish,
                    RecordName = PersistentData.PlayerName,
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
                    {
                        currentLevel.RecordName = PersistentData.PlayerName;
                        currentLevel.RecordTime = timeAtFinish;
                        Service.SendLevelComplete(currentLevel, 1);
                    }
                    else
                        Debug.Log("Current level not set, cant update attempts");
                }
                catch { }

                PersistentData.ResourceCount += currentLevel.Resource;
            }
            PersistentData.Validating = false;
            PersistentData.OverrideLevel = null;
            PersistentData.LevelMeta = null;
            SceneManager.LoadScene(0);
        }

        won = true;

        truck.GetComponent<HoverController>().enabled = false;
        StopAllCoroutines();

        GameMenu.Retry = () => { Restart(); StartCoroutine(RunGame()); };
        GameMenu.BackToMainMenu = () => FinishLevel();
  
        timeAtFinish = Time.time - startTime;
        GameMenu.Win(timeAtFinish, currentLevel.Resource);
    }

    public void Lose()
    {
        truck.GetComponent<HoverController>().enabled = false;
        StopAllCoroutines();

        GameMenu.Retry = () => { Restart(false); StartCoroutine(RunGame()); };
        GameMenu.BackToMainMenu = () => BackToMenu();

        GameMenu.Lose();

        try
        {
            if (currentLevel.ID != 0)
            {
                currentLevel.RecordTime = float.MaxValue;
                currentLevel.RecordName = PersistentData.PlayerName;
                Service.SendLevelComplete(currentLevel, 0);
                Debug.Log($"Updated attempts {currentLevel.ID} {currentLevel.Creator}");
                Service.GetMetaList();
            }
            else
                Debug.Log("Current level not set, cant update attempts");
        }
        catch { }
    }

    public void Pause()
    {
        paused = !paused;
        truck.GetComponent<HoverController>().enabled = !paused;

        Time.timeScale = paused ? 0 : 1;
        GameMenu.togglePause();
        GameMenu.Resume = () => { Time.timeScale = 1.0f; paused = false; truck.GetComponent<HoverController>().enabled = true; };
        GameMenu.Retry = () => { StopAllCoroutines(); Restart(); StartCoroutine(RunGame()); };
        GameMenu.BackToMainMenu = () => BackToMenu();
    }

    private void Restart(bool updateAttempt = true)
    {
        Time.timeScale = 1.0f;
        paused = false;

        try
        {
            if (currentLevel.ID != 0)
            {
                if(updateAttempt)
                {
                    currentLevel.RecordTime = float.MaxValue;
                    currentLevel.RecordName = PersistentData.PlayerName;
                    Service.SendLevelComplete(currentLevel, 0);
                    Debug.Log($"Updated attempts {currentLevel.ID} {currentLevel.Creator}");
                    Service.GetMetaList();
                }

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
        deliveryBox.transform.position = truck.transform.position + deliveryBox.Offset;
        deliveryBox.transform.rotation = truck.transform.rotation;
        startTime = Time.time;
        won = false;
    }

    private void BackToMenu()
    {
        if(PersistentData.Validating)
        {
            SceneManager.LoadScene(2);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }


    private void OnDestroy()
    {
        Time.timeScale = 1.0f;
    }
}

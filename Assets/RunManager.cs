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
    public GameObject WinCutscene;
    public TextMeshProUGUI Timer;
    public Image Timer_image;

    public GameMenu GameMenu;
    public LoadingScreen LoadingScreen;
    public LayerMask CollisionLayer;

    private GameObject winCutscene;
    public LevelMeta currentLevel;
    private float startTime;
    GameObject truck;
    BoxScript deliveryBox;
    new GameObject camera;
    Vector3 origin;
    private float timeAtFinish;
    private GameObject main_cam;
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
                Timer.enabled = Timer_image.enabled = false;
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
            if(Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 100000, CollisionLayer))
            {
                origin = hit.point + Vector3.up * 2;
            }
            truck = Instantiate(TruckPrefab, origin, Quaternion.identity);
            deliveryBox = Instantiate(DeliveryBoxPrefab, origin, Quaternion.identity).GetComponent<BoxScript>();
            deliveryBox.transform.position += deliveryBox.Offset;
            deliveryBox.Owner = truck.GetComponent<HoverController>();
            deliveryBox.GetComponent<Rigidbody>().isKinematic = true;
            camera = Instantiate(CameraPrefab, origin, Quaternion.identity);
            var cameraController = camera.GetComponent<CameraController>();
            cameraController.Target = truck.transform;
            camera.transform.position = origin + camera.GetComponent<CameraController>().TargetOffset;

            var lastSpline = SplineNoise3D.SplineLine[SplineNoise3D.SplineLine.Count - 1];
            GameObject win = Instantiate(WinPrefab, lastSpline.pos, Quaternion.identity);
            var winPoint = win.GetComponent<WinPoint>();
            winPoint.Radius = lastSpline.radius * 2f;
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
        yield return null;
        won = false;

        float t = 0.0f;
        while (t < currentLevel.AuthorTime || PersistentData.Validating)
        {
            t = Time.time - startTime;
            if (Timer)
            {
                Timer.enabled = Timer_image.enabled = true;
                Timer.text = $"{(!PersistentData.Validating ? (currentLevel.AuthorTime - t) : t):00.00}";
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

        //play cutscene
        winCutscene = Instantiate(WinCutscene, Vector3.up * 10000f, Quaternion.identity);
        truck.SetActive(false);
        camera.gameObject.SetActive(false);

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
                    Resource = 5
                };
                Debug.Log($"Uploading {JsonUtility.ToJson(meta)}");
                service.UploadLevel(meta, data);
                Debug.Log("Done Uploading");
                PersistentData.ResourceCount += PersistentData.ResourceDelta;
                PersistentData.ResourceDelta = 0;
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

                if(!PlayerPrefs.HasKey($"{currentLevel.Creator}+{currentLevel.ID}"))
                {
                    PersistentData.ResourceCount += currentLevel.Resource;
                    PlayerPrefs.SetInt($"{currentLevel.Creator}+{currentLevel.ID}", 1);
                }
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
        GameMenu.Win(timeAtFinish, !PlayerPrefs.HasKey($"{currentLevel.Creator}+{currentLevel.ID}") ? currentLevel.Resource : 0);
    }

    public void Lose(bool FailedDelivery = false)
    {
        if (won)
            return;

        truck.GetComponent<HoverController>().enabled = false;
        StopAllCoroutines();

        GameMenu.Retry = () => { Restart(false); StartCoroutine(RunGame()); };
        GameMenu.BackToMainMenu = () => BackToMenu();

        GameMenu.Lose(FailedDelivery);

        try
        {
            if (currentLevel.ID != 0 && !PersistentData.Validating)
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

        if(winCutscene != null)
            GameObject.Destroy(winCutscene);
        camera.gameObject.SetActive(true);
        truck.SetActive(true);
        truck.GetComponent<HoverController>().enabled = true;
        truck.transform.position = origin;
        camera.transform.position = origin + camera.GetComponent<CameraController>().TargetOffset;
        truck.transform.rotation = camera.transform.rotation = Quaternion.identity;
        truck.GetComponent<Rigidbody>().velocity = Vector3.zero;
        truck.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        deliveryBox.transform.position = truck.transform.position + deliveryBox.Offset;
        deliveryBox.transform.rotation = truck.transform.rotation;
        deliveryBox.Reset();
        startTime = Time.time;
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

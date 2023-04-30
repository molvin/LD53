using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
    public Serializer serializer;
    public ServiceTalker Service;
    public GameObject TruckPrefab;
    public GameObject CameraPrefab;
    public GameObject WinPrefab;

    private void Start()
    {
        StartRun();
    }
    public void StartRun()
    {
        IEnumerator Coroutine()
        {
            yield return new WaitForSeconds(0.5f);

            string data;
            Debug.Log($"Starting run {serializer}");
            if(PersistentData.LevelMeta != null)
            {
                Debug.Log("Reading from service");

                var meta = PersistentData.LevelMeta.Value;
                Serializer.LevelData level = Service.DownloadLevel(meta);
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
            Vector3 origin = SplineNoise3D.SplineLine[0].pos;
            Instantiate(TruckPrefab, origin, Quaternion.identity);
            Instantiate(CameraPrefab, origin, Quaternion.identity);
            var lastSpline = SplineNoise3D.SplineLine[SplineNoise3D.SplineLine.Count - 1];
            GameObject win = Instantiate(WinPrefab, lastSpline.pos, Quaternion.identity);
            var winPoint = win.GetComponent<WinPoint>();
            winPoint.Radius = lastSpline.radius;
            winPoint.Manager = this;
        }

        StartCoroutine(Coroutine());
    }

    public void Win()
    {
        Debug.Log("Finished Level");
        SceneManager.LoadScene(0);
    }
}

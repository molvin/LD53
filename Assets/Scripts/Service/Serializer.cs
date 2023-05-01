using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Serializer : MonoBehaviour
{
    [System.Serializable]
    public struct LevelData
    {
        public List<SplineNoise3D.Spline> SplineData;
    }

    public bool ShowDebugUI;
    public PointCloudManager PointCloud;

    private string debugSavePath = "levels/level.json";
    private string playerName = "tester";
    private bool running;
    private float progress = 1.0f;

    public string SerializeLevelToJson()
    {
        LevelData data = new LevelData{
            SplineData = SplineNoise3D.SplineLine
        };


        return JsonUtility.ToJson(data, true);
    }

    public IEnumerator InitializeFromJson(string json)
    {
        Debug.Log($"initializing: {json}");
        LevelData level = (LevelData) JsonUtility.FromJson(json, typeof(LevelData));
        SplineNoise3D.SplineLine = level.SplineData;
        if (level.SplineData.Count < 2)
            yield break;
        SplineNoise3D.Spline previous = level.SplineData[0];
        int resolution = 4;
        int current = 0;
        int iterations = (level.SplineData.Count - 1) * resolution;
        for(int splineIndex = 1; splineIndex < level.SplineData.Count; splineIndex++)
        {
            SplineNoise3D.Spline spline = level.SplineData[splineIndex];
            for(int i = 0; i < resolution; i++)
            {
                var lerpedSpline = SplineNoise3D.LerpSpline(previous, spline, i / (float)resolution);
                PointCloud.CreateIsoSurfaceSphere(lerpedSpline.pos, lerpedSpline.radius, SplineNoise3D.SplineNoise);
                yield return ++current / (float) iterations;
            }
            previous = spline;
        }
    }

    public IEnumerator Generate(string j)
    {
        PointCloud.Clear();
        running = true;
        var iter = InitializeFromJson(j);
        while (iter.MoveNext())
        {
            progress = (float)iter.Current;
            yield return progress;
        }
        running = false;
    }

    public void OnGUI()
    {
        if (!ShowDebugUI)
            return;
        debugSavePath = GUI.TextField(new Rect(20, 20, 200, 30), debugSavePath);
        if(GUI.Button(new Rect(220, 20, 110, 30), "Serialize To File"))
        {
            string data = SerializeLevelToJson();
            string path = $"{Application.dataPath}/{debugSavePath}";
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            File.WriteAllText(path, data);
        }

        if (GUI.Button(new Rect(20, 50, 110, 30), "Init From File"))
        {
            string path = $"{Application.dataPath}/{debugSavePath}";
            string data = File.ReadAllText(path);
            if(!running)
                StartCoroutine(Generate(data));
        }

        GUI.Label(new Rect(20, 90, 200, 30), $"Progress {progress:P0}");


        if (SplineNoise3D.SplineLine.Count > 1 && GUI.Button(new Rect(400, 50, 110, 30), "Upload"))
        {
            string data = SerializeLevelToJson();
            var service = FindObjectOfType<ServiceTalker>();
            LevelMeta meta = new LevelMeta
            {
                Wins = 0,
                Attempts = 0,
                AuthorTime = 60.0f,
                BestTime = 60.0f,
                RecordName = PersistentData.PlayerName,
                ID = PersistentData.PlayerId,
                Creator = PersistentData.PlayerName,
                Resource = 100
            };
            Debug.Log($"Uploading {data}");
            service.UploadLevel(meta, data);
            Debug.Log("Done Uploading");
        }
        if (SplineNoise3D.SplineLine.Count > 1 && GUI.Button(new Rect(400, 90, 110, 30), "Play and Upload"))
        {
            PersistentData.Validating = true;
            PersistentData.OverrideLevel = new LevelData {
                SplineData = SplineNoise3D.SplineLine
            };
            SceneManager.LoadScene(3);
        }
    }

}

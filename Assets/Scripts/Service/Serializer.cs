using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


public class Serializer : MonoBehaviour
{
    [System.Serializable]
    public struct LevelData
    {
        public Vector3 Start;
        public Vector3 End;
        public List<SplineNoise3D.Spline> SplineData;
    }

    public PointCloudManager PointCloud;

    private string debugSavePath = "levels/level.json";
    private bool running;
    private float progress = 1.0f;

    public string SerializeToJson()
    {
        LevelData data = new LevelData{
            Start = Vector3.zero,
            End = Vector3.one,
            SplineData = SplineNoise3D.SplineLine
        };


        return JsonUtility.ToJson(data, true);
    }

    public IEnumerator InitializeFromJson(string json)
    {
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

    public void OnGUI()
    {
        IEnumerator Generate(string j)
        {
            PointCloud.Clear();
            running = true;
            var iter = InitializeFromJson(j);
            while (iter.MoveNext())
            {
                progress = (float) iter.Current;
                yield return null;
            }
            running = false;
        }
        debugSavePath = GUI.TextField(new Rect(20, 20, 200, 30), debugSavePath);
        if(GUI.Button(new Rect(220, 20, 110, 30), "Serialize To File"))
        {
            string data = SerializeToJson();
            string path = $"{Application.dataPath}/{debugSavePath}";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, data);
        }

        if (GUI.Button(new Rect(20, 50, 110, 30), "Init From File"))
        {
            string path = $"{Application.dataPath}/{debugSavePath}";
            string data = System.IO.File.ReadAllText(path);
            if(!running)
                StartCoroutine(Generate(data));
        }
        GUI.Label(new Rect(20, 90, 200, 30), $"Progress {progress:P0}");
    }

}

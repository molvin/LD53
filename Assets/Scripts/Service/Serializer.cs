using System.Collections;
using System.Collections.Generic;
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

    private string debugSavePath = "levels/level.json";

    public string SerializeLevel()
    {
        LevelData data = new LevelData{
            Start = Vector3.zero,
            End = Vector3.one,
            SplineData = SplineNoise3D.SplineLine
        };

        return JsonUtility.ToJson(data, true);
    }

    public void OnGUI()
    {
        debugSavePath = GUI.TextField(new Rect(20, 20, 200, 30), debugSavePath);
        if(GUI.Button(new Rect(220, 20, 110, 30), "Serialize To File"))
        {
            string data = SerializeLevel();
            string path = $"{Application.dataPath}/{debugSavePath}";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, data);
        }
    }
}

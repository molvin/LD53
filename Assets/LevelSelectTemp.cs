using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectTemp : MonoBehaviour
{
    private void OnGUI()
    {
        int vertOffset = 0;
        foreach(string path in Directory.EnumerateFiles($"{Application.dataPath}/levels", "*.json"))
        {
            if(GUI.Button(new Rect(20, 20 + vertOffset, 300, 30), Path.GetFileName(path)))
            {
                PersistentData.LevelPath = path;
                SceneManager.LoadScene(3);
            }
            vertOffset += 30;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class LevelSelectHelper : MonoBehaviour
{
    public bool DebugUI;
    public ServiceTalker Service;

    private MetaFile? metaFile = null;

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        metaFile = Service.GetMetaList();
    }
    public List<LevelMeta> GetLevels()
    {
        return metaFile == null ? new List<LevelMeta>() : metaFile.Value.Levels; 
    }

    public void PlayLevel(LevelMeta meta)
    {
        PersistentData.LevelMeta = meta;
        SceneManager.LoadScene(3);
    }

    private void OnGUI()
    {
        if (!DebugUI)
            return;

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

        vertOffset = 0;
        if (GUI.Button(new Rect(350, 20 + vertOffset, 300, 30), "Refresh"))
            Refresh();

        if (metaFile == null)
            return;

        foreach (LevelMeta meta in GetLevels())
        {
            if (GUI.Button(new Rect(350, 80 + vertOffset, 300, 30), meta.Creator))
            {
                PlayLevel(meta);
            }
            vertOffset += 30;
        }

    }
}

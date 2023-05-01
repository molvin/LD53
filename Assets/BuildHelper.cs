using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildHelper : MonoBehaviour
{
    public ServiceTalker Service;
    public LevelMakerEditorController LevelEditor;
    public LevelMakerDoodadPlacer DoodadPlacer;

    private void Start()
    {
        Serializer.LevelData myLevel = new Serializer.LevelData();
        try
        {
            Debug.Log(PersistentData.PlayerId);
            Debug.Log(PersistentData.PlayerName);

            myLevel = Service.DownloadLevel(new LevelMeta
            {
                ID = PersistentData.PlayerId,
                Creator = PersistentData.PlayerName
            });

            Debug.Log($"My Level from server: {JsonUtility.ToJson(myLevel)}");


        }
        catch
        {
            Debug.Log("Player has no saved level");
        }

        LevelEditor.InitFromSpline(myLevel);
        DoodadPlacer.InitFromLevel(myLevel);

    }

}

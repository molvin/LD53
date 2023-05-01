using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildHelper : MonoBehaviour
{
    public ServiceTalker Service;
    public LevelMakerEditorController LevelEditor;

    private void Start()
    {
        try
        {
            Debug.Log(PersistentData.PlayerId);
            Debug.Log(PersistentData.PlayerName);

            Serializer.LevelData myLevel = Service.DownloadLevel(new LevelMeta
            {
                ID = PersistentData.PlayerId,
                Creator = PersistentData.PlayerName
            });

            Debug.Log($"My Level from server: {JsonUtility.ToJson(myLevel)}");

            SplineNoise3D.SplineLine = myLevel.SplineData;
            //TODO: set doodads
            LevelEditor.InitFromSpline();
        }
        catch
        {
            Debug.Log("Player has no saved level");
        }

    }

}

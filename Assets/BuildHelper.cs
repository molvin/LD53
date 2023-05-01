using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        if (PersistentData.Validating)
        {
            if(PersistentData.OverrideLevel.HasValue)
                myLevel = PersistentData.OverrideLevel.Value;

            PersistentData.Validating = false;
            PersistentData.OverrideLevel = null;
        }
        else
        {
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
        }


        LevelEditor.InitFromSpline(myLevel);
        DoodadPlacer.InitFromLevel(myLevel);

    }

}

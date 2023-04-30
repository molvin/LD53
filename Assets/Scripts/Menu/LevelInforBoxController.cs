using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInforBoxController : MonoBehaviour
{

    ServerStructs.LevelMeta data;

    public TMPro.TextMeshProUGUI Name;
    public TMPro.TextMeshProUGUI Wins;
    public TMPro.TextMeshProUGUI Attempts;
    public TMPro.TextMeshProUGUI Time;




    public void SetData(ServerStructs.LevelMeta data)
    {
        this.data = data;

        Name.text = data.Creator;
        Wins.text = "Wins: " + data.Wins;
        Attempts.text = "Attempts: " + data.Attempts;
        Time.text = "Record Time:" + data.Time;
       


    }




}

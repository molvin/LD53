using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInforBoxController : MonoBehaviour
{

    LevelMeta data;

    public TMPro.TextMeshProUGUI Name;
    public TMPro.TextMeshProUGUI Wins;
    public TMPro.TextMeshProUGUI Attempts;
    public TMPro.TextMeshProUGUI Time;

    public TMPro.TextMeshProUGUI Resources;



    public void SetData(LevelMeta data)
    {
        this.data = data;

        Name.text = data.Creator;
        Wins.text = "Wins: " + data.Wins;
        Attempts.text = "Attempts: " + data.Attempts;
        Time.text = "Time to beat: " + data.Time + " sec";
        Resources.text = "Resources to gain: " + data.Resource;
    }




}

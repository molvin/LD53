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
    public TMPro.TextMeshProUGUI Best_time;
    public TMPro.TextMeshProUGUI Champion;
    public TMPro.TextMeshProUGUI Resources;
 




    public void SetData(LevelMeta data)
    {
        this.data = data;

        Name.text = data.Creator;
        Wins.text = "Wins: " + data.Wins;
        Attempts.text = "Attempts: " + data.Attempts;
        Time.text = $"Time to beat: {data.AuthorTime:F2} sec";
        Best_time.text = $"Best time: {data.RecordTime:F2} sec";
        Champion.text = "Record holder: " + data.RecordName;

        if (!PlayerPrefs.HasKey($"{data.Creator}+{data.ID}"))
            Resources.text = "Reward for delivery: " + data.Resource;
        else
            Resources.text = "Delivery complete";
    }




}

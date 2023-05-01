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
        Time.text = "Time to beat: " + data.AuthorTime + " sec";
        Best_time.text = "Best time: " + data.RecordTime + " sec";
        Champion.text = "Record holder: " + data.RecordName;

        //TODO: set record name and best time
        Resources.text = "Resources to gain: " + data.Resource;
    }




}

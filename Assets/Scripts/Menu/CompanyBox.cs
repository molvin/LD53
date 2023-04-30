using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanyBox : MonoBehaviour
{

    public TMPro.TextMeshProUGUI company_name;
    public TMPro.TextMeshProUGUI resources;


    public void Start()
    {
        company_name.text = PersistentData.PlayerName;
        resources.text = "Resources" + PersistentData.ResourceCount;

    }

    public void goToEditor()
    {
      //  FindObjectOfType<LevelSelectHelper>(true).playEditor();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CompanyBox : MonoBehaviour
{

    public TMPro.TextMeshProUGUI company_name;
    public TMPro.TextMeshProUGUI resources;


    public void Start()
    {
        company_name.text = PersistentData.PlayerName;
        resources.text = "Resources: " + PersistentData.ResourceCount;

    }

    public void goToEditor()
    {
        SceneManager.LoadScene(2);

      //  FindObjectOfType<LevelSelectHelper>(true).playEditor();
    }

}

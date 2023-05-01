using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpMenu : MonoBehaviour
{
    public Button HelpButton;
    public List<GameObject> Objects;
    private bool toggled;

    private void Start()
    {
        HelpButton.onClick.AddListener(() => Toggle());
        if(PlayerPrefs.GetInt("Help", 0) == 0)
        {
            PlayerPrefs.SetInt("Help", 1);
            Toggle();
        }
    }

    private void Toggle()
    {
        toggled = !toggled;
        foreach(var go in Objects)
        {
            go.SetActive(toggled);
        }
    }
}

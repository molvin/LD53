using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{

    public PauseMenu pauseMenu;

    public WinLoseMenu winLoseMenu;

    public void Update()
    {
        if (Input.GetButtonDown("Pause") && !winLoseMenu.menuObj.gameObject.activeSelf)
        {
            togglePause();
        }

        if(Input.GetKeyDown(KeyCode.K))
        {
            Lose();
        }


        if (Input.GetKeyDown(KeyCode.L))
        {
            Win(26.4f, 8);
        }
    }



    public void Start()
    {
        winLoseMenu.Init(this);
        pauseMenu.Init(this);
    }

    public void togglePause()
    {
        pauseMenu.ToggleEnabled();
    }

    public void Lose()
    {
        winLoseMenu.Lose();
    }

    public void Win(float time, int resourcesGaind)
    {

        //TODO: check validating
        bool b = PersistentData.Validating;
        winLoseMenu.Win(time, resourcesGaind);

    }

    public System.Action Retry;
    public System.Action Resume;
    public System.Action BackToMainMenu;

    public void ExitGame()
    {
        Application.Quit();
    }

}

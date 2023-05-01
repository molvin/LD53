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
        winLoseMenu.Win(time, resourcesGaind);

    }

    public void Retry()
    {
       
        //TODO: per do you thinkg
    }
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject menu;
    public GameObject pauseMenuButtonGroup;
    public GameObject OptionMenuButtonGroup;
    public List<Animator> animators;
    public void Update()
    {
        if(Input.GetButton("Pause"))
        {
            menu.SetActive(!menu.activeSelf);
        }
    }

    public void Resume()
    {
        menu.SetActive(false);
    }

    public void Options()
    {
        TriggerTransitionAnim();
        StartCoroutine(ActivateAfterTime(OptionMenuButtonGroup, 1, true));
        pauseMenuButtonGroup.SetActive(false);

    }

    public void BackFromOptions()
    {
        TriggerTransitionAnim();
        StartCoroutine(ActivateAfterTime(pauseMenuButtonGroup, 1, true));
        OptionMenuButtonGroup.SetActive(false);
    }

    public void Restart()
    {

    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void TriggerTransitionAnim()
    {
        animators.ForEach(e => e.SetTrigger("Transition"));
    }

    public IEnumerator ActivateAfterTime(GameObject to_activate, float wait_time, bool state)
    {
        yield return new WaitForSeconds(wait_time);
        to_activate.SetActive(state);
    }
}

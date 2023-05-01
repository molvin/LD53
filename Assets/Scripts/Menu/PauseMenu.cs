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
    GameMenu parentMenu;
    public TMPro.TextMeshProUGUI back_to_menu_button_text;
    public void Init(GameMenu parentMenu)
    {
        this.parentMenu = parentMenu;
    }


    public void Resume()
    {
        NiceShutdown();
        StartCoroutine(ResumeAfterTime(1.2f));
    }

    public void NiceShutdown()
    {
        StopAllCoroutines();
        TriggerTransitionAnim();
        StartCoroutine(ActivateAfterTime(menu, 1, false));
        pauseMenuButtonGroup.SetActive(false);
    }

    public void ToggleEnabled()
    {
        back_to_menu_button_text.text = PersistentData.Validating ? "Back to builder" : "Back to menu";
        bool desierd_stade = !menu.activeSelf;
        Debug.Log(desierd_stade);
        if(desierd_stade)
        {
            menu.SetActive(true);
            pauseMenuButtonGroup.SetActive(false);
            BackFromOptions();
        }
        else
        {
            NiceShutdown();
        }
    }

    public void Options()
    {
        TriggerTransitionAnim();
        StopAllCoroutines();
        StartCoroutine(ActivateAfterTime(OptionMenuButtonGroup, 1, true));
        pauseMenuButtonGroup.SetActive(false);

    }

    public void BackFromOptions()
    {
        TriggerTransitionAnim();
        StopAllCoroutines();
        StartCoroutine(ActivateAfterTime(pauseMenuButtonGroup, 1, true));
        OptionMenuButtonGroup.SetActive(false);
    }

    public void Restart()
    {
        NiceShutdown();
        StartCoroutine(RetryAfterTime(1.2f));
    }

    public void BackToMainMenu()
    {
        NiceShutdown();
        StartCoroutine(MenuAfterTime(1));
    }

    public void ExitGame()
    {
        NiceShutdown();
        StartCoroutine(QutiAfterTime(1));
    }



    private void TriggerTransitionAnim()
    {
        animators.ForEach(e => e.SetTrigger("Transition"));
    }

    public IEnumerator ActivateAfterTime(GameObject to_activate, float wait_time, bool state)
    {
        yield return new WaitForSecondsRealtime(wait_time);
        to_activate.SetActive(state);
    }

    public IEnumerator MenuAfterTime(float wait_time)
    {
        yield return new WaitForSecondsRealtime(wait_time);
        parentMenu.BackToMainMenu();
    }
    public IEnumerator QutiAfterTime(float wait_time)
    {
        yield return new WaitForSecondsRealtime(wait_time);
        Application.Quit();
    }

    public IEnumerator RetryAfterTime(float wait_time)
    {
        yield return new WaitForSecondsRealtime(wait_time);
        parentMenu.Retry();
    }


    public IEnumerator ResumeAfterTime(float wait_time)
    {
        yield return new WaitForSecondsRealtime(wait_time);
        parentMenu.Resume();
    }
    
}

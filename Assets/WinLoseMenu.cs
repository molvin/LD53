using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static System.Net.Mime.MediaTypeNames;

public class WinLoseMenu : MonoBehaviour
{



    public TextMeshProUGUI time_text;
    public TextMeshProUGUI resources_text;

    public GameObject buttonGroup;
    public GameObject menuObj;
    public List<Animator> animators;
    GameMenu parentMenu;

    public TMPro.TextMeshProUGUI win_or_lose_text;



    public void Init(GameMenu parentMenu)
    {
        this.parentMenu = parentMenu;
        menuObj.SetActive(false);

    }

    public IEnumerator setTextAfterTime(float time, string text)
    {
        yield return new WaitForSecondsRealtime(time);
        win_or_lose_text.text = text;
    }


    public void Lose()
    {
        menuObj.SetActive(true);
        win_or_lose_text.text = "";

        HideTextStuff();
        TriggerTransitionAnim();
        StartCoroutine(ActivateAfterTime(buttonGroup, 1, true));
        StartCoroutine(setTextAfterTime(1, "You ran out of time"));


    }

    public void Win(float time, int resources_gained)
    {
        win_or_lose_text.text = "";

        menuObj.SetActive(true);
        HideTextStuff();
        TriggerTransitionAnim();
        StartCoroutine(ActivateAfterTime(buttonGroup, 1, true));
        StartCoroutine(ActivateAfterTime(time_text.gameObject, 1, true));
        StartCoroutine(ActivateAfterTime(resources_text.gameObject, 1, true));
        StartCoroutine(setTextAfterTime(1, "Dilivery compleated!"));


        time_text.text = "Your time: " + time;
        resources_text.text = "Resources gained: " + resources_gained;
    }

    public void HideTextStuff()
    {
        time_text.gameObject.SetActive(false);
        buttonGroup.SetActive(false);
        resources_text.gameObject.SetActive(false);

    }

    public void Retry()
    {
        HideTextStuff();
        TriggerTransitionAnim();
        
        StartCoroutine(ActivateAfterTime(menuObj, 1, false));

        StartCoroutine(RetryAfterTime(1));
    }

    public void ExitGame()
    {
        HideTextStuff();
        TriggerTransitionAnim();
        StartCoroutine(ActivateAfterTime(menuObj, 1, false));

        StartCoroutine(QuitAfterTime(1));
    }


    public void BackToMainMenu()
    {
        HideTextStuff();
        TriggerTransitionAnim();
        StartCoroutine(ActivateAfterTime(menuObj, 1, false));

        StartCoroutine(MenuAfterTime(1));
    }



    private void TriggerTransitionAnim()
    {
        parentMenu.menuAudioController.playButtonSwosh();
        animators.ForEach(e => e.SetTrigger("Transition"));
    }

    public IEnumerator ActivateAfterTime(GameObject to_activate, float wait_time, bool state)
    {
        yield return new WaitForSeconds(wait_time);
        to_activate.SetActive(state);
    }

    public IEnumerator MenuAfterTime(float wait_time)
    {
        yield return new WaitForSeconds(wait_time);
        parentMenu.BackToMainMenu();
    }

    public IEnumerator RetryAfterTime(float wait_time)
    {
        yield return new WaitForSeconds(wait_time);
        parentMenu.Retry();
    }

    public IEnumerator QuitAfterTime(float wait_time)
    {
        yield return new WaitForSeconds(wait_time);
        parentMenu.ExitGame();
    }


}

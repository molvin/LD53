using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public List<Animator> animators;

    public GameObject mainMenuButtonGroup;
    public GameObject optionsMenuButtonGroup;
    public GameObject LevelSelectMenuButtonGroup;

    public GameObject levelSelectThing;

    public GameObject creators;
    public GameObject name_select;
    public MenuAudioController menuAudioController;



    public void SetNameState()
    {
        name_select.SetActive(true);
        mainMenuButtonGroup.SetActive(false);

    }

    public void SubmitName()
    {

        string name = name_select.GetComponentInChildren<TMPro.TMP_InputField>().text;

        if (name != "Default" && name != "")
        {
            name_select.SetActive(false);
            PersistentData.PlayerName = name;
            MainToLevelSelectTransition();
        }
    }

    public IEnumerator ActivateAfterTime(GameObject to_activate, float wait_time, bool state)
    {
        yield return new WaitForSeconds(wait_time);
        to_activate.SetActive(state);
    }

    public void Play()
    {
        if (PersistentData.PlayerName == "Default")
        {
            SetNameState();
        } else
        {
            MainToLevelSelectTransition();
        }
    }

    public void Options()
    {
        mainMenuButtonGroup.SetActive(false);
        StartCoroutine(ActivateAfterTime(optionsMenuButtonGroup, 1, true));
        TriggerTransitionAnim();
        Debug.Log("options");
    }


    public void MainToLevelSelectTransition()
    {
        StartCoroutine(ActivateAfterTime(LevelSelectMenuButtonGroup, 1, true));
        StartCoroutine(ActivateAfterTime(levelSelectThing, 1, true));

        mainMenuButtonGroup.SetActive(false);
        creators.SetActive(false);
        MoveToLeftCornerAnim();

    }

    public void LevelSelectToMainTransition()
    {
        LevelSelectMenuButtonGroup.SetActive(false);
        StartCoroutine(ActivateAfterTime(mainMenuButtonGroup, 1, true));

        StartCoroutine(ActivateAfterTime(creators, 1, true));

        MoveToCenterAnim();
        levelSelectThing.SetActive(false);

    }


    public void BackToMainMenu()
    {
        optionsMenuButtonGroup.SetActive(false);
        mainMenuButtonGroup.SetActive(true);
        creators.SetActive(true);

        TriggerTransitionAnim();

        Debug.Log("back to menu");
    }

    public void Editor()
    {
        Debug.Log("editor");
    }

    public void Exit()
    {
        Debug.Log("quit");
        Application.Quit();
    }

    private void TriggerTransitionAnim()
    {
        menuAudioController.playButtonSwosh();
        animators.ForEach(e => e.SetTrigger("Transition"));
    }

    private void MoveToLeftCornerAnim()
    {
        animators.ForEach(e => e.SetTrigger("Left"));
        menuAudioController.playButtonSwosh();

    }

    private void MoveToCenterAnim()
    {
        animators.ForEach(e => e.SetTrigger("Center"));
        menuAudioController.playButtonSwosh();

    }

}

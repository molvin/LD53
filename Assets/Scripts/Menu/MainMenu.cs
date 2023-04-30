using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public List<Animator> animators;

    public GameObject mainMenuButtonGroup;
    public GameObject optionsMenuButtonGroup;
    public GameObject LevelSelectMenuButtonGroup;

    public GameObject levelSelectThing;

    public void Play()
    {
        MainToLevelSelectTransition();
        Debug.Log("play");
    }

    public void Options()
    {
        mainMenuButtonGroup.SetActive(false);
        optionsMenuButtonGroup.SetActive(true);

        TriggerTransitionAnim();
        Debug.Log("options");
    }


    public void MainToLevelSelectTransition()
    {
        LevelSelectMenuButtonGroup.SetActive(true);
        mainMenuButtonGroup.SetActive(false);
        levelSelectThing.SetActive(true);

        MoveToLeftCornerAnim();

    }

    public void LevelSelectToMainTransition()
    {
        LevelSelectMenuButtonGroup.SetActive(false);
        mainMenuButtonGroup.SetActive(true);
        MoveToCenterAnim();
        levelSelectThing.SetActive(false);

    }


    public void BackToMainMenu()
    {
        optionsMenuButtonGroup.SetActive(false);
        mainMenuButtonGroup.SetActive(true);

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
        animators.ForEach(e => e.SetTrigger("Transition"));
    }

    private void MoveToLeftCornerAnim()
    {
        animators.ForEach(e => e.SetTrigger("Left"));
    }

    private void MoveToCenterAnim()
    {
        animators.ForEach(e => e.SetTrigger("Center"));
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public List<Animator> animators;

    public GameObject mainMenuButtonGroup;
    public GameObject optionsMenuButtonGroup;

    public void Play()
    {
        Debug.Log("play");
    }

    public void Options()
    {
        mainMenuButtonGroup.SetActive(false);
        optionsMenuButtonGroup.SetActive(true);

        TriggerTransitionAnim();
        Debug.Log("options");
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
}

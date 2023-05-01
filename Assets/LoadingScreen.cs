using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LoadingScreen : MonoBehaviour
{
    public GameObject anim_object_group;
    public Image fade_image;
    //public List<Animator> animators;
    public AnimationCurve fade_curve;
    public bool start_as_hidden;
    public float fade_duration = 0.8f;

    private float time = 0;
    private int time_direction;



    public Action finished_fade_out;
    public Action finished_fade_in;

    public bool action_called = true;
    public void Update()
    {
        /*
        if(Input.GetKey(KeyCode.I))
        {
            fadeIn();
        }
        if (Input.GetKey(KeyCode.O))
        {
            fadeOut();
        }
        */
        time += Time.deltaTime * time_direction;
        time = Mathf.Clamp(time, 0, fade_duration);
        fade_image.color = new Color() { r = fade_image.color.r, g = fade_image.color.g, b = fade_image.color.b, a = fade_curve.Evaluate(time / fade_duration) };
        if(time == fade_duration)
        {
            anim_object_group.SetActive(true);
            if (!action_called)
            {
                finished_fade_in?.Invoke();
                action_called = true;
            }
        }
        else
        {
            anim_object_group.SetActive(false);
        }
        if(time == 0 && !action_called)
        {
           
            finished_fade_out?.Invoke();
            action_called = true;
           
        }
    }

    public void setProgress(float v)
    {

    }

    public void fadeIn()
    {
        time_direction = 1;
        action_called = false;
    }

    public void fadeOut()
    {
        time_direction = -1;
        action_called = false;
    }

    public void SetFadedOutState()
    {
        time = 0;
        time_direction = -1;
    }

    public void SetFadedInState()
    {
        time = fade_duration;
        time_direction = 1;

    }

    public void Start()
    {
        if (start_as_hidden) {
            SetFadedOutState();
        } else {
            SetFadedInState();
        }
    }


}

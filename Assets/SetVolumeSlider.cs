using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SetVolumeSlider : MonoBehaviour
{

    public AudioMixer mixer;
    public string floatName = "Master";
    public Slider slider;



    public void Start()
    {
        slider.SetValueWithoutNotify(1f);
    }
    public void SetLevel(float slider_value)
    {
        mixer.SetFloat(floatName, Mathf.Log10(slider_value) * 20);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DevOptions : MonoBehaviour
{
    public TMP_InputField IndexField;
    public LevelSelect levelSelect;
    private void Start()
    {
        if (!Application.isEditor)
        {
            gameObject.SetActive(false);
        }
    }

    public void MoveLevel()
    {
        int index = int.Parse(IndexField.text);
        levelSelect.MoveLevel(index);
    }
}

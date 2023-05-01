using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceCounter : MonoBehaviour
{
    public LevelMakerEditorController Controller;
    public TextMeshProUGUI Text;

    private void Update()
    {
        if(Text)
            Text.text = $"Resources: {Controller.CurrentResource}";
    }
}

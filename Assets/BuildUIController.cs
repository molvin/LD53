using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SplineNoise3D;

public class BuildUIController : MonoBehaviour
{
    public GameObject DoodadView;
    public GameObject GateView;
    public LevelMakerDoodadPlacer doodadPlacer;
    public LevelMakerEditorController gatePlacer;
    public List<GameObject> doodads;
    public int UiLayer;

    public void Start()
    {
        Button[] doodads = DoodadView.GetComponentsInChildren<Button>(true);
        for(int i=0;i< doodads.Length; i++)
        {
            int x = i;
            doodads[i].onClick.AddListener(() => setPlacer(x));
        }

        Button[] gates = GateView.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < doodads.Length; i++)
        {
            int x = i;
            doodads[i].onClick.AddListener(() => setPlacer(x));
        }
    }

    public void Update()
    {
        bool usingUi = IsPointerOverUIElement(GetEventSystemRaycastResults());
        gatePlacer.IsUsingUI = usingUi;
        doodadPlacer.IsUsingUI = usingUi;
        NumPress();
    }
    public void PressDoodad()
    {
        gatePlacer.DisableEdit = true;
        doodadPlacer.enabled = true;
        DoodadView.SetActive(true);
        GateView.SetActive(false);
    }
    public void PressGate()
    {
        gatePlacer.DisableEdit = false;
        doodadPlacer.enabled = false;
        DoodadView.SetActive(false);
        GateView.SetActive(true);
    }
    private void setPlacer(int value)
    {
        Debug.Log("Set placer" + value);
        if (!gatePlacer.DisableEdit)
        {
            gatePlacer.SetCurrentSelection(value);
        }
        else
        {
            if (value >= doodads.Count) return;
            doodadPlacer.SetDoodad(value);
        }      
    }
    private void NumPress()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            setPlacer(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            setPlacer(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            setPlacer(2);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            setPlacer(3);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            setPlacer(4);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            setPlacer(5);
        if (Input.GetKeyDown(KeyCode.Alpha7))
            setPlacer(6);
        if (Input.GetKeyDown(KeyCode.Alpha8))
            setPlacer(7);
        if (Input.GetKeyDown(KeyCode.Alpha9))
            setPlacer(8);
        if (Input.GetKeyDown(KeyCode.Alpha0))
            setPlacer(9);
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UiLayer)
                return true;
        }
        return false;
    }
    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}

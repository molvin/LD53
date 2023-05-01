using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMakerDoodadPlacer : MonoBehaviour
{
    private int CurrentDoodad;
    public List<GameObject> Doodads;
    private GameObject doodadPrefab;
    private GameObject shadowObject;
    public LayerMask ConnectorMask;
    public LayerMask DoodadMask;
    public bool IsUsingUI;

    // Update is called once per frame
    public void SetDoodad(int doodad)
    {
        doodadPrefab = Doodads[doodad];
        if (shadowObject != null)
            GameObject.Destroy(shadowObject);
        shadowObject = Instantiate(doodadPrefab);
        shadowObject.GetComponentInChildren<Doodad>().ID = doodad;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (shadowObject != null)
                GameObject.Destroy(shadowObject);
            shadowObject = null;
        }
        RemoveOld();
        UpdateShadow();
    }
    private void UpdateShadow()
    {
        if (IsUsingUI) return;
        Vector3 p1 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        Vector3 p2 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100));
        Vector3 direction = p2 - p1;
        RaycastHit hit;
        Physics.Raycast(p1, direction.normalized, out hit, 100000f, ConnectorMask);

        //is on spline
        if (shadowObject == null) return;
        if (hit.collider != null)
        {
            shadowObject.SetActive(true);
            //add a new one
            SplineNoise3D.Spline s = SplineNoise3D.getLerpSplineFromPoint(hit.point);
            shadowObject.transform.position = s.pos;
            if (Input.GetMouseButtonDown(0))
            {
                shadowObject = null;
                SetDoodad(doodadPrefab);
            }
        }
        else
        {
            shadowObject.SetActive(false);
            if (Input.GetMouseButtonDown(0))
            {
                if (shadowObject != null)
                    GameObject.Destroy(shadowObject);
                shadowObject = null;
            }
        }
    }
    private void RemoveOld()
    {
        if (IsUsingUI) return;
        Vector3 p1 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        Vector3 p2 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100));
        Vector3 direction = p2 - p1;
        RaycastHit hit;
        Physics.Raycast(p1, direction.normalized, out hit, 100000f, DoodadMask);

        //is on spline
        if (hit.collider != null)
        {
            //remove an old doodad
            if (Input.GetMouseButtonDown(0) && shadowObject == null)
            {
                if (shadowObject != null)
                    GameObject.Destroy(shadowObject);
                shadowObject = hit.collider.gameObject;
            }
        }
    }
}

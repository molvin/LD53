using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMakerDoodadPlacer : MonoBehaviour
{

    private GameObject doodadPrefab;
    private GameObject shadowObject;
    public LayerMask ConnectorMask;
    public LayerMask DoodadMask;
    public bool IsUsingUI;

    // Update is called once per frame
    public void SetDoodad(GameObject DoodadPrefab)
    {
        doodadPrefab = DoodadPrefab;
        if (shadowObject != null)
            GameObject.Destroy(shadowObject);
        shadowObject = Instantiate(doodadPrefab);
    }

    void Update()
    {
        if (IsUsingUI) return;
        Vector3 p1 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        Vector3 p2 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100));
        Vector3 direction = p2 - p1;
        RaycastHit hit;
        Physics.Raycast(p1, direction.normalized, out hit, 100000f, ConnectorMask);

        //is on spline
        if(hit.collider != null)
        {
            //remove an old doodad
            if(hit.collider.gameObject.GetComponentInChildren<Doodad>() != null && Input.GetMouseButtonDown(0))
            {
                if(shadowObject != null)
                    GameObject.Destroy(shadowObject);
                shadowObject = hit.collider.gameObject;
            }

            //add a new one
            if (shadowObject == null) return;
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
            if (Input.GetMouseButtonDown(0))
            {
                if (shadowObject != null)
                    GameObject.Destroy(shadowObject);
            }
        }
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
        if (hit.collider != null)
        {
            //add a new one
            if (shadowObject == null) return;
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
            if (Input.GetMouseButtonDown(0) && hit.collider.gameObject != shadowObject)
            {
                if (shadowObject != null)
                    GameObject.Destroy(shadowObject);
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
            if (Input.GetMouseButtonDown(0))
            {
                if (shadowObject != null)
                    GameObject.Destroy(shadowObject);
                shadowObject = hit.collider.gameObject;
            }
        }
    }
}

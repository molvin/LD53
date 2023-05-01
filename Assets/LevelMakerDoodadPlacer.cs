using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMakerDoodadPlacer : MonoBehaviour
{

    public GameObject DoodadPrefab;
    public GameObject ShadowObject;
    public LayerMask ConnectorMask;

    // Update is called once per frame
    void Update()
    {
        Vector3 p1 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        Vector3 p2 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100));
        Vector3 direction = p2 - p1;
        RaycastHit hit;
        Physics.Raycast(p1, direction.normalized, out hit, 100000f, ConnectorMask);

        //is on spline
        if(hit.collider != null)
        {
            SplineNoise3D.Spline s = SplineNoise3D.getLerpSplineFromPoint(hit.point);
        }
    }
}

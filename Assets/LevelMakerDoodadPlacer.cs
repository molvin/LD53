using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMakerDoodadPlacer : MonoBehaviour
{

    private GameObject doodadPrefab;
    private GameObject shadowObject;
    public LayerMask ConnectorMask;

    // Update is called once per frame
    public void SetDoodad(GameObject DoodadPrefab)
    {
        Debug.Log("Selected a doodad");
        doodadPrefab = DoodadPrefab;
        if (shadowObject != null)
            GameObject.Destroy(shadowObject);
        shadowObject = Instantiate(doodadPrefab);
    }

    void Update()
    {
        if (shadowObject == null) return;
        Vector3 p1 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        Vector3 p2 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100));
        Vector3 direction = p2 - p1;
        Debug.DrawLine(p1, p2);
        RaycastHit hit;
        Physics.Raycast(p1, direction.normalized, out hit, 100000f, ConnectorMask);

        //is on spline
        if(hit.collider != null)
        {
            Debug.DrawLine(p1, hit.point, Color.magenta);
            SplineNoise3D.Spline s = SplineNoise3D.getLerpSplineFromPoint(hit.point);
            shadowObject.transform.position = s.pos;
            shadowObject.transform.rotation = s.rot;

            if (Input.GetMouseButtonDown(1))
            {
                shadowObject = null;
                SetDoodad(doodadPrefab);
            }
        }
    }
}

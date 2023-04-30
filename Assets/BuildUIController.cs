using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildUIController : MonoBehaviour
{
    public GameObject DoodadPrefab;
    public GameObject ShadowObject;

    public void Update()
    {

    }

    public void GetPointOnSpline()
    {

    }

    public void derp(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        Vector3 V1 = p2 - p1;
        Vector3 V2 = p4 - p3;
        Vector3 V21 = p2 - p1;

        float v22 = np.dot(V2, V2)
        float v11 = np.dot(V1, V1)
        float v21 = np.dot(V2, V1)
        float v21_1 = np.dot(V21, V1)
        float v21_2 = np.dot(V21, V2)
        float denom = v21 * v21 - v22 * v11



        /*
        def closest_line_seg_line_seg(p1, p2, p3, p4):
    P1 = p1
    P2 = p3
    V1 = p2 - p1
    V2 = p4 - p3
    V21 = P2 - P1

    v22 = np.dot(V2, V2)
    v11 = np.dot(V1, V1)
    v21 = np.dot(V2, V1)
    v21_1 = np.dot(V21, V1)
    v21_2 = np.dot(V21, V2)
    denom = v21 * v21 - v22 * v11

    if np.isclose(denom, 0.):
        s = 0.
        t = (v11 * s - v21_1) / v21
    else:
        s = (v21_2 * v21 - v22 * v21_1) / denom
        t = (-v21_1 * v21 + v11 * v21_2) / denom

    s = max(min(s, 1.), 0.)
    t = max(min(t, 1.), 0.)

    p_a = P1 + s * V1
    p_b = P2 + t * V2

    return p_a, p_b
        */
    }
}

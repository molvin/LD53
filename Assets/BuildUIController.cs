using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SplineNoise3D;

public class BuildUIController : MonoBehaviour
{
    public GameObject DoodadPrefab;
    public GameObject ShadowObject;

    public void Update()
    {
        Spline s = GetPointOnSpline();
        Vector3 p1 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        Debug.DrawLine(p1, s.pos, Color.black);
    }

    public Spline GetPointOnSpline()
    {
        Vector3 p1 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        Vector3 p2 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
        Vector3 delta = p2 - p1;
        
        float closest = float.MaxValue;
        Spline splie = new Spline { };
        for (int i=0; i < SplineLine.Count; i++)
        {
            Vector3 delta2 = SplineLine[i].pos - p1;
            //float distance = Vector3.Dot(delta.normalized, delta2);
            float distance = delta2.magnitude;
            Vector3 testPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
            Debug.DrawLine(p1, testPoint, Color.magenta);
            Spline s = getLerpSplineFromPoint(testPoint);
            if (closest > (testPoint - s.pos).magnitude)
            {
                closest = (testPoint - s.pos).magnitude;
                splie = s;
            }
        }
        return splie;
    }

    public Vector3 getClosestPointBetweenTwoLines(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        Vector3 P1 = p1;
        Vector3 P2 = p3;
        Vector3 V1 = p2 - p1;
        Vector3 V2 = p4 - p3;
        Vector3 V21 = P2 - P1;

        float v22 = Vector3.Dot(V2, V2);
        float v11 = Vector3.Dot(V1, V1);
        float v21 = Vector3.Dot(V2, V1);
        float v21_1 = Vector3.Dot(V21, V1);
        float v21_2 = Vector3.Dot(V21, V2);
        float denom = v21 * v21 - v22 * v11;

        //s and t
        float s;
        float t;
        if (Mathf.Approximately(denom, 0f))
        {
            s = 0f;
            t = (v11 * s - v21_1) / v21;
        }
        else
        {
            s = (v21_2 * v21 - v22 * v21_1) / denom;
            t = (-v21_1 * v21 + v11 * v21_2) / denom;
        }

        s = Mathf.Max(Mathf.Min(s, 1f), 0f);
        t = Mathf.Max(Mathf.Min(t, 1f), 0f);

        Vector3 p_a = P1 + s * V1;
        Vector3 p_b = P2 + t * V2;
        Debug.DrawLine(p_a, p_b);
        return p_a;
    }
}

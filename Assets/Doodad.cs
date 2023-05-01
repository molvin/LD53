using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doodad : MonoBehaviour
{
    public int ID;
    public bool NonSerialized;
    void Update()
    {
        SplineNoise3D.Spline s = SplineNoise3D.getLerpSplineFromPoint(transform.position);
        Debug.DrawLine(s.pos, s.pos + Vector3.up * 10f, Color.black);
        transform.position = s.pos;
        transform.rotation = s.rot;
        transform.localScale = Vector3.one * s.radius;
    }
}

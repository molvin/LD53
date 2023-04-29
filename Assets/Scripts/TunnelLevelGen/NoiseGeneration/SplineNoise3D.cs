using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineNoise3D
{
    public static List<Spline> SplineLine = new List<Spline>();
    public static List<Spline> SplineHole = new List<Spline>();
    public static float SplineNoise(Vector3 point)
    {
        return (getPointOnSpline(point) - point).magnitude;
    }
    public static float SplineDistance(Vector3 point)
    {
        float smaletsFound = float.MaxValue;
        //OnLine
        for (int i = 0; i < SplineLine.Count - 1; i++)
        {
            if (IsOnLine(SplineLine[i].pos, SplineLine[i + 1].pos, point))
            {
                float value = LineDistance(SplineLine[i].pos, SplineLine[i + 1].pos, point);
                if (value < smaletsFound)
                    smaletsFound = value;
            } 
        }
        if (smaletsFound != float.MaxValue)
            return smaletsFound;

        //InJoint
        for (int i = 1; i < SplineLine.Count - 1; i++)
        {
            float hypo = Vector3.Distance(point, SplineLine[i].pos);
            if (hypo < SplineLine[i].radius)
                return (LineDistance(SplineLine[i - 1].pos, SplineLine[i].pos, point) + LineDistance(SplineLine[i].pos, SplineLine[i + 1].pos, point)) / 2f;
        }
        return 100;
    }
    public static float HoleNoise(Vector3 point, int splineWindow = 10)
    {
        float smaletsFound = float.MaxValue;
        int min = Mathf.Max(0, SplineHole.Count - splineWindow);
        //OnLine
        for (int i = min; i < SplineHole.Count - 1; i++)
        {
            if (IsOnLine(SplineHole[i].pos, SplineHole[i + 1].pos, point))
            {
                float value = LineDistance(SplineHole[i].pos, SplineHole[i + 1].pos, point);
                if (value < smaletsFound)
                    smaletsFound = value;
            }
        }
        if (smaletsFound != float.MaxValue)
            return smaletsFound;
        //InJoint
        for (int i = 1; i < SplineHole.Count - 1; i++)
        {
            float hypo = Vector3.Distance(point, SplineHole[i].pos);
            if (smaletsFound > hypo)
                smaletsFound = hypo;
            
            //if (hypo < SplineHole[i].radius)
              //  return (LineDistance(SplineHole[i - 1].pos, SplineHole[i].pos, point) + LineDistance(SplineHole[i].pos, SplineHole[i + 1].pos, point)) / 2f;
        }
        return smaletsFound;
    }

    public static float LineDistance(Vector3 lineA, Vector3 lineB, Vector3 pointC)
    {
        Vector3 AC = pointC - lineB;
        Vector3 AB = lineB - lineA;
        return Vector3.Cross(AC, AB).magnitude / AB.magnitude;
    }
    public static float distanceOnLine(Vector3 lineA, Vector3 lineB, Vector3 pointC)
    {
        Vector3 CA = pointC - lineA;
        Vector3 BA = lineB - lineA;
        return Vector3.Dot(CA, BA.normalized);
    }
    public static float distanceOnSpline(Vector3 pointC)
    {
        float total = 0f;
        for (int i = 0; i < SplineLine.Count - 1; i++)
        {
            float maxDist = (SplineLine[i].pos - SplineLine[i + 1].pos).magnitude;
            float dis = distanceOnLine(SplineLine[i].pos, SplineLine[i + 1].pos, pointC);
            if (dis > maxDist)
            {
                total += maxDist;
            }
            else
            {
                total += dis;
                return total;
            }
        }
        return 0f;
    }
    public static Vector3 getPointOnSpline(Vector3 pointC)
    {
        for (int i = 0; i < SplineLine.Count - 1; i++)
        {
            if (IsOnLine(SplineLine[i].pos, SplineLine[i + 1].pos, pointC))
            {
                float dis = LineDistance(SplineLine[i].pos, SplineLine[i + 1].pos, pointC);
                Vector3 lineDir = (SplineLine[i + 1].pos - SplineLine[i].pos).normalized;
                return SplineLine[i].pos + lineDir * dis;
            }
        }
        //InJoint
        float shortest = float.MaxValue;
        Vector3 joint = SplineLine[SplineLine.Count - 1].pos;
        for (int i = 0; i < SplineHole.Count; i++)
        {
            float hypo = Vector3.Distance(pointC, SplineHole[i].pos);
            if (shortest > hypo)
            {
                joint = SplineHole[i].pos;
                shortest = hypo;
            }
        }
        return joint;
    }
    public static bool IsOnLine(Vector3 lineA, Vector3 lineB, Vector3 pointC)
    {
        float alongLine = distanceOnLine(lineA, lineB, pointC);
        Vector3 BA = lineB - lineA;
        return alongLine > 0f && alongLine <= BA.magnitude;
    }
    //Spline stuff
    public static void AddSplineSegment(Transform trans, float radius)
    {
        SplineLine.Add(new Spline { pos = trans.position, radius = radius, up = trans.up });
        SplineHole.Add(new Spline { pos = trans.position, radius = radius, up = trans.up });
    }
    public static void AddSplineSegment(Vector3 pos, float radius)
    {
        SplineLine.Add(new Spline { pos = pos, radius = radius, up = Vector3.up });
        SplineHole.Add(new Spline { pos = pos, radius = radius, up = Vector3.up });
    }
    public static Spline LearpSpline(Spline a, Spline b, float t)
    {
        return new Spline
        {
            radius = Mathf.Lerp(a.radius, b.radius, t),
            up = Vector3.Slerp(a.up, b.up, t),
            downFactor = Mathf.Lerp(a.downFactor, b.downFactor, t),
            rightFactor = Mathf.Lerp(a.rightFactor, b.rightFactor, t),
            upFactor = Mathf.Lerp(a.upFactor, b.upFactor, t),
            leftFactor = Mathf.Lerp(a.leftFactor, b.leftFactor, t)
        };
    }
    public struct Spline
    {
        public Vector3 pos;
        public float radius;
        public Vector3 up;
        public float downFactor;
        public float rightFactor;
        public float upFactor;
        public float leftFactor;
    }
}

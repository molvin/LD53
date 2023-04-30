using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineNoise3D
{
    private static float penaltySize = 5f;
    public static List<Spline> SplineLine = new List<Spline>();
    public static float SplineNoise(Vector3 point)
    {
        Spline spline = getLerpSplineFromPoint(point);

        float tunnel = (point - spline.pos).magnitude / spline.radius;
        float wall = wallNoise(spline, point);
        return Mathf.Max(tunnel, wall);
    }
    public static float wallNoise(Spline spline, Vector3 point)
    {
        Vector3 delta = point - spline.pos;
        return Mathf.Max(
            Mathf.Max(wallCalculate(spline.down, spline.radius, delta), penaltyWallCalculate(spline.right, spline.radius, delta, point)),
            Mathf.Max(penaltyWallCalculate(spline.left, spline.radius, delta, point), penaltyWallCalculate(spline.up, spline.radius, delta, point))
        );
    }
    public static float wallCalculate(Vector3 dir, float radius, Vector3 deltaPos)
    {
        float inDown = Vector3.Dot(dir.normalized, deltaPos);
        if (inDown < 0f)
            return 0f;
        float floor = (1f - dir.magnitude) * radius;
        return floor == 0 ? float.MaxValue : inDown / floor;
    }
    public static float penaltyWallCalculate(Vector3 dir, float radius, Vector3 deltaPos, Vector3 pos)
    {
        float inDown = Vector3.Dot(dir.normalized, deltaPos);
        if (inDown < 0f)
            return 0f;
        float floor = (1f - dir.magnitude) * radius + ((0.5f - Perlin3D.PerlinNoise3D(pos)) * penaltySize);
        return floor == 0 ? float.MaxValue : inDown / floor;
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
  

    public static float LineDistance(Vector3 lineA, Vector3 lineB, Vector3 pointC)
    {
        Vector3 AB = lineB - lineA;
        Vector3 AC = pointC - lineA;
        return Vector3.Dot(AB.normalized, AC);
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
        Vector3 closest = Vector3.zero;
        float shortest = float.MaxValue;
        for (int i = 0; i < SplineLine.Count - 1; i++)
        {
            if (IsOnLine(SplineLine[i].pos, SplineLine[i + 1].pos, pointC))
            {
                float dis = LineDistance(SplineLine[i].pos, SplineLine[i + 1].pos, pointC);
                Vector3 lineDir = (SplineLine[i + 1].pos - SplineLine[i].pos).normalized;
                Vector3 closePoint = SplineLine[i].pos + lineDir * dis;
                float d = (closePoint - pointC).magnitude;
                if (d < shortest)
                {
                    closest = closePoint;
                    shortest = d;
                }
            }
        }
        //InJoint
        for (int i = 0; i < SplineLine.Count; i++)
        {
            float hypo = Vector3.Distance(pointC, SplineLine[i].pos);
            if (hypo < shortest)
            {
                closest = SplineLine[i].pos;
                shortest = hypo;
            }
        }
        return closest;
    }
    public static Spline getLerpSplineFromPoint(Vector3 pointC)
    {
        Spline closest = new Spline { };
        float shortest = float.MaxValue;
        for (int i = 0; i < SplineLine.Count - 1; i++)
        {
            if (IsOnLine(SplineLine[i].pos, SplineLine[i + 1].pos, pointC))
            {

                float dis = LineDistance(SplineLine[i].pos, SplineLine[i + 1].pos, pointC);
                float factor = dis / (SplineLine[i].pos - SplineLine[i + 1].pos).magnitude;
                Spline s = LerpSpline(SplineLine[i], SplineLine[i + 1], factor);
                float d = (s.pos - pointC).magnitude;
                if (d < shortest)
                {
                    closest = s;
                    shortest = d;
                }
            }
        }
        //InJoint
        for (int i = 0; i < SplineLine.Count; i++)
        {
            float hypo = Vector3.Distance(pointC, SplineLine[i].pos);
            if (hypo < shortest)
            {
                closest = SplineLine[i];
                shortest = hypo;
            }
        }
        return closest;
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
        SplineLine.Add(new Spline { 
            pos = trans.position, 
            radius = radius,
            up = trans.up * 0.5f,
            down = Vector3.zero,
            right = Vector3.zero,
            left = Vector3.zero
        });
    }
    public static void AddSplineSegment(Vector3 pos, float radius)
    {
        SplineLine.Add(new Spline { pos = pos, radius = radius, up = Vector3.up });
    }
    public static void AddSplineSegment(Vector3 pos, Quaternion rot, float radius, Vector4 Roundness)
    {
        SplineLine.Add(new Spline {
            pos = pos,
            radius = radius,
            up = rot * Vector3.up * Roundness.x,
            right = rot * Vector3.right * Roundness.y,
            down = rot * Vector3.down * Roundness.z,
            left = rot * Vector3.left * Roundness.w
        });
    }
    public static void InsertSplineSegment(int index, Vector3 pos, Quaternion rot, float radius, Vector4 Roundness)
    {
        SplineLine.Insert(index, new Spline {
            pos = pos,
            radius = radius,
            up = rot * Vector3.up * Roundness.x,
            right = rot * Vector3.right * Roundness.y,
            down = rot * Vector3.down * Roundness.z,
            left = rot * Vector3.left * Roundness.w
        });
    }
    public static Spline LerpSpline(Spline a, Spline b, float t)
    {
        return new Spline
        {
            pos = Vector3.Lerp(a.pos, b.pos, t),
            radius = Mathf.Lerp(a.radius, b.radius, t),
            up = Vector3.Slerp(a.up, b.up, t),
            down = Vector3.Slerp(a.down, b.down, t),
            right = Vector3.Slerp(a.right, b.right, t),
            left = Vector3.Slerp(a.left, b.left, t)
        };
    }
    
    [System.Serializable]
    public struct Spline
    {
        public Vector3 pos;
        public float radius;
        public Vector3 up;
        public Vector3 right;
        public Vector3 down;
        public Vector3 left;
    }
}

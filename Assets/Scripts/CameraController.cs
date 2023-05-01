using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;
    public Vector3 TargetOffset;
    public Vector3 LookOffset;
    public float Smoothing;
    public float SplineWeight = 0.3f;
    public float SplineLookAheadDist = 10.0f;
    public int Steps = 10;
    public float StepLength = 1.0f;
    private Vector3 velocity;

    private void Start()
    {
        Target = FindObjectOfType<HoverController>().transform;
    }

    private void FixedUpdate()
    {
        //TODO: camera collision with walls?

        Vector3 targetPos = Target.transform.position + Target.localRotation * TargetOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, Smoothing * Time.fixedDeltaTime);

        var spline = SplineNoise3D.getLerpSplineFromPoint(Target.position);
        Vector3 splinePos = spline.pos;
        Vector3 prev = spline.pos;
        for (int i = 0; i < Steps; i++)
        {
            splinePos += spline.toNext * StepLength;
            Debug.DrawLine(prev, splinePos, Color.magenta);
            prev = splinePos;

            spline = SplineNoise3D.getLerpSplineFromPoint(splinePos);
        }

        Vector3 lookPos = Vector3.Lerp(Target.transform.position + Target.localRotation * LookOffset, splinePos, 1f - Mathf.Pow(SplineWeight, Time.fixedDeltaTime));
        transform.LookAt(lookPos, Vector3.up);
    }

}

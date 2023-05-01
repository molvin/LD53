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

        Vector3 splinePos = Target.position;
        Vector3 prev = splinePos;
        for (int i = 0; i < Steps; i++)
        {
            var spline = SplineNoise3D.getLerpSplineFromPoint(splinePos);
            splinePos += spline.rot * Vector3.forward * StepLength;
            Debug.DrawLine(prev, splinePos, Color.magenta);
            prev = splinePos;
        }

        Vector3 lookPos = 
            (Target.transform.position + Target.localRotation * LookOffset) * (1 - SplineWeight) +
            splinePos * SplineWeight;
        transform.LookAt(lookPos, Vector3.up);
    }

}

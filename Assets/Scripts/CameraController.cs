using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;
    public Vector3 TargetOffset;
    public Vector3 LookOffset;
    public float Smoothing;
    public float SplineWeight = 0.2f;
    public float LerpSpeed = 0.1f;
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
        Vector3 targetProj = spline.pos + spline.rot * Vector3.down * spline.radius;

        Vector3 ExpectedCameraPos = Target.position + Target.rotation * TargetOffset;
        Quaternion forward = Quaternion.LookRotation((targetProj - ExpectedCameraPos).normalized, Vector3.up);
        Vector3 targetPos = Target.transform.position + forward * TargetOffset;
        Vector3 desired = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, Smoothing * Time.fixedDeltaTime);
        Vector3 toDesired = (desired - Target.transform.position);
        if (Physics.SphereCast(Target.transform.position, 0.5f, toDesired.normalized, out RaycastHit Hit))
        {
            if (Hit.distance < toDesired.magnitude)
            {
                desired = Target.transform.position + toDesired.normalized * Hit.distance;
            }
        }
        transform.position = desired;

        Vector3 lookPos = Vector3.Lerp(Target.transform.position + Target.localRotation * LookOffset, targetProj, SplineWeight);
        Quaternion targetRot = Quaternion.LookRotation((lookPos - transform.position).normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, LerpSpeed);
    }

}

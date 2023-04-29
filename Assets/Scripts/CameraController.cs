using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;
    public Vector3 TargetOffset;
    public Vector3 LookOffset;
    public float Smoothing;
    private Vector3 velocity;


    private void FixedUpdate()
    {

        Vector3 targetPos = Target.transform.position + Target.localRotation * TargetOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, Smoothing * Time.fixedDeltaTime);

        Vector3 lookPos = Target.transform.position + Target.localRotation * LookOffset;
        transform.LookAt(lookPos, Vector3.up);
    }

}

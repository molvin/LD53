using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 Offset;
    public float Smoothing;
    public Transform Target;
    private Vector3 velocity;

    private void LateUpdate()
    {
        Vector3 targetPos = Target.transform.position + Target.localRotation * Offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, Smoothing * Time.deltaTime);
        transform.LookAt(Target, Vector3.up);
    }

}

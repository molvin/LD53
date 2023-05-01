using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxScript : MonoBehaviour
{
    public HoverController Owner;

    public float Damping = 3.0f;
    public float LerpDistance = 0.1f;
    public float MaxDistance = 1.0f;
    public float MaxDeltaVelocity = 50.0f;

    private Vector3 PreviousPosition;
    private Vector3 PreviousVelocity;

    private new Rigidbody rigidbody;
    private new BoxCollider collider;

    private List<Vector3> Vertices = new List<Vector3>();
    [HideInInspector]
    public Vector3 Offset = new Vector3(0f, 0.15f, -0.7f);

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        collider = GetComponent<BoxCollider>();
        collider.enabled = false;

        PreviousPosition = transform.position;

        Vertices.Add(Vector3.Scale(transform.localScale, new Vector3(-0.5f, 0.5f, -0.5f)));
        Vertices.Add(Vector3.Scale(transform.localScale, new Vector3(-0.5f, -0.5f, -0.5f)));
        Vertices.Add(Vector3.Scale(transform.localScale, new Vector3(0.5f, -0.5f, -0.5f)));
        Vertices.Add(Vector3.Scale(transform.localScale, new Vector3(0.5f, 0.5f, -0.5f)));
        Vertices.Add(Vector3.Scale(transform.localScale, new Vector3(-0.5f, 0.5f, 0.5f)));
        Vertices.Add(Vector3.Scale(transform.localScale, new Vector3(-0.5f, -0.5f, 0.5f)));
        Vertices.Add(Vector3.Scale(transform.localScale, new Vector3(0.5f, -0.5f, 0.5f)));
        Vertices.Add(Vector3.Scale(transform.localScale, new Vector3(0.5f, 0.5f, 0.5f)));
    }

    private void FixedUpdate()
    {
        Vector3 Origin = Owner.transform.position + Owner.transform.rotation * Offset;
        Vector3 Delta = Origin - transform.position;
        if (Delta.magnitude < MaxDistance)
        {
            Vector3 Velocity = (transform.position - PreviousPosition) / Time.fixedDeltaTime;
            Vector3 DeltaVelocity = Velocity - Owner.GetComponent<Rigidbody>().velocity;
            if (DeltaVelocity.magnitude > MaxDeltaVelocity)
            {
                Debug.Log(DeltaVelocity.magnitude);
                rigidbody.isKinematic = false;
                collider.enabled = true;
            }
            else
            {
                rigidbody.isKinematic = true;
                collider.enabled = false;

                float Distance = Delta.magnitude - LerpDistance;
                if (Distance > 0.0f)
                {
                    Vector3 TargetPos = transform.position + Delta.normalized * Distance;
                    transform.position = Vector3.Lerp(transform.position, TargetPos, Damping);
                }
            }
        }
        else
        {
            rigidbody.isKinematic = false;
            collider.enabled = true;
        }
        if (rigidbody.isKinematic)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Owner.transform.rotation, Damping);
        }

        PreviousPosition = transform.position;
    }

    private void __FixedUpdate()
    {
        foreach (var v in Vertices)
        {
            Vector3 Vert = transform.TransformPoint(v);
            Vector3 toOrigin = Owner.transform.position + transform.rotation * Offset - Vert;
            if (toOrigin.magnitude > 1.0f)
            {
                rigidbody.velocity -= Vector3.Project(rigidbody.velocity, -toOrigin.normalized);
                rigidbody.MovePosition(transform.position + toOrigin.normalized * (toOrigin.magnitude - 1.0f));
            }
        }
    }

    private void _FixedUpdate()
    {

        transform.rotation = Owner.transform.rotation;
        Vector3 deltaPosition = Owner.transform.position + Owner.transform.rotation * Offset - transform.position;
        transform.position += deltaPosition;

        Vector3 newVelocity = (transform.position - PreviousPosition) / Time.fixedDeltaTime;
        if ((newVelocity - PreviousVelocity).magnitude > 10.0f)
        {
            rigidbody.isKinematic = false;
            rigidbody.velocity = newVelocity + Vector3.up * 3.0f;
        }
        PreviousVelocity = newVelocity;
        PreviousPosition = transform.position;
    }
}

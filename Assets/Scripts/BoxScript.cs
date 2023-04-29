using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxScript : MonoBehaviour
{
    public HoverController Owner;
    public Vector3 Offset = new Vector3(0.0f, 0.35f, -1.0f);

    private Vector3 PreviousPosition;
    private Vector3 PreviousVelocity;
    private new Rigidbody rigidbody;
    private Vector3 velocity;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        PreviousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (!rigidbody.isKinematic)
        {
            return;
        }
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

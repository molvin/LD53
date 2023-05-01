using System.Collections;
using UnityEngine;

public class BoxScript : MonoBehaviour
{
    public HoverController Owner;

    public float Damping = 3.0f;
    public float LerpDistance = 0.1f;
    public float MaxDistance = 1.0f;
    public float MaxDeltaVelocity = 13.0f;

    private new Rigidbody rigidbody;
    private new BoxCollider collider;
    private Rigidbody OwnerRigidbody;
    private Vector3 PrevOwnerVel;

    [HideInInspector]
    public Vector3 Offset = new Vector3(0f, 0.15f, -0.7f);
    private float GracePeriod = 2f;

    public void Reset()
    {
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.isKinematic = true;
        GracePeriod = 2f;
    }
    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        collider = GetComponent<BoxCollider>();
        collider.enabled = false;
        OwnerRigidbody = Owner.GetComponent<Rigidbody>();
        GracePeriod = 2f;
    }

    private void Update()
    {
        if (GracePeriod > 0)
        {
            GracePeriod -= Time.deltaTime;
            transform.position = Owner.transform.position + Owner.transform.rotation * Offset;
            return;
        }
        if (!rigidbody.isKinematic)
        {
            return;
        }

        Vector3 Origin = Owner.transform.position + Owner.transform.rotation * Offset;
        Vector3 Delta = Origin - transform.position;

        if (Delta.magnitude > MaxDistance - LerpDistance)
        {
            Vector3 OnTruck = Vector3.ProjectOnPlane(Delta, Owner.GroundNormal).normalized;
            transform.position = Origin - OnTruck * (MaxDistance - LerpDistance);
        }
    }

    IEnumerator Lose()
    {
        yield return new WaitForSeconds(1f);
        FindObjectOfType<RunManager>().Lose(true);
    }

    private void FixedUpdate()
    {
        if (GracePeriod > 0)
        {
            transform.position = Owner.transform.position + Owner.transform.rotation * Offset;
            return;
        }
        if (!rigidbody.isKinematic)
        {
            return;
        }

        Vector3 OwnerForwardVel = Vector3.Project(OwnerRigidbody.velocity, Owner.transform.forward);
        if ((PrevOwnerVel - OwnerForwardVel).magnitude > MaxDeltaVelocity)
        {
            rigidbody.isKinematic = false;
            rigidbody.velocity = PrevOwnerVel * 1f;
            collider.enabled = true;
            StartCoroutine(Lose());
        }
        PrevOwnerVel = OwnerForwardVel;

        Vector3 Origin = Owner.transform.position + Owner.transform.rotation * Offset;
        Vector3 Delta = Origin - transform.position;

        if (Delta.magnitude > MaxDistance - LerpDistance)
        {
            Vector3 OnTruck = Vector3.ProjectOnPlane(Delta, Owner.GroundNormal).normalized;
            transform.position = Origin - OnTruck * (MaxDistance - LerpDistance);
        }
        else
        {
            float Distance = Delta.magnitude - LerpDistance;
            if (Distance > 0.0f)
            {
                Vector3 TargetPos = transform.position + Delta.normalized * Distance;
                transform.position = Vector3.Lerp(transform.position, TargetPos, Damping);
            }
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Owner.transform.rotation, Damping);
    }
}

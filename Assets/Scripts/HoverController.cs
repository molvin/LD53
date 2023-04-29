using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HoverController : MonoBehaviour
{
    public float LinearForce = 1750.0f;
    public float AngularForce = 750.0f;
    public float MinHeight = 0.0f;
    public float MaxHeight = 1.5f;
    public float VisionHeight = 20.0f;
    public float MaxCompression = 0.8f;
    public float SpringForce = 4.0f;
    public float Damping = 0.6f;
    public float AlignedFrictionCoef = 0.3f;
    public float PerpendicularFrictionCoef = 0.8f;
    public float AlignmentRotationSpeed = 3.5f;
    public float GravitationalPull = 0.2f;
    public float OverrideGravity = 12.0f;
    public LayerMask Mask;

    private new Rigidbody rigidbody;
    private List<Vector3> Offsets = new List<Vector3>();
    private List<Vector3> Normals = new List<Vector3>();
    private List<float> PreviousHeights = new List<float>();

    private Vector3 InputVector;
    private Vector3 UpNormal;

    public Vector3 GroundNormal => UpNormal;

    private List<Vector3> IcoNormals = new List<Vector3>()
    {
        new Vector3( 0.1876f, -0.7947f, 0.5774f),
        new Vector3( 0.6071f, -0.7947f, 0.0000f),
        new Vector3( -0.4911f, -0.7947f, 0.3568f),
        new Vector3( -0.4911f, -0.7947f, -0.3568f),
        new Vector3( 0.1876f, -0.7947f, -0.5774f),
        new Vector3( 0.9822f, -0.1876f, 0.0000f),
        new Vector3( 0.3035f, -0.1876f, 0.9342f),
        new Vector3( -0.7946f, -0.1876f, 0.5774f),
        new Vector3( -0.7946f, -0.1876f, -0.5774f),
        new Vector3( 0.3035f, -0.1876f, -0.9342f),
        new Vector3( 0.7946f, 0.1876f, 0.5774f),
        new Vector3( -0.3035f, 0.1876f, 0.9342f),
        new Vector3( -0.9822f, 0.1876f, 0.0000f),
        new Vector3( -0.3035f, 0.1876f, -0.9342f),
        new Vector3( 0.7946f, 0.1876f, -0.5774f),
        new Vector3( 0.4911f, 0.7947f, 0.3568f),
        new Vector3( -0.1876f, 0.7947f, 0.5774f),
        new Vector3( -0.6071f, 0.7947f, 0.0000f),
        new Vector3( -0.1876f, 0.7947f, -0.5774f),
        new Vector3( 0.4911f, 0.7947f, -0.3f) 
    };

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.centerOfMass = Vector3.down;
        //Offsets.Add(new Vector3(0.5f, -0.5f, 0.5f));
        //Offsets.Add(new Vector3(0.5f, -0.5f, -0.5f));
        //Offsets.Add(new Vector3(-0.5f, -0.5f, 0.5f));
        //Offsets.Add(new Vector3(-0.5f, -0.5f, -0.5f));

        // Front
        Offsets.Add(new Vector3(1.0f, -0.5f, 2.0f));
        Offsets.Add(new Vector3(-1.0f, -0.5f, 2.0f));
        // Mid
        Offsets.Add(new Vector3(1.0f, -0.5f, 0.67f));
        Offsets.Add(new Vector3(-1.0f, -0.5f, 0.67f));
        Offsets.Add(new Vector3(1.0f, -0.5f, -0.67f));
        Offsets.Add(new Vector3(-1.0f, -0.5f, -0.67f));
        // Back
        Offsets.Add(new Vector3(1.0f, -0.5f, -2.0f));
        Offsets.Add(new Vector3(-1.0f, -0.5f, -2.0f));
        // Central
        Offsets.Add(new Vector3(0.0f, -0.5f, 1.0f));
        Offsets.Add(new Vector3(0.0f, -0.5f, -1.0f));


        for (int i = 0; i < Offsets.Count; i++)
        {
            Normals.Add(((Offsets[i]).normalized + Vector3.down) * 0.5f);
            //Normals.Add(Vector3.down);
            PreviousHeights.Add(MaxHeight);
        }
    }

    private void Update()
    {
        // TODO: Heurisitic ground normal
        Vector3 LocalInput = Input.GetAxisRaw("Horizontal") * Vector3.right;
        LocalInput += Input.GetAxisRaw("Vertical") * Vector3.forward;
        InputVector += Vector3.ClampMagnitude(LocalInput, 1.0f) * Time.deltaTime;
        //InputVector += Vector3.ProjectOnPlane(transform.rotation * LocalInput, UpNormal).normalized * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + UpNormal * 3.0f);
        Gizmos.color = Color.blue;
        foreach (var IcoNormal in IcoNormals)
        {
            Gizmos.DrawLine(transform.position, transform.position + IcoNormal * VisionHeight);
        }
    }

    private void FixedUpdate()
    {
        UpNormal = Vector3.up;
        foreach (var IcoNormal in IcoNormals)
        {
            if (Physics.Raycast(
                transform.position,
                IcoNormal,
                out RaycastHit hit,
                VisionHeight,
                Mask))
            {
                UpNormal -= IcoNormal * (1.0f - (hit.distance / VisionHeight));
            }
        }
        UpNormal.Normalize();

        // Friction
        Vector3 Right = Vector3.ProjectOnPlane(transform.right, UpNormal).normalized;
        float Dot = Vector3.Dot(Right, rigidbody.velocity);
        rigidbody.velocity -= Right * Dot * PerpendicularFrictionCoef;

        Quaternion delta = Quaternion.FromToRotation(transform.up, UpNormal);//.ToAngleAxis(out float Angle, out Vector2 Axis);
        delta.ToAngleAxis(out float Angle, out Vector3 axis);
        rigidbody.AddTorque(axis.normalized * Angle * AlignmentRotationSpeed * Time.fixedDeltaTime, ForceMode.Acceleration);

        // Consume input vector
        float Forward = InputVector.z;
        float Rotation = InputVector.x;
        InputVector = Vector3.zero;

        Vector3 ForwardInput = Vector3.ProjectOnPlane(transform.TransformDirection(Vector3.forward), UpNormal) * Forward;
        Vector3 TurningInput = Vector3.up * Rotation;

        float Range = MaxHeight - MinHeight;
        float Hits = 0.0f;
        for (int i = 0; i < Normals.Count; i++)
        {
            Vector3 LocalOffset = transform.rotation * Offsets[i];
            Vector3 Origin = transform.position + LocalOffset;
            Vector3 Normal = transform.TransformDirection(Normals[i]);

            float suspensionDistance = 1.0f;
            if (Physics.Raycast(
                Origin,
                Normal,
                out RaycastHit hit,
                MaxHeight,
                Mask))
            {
                Hits += 1.0f;
                Debug.DrawLine(Origin, hit.point, Color.red);

                suspensionDistance = (hit.distance - MinHeight);

                float suspensionCompression = Mathf.Clamp((Range - suspensionDistance) / Range, 0, MaxCompression);
                float suspensionForce = suspensionCompression * SpringForce;
                float damping = Damping * (PreviousHeights[i] - suspensionDistance) / Time.fixedDeltaTime;
                suspensionForce += damping;

                if (suspensionForce > 0.0f)
                {
                    Vector3 Force = suspensionForce * -Normal;
                    rigidbody.AddForceAtPosition(Force, Origin);
                }
            }
            else
            {
                Debug.DrawLine(Origin, Origin + Normal * MaxHeight, Color.green);
            }
            PreviousHeights[i] = suspensionDistance;
        }

        Debug.DrawLine(transform.position, transform.position + UpNormal * 3.0f);

        Vector3 Gravity = -UpNormal * OverrideGravity * GravitationalPull * Time.fixedDeltaTime;
        Gravity += Vector3.down * (OverrideGravity - 9.81f) * Time.fixedDeltaTime;
        // Gravitational pull
        rigidbody.AddForce(Gravity);
        // Input
        rigidbody.AddForce(ForwardInput * LinearForce * Hits / Normals.Count);
        rigidbody.AddRelativeTorque(TurningInput * AngularForce * Hits / Normals.Count);


        //float yVel = rigidbody.velocity.y;
        //Vector2 horizontalVel = Vector2.ClampMagnitude(new Vector2(rigidbody.velocity.x, rigidbody.velocity.z), MaxSpeed);
        //rigidbody.velocity = new Vector3(horizontalVel.x, yVel, horizontalVel.y);
    }
}

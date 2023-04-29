using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HoverController : MonoBehaviour
{
    public float LinearForce = 100.0f;
    public float MaxSpeed = 30.0f;
    public float MinHeight = 0.5f;
    public float MaxHeight = 5.0f;
    public float MaxCompression = 0.7f;
    public float SpringForce = 35.0f;
    public float Damping = 0.35f;
    public LayerMask Mask;

    private new Rigidbody rigidbody;
    private List<Vector3> Offsets = new List<Vector3>();
    private List<Vector3> Normals = new List<Vector3>();
    private List<float> PreviousHeights = new List<float>();

    private Vector3 InputVector;
    private Vector3 UpNormal;

    private void Awake()
    {
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

        rigidbody = GetComponent<Rigidbody>();        
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

    private void FixedUpdate()
    {
        transform.up = Vector3.Lerp(transform.up, Vector3.up, Time.fixedDeltaTime);
        // Consume input vector
        float Forward = InputVector.z;
        float Rotation = InputVector.x;
        InputVector = Vector3.ProjectOnPlane(transform.TransformDirection(Vector3.forward), UpNormal) * Forward;
        rigidbody.AddForce(InputVector * LinearForce);
        rigidbody.AddRelativeTorque(Vector3.up * Rotation * LinearForce);
        InputVector = Vector3.zero;

        float Range = MaxHeight - MinHeight;

        Vector3 HitPoints = Vector3.zero;

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
                Debug.DrawLine(Origin, hit.point, Color.red);

                HitPoints += hit.normal;
                HitPoints += (hit.point - transform.position).normalized;

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
        if (HitPoints != Vector3.zero)
        {
            UpNormal = HitPoints.normalized;
        }
        else
        {
            UpNormal = Vector3.up;
        }

        rigidbody.AddForce(-UpNormal * 9.81f * 0.3f * Time.fixedDeltaTime);

        float yVel = rigidbody.velocity.y;
        Vector2 horizontalVel = Vector2.ClampMagnitude(new Vector2(rigidbody.velocity.x, rigidbody.velocity.z), MaxSpeed);
        rigidbody.velocity = new Vector3(horizontalVel.x, yVel, horizontalVel.y);
    }
}

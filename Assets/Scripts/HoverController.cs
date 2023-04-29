using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverController : MonoBehaviour
{
    public float LinearForce = 100.0f;
    public float MinHeight = 0.5f;
    public float MaxHeight = 5.0f;
    public float MaxCompression = 0.7f;
    public float SpringForce = 35.0f;
    public float Damping = 0.35f;

    private new Rigidbody rigidbody;
    private List<Vector3> Offsets = new List<Vector3>();
    private List<Vector3> Normals = new List<Vector3>();
    private List<float> PreviousHeights = new List<float>();

    private Vector3 InputVector;

    private void Awake()
    {
        Offsets.Add(new Vector3(0.5f, -0.5f, 0.5f));
        Offsets.Add(new Vector3(0.5f, -0.5f, -0.5f));
        Offsets.Add(new Vector3(-0.5f, -0.5f, 0.5f));
        Offsets.Add(new Vector3(-0.5f, -0.5f, -0.5f));

        Normals.Add(Vector3.down);
        Normals.Add(Vector3.down);
        Normals.Add(Vector3.down);
        Normals.Add(Vector3.down);

        rigidbody = GetComponent<Rigidbody>();        
        for (int i = 0; i < Offsets.Count; i++)
        {
            PreviousHeights.Add(MaxHeight);
        }
    }

    private void Update()
    {
        // TODO: Heurisitic ground normal
        Vector3 LocalInput = Input.GetAxisRaw("Horizontal") * Vector3.right;
        LocalInput += Input.GetAxisRaw("Vertical") * Vector3.forward;
        InputVector += transform.rotation * LocalInput.normalized * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        // Consume input vector
        rigidbody.AddForce(InputVector * LinearForce);
        InputVector = Vector3.zero;

        float Range = MaxHeight - MinHeight;
        for (int i = 0; i < Normals.Count; i++)
        {
            float suspensionDistance = 1.0f;
            if (Physics.Raycast(
                transform.position + Offsets[i],
                Normals[i],
                out RaycastHit hit,
                MaxHeight))
            {
                suspensionDistance = (hit.distance - MinHeight);
                float suspensionCompression = Mathf.Clamp((Range - suspensionDistance) / Range, 0, MaxCompression);
                float suspensionForce = suspensionCompression * SpringForce;
                float damping = Damping * (PreviousHeights[i] - suspensionDistance) / Time.fixedDeltaTime;
                suspensionForce += damping;
                if (suspensionForce > 0.0f)
                {
                    Vector3 Force = suspensionForce * -Normals[i];
                    rigidbody.AddForceAtPosition(Force, transform.position + Offsets[i]);
                }
            }
            PreviousHeights[i] = suspensionDistance;
        }
    }
}

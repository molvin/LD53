using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Audio;
using UnityEngine;

public class HoverController : MonoBehaviour
{
    [Header("Acceleration")]
    public float LinearForce = 1750.0f;
    public float AngularForce = 750.0f;
    public float RollForce = 750.0f;
    [Header("Dampning")]
    public float MinHeight = 0.0f;
    public float MaxHeight = 1.5f;
    public float VisionHeight = 20.0f;
    public float MaxCompression = 0.8f;
    public float SpringForce = 4.0f;
    public float Damping = 0.6f;
    [Header("Friction")]
    public float AlignedFrictionCoef = 0.3f;
    public float ForwardFictionCoef = 0.2f;
    public float PerpendicularFrictionCoef = 0.8f;
    public float SlideFrictionCoef = 0.2f;
    public float SlideSmoothing = 0.01f;
    [Header("Physics")]
    public float AlignmentRotationSpeed = 3.5f;
    public float GravitationalPull = 0.2f;
    public float OverrideGravity = 12.0f;
    public LayerMask Mask;
    [Header("audio")]
    public AudioSource PlayerAudioSource;
    public AudioClip[] AudioClipsLooping;
    public AudioClip[] OneShotAudioClips;
    private int SoundStateLooping;
    public AudioSource Idle;
    public AudioSource Accelerate;
    public AudioSource TopSpeed;

    private new Rigidbody rigidbody;
    private List<Vector3> Offsets = new List<Vector3>();
    private List<Vector3> Normals = new List<Vector3>();
    private List<float> PreviousHeights = new List<float>();

    private Vector3 InputVector;
    private Vector3 AlignmentNormal;
    private Vector3 UpNormal;
    private float rightCoef;
    private float slideDelta;
    private float CurrentSpeed;
    private float CurrentAcceleration;
    private float PreviousSpeed;

    public Transform[] Thrusters;

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
        rigidbody.centerOfMass = Vector3.down * 0.5f;
        rigidbody.inertiaTensor = new Vector3(1.42f, 1.67f, 0.42f);
        rigidbody.inertiaTensorRotation = Quaternion.identity;
        //Offsets.Add(new Vector3(0.5f, -0.5f, 0.5f));
        //Offsets.Add(new Vector3(0.5f, -0.5f, -0.5f));
        //Offsets.Add(new Vector3(-0.5f, -0.5f, 0.5f));
        //Offsets.Add(new Vector3(-0.5f, -0.5f, -0.5f));
        // Front
        Offsets.Add( new Vector3(1.0f, -0.4f, 2.0f) * 0.5f);
        Offsets.Add(new Vector3(-1.0f, -0.4f, 2.0f) * 0.5f);
        // Mid
        Offsets.Add(new Vector3(1.0f, -0.4f, 0.67f) * 0.5f);
        Offsets.Add(new Vector3(-1.0f, -0.4f, 0.67f) * 0.5f);
        Offsets.Add(new Vector3(1.0f, -0.4f, -0.67f) * 0.5f);
        Offsets.Add(new Vector3(-1.0f, -0.4f, -0.67f) * 0.5f);
        // Back
        Offsets.Add(new Vector3(1.0f, -0.4f, -2.0f) * 0.5f);
        Offsets.Add( new Vector3(-1.0f, -0.4f, -2.0f) * 0.5f);
        // Central
        Offsets.Add(new Vector3(0.0f, 0f, 2.0f) * 0.5f);
        Offsets.Add(new Vector3(0.0f, -0.4f, 1.0f) * 0.5f);
        Offsets.Add(new Vector3(0.0f, -0.4f, -1.0f) * 0.5f);


        for (int i = 0; i < Offsets.Count; i++)
        {
            Normals.Add(((Offsets[i]).normalized + Vector3.down) * 0.5f);
            //Normals.Add(Vector3.down);
            PreviousHeights.Add(MaxHeight);
        }
        SoundStateLooping = 4;
    }

    private void Update()
    {
        // TODO: Heurisitic ground normal
        Vector3 LocalInput = Input.GetAxisRaw("Horizontal") * Vector3.right;
        LocalInput += Input.GetAxisRaw("Forward") * Vector3.forward;
        InputVector += Vector3.ClampMagnitude(LocalInput, 1.0f) * Time.deltaTime;
        //InputVector += Vector3.ProjectOnPlane(transform.rotation * LocalInput, UpNormal).normalized * Time.deltaTime;

        if (Input.GetButton("Slide"))
            rightCoef = SlideFrictionCoef;
        rightCoef = Mathf.SmoothDamp(rightCoef, PerpendicularFrictionCoef, ref slideDelta, SlideSmoothing);
        // Debug.Log(" SPEED : " + rigidbody.velocity.magnitude);
        /*
        if (Input.GetAxisRaw("Forward") > 0 && SoundStateLooping != 2 && rigidbody.velocity.magnitude > 25)
        {
            //Debug.Log("  JAG HÄNDER !!! " + SoundStateLooping + "   ANNDD    " + rigidbody.velocity.magnitude);
            //SONIC BOOM PLAY ONCE!
            SoundStateLooping = 2;
            PlayerAudioSource.PlayOneShot(OneShotAudioClips[SoundStateLooping], 1.5f);
            PlayerAudioSource.clip = AudioClipsLooping[SoundStateLooping];
            PlayerAudioSource.Play();
        }
        else if((Input.GetAxisRaw("Forward") > 0 || Input.GetAxisRaw("Forward") < 0) && SoundStateLooping != 1 && rigidbody.velocity.magnitude <= 15)
        {
            //ENGINE EXPLOSION?! PLAY ONCE
            SoundStateLooping = 1;
            PlayerAudioSource.PlayOneShot(OneShotAudioClips[SoundStateLooping], 1.2f);
            PlayerAudioSource.clip = AudioClipsLooping[SoundStateLooping];
            PlayerAudioSource.Play();
        }
        else if(Input.GetAxisRaw("Forward") == 0 && SoundStateLooping != 0 && rigidbody.velocity.magnitude <= 12)
        {
            //BREAK NOISE PLAY ONCE
            SoundStateLooping = 0;
            PlayerAudioSource.PlayOneShot(OneShotAudioClips[SoundStateLooping], 1f);
            PlayerAudioSource.clip = AudioClipsLooping[SoundStateLooping];
            PlayerAudioSource.Play();
        }
        */
        float SpeedVolume = Mathf.Clamp01(CurrentSpeed / 20f);
        float AccelerationVolume = Mathf.Clamp01(CurrentAcceleration / 20f);
        float IdleVolume = ((1f - Mathf.Clamp01(CurrentSpeed / 20f)) + 1f) * 0.5f;

        Idle.volume = Mathf.Lerp(Idle.volume, IdleVolume, 4f * Time.deltaTime);
        TopSpeed.volume = Mathf.Lerp(TopSpeed.volume, SpeedVolume, 4f * Time.deltaTime);
        Accelerate.volume = Mathf.Lerp(Accelerate.volume, AccelerationVolume, 4f * Time.deltaTime);
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
                UpNormal -= IcoNormal * (1.0f - (Mathf.Pow(hit.distance, 2f) / Mathf.Pow(VisionHeight, 2f)));
            }
        }
        UpNormal.Normalize();
        AlignmentNormal = Vector3.Lerp(UpNormal.normalized, Vector3.up, 0.5f);

        // Consume input vector
        //Vector3 LocalInput = Input.GetAxisRaw("Horizontal") * Vector3.right;
        //LocalInput += Input.GetAxisRaw("Forward") * Vector3.forward;
        //InputVector = Vector3.ClampMagnitude(LocalInput, 1f) * Time.fixedDeltaTime;
        float Forward = InputVector.z;
        if (Forward < 0f)
        {
            Forward *= 0.5f;
        }
        float Rotation = InputVector.x;
        InputVector = Vector3.zero;

        Vector3 ForwardInput = Vector3.ProjectOnPlane(transform.TransformDirection(Vector3.forward), UpNormal) * Forward;
        Vector3 TurningInput = Vector3.up * Rotation;
        Vector3 RollInput = Vector3.forward * Rotation;

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

        float GroundConnectedness = Hits / Normals.Count;

        // Friction
        Vector3 Right = Vector3.ProjectOnPlane(transform.right, AlignmentNormal).normalized;
        float Dot = Vector3.Dot(Right, rigidbody.velocity);

        rigidbody.velocity -= Right * Dot * rightCoef * GroundConnectedness * Time.fixedDeltaTime;

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, AlignmentNormal).normalized;
        rigidbody.velocity -= forward * Vector3.Dot(forward, rigidbody.velocity) * ForwardFictionCoef * GroundConnectedness * Time.fixedDeltaTime;

        Quaternion delta = Quaternion.FromToRotation(transform.up, AlignmentNormal);//.ToAngleAxis(out float Angle, out Vector2 Axis);
        delta.ToAngleAxis(out float Angle, out Vector3 axis);
        rigidbody.AddTorque(axis.normalized * Angle * AlignmentRotationSpeed * Time.fixedDeltaTime, ForceMode.Acceleration);

        Vector3 Gravity = -AlignmentNormal * OverrideGravity * GravitationalPull * Time.fixedDeltaTime;
        Gravity += Vector3.down * (OverrideGravity - 9.81f) * Time.fixedDeltaTime;
        // Gravitational pull
        rigidbody.AddForce(Gravity);
        // Input
        if (ForwardInput.y > 0 && GravitationalPull > 0)
        {
            ForwardInput.y *= 2;
        }

        rigidbody.AddForce(ForwardInput * LinearForce * GroundConnectedness);
        rigidbody.AddRelativeTorque(TurningInput * AngularForce * GroundConnectedness);
        rigidbody.AddRelativeTorque(RollInput * RollForce * (1.0f - GroundConnectedness));


        //float yVel = rigidbody.velocity.y;
        //Vector2 horizontalVel = Vector2.ClampMagnitude(new Vector2(rigidbody.velocity.x, rigidbody.velocity.z), MaxSpeed);
        //rigidbody.velocity = new Vector3(horizontalVel.x, yVel, horizontalVel.y);

        CurrentSpeed = rigidbody.velocity.magnitude;
        CurrentAcceleration = (CurrentSpeed - PreviousSpeed) / Time.fixedDeltaTime;
        PreviousSpeed = CurrentSpeed;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.Rendering;
using UnityEngine;
using static SplineNoise3D;

public class LevelMakerEditorController : MonoBehaviour
{
    public float MoveSpeed = 10f;
    public float TurnSpeed = 3f;
    public float SpeedUp = 2.5f;

    public Mesh VisualMesh;
    public Material VisualMeshMaterial;

    private float Yaw = 0f;
    private float Pitch = 0f;

    private Vector3 WorkingPos = Vector3.zero;
    private Quaternion WorkingRot = Quaternion.identity;
    private Vector4 Roundness = Vector4.one;

    private void Update()
    {
        float Horizontal = Input.GetAxisRaw("Horizontal");
        float Vertical = Input.GetAxisRaw("Vertical");
        float Up = Input.GetKey(KeyCode.E) ? 1f : 0f;
        float Down = Input.GetKey(KeyCode.Q) ? 1f : 0f;

        Vector3 Movement = Vector3.right * Horizontal + Vector3.forward * Vertical;
        Movement += Vector3.up * Up + Vector3.down * Down;
        float Speed = MoveSpeed * (Input.GetKey(KeyCode.LeftShift) ? SpeedUp : 1f);
        transform.position += transform.rotation * Movement * Speed * Time.deltaTime;

        Yaw += TurnSpeed * Input.GetAxisRaw("CameraHorizontal");
        Pitch -= TurnSpeed * Input.GetAxisRaw("CameraVertical");
        Pitch = Mathf.Clamp(Pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(Pitch, Yaw, 0f);

        DrawVisualizationMesh();

        UpdateSpline();
    }

    private void DrawVisualizationMesh()
    {
        if (VisualMesh && VisualMeshMaterial)
        {
            Graphics.DrawMesh(VisualMesh, WorkingPos, WorkingRot, VisualMeshMaterial, 0);
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < SplineNoise3D.SplineLine.Count - 1; i++)
        {
            Debug.DrawLine(SplineNoise3D.SplineLine[i].pos, SplineNoise3D.SplineLine[i + 1].pos, Color.red);
        }
        for (int i = 0; i < SplineNoise3D.SplineHole.Count - 1; i++)
        {
            Debug.DrawLine(SplineNoise3D.SplineHole[i].pos, SplineNoise3D.SplineHole[i + 1].pos, Color.green);
        }
        for (int i = 0; i < SplineNoise3D.SplineLine.Count; i++)
        {
            Gizmos.DrawSphere(SplineNoise3D.SplineLine[i].pos, 0.2f);
        }

        if(SplineNoise3D.SplineLine.Count > 0)
        {
            Spline s = SplineNoise3D.getLerpSplineFromPoint(transform.position);
            Debug.DrawLine(transform.position, s.pos, Color.magenta);
        }
    }

    private void UpdateSpline()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SplineNoise3D.AddSplineSegment(WorkingPos, WorkingRot, Random.Range(1f, 10f), Roundness);
            WorkingPos += WorkingRot * Vector3.forward * 10f;
        }
    }

    /*
    public float speed = 5;
    public float roll_speed = 5;
    public float splineDistance;
    public float mouse_sensitivity = 5;

    private void Start()
    {
        lastSpline = transform.position;
    }
    void Update()
    {
        UpdateSpline();
        Vector3 vel = Vector3.zero;
        var rot_dir = boolToInt(Input.GetKey(KeyCode.A)) - boolToInt(Input.GetKey(KeyCode.D));
        float mouse_y = Input.GetAxis("CameraVertical");
        float mouse_x = Input.GetAxis("CameraHorizontal");
        transform.eulerAngles += 
            new Vector3(
                mouse_y * mouse_sensitivity * boolToInt(Input.GetMouseButton(1)),
                mouse_x * mouse_sensitivity * boolToInt(Input.GetMouseButton(1)),
                rot_dir * roll_speed
            );
        
        vel += transform.forward * boolToInt(Input.GetKey(KeyCode.W));
        this.transform.position += vel * speed * Time.deltaTime;
    }

    public void UpdateSpline()
    {
    }
    public float SuperNoiseHole(Vector3 point)
    {
        //distance along spline X
        float dist = SplineNoise3D.SplineNoise(point);
        return dist;
    }

    private int boolToInt(bool b)
    {
        return b ? 1 : 0;
    }
    */
}

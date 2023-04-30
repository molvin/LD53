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

    public float TooCloseDistance = 3f;
    public float HandleLength = 3f;
    public float HandleDistance = 2f;

    private MeshRenderer CurrentSplineEdit;
    private List<GameObject> SplineTransforms = new List<GameObject>();
    private List<GameObject> IntermediatePoints = new List<GameObject>();

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

        if (Input.GetMouseButton(1))
        {
            Yaw += TurnSpeed * Input.GetAxisRaw("CameraHorizontal");
            Pitch -= TurnSpeed * Input.GetAxisRaw("CameraVertical");
            Pitch = Mathf.Clamp(Pitch, -89f, 89f);
        }

        transform.rotation = Quaternion.Euler(Pitch, Yaw, 0f);

        if (Input.GetMouseButtonDown(0))
        {
            if (!CurrentSplineEdit || GetAxis() == null)
            {
                SelectSplinePoint();
            }
        }

        if (Input.GetMouseButton(0) && CurrentSplineEdit)
        {
            int? Axis = GetAxis();
            if (Axis != null)
            {
                DragCurrent(Axis.Value);
            }
        }

        if (!CurrentSplineEdit)
        {
            DrawVisualizationMesh();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                AddSplinePoint();
            }
        }
        else if (IntermediatePoints.Contains(CurrentSplineEdit.gameObject) && CurrentSplineEdit.material.color != Color.red)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                InsertSplinePoint();
            }
        }
    }

    private int? GetAxis()
    {
        Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
        {
            Vector3 lineDirection = lineEnd - lineStart;
            float lineLength = lineDirection.magnitude;
            lineDirection.Normalize();

            Vector3 pointDirection = point - lineStart;
            float dot = Vector3.Dot(pointDirection, lineDirection);

            dot = Mathf.Clamp(dot, 0f, lineLength);

            return lineStart + lineDirection * dot;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Transform trans = CurrentSplineEdit.transform;
        Vector3 pointOnLineForward = ClosestPointOnLine(trans.position, trans.forward * HandleLength, ray.origin);
        Vector3 pointOnLineRight = ClosestPointOnLine(trans.position, trans.right * HandleLength, ray.origin);
        Vector3 pointOnLineUp = ClosestPointOnLine(trans.position, trans.up * HandleLength, ray.origin);

        float forwardDistance = Vector3.Distance(pointOnLineForward, ray.origin);
        float rightDistance = Vector3.Distance(pointOnLineRight, ray.origin);
        float upDistance = Vector3.Distance(pointOnLineUp, ray.origin);

        if (forwardDistance < HandleDistance && forwardDistance < rightDistance && forwardDistance < upDistance)
        {

        }
        if (rightDistance < HandleDistance && rightDistance < forwardDistance && rightDistance < upDistance)
        {

        }
        if (upDistance < HandleDistance && upDistance < rightDistance && upDistance < forwardDistance)
        {

        }

        return null;
    }

    private void DragCurrent(int Axis)
    {

    }
    
    private void SelectSplinePoint()
    {
        if (CurrentSplineEdit)
        {
            CurrentSplineEdit.material.color = Color.gray;
        }
        CurrentSplineEdit = null;

        bool IsTooClose = false;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            foreach (var cube in SplineTransforms)
            {
                if (cube.transform == hit.transform)
                {
                    CurrentSplineEdit = cube.GetComponent<MeshRenderer>();
                }
            }

            foreach (var sphere in IntermediatePoints)
            {
                if (sphere.transform == hit.transform)
                {
                    CurrentSplineEdit = sphere.GetComponent<MeshRenderer>();
                    int Index = IntermediatePoints.IndexOf(sphere);
                    if (Vector3.Distance(SplineTransforms[Index].transform.position, SplineTransforms[Index + 1].transform.position) < TooCloseDistance)
                    {
                        IsTooClose = true;
                    }
                }
            }
        }

        if (CurrentSplineEdit)
        {
            CurrentSplineEdit.material.color = IsTooClose ? Color.red : Color.yellow;
        }
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
        for (int i = 0; i < SplineNoise3D.SplineLine.Count; i++)
        {
            //Gizmos.DrawSphere(SplineNoise3D.SplineLine[i].pos, 0.2f);
        }

        if(SplineNoise3D.SplineLine.Count > 0)
        {
            Spline s = SplineNoise3D.getLerpSplineFromPoint(transform.position);
            Debug.DrawLine(transform.position, s.pos, Color.magenta);
        }

        Vector3 HandlePos = CurrentSplineEdit ? CurrentSplineEdit.transform.position : WorkingPos;
        Quaternion HandleRot = CurrentSplineEdit ? CurrentSplineEdit.transform.rotation : WorkingRot;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(HandlePos, HandleRot * Vector3.forward * HandleLength);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(HandlePos, HandleRot * Vector3.right * HandleLength);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(HandlePos, HandleRot * Vector3.up * HandleLength);
    }

    private void AddSplinePoint()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = WorkingPos;
        cube.transform.rotation = WorkingRot;
        SplineTransforms.Add(cube);
        SplineNoise3D.AddSplineSegment(WorkingPos, WorkingRot, Random.Range(1f, 10f), Roundness);
        WorkingPos += WorkingRot * Vector3.forward * 10f;

        if (SplineTransforms.Count > 1)
        {
            Vector3 Pos = Vector3.Lerp(SplineTransforms[SplineTransforms.Count - 1].transform.position, SplineTransforms[SplineTransforms.Count - 2].transform.position, 0.5f);
            Quaternion Rot = Quaternion.Slerp(SplineTransforms[SplineTransforms.Count - 1].transform.rotation, SplineTransforms[SplineTransforms.Count - 2].transform.rotation, 0.5f);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = Pos;
            sphere.transform.rotation = Rot;
            IntermediatePoints.Add(sphere);
        }
    }

    private void InsertSplinePoint()
    {
        int Index = IntermediatePoints.IndexOf(CurrentSplineEdit.gameObject);
        Vector3 NewPos = CurrentSplineEdit.transform.position;
        Quaternion NewRot = CurrentSplineEdit.transform.rotation;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = NewPos;
        cube.transform.rotation = NewRot;
        SplineTransforms.Insert(Index + 1, cube);

        InsertSplineSegment(Index + 1, NewPos, NewRot, Random.Range(1f, 10f), Roundness);

        CurrentSplineEdit.transform.position = NewPos;


        {
            Vector3 InterPos = Vector3.Lerp(SplineTransforms[Index].transform.position, SplineTransforms[Index + 1].transform.position, 0.5f);
            Quaternion InterRot = Quaternion.Slerp(SplineTransforms[Index].transform.rotation, SplineTransforms[Index + 1].transform.rotation, 0.5f);
            CurrentSplineEdit.transform.position = InterPos;
            CurrentSplineEdit.transform.rotation = InterRot;

            Vector3 AddPos = Vector3.Lerp(SplineTransforms[Index + 1].transform.position, SplineTransforms[Index + 2].transform.position, 0.5f);
            Quaternion AddRot = Quaternion.Slerp(SplineTransforms[Index + 1].transform.rotation, SplineTransforms[Index + 2].transform.rotation, 0.5f);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = AddPos;
            sphere.transform.rotation = AddRot;
            IntermediatePoints.Insert(Index + 1, sphere);
        }

        CurrentSplineEdit.material.color = Color.gray;
        CurrentSplineEdit = null;
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

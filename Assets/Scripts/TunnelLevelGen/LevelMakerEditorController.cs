using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

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
    public float AddDistance = 50f;
    public float MinRadius = 5f;
    public float MaxRadius = 30f;

    private MeshRenderer CurrentSplineEdit;
    private List<GameObject> SplineTransforms = new List<GameObject>();
    private List<GameObject> IntermediatePoints = new List<GameObject>();
    private List<float> Scales = new List<float>();

    private float Yaw = 0f;
    private float Pitch = 0f;

    private Vector3 WorkingPos = Vector3.zero;
    private Quaternion WorkingRot = Quaternion.identity;
    private float WorkingScale = 3f;

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
            SelectSplinePoint();
        }

        if (Input.GetMouseButton(0) && CurrentSplineEdit)
        {
            DragCurrent();

            if (IntermediatePoints.Contains(CurrentSplineEdit.gameObject))
            {
                int Index = IntermediatePoints.IndexOf(CurrentSplineEdit.gameObject);
                if (Vector3.Distance(SplineTransforms[Index].transform.position, CurrentSplineEdit.transform.position) < TooCloseDistance
                || Vector3.Distance(SplineTransforms[Index + 1].transform.position, CurrentSplineEdit.transform.position) < TooCloseDistance)
                {
                    CurrentSplineEdit.material.color = Color.red;
                }
                else
                {
                    CurrentSplineEdit.material.color = Color.yellow;
                }
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

        if (Input.GetKey(KeyCode.LeftShift))
        {
            Scale();
        }
        else
        {
            RotateAround();
        }

        UpdatePointRotations();
        UpdateIntermediates();

        if (SplineTransforms.Count > 0)
        {
            Transform Last = SplineTransforms[SplineTransforms.Count - 1].transform;
            WorkingPos = Last.position + Last.forward * AddDistance;
            WorkingRot = Quaternion.LookRotation(Last.forward, WorkingRot * Vector3.up);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            SplineNoise3D.SplineLine.Clear();
            for (int i = 0; i < SplineTransforms.Count; i++)
            {
                Transform Trans = SplineTransforms[i].transform;
                SplineNoise3D.AddSplineSegment(Trans.position, Trans.rotation, Scales[i], Vector4.one * 0.5f);
            }
            StartCoroutine(FindObjectOfType<Serializer>().Generate(FindObjectOfType<Serializer>().SerializeLevelToJson()));
        }
    }

    private Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 lineDirection = lineEnd - lineStart;
        float lineLength = lineDirection.magnitude;
        lineDirection.Normalize();

        Vector3 pointDirection = point - lineStart;
        float dot = Vector3.Dot(pointDirection, lineDirection);

        dot = Mathf.Clamp(dot, 0f, lineLength);

        return lineStart + lineDirection * dot;
    }

    private void DragCurrent()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == CurrentSplineEdit.transform)
        {
            hit.transform.position = ray.origin + ray.direction * (hit.transform.position - ray.origin).magnitude;
        }
        else
        {
            Vector3 closest = ClosestPointOnLine(ray.origin, ray.direction * 10000f, CurrentSplineEdit.transform.position);
            CurrentSplineEdit.transform.position = ray.origin + ray.direction * (closest - ray.origin).magnitude;
        }

        /*
        if (SplineTransforms.Contains(CurrentSplineEdit.gameObject))
        {
            int Index = SplineTransforms.IndexOf(CurrentSplineEdit.gameObject);
            Spline s = SplineLine[Index];
            s.pos = CurrentSplineEdit.transform.position;
            SplineLine[Index] = s;
        }
        */
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
        int IntermediateIndex = -1;
        if (CurrentSplineEdit && IntermediatePoints.Contains(CurrentSplineEdit.gameObject))
        {
            IntermediateIndex = IntermediatePoints.IndexOf(CurrentSplineEdit.gameObject);
        }

        for (int i = 0; i < SplineTransforms.Count - 1; i++)
        {
            if (i == IntermediateIndex)
            {
                Debug.DrawLine(SplineTransforms[i].transform.position, CurrentSplineEdit.transform.position, Color.red);
                Debug.DrawLine(CurrentSplineEdit.transform.position, SplineTransforms[i + 1].transform.position, Color.red);
            }
            else
            {
                Debug.DrawLine(SplineTransforms[i].transform.position, SplineTransforms[i + 1].transform.position, Color.red);
            }
        }

        for (int i = 0; i < SplineTransforms.Count; i++)
        {
            Gizmos.DrawWireSphere(SplineTransforms[i].transform.position, Scales[i]);
        }
        Gizmos.DrawWireSphere(WorkingPos, WorkingScale);

        /*
        for (int i = 0; i < SplineLine.Count; i++)
        {
            Transform trans = SplineTransforms[i].transform;
            Gizmos.DrawLine(trans.position, trans.position + trans.up * SplineLine[i].radius);
        }

        if(SplineNoise3D.SplineLine.Count > 0)
        {
            Spline s = SplineNoise3D.getLerpSplineFromPoint(transform.position);
            Debug.DrawLine(transform.position, s.pos, Color.magenta);
        }
        */

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
        cube.transform.localScale = Vector3.one * MinRadius;
        SplineTransforms.Add(cube);
        Scales.Add(WorkingScale);
        //SplineNoise3D.AddSplineSegment(WorkingPos, WorkingRot, Random.Range(1f, 10f), Roundness);
        WorkingPos += WorkingRot * Vector3.forward * AddDistance;

        if (SplineTransforms.Count > 1)
        {
            Vector3 Pos = Vector3.Lerp(SplineTransforms[SplineTransforms.Count - 1].transform.position, SplineTransforms[SplineTransforms.Count - 2].transform.position, 0.5f);
            Quaternion Rot = Quaternion.Slerp(SplineTransforms[SplineTransforms.Count - 1].transform.rotation, SplineTransforms[SplineTransforms.Count - 2].transform.rotation, 0.5f);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = Pos;
            sphere.transform.rotation = Rot;
            sphere.transform.localScale = Vector3.one * MinRadius;
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
        Scales.Insert(Index + 1, WorkingScale);

        //InsertSplineSegment(Index + 1, NewPos, NewRot, Random.Range(1f, 10f), Roundness);

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

    private void Scale()
    {
        float Scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (Mathf.Abs(Scroll) > float.Epsilon)
        {
            if (CurrentSplineEdit && SplineTransforms.Contains(CurrentSplineEdit.gameObject))
            {
                int Index = SplineTransforms.IndexOf(CurrentSplineEdit.gameObject);
                Scales[Index] = Mathf.Clamp(Scales[Index] + Scroll * 4, MinRadius, MaxRadius);
            }
            else
            {
                WorkingScale = Mathf.Clamp(WorkingScale + Scroll * 4, MinRadius, MaxRadius);
            }
        }
    }

    private void RotateAround()
    {
        float Scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (Mathf.Abs(Scroll) > float.Epsilon)
        {
            if (CurrentSplineEdit != null)
            {
                CurrentSplineEdit.transform.RotateAroundLocal(Vector3.forward, Scroll * 2f);
            }
            else
            {
                Quaternion delta = Quaternion.AngleAxis(Scroll * 2f * Mathf.Rad2Deg, WorkingRot * Vector3.forward);
                WorkingRot *= delta;
            }
        }
    }

    private void UpdatePointRotations()
    {
        for (int i = 1; i < SplineTransforms.Count - 1; i++)
        {
            Transform trans = SplineTransforms[i].transform;

            Vector3 FirstDir = (trans.position - SplineTransforms[i - 1].transform.position).normalized;
            Vector3 SecondDir = (SplineTransforms[i + 1].transform.position - trans.position).normalized;
            Quaternion Rot = Quaternion.LookRotation((FirstDir + SecondDir).normalized, trans.up);

            trans.rotation = Rot;
        }

        if (SplineTransforms.Count > 1)
        {
            Transform Last = SplineTransforms[SplineTransforms.Count - 1].transform;
            Transform SecondLast = SplineTransforms[SplineTransforms.Count - 2].transform;
            Last.rotation = Quaternion.LookRotation((Last.position - SecondLast.position).normalized, Last.up);
        }
    }

    private void UpdateIntermediates()
    {
        for (int i = 0; i < SplineTransforms.Count - 1; i++)
        {
            Vector3 Pos = Vector3.Lerp(SplineTransforms[i].transform.position, SplineTransforms[i + 1].transform.position, 0.5f);

            Transform InterTrans = IntermediatePoints[i].transform;

            Quaternion First = Quaternion.LookRotation((InterTrans.position - SplineTransforms[i].transform.position).normalized, SplineTransforms[i].transform.up);
            Quaternion Second = Quaternion.LookRotation((SplineTransforms[i + 1].transform.position - InterTrans.position).normalized, SplineTransforms[i + 1].transform.up);
            Quaternion Rot = Quaternion.Slerp(First, Second, 0.5f);

            if (!CurrentSplineEdit || CurrentSplineEdit.transform != IntermediatePoints[i].transform)
            {
                InterTrans.position = Pos;
            }
            InterTrans.rotation = Rot;
        }
    }
}

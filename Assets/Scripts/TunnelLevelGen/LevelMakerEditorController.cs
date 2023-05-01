using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;

public class LevelMakerEditorController : MonoBehaviour
{
    public float MoveSpeed = 10f;
    public float TurnSpeed = 3f;
    public float SpeedUp = 2.5f;

    public GameObject AddPrefab;
    public List<GameObject> ShapePrefabs;

    public Mesh VisualMesh;
    public Material SelectionUIMaterial;

    public float MinRadius = 3f;
    public float MaxRadius = 10f;
    float AddDistance => MaxRadius * 2f;

    private GameObject CurrentSplineEdit;
    private List<GameObject> SplineTransforms = new List<GameObject>();
    private List<GameObject> IntermediatePoints = new List<GameObject>();
    private List<float> _Scales = new List<float>();
    private List<int> Shapes = new List<int>();
    float Scales(int Index) => TransformScale(_Scales[Index]);

    private float Yaw = 0f;
    private float Pitch = 0f;

    private Vector3 WorkingPos = Vector3.zero;
    private Quaternion WorkingRot = Quaternion.identity;
    private float _WorkingScale = 15f;
    private float TooCloseDistance => MinRadius * 2f;
    float WorkScale => TransformScale(_WorkingScale);

    float TransformScale(float Value) => Mathf.Lerp(MinRadius, MaxRadius, (float)(Mathf.RoundToInt((Value - MinRadius) / (MaxRadius - MinRadius) * 3f)) / 3f);

    private int CurrentSelection;
    const float TilingX = 1f / 4f;
    const float TIlingY = 1f / 3f;

    public bool DisableEdit = false;

    private void Awake()
    {
        _WorkingScale = (MinRadius + MaxRadius) * 0.5f;
        AddPrefab = Instantiate(AddPrefab);
    }

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

        if (DisableEdit)
        {
            return;
        }

        NumPress();
        if (SelectionUIMaterial)
        {
            Vector2 Offset = new Vector2(1f + CurrentSelection % 4 * TilingX, 2f / 3f - (int)(CurrentSelection / 4) * TIlingY);
            SelectionUIMaterial.SetTextureOffset("_BaseMap", Offset);
            Graphics.DrawMesh(VisualMesh, transform.position + transform.forward * 5f - transform.right * 3f - transform.up * 2f, Quaternion.LookRotation(transform.forward, transform.up), SelectionUIMaterial, 0);
        }

        if (Input.GetMouseButtonDown(0))
        {
            SelectSplinePoint();
        }

        if (Input.GetMouseButton(0) && CurrentSplineEdit && SplineTransforms.Contains(CurrentSplineEdit.gameObject))
        {
            DragCurrent();
        }

        if (CurrentSplineEdit && IntermediatePoints.Contains(CurrentSplineEdit.gameObject))
        {
            int Index = IntermediatePoints.IndexOf(CurrentSplineEdit.gameObject);
            if (Vector3.Distance(SplineTransforms[Index].transform.position, CurrentSplineEdit.transform.position) < TooCloseDistance
            || Vector3.Distance(SplineTransforms[Index + 1].transform.position, CurrentSplineEdit.transform.position) < TooCloseDistance)
            {
                CurrentSplineEdit.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            }
            else
            {
                CurrentSplineEdit.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
            }
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (CurrentSplineEdit != null && IntermediatePoints.Contains(CurrentSplineEdit.gameObject))
            {
                if (CurrentSplineEdit.GetComponentInChildren<MeshRenderer>().material.color != Color.red)
                {
                    InsertSplinePoint();
                }
            }
            else
            {
                if (CurrentSplineEdit != null)
                {
                    CurrentSplineEdit.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
                    CurrentSplineEdit = null;
                }

                AddSplinePoint();
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

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            Delete();
        }

        UpdatePointRotations();
        UpdateIntermediates();
        for (int i = 0; i < SplineTransforms.Count; i++)
        {
            SplineTransforms[i].transform.localScale = Vector3.one * Scales(i);
        }

        if (SplineTransforms.Count > 0)
        {
            Transform Last = SplineTransforms[SplineTransforms.Count - 1].transform;
            WorkingPos = Last.position + Last.forward * AddDistance;
            WorkingRot = Quaternion.LookRotation(Last.forward, WorkingRot * Vector3.up);
        }

        AddPrefab.transform.position = WorkingPos;
        AddPrefab.transform.rotation = WorkingRot;
        AddPrefab.transform.localScale = Vector3.one * WorkScale;

        if (Input.GetKeyDown(KeyCode.B))
        {
            SplineNoise3D.SplineLine.Clear();
            for (int i = 0; i < SplineTransforms.Count; i++)
            {
                Transform Trans = SplineTransforms[i].transform;
                SplineNoise3D.AddSplineSegment(Trans.position, Trans.rotation, Scales(i), GetShape(Shapes[i]), (byte) Shapes[i]);
                if (i < SplineTransforms.Count - 1)
                {
                    Vector4 FirstShape = GetShape(Shapes[i]);
                    Vector4 SecondShape = GetShape(Shapes[i + 1]);
                    Vector4 Shape = Vector4.Lerp(FirstShape, SecondShape, 0.5f);
                    float scale = (Scales(i) + Scales(i + 1)) * 0.5f;
                    Transform IT = IntermediatePoints[i].transform;
                    // NOTE: Uncomment to add intermediate points
                    //SplineNoise3D.AddSplineSegment(IT.position, IT.rotation, scale, Shape);
                }
            }
            StartCoroutine(FindObjectOfType<Serializer>().Generate(FindObjectOfType<Serializer>().SerializeLevelToJson()));
        }
    }

    private void Delete()
    {
        if (CurrentSplineEdit && SplineTransforms.Contains(CurrentSplineEdit.gameObject))
        {
            int index = SplineTransforms.IndexOf(CurrentSplineEdit.gameObject);
            GameObject cube = SplineTransforms[index];
            SplineTransforms.RemoveAt(index);
            Destroy(cube);
            if (IntermediatePoints.Count > 0)
            {
                GameObject sphere = IntermediatePoints[0];
                IntermediatePoints.RemoveAt(0);
                Destroy(sphere);
            }
        }
    }

    private void NumPress()
    {
        int prev = CurrentSelection;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CurrentSelection = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CurrentSelection = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CurrentSelection = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CurrentSelection = 3;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            CurrentSelection = 4;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            CurrentSelection = 5;
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            CurrentSelection = 6;
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            CurrentSelection = 7;
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            CurrentSelection = 8;
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            CurrentSelection = 9;
        }
        
        if (prev != CurrentSelection && CurrentSplineEdit && SplineTransforms.Contains(CurrentSplineEdit.gameObject))
        {
            int index = SplineTransforms.IndexOf(CurrentSplineEdit.gameObject);
            Shapes[index] = CurrentSelection;

            GameObject original = CurrentSplineEdit;
            GameObject newObject = Instantiate(ShapePrefabs[CurrentSelection]);
            newObject.transform.position = original.transform.position;
            newObject.transform.rotation = original.transform.rotation;
            newObject.transform.localScale = original.transform.localScale;
            SplineTransforms[index] = newObject;
            Destroy(original);
            CurrentSplineEdit = newObject;
            CurrentSplineEdit.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
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

        if (SplineTransforms.Contains(CurrentSplineEdit.gameObject) && SplineTransforms.Count > 1)
        {
            int Index = SplineTransforms.IndexOf(CurrentSplineEdit.gameObject);
            Vector3 Reference;
            if (Index == 0)
            {
                Reference = SplineTransforms[1].transform.position;
            }
            else
            {
                Reference = SplineTransforms[Index - 1].transform.position;
            }

            Vector3 fromRef = SplineTransforms[Index].transform.position - Reference;
            if (fromRef.magnitude > 4f * MaxRadius)
            {
                SplineTransforms[Index].transform.position = Reference + fromRef.normalized * 4f * MaxRadius;
            }
        }
    }

    private void SelectSplinePoint()
    {
        if (CurrentSplineEdit)
        {
            CurrentSplineEdit.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        }
        CurrentSplineEdit = null;

        bool IsTooClose = false;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == AddPrefab.transform)
            {
                AddSplinePoint();
            }
            else
            {
                int i = 0;
                foreach (var cube in SplineTransforms)
                {
                    if (cube.transform == hit.transform)
                    {
                        CurrentSplineEdit = cube;
                        CurrentSelection = Shapes[i];
                        _WorkingScale = _Scales[i];
                        WorkingRot = CurrentSplineEdit.transform.rotation;
                    }
                    i++;
                }

                foreach (var sphere in IntermediatePoints)
                {
                    if (sphere.transform == hit.transform)
                    {
                        CurrentSplineEdit = sphere;
                        int Index = IntermediatePoints.IndexOf(sphere);
                        if (Vector3.Distance(SplineTransforms[Index].transform.position, SplineTransforms[Index + 1].transform.position) < TooCloseDistance)
                        {
                            IsTooClose = true;
                        }
                        else
                        {
                            InsertSplinePoint();
                        }
                        break;
                    }
                }
            }
        }

        if (CurrentSplineEdit)
        {
            CurrentSplineEdit.GetComponentInChildren<MeshRenderer>().material.color = IsTooClose ? Color.red : Color.yellow;
        }
    }

    private void _OnDrawGizmos()
    {
        for (int i = 0; i < SplineTransforms.Count - 1; i++)
        {
            Debug.DrawLine(SplineTransforms[i].transform.position, IntermediatePoints[i].transform.position, Color.red);
            Debug.DrawLine(IntermediatePoints[i].transform.position, SplineTransforms[i + 1].transform.position, Color.red);
        }

        for (int i = 0; i < SplineTransforms.Count; i++)
        {
            Gizmos.DrawWireSphere(SplineTransforms[i].transform.position, Scales(i));
        }
        Gizmos.DrawWireSphere(WorkingPos, WorkScale);
    }

    private void AddSplinePoint()
    {
        AddSplinePoint((byte)CurrentSelection, WorkingPos, WorkingRot, MinRadius);
 
        WorkingPos += WorkingRot * Vector3.forward * AddDistance;
    }

    private void AddSplinePoint(byte selection, Vector3 pos, Quaternion rot, float radius)
    {
        GameObject cube = Instantiate(ShapePrefabs[selection]);
        cube.transform.position = pos;
        cube.transform.rotation = rot;
        cube.transform.localScale = Vector3.one * radius;

        SplineTransforms.Add(cube);
        Shapes.Add(CurrentSelection);
        _Scales.Add(WorkScale);

        if (SplineTransforms.Count > 1)
        {
            Vector3 Pos = Vector3.Lerp(SplineTransforms[SplineTransforms.Count - 1].transform.position, SplineTransforms[SplineTransforms.Count - 2].transform.position, 0.5f);
            Quaternion Rot = Quaternion.Slerp(SplineTransforms[SplineTransforms.Count - 1].transform.rotation, SplineTransforms[SplineTransforms.Count - 2].transform.rotation, 0.5f);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = Pos;
            sphere.transform.rotation = Rot;
            sphere.transform.localScale = Vector3.one * radius;
            IntermediatePoints.Add(sphere);
        }
    }

    private void InsertSplinePoint()
    {
        int Index = IntermediatePoints.IndexOf(CurrentSplineEdit.gameObject);
        Vector3 NewPos = CurrentSplineEdit.transform.position;
        Quaternion NewRot = CurrentSplineEdit.transform.rotation;

        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject cube = Instantiate(ShapePrefabs[CurrentSelection]);
        cube.transform.position = NewPos;
        cube.transform.rotation = NewRot;
        cube.transform.localScale = Vector3.one * MinRadius;
        SplineTransforms.Insert(Index + 1, cube);
        Shapes.Insert(Index + 1, CurrentSelection);
        _Scales.Insert(Index + 1, WorkScale);

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
            sphere.transform.localScale = Vector3.one * MinRadius;
            IntermediatePoints.Insert(Index + 1, sphere);
        }

        CurrentSplineEdit.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
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
                _Scales[Index] = Mathf.Clamp(_Scales[Index] + Scroll * 14f, MinRadius, MaxRadius);
            }
            else
            {
                _WorkingScale = Mathf.Clamp(_WorkingScale + Scroll * 14f, MinRadius, MaxRadius);
            }
        }
    }

    private void RotateAround()
    {
        float Scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (Mathf.Abs(Scroll) > float.Epsilon)
        {
            Quaternion delta = Quaternion.AngleAxis(Scroll * 2f * Mathf.Rad2Deg, WorkingRot * Vector3.forward);
            WorkingRot *= delta;
            if (CurrentSplineEdit != null)
            {
                CurrentSplineEdit.transform.rotation = WorkingRot;
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
        for (int i = 0; i < IntermediatePoints.Count; i++)
        {
            Transform First = SplineTransforms[i].transform;
            Transform Second = SplineTransforms[i + 1].transform;

            Vector3 Pos = Vector3.Lerp(First.position, Second.position, 0.5f);
            Quaternion Rot = Quaternion.Slerp(First.rotation, Second.rotation, 0.5f);

            IntermediatePoints[i].transform.position = Pos;
            IntermediatePoints[i].transform.rotation = Rot;
        }
            /*
        for (int i = 0; i < SplineTransforms.Count - 1; i++)
        {
            Vector3 Pos;
            if (i > 0 && i < SplineTransforms.Count - 2)
            {
                float Distance = Vector3.Distance(SplineTransforms[i].transform.position, SplineTransforms[i + 1].transform.position);
                Vector3 Dir1 = (SplineTransforms[i].transform.position - SplineTransforms[i - 1].transform.position).normalized;
                Vector3 Dir2 = (SplineTransforms[i + 1].transform.position - SplineTransforms[i + 2].transform.position).normalized;
                Vector3 Point1 = SplineTransforms[i].transform.position + Dir1 * Distance;
                Vector3 Point2 = SplineTransforms[i + 1].transform.position + Dir2 * Distance;

                Vector3 Point = (Point1 + Point2) * 0.5f;
                Vector3 Pos1 = Vector3.Lerp(SplineTransforms[i].transform.position, Point, 0.4f);
                Vector3 Pos2 = Vector3.Lerp(Point, SplineTransforms[i + 1].transform.position, 0.6f);
                Pos = Vector3.Lerp(Pos1, Pos2, 0.5f);
            }
            else
            {
                Pos = Vector3.Lerp(SplineTransforms[i].transform.position, SplineTransforms[i + 1].transform.position, 0.5f);
            }

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
            */
    }

    private Vector4 GetShape(int Selection)
    {
        // x = up
        // y = right
        // z = down
        // w = left
        Vector4 Shape = Vector4.zero;
        switch (Selection)
        {
            case 0: { break; }
            case 1: { Shape.x = 0.5f; break; }
            case 2: { Shape.x = 0.5f; Shape.w = 0.5f; break; }
            case 3: { Shape.x = 0.5f; Shape.y = 0.5f; break; }
            case 4: { Shape.x = 0.5f; Shape.y = 0.5f; Shape.w = 0.5f; break; }
            case 5: { Shape.z = 0.5f; break; }
            case 6: { Shape.z = 0.5f; Shape.x = 0.5f; break; }
            case 7: { Shape.z = 0.5f; Shape.x = 0.5f; Shape.w = 0.5f; break; }
            case 8: { Shape.z = 0.5f; Shape.x = 0.5f; Shape.y = 0.5f; break; }
            case 9: { Shape.z = 0.5f; Shape.x = 0.5f; Shape.y = 0.5f; Shape.w = 0.5f; break; }
        }
        return Shape;
    }

    public void InitFromSpline()
    {
        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        foreach (SplineNoise3D.Spline spline in SplineNoise3D.SplineLine)
        {
            pos = spline.pos;
            rot = spline.rot;
            AddSplinePoint(spline.shape, pos, rot, spline.radius);
        }

        WorkingPos = pos + rot * Vector3.forward * AddDistance;
    }
}

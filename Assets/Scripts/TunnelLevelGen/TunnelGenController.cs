using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Rendering;
using UnityEngine;

public class TunnelGenController : MonoBehaviour
{
    public float speed = 5;
    public float roll_speed = 5;
    public float splineDistance;
    public float mouse_sensitivity = 5;

    public PointCloudManager PCM;
    private Vector3 lastSpline;
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


        Debug.DrawLine(transform.position, SplineNoise3D.getPointOnSpline(transform.position), Color.green);

    }
    private void Start()
    {
        lastSpline = transform.position;
    }
    void Update()
    {
        UpdateSpline();
        Vector3 vel = Vector3.zero;
        var rot_dir = boolToInt(Input.GetKey(KeyCode.A)) - boolToInt(Input.GetKey(KeyCode.D));
        float mouse_y = Input.GetAxis("Mouse Y");
        float mouse_x = Input.GetAxis("Mouse X");
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
        if((lastSpline - transform.position).magnitude > splineDistance)
        {
            SplineNoise3D.AddSplineSegment(transform, 10f);
            PCM.InitializeIsoSurfaceSphere(transform.position, 1f, SuperNoiseHole);
            lastSpline = transform.position;
            Debug.Log("generate");
        }
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilderConnector : MonoBehaviour
{
    public Transform start;
    public Transform finish;
    public LineRenderer LR;
    private Vector3[] points = new Vector3[2];
    // Update is called once per frame
    void Update()
    {
        transform.position = start.position;
        transform.forward = start.forward;
        float distance = (finish.position - start.position).magnitude;
        transform.localScale = new Vector3(start.localScale.x, start.localScale.y, distance);
        points[0] = start.position;
        points[1] = finish.position;
        LR.SetPositions(points);
    }
}

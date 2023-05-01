using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilderConnector : MonoBehaviour
{
    public Transform finish;
    public GameObject ConnectorPrefab;

    private LineRenderer LR;
    private Vector3[] points = new Vector3[2];
    private GameObject connector;
    private void Start()
    {
        connector = GameObject.Instantiate(ConnectorPrefab);
        LR = connector.GetComponentInChildren<LineRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
        if (finish == null) return;
        float distance = (finish.position - transform.position).magnitude;
        connector.transform.forward = (finish.position - transform.position).normalized;
        connector.transform.position = transform.position;
        connector.transform.localScale = new Vector3(1f, 1f, distance);
        points[0] = transform.position;
        points[1] = finish.position;
        LR.SetPositions(points);
    }
    private void OnDestroy()
    {
        GameObject.Destroy(connector);
    }
}

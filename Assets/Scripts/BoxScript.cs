using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoxScript : MonoBehaviour
{
    public HoverController Owner;
    public Vector3 Offset = new Vector3(0.0f, 0.5f, -1.0f);
    public float GravitySuction = 100.0f;
    public float SuctionDistance = 3.0f;

    private new Rigidbody rigidbody;
    private MeshFilter Mesh;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        Mesh = GetComponent<MeshFilter>();
    }

    private void FixedUpdate()
    {
    }
}

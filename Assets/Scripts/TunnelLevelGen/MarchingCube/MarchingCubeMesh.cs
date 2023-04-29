using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Threading;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
public class MarchingCubeMesh : MonoBehaviour
{
    //Mesh shit
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public Mesh mesh;
    [NonSerialized]
    public Vector3[] optimizedVerts;
    [NonSerialized]
    public int[] optimizedTris;
    private int size = 8;

    //Unity callbacks
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.05f);
        Gizmos.DrawCube(transform.position + Vector3.one * (size / 2f), new Vector3(size, size, size));
    }
    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        //Inizialize
        mesh = new Mesh();
        meshFilter.sharedMesh = mesh;
    }
}

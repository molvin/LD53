using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudManager : MonoBehaviour
{
    public MarchingCubeMesh Prefab8X8;
    public string GeoLayer;

    public float isoSurface;
    private Dictionary<Vector3Int, MarchingCubeMesh> meshGrid = new Dictionary<Vector3Int, MarchingCubeMesh>();

    //Shader shit
    MarchingCubeShaderAPI marchShader;
    private float[] pointCloud;
    private Vector3[] verts;
    private int[] tris;
    private int size = 8;

    public void Awake()
    {
        Perlin3D.scale = 40f;
        marchShader = new MarchingCubeShaderAPI(out pointCloud, out verts, out tris);
        marchShader.IsoSurface = isoSurface;
    }
    public void InitializeIsoSurfaceSphere(Vector3 brushPoint, float brushRadius, Func<Vector3, float> initDef)
    {
        int halfExtend = Mathf.FloorToInt(brushRadius / (size * 1f)) + 2;
        Vector3Int Chunk = new Vector3Int((int)(brushPoint.x / (size * 1f)), (int)(brushPoint.y / (size * 1f)), (int)(brushPoint.z / (size * 1f)));

        for (int x = -halfExtend; x <= halfExtend; x++)
            for (int y = -halfExtend; y <= halfExtend; y++)
                for (int z = -halfExtend; z <= halfExtend; z++)
                    UpdateIsoSurfaceChunk(Chunk + new Vector3Int(x, y, z), initDef);
    }

    public void CreateIsoSurfaceSphere(Vector3 brushPoint, float brushRadius, Func<Vector3, float> initDef)
    {
        int halfExtend = Mathf.FloorToInt(brushRadius / (size * 1f)) + 2;
        Vector3Int Chunk = new Vector3Int((int)(brushPoint.x / (size * 1f)), (int)(brushPoint.y / (size * 1f)), (int)(brushPoint.z / (size * 1f)));

        for (int x = -halfExtend; x <= halfExtend; x++)
            for (int y = -halfExtend; y <= halfExtend; y++)
                for (int z = -halfExtend; z <= halfExtend; z++)
                    CreateIsoSurfaceChunk(Chunk + new Vector3Int(x, y, z), initDef);
    }


    public void UpdateIsoSurfaceChunk(Vector3Int chunkId, Func<Vector3, float> initDef)
    {
        if (!meshGrid.ContainsKey(chunkId))
        {
            MarchingCubeMesh mesh = Instantiate(Prefab8X8, chunkId * size, Quaternion.identity, transform);
            meshGrid.Add(chunkId, mesh);
        }
        MarchingCubeMesh temp = meshGrid[chunkId];
        SetPointCloud(initDef, new Vector3(chunkId.x, chunkId.y, chunkId.z) * (size * 1f));
        marchShader.MarchCloud(ref pointCloud, ref verts, ref tris);
        temp.gameObject.layer = LayerMask.NameToLayer(GeoLayer);
        CleanVerts(temp);
    }

    public void CreateIsoSurfaceChunk(Vector3Int chunkId, Func<Vector3, float> initDef)
    {
        if (meshGrid.ContainsKey(chunkId))
        {
            return;
        }
        MarchingCubeMesh mesh = Instantiate(Prefab8X8, chunkId * size, Quaternion.identity, transform);
        meshGrid.Add(chunkId, mesh);
        MarchingCubeMesh temp = meshGrid[chunkId];
        SetPointCloud(initDef, new Vector3(chunkId.x, chunkId.y, chunkId.z) * (size * 1f));
        marchShader.MarchCloud(ref pointCloud, ref verts, ref tris);
        temp.gameObject.layer = LayerMask.NameToLayer(GeoLayer);
        CleanVerts(temp);
    }

    private void CleanVerts(MarchingCubeMesh current)
    {
        int lastRealIndex = 0;
        for (int i = 0; i < verts.Length; i++)
        {
            if (verts[i].x >= 0)
            {
                verts[lastRealIndex] = verts[i];
                lastRealIndex++;
            }
        }

        //Make smaller
        current.optimizedVerts = new Vector3[lastRealIndex];
        current.optimizedTris = new int[lastRealIndex];
        Array.Copy(verts, current.optimizedVerts, lastRealIndex);
        Array.Copy(tris, current.optimizedTris, lastRealIndex);

        //Apply mesh
        current.meshFilter.sharedMesh.Clear();
        current.meshFilter.sharedMesh.vertices = current.optimizedVerts;
        current.meshFilter.sharedMesh.triangles = current.optimizedTris;
        current.meshFilter.sharedMesh.RecalculateNormals();
        current.meshCollider.sharedMesh = current.meshFilter.sharedMesh;
    }
    public void SetPointCloud(Func<Vector3, float> initDef, Vector3 pos)
    {
        for (int z = 0; z < (size + 1); z++)
            for (int y = 0; y < (size + 1); y++)
                for (int x = 0; x < (size + 1); x++)
                {
                    int id = x + ((size + 1) * y) + ((size + 1) * (size + 1) * z);
                    pointCloud[id] = initDef.Invoke(pos + new Vector3(x, y, z));
                }
    }

    public void Clear()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        meshGrid.Clear();
    }

    public void OnDestroy()
    {
        marchShader.Release();
    }
}

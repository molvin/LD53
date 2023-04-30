using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testCreate : MonoBehaviour
{
    public PointCloudManager PCM;
    public float scale;
    public float maxIsoDepth;
    // Start is called before the first frame update
    // Update is called once per frame
    void Update()
    {
        createNoiseEnvo();
    }

    public void createNoiseEnvo()
    {
        PCM.InitializeIsoSurfaceSphere(Vector3.zero, 10f, SuperNoise);
      
    }
    public float SuperNoise(Vector3 point)
    {
        return 0f;
        Perlin3D.scale = scale;
        return Perlin3D.PerlinNoise3D(point) * maxIsoDepth;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public Serializer serializer;
    public GameObject TruckPrefab;
    public GameObject CameraPrefab;

    private void Start()
    {
        StartRun();
    }
    public void StartRun()
    {
        IEnumerator Coroutine()
        {
            Debug.Log($"Starting run {serializer}");
            if (PersistentData.LevelPath != null)
            {
                string data = System.IO.File.ReadAllText(PersistentData.LevelPath);
                yield return new WaitForSeconds(0.5f);
                var generateIter = serializer.Generate(data);
                Debug.Log("Generating");
                while (generateIter.MoveNext())
                {
                    float progress = (float)generateIter.Current;
                    yield return null;
                }
            }
            else
            {
                Debug.LogError("No Level Path Set");
            }
            yield return new WaitForSeconds(0.5f);

            DestroyImmediate(Camera.main.gameObject);
            Vector3 origin = SplineNoise3D.SplineLine[0].pos;
            Instantiate(TruckPrefab, origin, Quaternion.identity);
            Instantiate(CameraPrefab, origin, Quaternion.identity);

        }

        StartCoroutine(Coroutine());
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class TunnelGenController : MonoBehaviour
{


    public float speed = 5;

    // Update is called once per frame
    void Update()
    {
        Vector3 vel = Vector3.zero;
       

        vel += Vector3.up * boolToInt(Input.GetKey(KeyCode.Space));
        vel += Vector3.down * boolToInt(Input.GetKey(KeyCode.LeftShift));
        vel += Vector3.right * boolToInt(Input.GetKey(KeyCode.D));
        vel += Vector3.left * boolToInt(Input.GetKey(KeyCode.A));
        vel += Vector3.forward * boolToInt(Input.GetKey(KeyCode.W));
        vel += Vector3.back * boolToInt(Input.GetKey(KeyCode.S));

        this.transform.position += vel * speed * Time.deltaTime;
    }

    private int boolToInt(bool b)
    {
        return b ? 1 : 0;
    }
}

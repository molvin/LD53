using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Rendering;
using UnityEngine;

public class TunnelGenController : MonoBehaviour
{
    // forward
    // tilt

    //rotate front 

    public float speed = 5;

    public float roll_speed = 5;


    public float mouse_sensitivity = 5;
    // Update is called once per frame
    void Update()
    {
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
        
        vel += Vector3.forward * boolToInt(Input.GetKey(KeyCode.W));
        this.transform.position += vel * speed * Time.deltaTime;
    }

    private int boolToInt(bool b)
    {
        return b ? 1 : 0;
    }
}

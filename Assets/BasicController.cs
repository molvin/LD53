using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicController : MonoBehaviour
{
    public float Speed;
    public float SprintMultiplier;
    public float RotationSpeed;

    private Vector3 lastMousePos;

    void Update()
    {
        float speed = Speed;
        if (Input.GetKey(KeyCode.LeftShift))
            speed *= SprintMultiplier;
        Vector3 velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * speed * Time.deltaTime;
        transform.position += transform.TransformDirection(velocity);
        
        if(Input.GetMouseButton(1))
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePos;
            transform.Rotate(transform.up, mouseDelta.x * RotationSpeed * Time.deltaTime);
            transform.Rotate(transform.right, mouseDelta.y * RotationSpeed * Time.deltaTime);
        }
        lastMousePos = Input.mousePosition;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed = 1;

    private Camera cam;

    public void Start()
    {
        cam = Camera.main;
    }


    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W))
        {
            cam.transform.position += new Vector3(0, 1 * speed, 0);
        }

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            cam.transform.position += new Vector3(0, -1 * speed, 0);
        }
    }
}
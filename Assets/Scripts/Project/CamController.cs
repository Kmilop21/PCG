using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public float Speed;
    public float RotSpeed = 5f;
    private Vector3 dir;

    private CharacterController controller;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float x = 0, y = 0, z = 0;
        if (Input.GetKey(KeyCode.D))
            x = 1;
        else if (Input.GetKey(KeyCode.A))
            x = -1;
        
        if (Input.GetKey(KeyCode.Q))
            y = 1;
        else if (Input.GetKey(KeyCode.E))
            y = -1;


        if (Input.GetKey(KeyCode.W))
            z = 1;
        else if (Input.GetKey(KeyCode.S))
            z = -1;

        controller.Move(new Vector3(x, y, z) * Speed * Time.deltaTime);

        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            if (mouseX != 0 || mouseY != 0)
            {
                transform.Rotate(Vector3.right, mouseY * RotSpeed * Time.deltaTime);
                transform.Rotate(Vector3.up, mouseX * RotSpeed * Time.deltaTime, Space.World);
            }
        }
    }
}

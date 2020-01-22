using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public float boundariesX1 = 0f;
    public float boundariesX2 = 50f;
    public float speed = 3f;

    void Start(){
       // cam.aspect = 16f/9f;

    }
    void LateUpdate()
    {

        Vector3 newPosition = player.position;
        newPosition.z = -10;

        transform.position = Vector3.Slerp(new Vector3(transform.position.x, transform.position.y, transform.position.z), 
            new Vector3(newPosition.x + offset.x, newPosition.y + offset.y, newPosition.z), speed * Time.deltaTime);

        transform.position += new Vector3(Input.GetAxisRaw("Mouse X") * Time.deltaTime * speed, Input.GetAxisRaw("Mouse Y") * Time.deltaTime * speed, 0);

    }
}

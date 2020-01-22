using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotating : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;
    public float rotateSpeed = 200f;
    private Rigidbody2D rb;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Vector2 direction = (Vector2)target.position - (Vector2)this.transform.position;

        //direction.Normalize();

        //float rotateAmount = Vector3.Cross(direction, transform.right).z;

        //rb.angularVelocity = -rotateAmount * rotateSpeed;

        //rb.velocity = transform.up * speed;


        //Quaternion rotation = Quaternion.LookRotation
        //    (target.position - transform.position, transform.TransformDirection(Vector3.right));
        //transform.rotation = new Quaternion(0, 0, rotation.z, rotation.w);


        transform.right = target.position - transform.position;
    }
}

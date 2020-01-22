using UnityEngine;
using System.Collections;

public class  MouseFollow : MonoBehaviour
{

    private Vector3 mousePosition;
    public float moveSpeed = 0.1f;

    void Update()
    {
        //var dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        //var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //transform.position = Input.mousePosition;
        //transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePosition;
    }
}
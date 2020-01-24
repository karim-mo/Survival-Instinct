using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class leftMeteorsMovement : MonoBehaviour
{
    
    void Start()
    {
        StartCoroutine("Move");   
    }



    IEnumerator Move()
    {
        Vector3 pos = transform.position;

        for(int i = 0; i < 5; i++)
        {
            pos.y += 0.2f;
            transform.position = pos;
            yield return new WaitForSeconds(0.1f);
        }

        for (int i = 0; i < 5; i++)
        {
            pos.y -= 0.2f;
            transform.position = pos;
            yield return new WaitForSeconds(0.1f);
        }

        StartCoroutine("Move");
    }
}

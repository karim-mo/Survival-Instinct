using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpIdle : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine("Idle");
    }

    IEnumerator Idle()
    {        
        for (int i = 0; i < 10; i++)
        {
            Vector3 pos = transform.position;
            pos.y -= 0.02f;
            transform.position = pos;
            yield return new WaitForSeconds(0.05f);
        }
        for (int i = 0; i < 10; i++)
        {
            Vector3 pos = transform.position;
            pos.y += 0.02f;
            transform.position = pos;
            yield return new WaitForSeconds(0.05f);
        }
        StartCoroutine("Idle");
    }
}

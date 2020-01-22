using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBoom : MonoBehaviour
{
    public float time;

    private void Start()
    {
        StartCoroutine(StartLaser());
    }

    IEnumerator StartLaser()
    {
        yield return new WaitForSeconds(time);
        StartCoroutine("Laser");
    }

    IEnumerator Laser()
    {
        yield return new WaitForSeconds(2f);
        for(float i = 0; i <= 1; i += 0.1f)
        {
            transform.localScale = new Vector3(transform.localScale.x, i, transform.localScale.z);
            yield return new WaitForSeconds(0.07f);
        }
        transform.localScale = new Vector3(transform.localScale.x, 2, transform.localScale.z);
        gameObject.GetComponent<CapsuleCollider2D>().enabled = true;
        yield return new WaitForSeconds(0.7f);
        transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        gameObject.GetComponent<CapsuleCollider2D>().enabled = false;
        StartCoroutine("Laser");
    }
}

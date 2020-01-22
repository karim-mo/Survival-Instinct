using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostShoot : MonoBehaviour
{

    public GameObject prefab;
    public Transform muzzle;

    public void Shoot()
    {

        StartCoroutine("Shooting");
    }


    IEnumerator Shooting()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject sp = Instantiate(prefab, muzzle.position, prefab.transform.rotation);

            Vector2 direction = (Quaternion.Euler(0, 0, 65) * Vector2.down).normalized;
            sp.GetComponent<Rigidbody2D>().AddForce(direction * Time.fixedDeltaTime * 2);
            yield return new WaitForSeconds(0.1f);

        }
    }
}

    


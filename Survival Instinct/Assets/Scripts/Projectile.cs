using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{

    private float timer = 1f;
    public GameObject particle;


    void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            GameObject parti = Instantiate(particle, transform.position, Quaternion.identity);
            Destroy(gameObject);
            timer = 0.5f;
        } 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Ground")
        {
            
            GameObject parti = Instantiate(particle, transform.position, Quaternion.identity);
            AudioManager.Play("BulletHit");
            Destroy(gameObject);
        }
    }

}
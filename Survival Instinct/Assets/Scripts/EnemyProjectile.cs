using UnityEngine;
using System.Collections;
using Photon.Pun;

public class EnemyProjectile : MonoBehaviour, IPunObservable
{
    public float damage;

    private float timer = 0.7f;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Destroy(gameObject);
            timer = 0.5f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            AudioManager.Play("ProjectileHit");         
            if (!collision.GetComponent<PlayerController>().isJumpDashing)
                collision.GetComponent<PlayerController>().DamagePlayer(damage);
            Destroy(gameObject);
        }
        if(collision.tag == "Ground")
        {
            AudioManager.Play("ProjectileHit");
            Destroy(gameObject);
        }
        if (collision.tag == "Shield")
        {
            PlayerController player = collision.gameObject.transform.parent.gameObject.GetComponent<PlayerController>();
            player.shield.DamageShield(damage);
            
            //FindObjectOfType<PlayerController>().shield.DamageShield(damage);
            AudioManager.Play("Shield");
            Destroy(gameObject);
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(damage);
        }
        else
        {
            this.damage = (float)stream.ReceiveNext();
        }
    }

}
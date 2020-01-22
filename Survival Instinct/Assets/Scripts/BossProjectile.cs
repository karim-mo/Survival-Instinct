using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BossProjectile : MonoBehaviourPun, IPunObservable
{
    public float damage;

    void Start()
    {
        Destroy(gameObject, 6);
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

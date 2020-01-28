using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ExploDEATH : MonoBehaviourPun, IPunObservable
{

    BoxCollider2D bc;

    public float damage;

    private void Start()
    {
        bc = GetComponent<BoxCollider2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            collision.GetComponent<PlayerController>().DamagePlayer(damage);
        }
    }

    public void activateBC()
    {
        if(bc != null) bc.enabled = true;
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

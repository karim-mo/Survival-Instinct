using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Meteors : MonoBehaviourPun, IPunObservable
{
    public float timer;
    public GameObject explo;
    public float damage;

    private void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "meteorGround")
        {
            GameObject _explo = PhotonNetwork.Instantiate(explo.name, transform.position, Quaternion.identity);
            _explo.GetComponent<ExploDEATH>().damage = 0.5f * damage;
            PhotonNetwork.Destroy(this.gameObject);
        }

        if (collision.tag == "Player" && !collision.GetComponent<PlayerController>().iframed)
        {
            Vector3 pos = transform.position;
            pos.y -= 0.5f;
            GameObject _explo = PhotonNetwork.Instantiate(explo.name, pos, Quaternion.identity);
            _explo.GetComponent<ExploDEATH>().damage = damage;
            PhotonNetwork.Destroy(this.gameObject);
        }

        if (collision.tag == "Shield")
        {
            Vector3 pos = transform.position;
            pos.y -= 0.5f;
            pos.x = pos.x > collision.gameObject.transform.position.x ? pos.x -= 0.5f : pos.x += 0.5f;
            GameObject _explo = PhotonNetwork.Instantiate(explo.name, pos, Quaternion.identity);
            _explo.GetComponent<ExploDEATH>().damage = 0.3f * damage;

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject p in players)
            {
                //Debug.Log(p);
                if (p.GetComponent<PlayerController>().isMine())
                {
                    PlayerController player = p.GetComponent<PlayerController>();
                    player.shield.DamageShield(damage);
                    break;
                }
            }

            PhotonNetwork.Destroy(this.gameObject);
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

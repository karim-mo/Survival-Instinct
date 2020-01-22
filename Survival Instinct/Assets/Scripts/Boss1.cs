using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Boss1 : MonoBehaviourPun, IPunObservable
{
    public LayerMask Boundary;
    public float health;
    public float damage;
    public GameObject onDeath;
    public GameObject fromPlayer;

    [Header("Firing & Powerups")]
    public GameObject bullet;
    public float fireRate = 10000f;
    public float projectileSpeed = 1f;
    public float projectileCount = 20f;
    public float spreadAngle = 8.0f;

    private Rigidbody2D rb;
    private PlayerController player;
    private bool left = false;
    bool canMove = true;
    bool unity = false;
    bool iframed = false;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        StartCoroutine("BossSequence");
    }

    private void Update()
    {
        if ((transform.position.x < -33f && left) || (transform.position.x > 14f && !left)) left = !left;
        if(health <= 0)
        { 
            Instantiate(onDeath, transform.position, Quaternion.identity);
            int rnd = Random.Range(FindObjectOfType<LevelStats>().minExpRange, FindObjectOfType<LevelStats>().maxExpRange);
            FindObjectOfType<Experience>().experience += rnd;
            LevelStats.totalExp += rnd;
            PlayerPrefs.SetInt("Exp", FindObjectOfType<Experience>().experience);
            FindObjectOfType<TextOverlay>().Overlay("+" + rnd + " EXP");
            if (PhotonNetwork.OfflineMode) { Destroy(gameObject); EnemyAI.PlayerKills++; }
            else photonView.RPC("DestroyEnemyByID", RpcTarget.AllBuffered, photonView.ViewID);
            EnemyAI.deaths++;          
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (iframed) return;
        //if (collision.tag == "Bullet")
        //{
        //    StartCoroutine("IFrame");
        //    Vector3 a = transform.position;
        //    a.z = -0.4f;
        //    GameObject parti = Instantiate(fromPlayer, a, Quaternion.identity);
        //    parti.transform.parent = this.transform;
        //    health -= player.damage;
        //    FindObjectOfType<TextOverlay>().Overlay(transform, "-" + player.damage, false);
        //}
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ghosts")
        {
            PlayerController PC = null;
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject p in players)
            {
                if (p.GetComponent<PlayerController>().isMine())
                {
                    PC = p.GetComponent<PlayerController>();
                }
            }
            health -= PC.damage;
            if (health > 0)
            {
                photonView.RPC("SetHealth", RpcTarget.OthersBuffered, photonView.ViewID, PC.damage);
            }
            FindObjectOfType<TextOverlay>().Overlay(transform, "-" + 60, false);
            Vector3 a = transform.position;
            a.z = -0.4f;
            GameObject parti = Instantiate(fromPlayer, a, Quaternion.identity);
            parti.transform.parent = this.transform;
        }
        if (collision.tag == "Sword")
        {
            GameObject pl = collision.gameObject.transform.parent.gameObject.transform.parent.gameObject;
            //Debug.Log(pl.GetComponent<PhotonView>().ViewID);
            PlayerController PC = pl.GetComponent<PlayerController>();
            health -= PC.damage;
            if (health > 0)
            {
                photonView.RPC("SetBossHealth", RpcTarget.OthersBuffered, photonView.ViewID, PC.damage);
            }
            FindObjectOfType<TextOverlay>().Overlay(transform, "-" + PC.damage, false);
            Vector3 a = transform.position;
            a.z = -0.4f;
            GameObject parti = Instantiate(fromPlayer, a, Quaternion.identity);


            //Player lifesteal
            if (EnemyAI.lifesteal)
            {
                bool rnd = (Random.Range(1, 100) / 100f) <= 1.0f ? true : false;
                if (rnd)
                {
                    PC.health += PC.lifeSteal * PC.damage;
                }
            }
            AudioManager.Play("EnemyHit");
        }
    }
    void FixedUpdate()
    {
        if (!left && canMove)
        {
            rb.AddForce(Vector2.right * 400 * Time.fixedDeltaTime);
        }
        else if(left && canMove)
        {
            rb.AddForce(Vector2.left * 400 * Time.fixedDeltaTime);
        }

    }

    [PunRPC]
    public void DestroyEnemyByID(int enemyID)
    {
        EnemyAI.PlayerKills++;
        Destroy(PhotonView.Find(enemyID).gameObject);
    }

    protected IEnumerator IFrame()
    {
        iframed = true;       
        yield return new WaitForSeconds(0.2f);
        iframed = false;
    }

    IEnumerator Movement()
    {
        left = !left;
        yield return new WaitForSeconds(7);
        StartCoroutine("Movement");
    }

    IEnumerator BossSequence()
    {
        StartCoroutine("Movement");
        yield return new WaitForSeconds(Random.Range(3, 8));
        StopCoroutine("Movement");
        canMove = false;
        FindObjectOfType<ERROR>().DisplayError("Boss attack incoming. Start running!");
        yield return new WaitForSeconds(1f);
        StartCoroutine("Attack1");
        while (!unity) { yield return null; }
        unity = false;
        StopCoroutine("Attack1");
        canMove = true;
        StartCoroutine("BossSequence");
    }

    IEnumerator Attack1()
    {
        float sa = spreadAngle;
        for (int i = 0; i < (360/sa) * 6; i++)
        {
            if(i % (360/sa) == 0 && i > 0)
            {
                sa -= 4;
            }
            GameObject projectile = PhotonNetwork.Instantiate(bullet.name, transform.position, Quaternion.identity); //fuck unity
            Vector2 direction = Vector2.one;
            direction = Quaternion.Euler(0, 0, sa * i % 360) * direction;
            if(PhotonNetwork.OfflineMode) projectile.GetComponent<Rigidbody2D>().AddForce(direction * 0.02f);
            else photonView.RPC("SendBullet", RpcTarget.AllBuffered, projectile.GetComponent<PhotonView>().ViewID, direction);
            projectile.GetComponent<BossProjectile>().damage = damage;
            yield return new WaitForSeconds(0.03f);
        }
        unity = true;
        
        yield return new WaitForSeconds(0);
        //StartCoroutine("Attack1");
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

    [PunRPC]
    public void SetBossHealth(int eViewID, float dmg)
    {
        PhotonView enemy = PhotonView.Find(eViewID);
        if (enemy == null) return;
        enemy.gameObject.GetComponent<Boss1>().health -= dmg;
    }

    [PunRPC]
    public void SendBullet(int bViewID, Vector2 dir)
    {
        PhotonView.Find(bViewID).gameObject.GetComponent<Rigidbody2D>().AddForce(dir * 0.02f);
    }
}

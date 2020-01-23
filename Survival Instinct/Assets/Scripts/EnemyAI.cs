using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Photon.Pun;
using Photon.Realtime;


public class EnemyAI : MonoBehaviourPun, IPunObservable
{
    #region vars
    [Header("Code Related")]
    public Transform target;
    public GameObject fromPlayer;
    public EnemyController controller;
    public Path path;
    public Transform muzzle;
    public GameObject bullet;
    public Transform fly;
    public GameObject transition;
    public GameObject onDeath;
    public GameObject Trail;
    public LayerMask groundLayer;
    public int editorChildCount;
    public bool pathIsEnded = false;
    public float updateRate = 2f;
    public static bool lifesteal = false;

    [Header("Enemy Configurations")]
    public float Health = 20f;
    public float jumpForce = 200f;
    public float speed = 100f;
    public float damage = 10f;
    public float nextWaypointDistance = 1f;
    public float jumpoffset = 3f;
    public int EnemyType;
    public int intensity;
    public float projectileSpeed = 15f;
    public ForceMode2D fMode;

    public static int deaths = 0;
    public static int PlayerKills = 0;

    private bool jump = false;
    private Seeker seeker;
    private Rigidbody2D rb;
    private Animator anim;
    private PlayerController player;
    private float iFrame = 0.3f;
    private bool iframed = false;
    private float hitTimer = 1.5f;
    private float jumptimer = 0.5f;
    private float gndtimer = 15f;
    private float unreachable = 3f;
    private bool facingRight = false;
    private bool facingPlayer = false;
    private int currentWayPoint = 0;
    private bool knockback = false;
    private bool canMove = true;
    private bool canAttack = true;
    private bool isAttacking = false;
    private bool canShoot = true;
    private float dmg;
    private float streamDmg;
    #endregion


    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Physics2D.IgnoreLayerCollision(8, 12);
        Physics2D.IgnoreLayerCollision(12, 12);
        Physics2D.IgnoreLayerCollision(12, 13);
        if (EnemyType == 0) anim = GetComponent<Animator>();
        iFrame = GameObject.FindGameObjectWithTag("PlayerWeapon").GetComponent<Gun>().fireRate / 1000;

        //GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        //foreach (GameObject p in players)
        //{
        //    if (p.GetComponent<PlayerController>().isMine())
        //    {
        //        player = p.GetComponent<PlayerController>();
        //        break;
        //    }
        //}
        seeker.StartPath(transform.position, target.position, OnPathComplete);

        StartCoroutine(UpdatePath());
    }

    private void Update()
    {
        //Debug.Log("x " + damage);
        if (Health <= 0)
        {
            Instantiate(onDeath, transform.position, Quaternion.identity);
            int rnd = Random.Range(FindObjectOfType<LevelStats>().minExpRange, FindObjectOfType<LevelStats>().maxExpRange);
            FindObjectOfType<Experience>().experience += rnd;
            LevelStats.totalExp += rnd;
            PlayerPrefs.SetInt("Exp", FindObjectOfType<Experience>().experience);
            FindObjectOfType<TextOverlay>().Overlay("+" + rnd + " EXP");
            //player.enemyKills++;
            deaths++;
            if (PhotonNetwork.OfflineMode) { Destroy(gameObject); PlayerKills++; }
            else photonView.RPC("DestroyEnemyByID", RpcTarget.AllBuffered, photonView.ViewID, false);
            
        }
        IFrameHandling(iframed);
        GroundUpdate();
        FlyingUpdate();
        //Debug.Log(isAttacking);
        jumptimer -= Time.deltaTime;
        //Debug.Log(PlayerKills);
    }

    private void FixedUpdate()
    {
        if (path == null) return;

        if(currentWayPoint >= path.vectorPath.Count)
        {
            if (pathIsEnded) return;
            pathIsEnded = true;
            gndtimer = 15f;
            rb.velocity= Vector2.zero;
            return;
        }
        pathIsEnded = false;

        //Direction to next node
        Vector3 dir = (path.vectorPath[currentWayPoint] - transform.position).normalized;
        dir *= speed * Time.fixedDeltaTime;
        GroundFixedUpdates(dir);
        FlyingFixedUpdates(dir);



        float dist = Vector3.Distance(transform.position, path.vectorPath[currentWayPoint]);
        if(dist < nextWaypointDistance)
        {
            currentWayPoint++;
            return;
                
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Shield")
        {
            if (isAttacking)
            {
                //Knockback
                if (collision.transform.position.x < transform.position.x)
                {
                    StartCoroutine("Knockback");
                    rb.AddForce(new Vector2(10000 * Time.fixedDeltaTime, 0));
                }
                else { StartCoroutine("Knockback"); rb.AddForce(new Vector2(-10000 * Time.fixedDeltaTime, 0)); }
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                foreach (GameObject p in players)
                {
                    Debug.Log(p);
                    if (p.GetComponent<PlayerController>().isMine())
                    {
                        PlayerController player = p.GetComponent<PlayerController>();
                        player.shield.DamageShield(damage);
                        break;
                    }
                }
                AudioManager.Play("Shield");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.tag == "Player")
        {
            //Debug.Log(isAttacking);
            //Debug.Log(collision.GetComponent<PhotonView>().ViewID);
            if (!collision.GetComponent<PlayerController>().isJumpDashing && EnemyType == 0 && isAttacking)
            {
                //Debug.Log("xd");
                AudioManager.Play("MeleeHit");
                collision.GetComponent<PlayerController>().DamagePlayer(damage);
                collision.GetComponent<PlayerController>().Knockback(10000, -transform.localScale.x / Mathf.Abs(transform.localScale.x));
            }
            else if (collision.GetComponent<PlayerController>().isJumpDashing)
            {
                Health -= collision.GetComponent<PlayerController>().damage;
                FindObjectOfType<TextOverlay>().Overlay(transform, "-" + collision.GetComponent<PlayerController>().damage, false);
                Vector3 a = transform.position;
                a.z = -0.4f;
                GameObject parti = Instantiate(fromPlayer, a, Quaternion.identity);
                collision.GetComponent<PlayerController>().health += 10;
            }
            else if (!collision.GetComponent<PlayerController>().isJumpDashing && EnemyType == 0 && !collision.GetComponent<PlayerController>().iframed && !isAttacking)
            {
                //Debug.Log("xdd");
                collision.GetComponent<PlayerController>().Knockback(5000, -transform.localScale.x / Mathf.Abs(transform.localScale.x));
            }


            
            //AudioManager.Play("Melee");

        }

        if (collision.tag == "Ghosts")
        {
            //GameObject pl = collision.gameObject.transform.parent.gameObject.transform.parent.gameObject;
            //Debug.Log(pl.GetComponent<PhotonView>().ViewID);
            PlayerController PC = null;
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject p in players)
            {
                if (p.GetComponent<PlayerController>().isMine())
                {
                    PC = p.GetComponent<PlayerController>();
                }
            }
            Health -= PC.damage;
            if (Health > 0)
            {
                photonView.RPC("SetHealth", RpcTarget.OthersBuffered, photonView.ViewID, PC.damage);
            }
            FindObjectOfType<TextOverlay>().Overlay(transform, "-" + PC.damage, false);
            Vector3 a = transform.position;
            a.z = -0.4f;
            GameObject parti = Instantiate(fromPlayer, a, Quaternion.identity);
            parti.transform.parent = this.transform;
        }

        //if (collision.tag == "Bullet")
        //{
        //    Vector3 a = transform.position;
        //    a.z = -0.4f;
            
        //    if (!iframed)
        //    {
        //        GameObject pl = collision.gameObject.transform.parent.gameObject.transform.parent.gameObject;
        //        //Debug.Log(pl.GetComponent<PhotonView>().ViewID);
        //        PlayerController PC = pl.GetComponent<PlayerController>();
        //        iframed = true;
        //        GameObject parti = Instantiate(fromPlayer, a, Quaternion.identity);
        //        parti.transform.parent = this.transform;
        //        Health -= PC.damage;
        //        FindObjectOfType<TextOverlay>().Overlay(transform, "-" + PC.damage, false);
        //    }
            
        //}

        if (collision.tag == "Sword")
        {
            //Take dmg
            if (!iframed)
            {
                GameObject pl = collision.gameObject.transform.parent.gameObject.transform.parent.gameObject;
                //Debug.Log(pl.GetComponent<PhotonView>().ViewID);
                PlayerController PC = pl.GetComponent<PlayerController>();
                Health -= PC.damage;
                if(Health > 0)
                {
                    photonView.RPC("SetHealth", RpcTarget.OthersBuffered, photonView.ViewID, PC.damage);
                }
                FindObjectOfType<TextOverlay>().Overlay(transform, "-" + PC.damage, false);
                iframed = true;
                Vector3 a = transform.position;
                a.z = -0.4f;
                GameObject parti = Instantiate(fromPlayer, a, Quaternion.identity);
            }

            //Knockback
            if (collision.transform.position.x < transform.position.x)
            {
                StartCoroutine("Knockback");
                rb.AddForce(new Vector2(3000 * Time.fixedDeltaTime, 0));
            }
            else { StartCoroutine("Knockback"); rb.AddForce(new Vector2(-3000 * Time.fixedDeltaTime, 0)); }

            //Player lifesteal
            if (lifesteal)
            {
                GameObject pl = collision.gameObject.transform.parent.gameObject.transform.parent.gameObject;
                //Debug.Log(pl.GetComponent<PhotonView>().ViewID);
                PlayerController PC = pl.GetComponent<PlayerController>();
                bool rnd = (Random.Range(1, 100) / 100f) <= 1.0f ? true : false;
                if (rnd)
                {
                    PC.health += PC.lifeSteal * PC.damage;
                    //Debug.Log(PC.lifeSteal + " " + PC.damage);
                    FindObjectOfType<TextOverlay>().Overlay(pl.transform, "+" + Mathf.Ceil(PC.lifeSteal * PC.damage), true, new Color(0, 0.6509434f, 0.1872309f));
                }
            }
            AudioManager.Play("EnemyHit");
        }
    }
    
    [PunRPC]
    public void DestroyEnemyByID(int enemyID, bool transformation)
    {
        if (!transformation) PlayerKills++;

        Destroy(PhotonView.Find(enemyID).gameObject);
    }

    private bool IsGrounded()
    {
        Vector2 position = transform.position;
        Vector2 direction = Vector2.down;
        float distance = 0.5f;
        Debug.DrawRay(position, direction, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundLayer);
        if (hit.collider != null)
        {
            return true;
        }

        return false;
    }

    private bool ObstacleTrigger()
    {
        Vector2 position = transform.position;
        Vector2 direction = transform.localScale.x > 0 ? Vector2.left : Vector2.right;
        float distance = 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundLayer);
        if (hit.collider != null)
        {
            return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if(EnemyType == 0)
            Gizmos.DrawRay(transform.position, transform.localScale.x > 0 ? Vector2.left : Vector2.right);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
        for (int i = editorChildCount; i < transform.childCount; i++)
        {
            theScale = transform.GetChild(i).localScale;
            theScale.x *= -1;
            transform.GetChild(i).localScale = theScale;
        }
    }

    private void FlipY()
    {
        facingPlayer = !facingPlayer;
        Vector3 theScale = transform.localScale;
        theScale.y *= -1;
        transform.localScale = theScale;
        for (int i = editorChildCount; i < transform.childCount; i++)
        {
            theScale = transform.GetChild(i).localScale;
            theScale.y *= -1;
            transform.GetChild(i).localScale = theScale;
        }
    }

    public void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWayPoint = 0;
        }
    }

    private void GroundUpdate()
    {
        //Transforming to flying
        gndtimer -= Time.deltaTime;
        if (gndtimer <= 0 && EnemyType == 0)
        {
            Transform sp = transform;
            sp.position = new Vector3(sp.position.x, sp.position.y, 0);
            PhotonNetwork.Instantiate(transition.name, transform.position, Quaternion.identity);
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            Transform pl = null;
            float currDist = 999999f;
            foreach (GameObject p in players)
            {

                if (Vector2.Distance(p.transform.position, sp.position) < currDist)
                {
                    currDist = Vector2.Distance(p.transform.position, sp.position);
                    pl = p.GetComponent<Transform>();
                }
            }
            PhotonNetwork.Instantiate(fly.name, sp.position, sp.rotation).GetComponent<EnemyAI>().target = pl;
            if (PhotonNetwork.OfflineMode) Destroy(gameObject);
            else photonView.RPC("DestroyEnemyByID", RpcTarget.AllBuffered, photonView.ViewID, true);
        }
    }

    private void GroundFixedUpdates(Vector3 moveDir)
    {
        //Rush player
        if(EnemyType == 0 && !ObstacleTrigger() && Vector2.Distance(transform.position, target.position) <= 3.5f && canAttack && IsGrounded())
        {           
            StartCoroutine(BlockMovement(1f));
            StartCoroutine(Attack(moveDir));           
        }

        //Jump on obstacles infront
        if (ObstacleTrigger() && EnemyType == 0 && jumptimer <= 0)
        {
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * Random.Range(jumpForce, jumpForce * 1.5f) * Time.fixedDeltaTime);
            jumptimer = 0.5f;
        }

        //Movement
        if (EnemyType == 0 && !isAttacking && canMove)
        {
            moveDir = new Vector3(moveDir.x, rb.velocity.y, moveDir.z);
            if (!knockback) rb.velocity = moveDir;          
        }

        //Sprites
        if(EnemyType == 0)
        {
            if (rb.velocity.x > 0 && !facingRight)
            {
                Flip();
            }
            else if (rb.velocity.x < 0 && facingRight)
            {
                Flip();
            }
        }

        //Jump Handling & Gravity
        if (transform.position.y + jumpoffset < target.position.y && IsGrounded() && EnemyType == 0 && jumptimer <= 0)
        {
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * Random.Range(jumpForce, jumpForce * 1.5f) * Time.fixedDeltaTime);
            jumptimer = 0.5f;
        }
        if (GetComponent<Rigidbody2D>().velocity.y < 0 && EnemyType == 0)
        {
            StartCoroutine(ResetGravity());
        }

    }

    private void FlyingUpdate()
    {
        //Debug.Log(transform.rotation.z);
        //Shooting
        hitTimer -= Time.deltaTime;
        if (hitTimer <= 0 && EnemyType == 1 && Vector2.Distance(transform.position, target.position) > 1f
            && Vector2.Distance(transform.position, target.position) < 3f && canShoot)
        {
            StartCoroutine(BlockMovement(0.7f));
            StartCoroutine(CastAttack(1f));
            hitTimer = 2f;
        }
    }

    private void FlyingFixedUpdates(Vector3 moveDir)
    {
        if (!canMove) return;
        //Sprites
        if (EnemyType == 1)
        {
            //Debug.Log(transform.rotation.z);
            //Debug.Log(UnityEditor.TransformUtils.GetInspectorRotation(gameObject.transform).z);
            float rotZ = UnityEditor.TransformUtils.GetInspectorRotation(gameObject.transform).z;
            //float rotZ = 55;
            transform.right = target.position - transform.position;
            if(transform.position.y > target.position.y)
            {
                
                if(Mathf.Abs(rotZ) > 90f)
                {
                    Vector3 theScale = transform.localScale;
                    theScale.y = -2.5f;
                    transform.localScale = theScale;
                }
                else if(Mathf.Abs(rotZ) < 90f)
                {
                    Vector3 theScale = transform.localScale;
                    theScale.y = 2.5f;
                    transform.localScale = theScale;
                }
            }
            else if (transform.position.y < target.position.y)
            {
                //Debug.Log(transform.right);
                if (Mathf.Abs(rotZ) < 90f)
                {
                    Vector3 theScale = transform.localScale;
                    theScale.y = 2.5f;
                    transform.localScale = theScale;
                }
                else if (Mathf.Abs(rotZ) > 90f)
                {
                    Vector3 theScale = transform.localScale;
                    theScale.y = -2.5f;
                    transform.localScale = theScale;
                }
            }
            //if (rb.velocity.x > 0 && !facingRight)
            //{
            //    Flip();
            //}
            //else if (rb.velocity.x < 0 && facingRight)
            //{
            //    Flip();
            //}
        }
        //Movement
        if (EnemyType == 1 && Vector2.Distance(transform.position, target.position) > 3f) rb.velocity = moveDir; //rb.AddForce(moveDir, fMode);
    }

    private void IFrameHandling(bool iframe)
    {
        if (!iframe) return;

        iFrame -= Time.deltaTime;
        if (iFrame <= 0)
        {
            iFrame = 0.3f;
            iframed = false;
        }
        
    }

    IEnumerator UpdatePath()
    {
        seeker.StartPath(transform.position, target.position, OnPathComplete);
        yield return new WaitForSeconds(1f / updateRate);
        StartCoroutine(UpdatePath());
    }

    IEnumerator Knockback()
    {
        knockback = true;
        yield return new WaitForSeconds(0.2f);
        knockback = false;
    }

    IEnumerator ResetGravity()
    {
        GetComponent<Rigidbody2D>().gravityScale = 5;
        yield return new WaitForSeconds(0.1f);
        GetComponent<Rigidbody2D>().gravityScale = 2;
    }

    IEnumerator CastAttack(float time)
    {
        canShoot = false;
        yield return new WaitForSeconds(time);
        canShoot = true;
        Vector2 myPos = new Vector2(muzzle.position.x, muzzle.position.y);
        Vector2 direction = new Vector2(target.position.x, target.position.y) - myPos;
        direction.Normalize();
        GameObject projectile = PhotonNetwork.Instantiate(bullet.name, muzzle.transform.position, Quaternion.identity);
        direction = new Vector2(target.position.x, target.position.y) - myPos;
        direction.Normalize();
        projectile.GetComponent<Rigidbody2D>().velocity = muzzle.right * projectileSpeed;
        projectile.GetComponent<EnemyProjectile>().damage = damage;
    }

    IEnumerator BlockMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    IEnumerator Attack(Vector3 moveDir)
    {
        canAttack = false;
        rb.velocity = Vector2.zero;
        anim.speed = 0;
        yield return new WaitForSeconds(1f);
        AudioManager.Play("MeleeDash");
        Trail.GetComponent<TrailRenderer>().enabled = true;
        moveDir.x = moveDir.x == 0 ? (target.position.x < transform.position.x ? -1 : 1) : moveDir.x;
        rb.AddForce(new Vector2(moveDir.normalized.x * 400, 0));
        isAttacking = true;
        anim.speed = 1;
        yield return new WaitForSeconds(.25f);
        isAttacking = false;
        rb.velocity = Vector2.zero;
        Trail.GetComponent<TrailRenderer>().enabled = false;
        yield return new WaitForSeconds(1f);
        canAttack = true;       
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Health);
            stream.SendNext(damage);
            stream.SendNext(isAttacking);
        }
        else
        {
            this.Health = (float)stream.ReceiveNext();
            this.damage = (float)stream.ReceiveNext();
            this.isAttacking = (bool)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void SetHealth(int eViewID, float dmg)
    {
        PhotonView enemy = PhotonView.Find(eViewID);
        if (enemy == null) return;
        enemy.gameObject.GetComponent<EnemyAI>().Health -= dmg;
    }
}

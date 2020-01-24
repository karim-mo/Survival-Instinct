using UnityEngine;
using System.Collections;
using Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

using Photon.Pun;
using Photon.Realtime;


public class PlayerController : MonoBehaviourPun, IPunObservable
{
    #region vars
    [Header("Player vars")]
    public float speed = 7f;
    [SerializeField, GetSet("health")]
    protected float Health = 150f;
    public float health { get { return Health; } set { Health = value; } }
    public float damage = 10;
    public int StatPoints;
    public float lifeSteal = 0.3f;
    public float jumpForce = 15f;
    public float dashSpeed = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public int consecMelee = 10;

    [Header("Particles and misc.")]
    public int editorChildCount;
    public LayerMask groundLayer;
    public GameObject GhostAttack1_Prefab;
    public GameObject GhostPrefab;
    public RectTransform bar;
    public Shield shield;
    public Transform shieldspawn;
    public Animator transition;
    public GameObject playerName;
    public GameObject hpBar;

    [Header("Collision")]
    public float collisionRadius = 0.25f;
    public Vector2 bottomOffset;


    [Header("Sword Classes")]
    [SerializeField] protected GameObject sword;


    #region protected  
    protected Rigidbody2D rb;
    protected Animator anim;
    protected Ghosting ghosts;
    protected Color debugCollisionColor = Color.red;
    protected CinemachineFramingTransposer composer;

    protected bool SpeedPower = false;
    protected bool IFramePower = false;
    protected bool RangePower = false;
    protected bool canGroundDash = true;
    protected bool canJumpDash = true;
    protected bool jumpEnhance = true;
    protected bool facingRight = true;
    public    bool FacingRight { get { return facingRight; } }
    
    
    protected bool canZAttack = true;
    protected bool onGround;
    protected bool waterFrame = false;   
    protected bool dead = false;
    

    protected float move = 0f;
    protected float lasertimer = 10f;
    protected float regentimer = 10f;
    protected float spreadtimer = 10f;
    protected float waterTimer = 0.4f;
    protected float consecutiveMelee = 0;
    protected float jumptimer = 0f;
    #endregion


    [HideInInspector]
    public bool isJumpDashing = false;
    [HideInInspector]
    public bool DashAnim = false;
    [HideInInspector]
    public bool iframed = false;
    [HideInInspector]
    public int enemyKills = 0;
    [HideInInspector]
    public bool jump = false;
    [HideInInspector]
    public bool canMove = true;
    [HideInInspector]
    public bool isAttacking = false;
    [HideInInspector]
    public bool canAttack = true;
    [HideInInspector]
    public float maxHp;
    #endregion

    protected void Start()
    {
        Cursor.visible = false;
        if (GhostAttack1_Prefab == null || GhostPrefab == null)
        {
            Debug.LogError("No Ghost prefab attached ???");
            return;
        }
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        composer = GameObject.FindGameObjectWithTag("vcam").GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        shield = GetComponent<Shield>();
        
        if (PhotonNetwork.OfflineMode)
        {
            transition = GameObject.FindGameObjectWithTag("tran").GetComponent<Animator>();
        }
        UpdateStats();
    }

    protected void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) return;

        onGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, collisionRadius, groundLayer);
        Physics2D.IgnoreLayerCollision(8, 12);
        Physics2D.IgnoreLayerCollision(8, 13);

        if (!SpeedPower) speed = PlayerStats.Speed;

        if (health >= PlayerStats.maxHealth) health = PlayerStats.maxHealth; //should be edited later
            

        if (Input.GetKeyDown(KeyCode.Escape)) { if (Time.timeScale == 1) Time.timeScale = 0; else Time.timeScale = 1; }

        if (Input.GetMouseButtonDown(1) && !shield.shieldUP && shield.recharging != 2 && canMove && Application.platform != RuntimePlatform.Android)
        {
            shield.SpawnShield(shieldspawn.position, this.transform);
            Disable();
        }
        if (Input.GetMouseButtonUp(1) && shield.shieldUP && Application.platform != RuntimePlatform.Android)
        {
            shield.Despawn();
            Enable();
        }

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            Cursor.visible = !Cursor.visible;
        }

        jumptimer -= Time.deltaTime;

    }

    protected void FixedUpdate()
    {
        

    }

    protected void VelocityHandling(int upState = 0, int downState = 0, int idleState = 0)
    {
        if (rb.velocity.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (rb.velocity.x < 0 && facingRight)
        {
            Flip();
        }

        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) return;

        if (rb.velocity.y < 0 && jumpEnhance)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump") && jumpEnhance)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        if (dead) return;

        if (rb.velocity.y < -5 && !isAttacking && !DashAnim && !shield.shieldUP)
        {
            anim.SetInteger("state", downState);
        }
        else if (rb.velocity.y > 5 && !isAttacking && !DashAnim && !shield.shieldUP)
        {
            anim.SetInteger("state", upState);
        }

        if (onGround && shield.shieldUP) rb.velocity = Vector2.zero;
        
        //Debug.Log(isAttacking);
        if (rb.velocity.x == 0 && !isAttacking && !DashAnim) anim.SetInteger("state", idleState);
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, collisionRadius);
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Laser")
        {
            DamagePlayer(10);
            int rnd = Random.Range(1, 99999) % 2 == 0 ? 10000 : -10000;
            StartCoroutine(BlockMovement(.5f));
            rb.AddForce(new Vector2(rnd * Time.fixedDeltaTime, Mathf.Abs(rnd) * Time.fixedDeltaTime));
        }
        if(collision.tag == "MovPower")
        {
            if (SpeedPower) return;
            AudioManager.Play("Pickup");
            SpeedPower = true;
            speed += 100f;
            Destroy(collision.gameObject);
        }
        if(collision.tag == "ShieldPower")
        {
            if (IFramePower) return;
            AudioManager.Play("Pickup");
            StartCoroutine(IFrame(10));
            Destroy(collision.gameObject);
        }
        if(collision.tag == "RangePower")
        {
            if (RangePower) return;
            AudioManager.Play("Pickup");
            StartCoroutine("SwordPower");
            Destroy(collision.gameObject);
        }
    }

    protected void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Water")
        {
            if (!waterFrame)
            {
                waterFrame = true;
                health -= 10;
                StartCoroutine("WaterFrame");
            }
        }
    }

    public void Jump(Vector2 dir, bool jump2)
    {
        if (!canMove) return;
        if (dead) return;
        if (jump2) return;
        if (!onGround) return;
        jump = true;
        rb.velocity = new Vector2(rb.velocity.x, 0);
        dir *= Time.fixedDeltaTime;
        rb.velocity += dir * jumpForce;
    }

    public void Move(Vector2 dir, int animState)
    {
        if (!canMove) return;
        if (dead) return;
        if (rb.velocity.x != 0 && !isAttacking && !DashAnim) anim.SetInteger("state", animState);
        dir = new Vector2(dir.x * speed * Time.fixedDeltaTime, rb.velocity.y);
        rb.velocity = dir;
    }

    public void Dash(float x, float y, int animState)
    {
        if (!canMove) return;
        if (dead) return;
        if (!canGroundDash && onGround) return;
        if (!canJumpDash && !onGround) return;

        
        if (onGround) StartCoroutine(OnGroundDash());
        else
        {
            StartCoroutine(Dash(0.3f, animState));
            StartCoroutine(OnJumpDash());
            
        }
        AudioManager.Play("Dash");
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.1f, .2f, 14, 90, false, true);
        
        
        FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));
        FindObjectOfType<Ghosting>().StartGhosting(onGround ? 3 : 5);

        

        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);
        rb.AddForce(dir.normalized * dashSpeed * Time.fixedDeltaTime);
    }

    public void Knockback(int force, float dir)
    {
        if (!canMove) return;

        float rnd = force * dir;
        StartCoroutine(BlockMovement(.3f));
        rb.AddForce(new Vector2(rnd * Time.fixedDeltaTime, Mathf.Abs(rnd) * Time.fixedDeltaTime));
    }
        
    public void DamagePlayer(float dmg)
    {
        if (iframed)
        {
            AudioManager.Play("IFrame");
            return;
        }
        SpeedPower = false;
        health -= dmg;
        StartCoroutine(IFrame(1f));
    }

    public bool isMine()
    {
        return photonView.IsMine;
    }


    public void UpdateStats()
    {
        health = PlayerStats.maxHealth;
        Debug.Log(PlayerStats.maxHealth);
        shield.shieldAmt = PlayerStats.maxShield;
        shield.health = shield.shieldAmt;
        speed = PlayerStats.Speed;
        damage = PlayerStats.Damage;
        lifeSteal = PlayerStats.LSteal / 100f;
        StatPoints = PlayerPrefs.GetInt("PTS");
    }

    public void Disable()
    {
        rb.velocity = Vector2.zero;
        canMove = false;
        canAttack = false;
        canZAttack = false;
        canGroundDash = false;
        canJumpDash = false;
    }

    public void Enable()
    {
        canMove = true;
        canAttack = true;
        canZAttack = true;
        canGroundDash = true;
        canJumpDash = true;
    }

    protected bool IsGrounded()
    {
        Vector2 position = transform.position;
        Vector2 direction = Vector2.down;
        float distance = 0.4f;
        Debug.DrawRay(position, direction, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundLayer);
        if (hit.collider != null)
        {
            return true;
        }

        return false;
    }

    protected void Flip()
    {
        facingRight = !facingRight;
        Vector3 _scale = transform.localScale;
        _scale.x *= -1;
        transform.localScale = _scale;
        for(int i = editorChildCount; i < transform.childCount; i++)
        {
            for(int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                _scale = transform.GetChild(i).GetChild(j).localScale;
                _scale.x *= -1;
                transform.GetChild(i).GetChild(j).localScale = _scale;
            }
        }
    }
    
    protected IEnumerator Attacking(float time, int state)
    {
        isAttacking = true;
        anim.SetInteger("state", state);
        yield return new WaitForSeconds(time);
        isAttacking = false;
    }

    protected IEnumerator Attacking(float time, int state, bool hasSword)
    {
        if (hasSword)
        {
            StartCoroutine("Sword");
            
        }
        isAttacking = true;
        AudioManager.Play("Melee");
        anim.SetInteger("state", state);
        yield return new WaitForSeconds(time);
        isAttacking = false;
        if (hasSword)
        {
            sword.SetActive(false);
            EnemyAI.lifesteal = false;
        }

    }

    protected IEnumerator Sword()
    {
        yield return new WaitForSeconds(0.03f);
        sword.SetActive(true);
        EnemyAI.lifesteal = true;
    }

    protected IEnumerator WaterFrame()
    {
        yield return new WaitForSeconds(0.4f);
        waterFrame = false;
    }

    protected IEnumerator Dash(float time, int animState)
    {
        
        isAttacking = true;
        rb.gravityScale = 0;
        anim.SetInteger("state", animState);
        yield return new WaitForSeconds(time);
        isAttacking = false;
        rb.gravityScale = 5f;

    }

    protected IEnumerator Attack1CD(float cd)
    {
        canAttack = false;
        yield return new WaitForSeconds(cd);
        canAttack = true;
    }

    protected IEnumerator Attack2CD(float cd)
    {
        canZAttack = false;
        yield return new WaitForSeconds(cd);
        canZAttack = true;
    }

    protected IEnumerator OnGroundDash()
    {
        yield return new WaitForSeconds(.1f);
        canGroundDash = false;
        composer.m_XDamping = 3f;
        StartCoroutine(Damping());
        yield return new WaitForSeconds(1f);
        canGroundDash = true;
        //composer.m_XDamping = 0f;
    }

    protected IEnumerator OnJumpDash()
    {
        isJumpDashing = true;
        
        yield return new WaitForSeconds(.2f);
        //anim.SetInteger("state", 6);
        canJumpDash = false;
        isJumpDashing = false;
        
        composer.m_XDamping = 3f;
        StartCoroutine(Damping());
        yield return new WaitForSeconds(1f);
        //DashAnim = false;
        canJumpDash = true;
        
        

    }

    protected IEnumerator Damping()
    {
        
        for(float i = 3f; i >= 0; i-= 0.1f)
        {
            composer.m_XDamping = i;
            yield return new WaitForSeconds(0.03166f);
        }
    }

    protected IEnumerator BlockMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
    //p.GetComponent<SpriteRenderer>().color = new Color(0.5647059f, 0.5647059f, 0.5647059f);
    protected IEnumerator IFrame(float time)
    {
        if (photonView.IsMine)
        {
            iframed = true;
            for (int i = 0; i < 15; i++)
            {
                if (i % 2 == 0) gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 0.3254717f, 0.3254717f);
                else gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
                yield return new WaitForSeconds(time / 15);
            }
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
            yield return new WaitForSeconds(.1f);
            iframed = false;
        }
    }

    protected IEnumerator SwordPower()
    {
        //Long range
        BoxCollider2D collider = sword.GetComponent<BoxCollider2D>();
        BoxCollider2D temp = GameObject.FindGameObjectWithTag("AMAZINGLOGIC").GetComponent<BoxCollider2D>();
        collider.offset = new Vector2(0.9716845f, 0.0600639f);
        collider.size = new Vector2(2.329218f, 0.2732264f);
        temp.offset = new Vector2(0.06794643f, 0.0600639f);
        temp.size = new Vector2(0.5217419f, 0.2732264f);

        //Ghosting
        int CM = consecMelee;
        int count = FindObjectOfType<GhostingAttack>().count;
        FindObjectOfType<GhostingAttack>().count = 6;
        consecMelee = 2;

        yield return new WaitForSeconds(10f);
        //Reset
        collider.offset = temp.offset;
        collider.size = temp.size;

        //Ghosting Reset
        consecMelee = CM;
        FindObjectOfType<GhostingAttack>().count = count;
    }

    protected IEnumerator OnDeath(int animState)
    {
        canMove = false;
        canAttack = false;
        canZAttack = false;
        canGroundDash = false;
        canJumpDash = false;
        FindObjectOfType<ERROR>().DisplayError("You died!");
        GameObject[] Enemies;
        Enemies = GameObject.FindGameObjectsWithTag("Enemy");
        FindObjectOfType<RandomSpawner>().StopAllWaves();
        foreach (GameObject g in Enemies) Destroy(g);

        anim.SetInteger("state", animState);
        yield return new WaitForSeconds(.3f);
        PlayerPrefs.SetInt("scene", SceneManager.GetActiveScene().buildIndex);
        if(PhotonNetwork.OfflineMode) transition.SetTrigger("FadeOut");
        else
        {
            photonView.RPC("DisconnectAll", RpcTarget.All);
        }

    }

    [PunRPC]
    public void DisconnectAll()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.LoadLevel(0);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(PlayerStats.maxHealth);
            stream.SendNext(health);
        }
        else
        {
            this.maxHp = (float)stream.ReceiveNext();
            this.health = (float)stream.ReceiveNext();
        }

    }
}
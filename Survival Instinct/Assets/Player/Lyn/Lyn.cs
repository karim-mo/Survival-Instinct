using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;

public class Lyn : PlayerController
{  
    [Header("Gun & Powerups")]
    public Gun gun;

    [Header("JoyStick")]
    public Joystick joystick;
    [HideInInspector]
    public float horizontal;

    private new void Start()
    {
        FindObjectOfType<GhostingAttack>().Ghost = GhostAttack1_Prefab;
        FindObjectOfType<Ghosting>().Ghost = GhostPrefab;
        base.Start();       
    }

    private new void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            VelocityHandling();
            return;
        }
        base.Update();

        

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump(Vector2.up);
            //jump = false;
        }
        if (health <= 0 && !dead)
        {
            dead = true;
            StartCoroutine(OnDeath(9));
        }

        if (Input.GetMouseButton(0) && !isAttacking && canAttack && !DashAnim && Application.platform != RuntimePlatform.Android)
        {
            Attack();
        }

        if (Input.GetKey(KeyCode.Z) && !isAttacking && canZAttack && !DashAnim)
        {
            //canMove = false;
            StartCoroutine(Attack2CD(.5f));
            //FindObjectOfType<GhostingFire>().StartGhosting();
            StartCoroutine(Attacking(1f, 7, false));
        }

        VelocityHandling(3, 5, 0);
        //if (shield.shieldUP) anim.SetInteger("state", 10);
    }

    public void Attack()
    {
        consecutiveMelee++;
        StartCoroutine(Attack1CD(.1f));
        if (consecutiveMelee >= consecMelee) { FindObjectOfType<GhostingAttack>().StartGhosting(); AudioManager.Play("Ghosts"); consecutiveMelee = 0; }
        StartCoroutine(Attacking(0.250f, 2, true));
    }
    private new void FixedUpdate()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        if (Application.platform == RuntimePlatform.Android)
        {
            if(joystick.Horizontal >= .1f)
            {
                horizontal = 1;
                Move(new Vector2(horizontal, 0), 1);
            }
            else if(joystick.Horizontal <= -.1f)
            {
                horizontal = -1;
                Move(new Vector2(horizontal, 0), 1);
            }
            else
            {
                horizontal = 0;
                Move(new Vector2(horizontal, 0), 1);
            }
        }
        else
        {
            //Movement
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            Move(new Vector2(x, y), 1);
            

            float xR = Input.GetAxisRaw("Horizontal");
            if (Input.GetKey(KeyCode.LeftShift)) if (xR != 0) Dash(xR, 0, 6);
        }
    
    }

    public void Shoot()
    {
        gun.Shoot(380 * Time.fixedDeltaTime, 1);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class AndroidUI : MonoBehaviourPun
{
    private PlayerController player;
    private Lyn lyn;

    bool mouseHold;
    bool attack;

    private void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            if (p.GetComponent<PlayerController>().isMine())
            {
                player = p.GetComponent<PlayerController>();
                lyn = p.GetComponent<Lyn>();
                break;
            }
        }
    }

    private void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        if (mouseHold) JumpBtn();
        if (attack) attackBtn();
    }

    public void onJumpTouchDown()
    {
        mouseHold = true;
    }
    public void OnJumpTouchUp()
    {
        mouseHold = false;
    }

    public void onAttackTouchDown()
    {
        attack = true;
    }
    public void OnAttackTouchUp()
    {
        attack = false;
    }

    public void attackBtn()
    {
        if (!lyn.isAttacking && lyn.canAttack && !lyn.DashAnim) lyn.Attack();
    }

    public void DashBtn()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        float horizontal = lyn.horizontal;
        if (horizontal == 0) return;
        lyn.Dash(horizontal, 0, 6);
    }

    public void JumpBtn()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        lyn.Jump(Vector2.up, lyn.jump);
        lyn.jump = false;
    }

    public void ShieldBtn()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        if (!lyn.shield.shieldUP && lyn.shield.recharging != 2 && lyn.canMove)
        {
            lyn.shield.SpawnShield(lyn.shieldspawn.position, lyn.gameObject.transform);
            lyn.Disable();
        }
        else if (lyn.shield.shieldUP)
        {
            lyn.shield.Despawn();
            lyn.Enable();
        }
    }
}

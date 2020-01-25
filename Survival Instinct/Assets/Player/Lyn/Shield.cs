using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Shield : MonoBehaviourPun
{

    public static class States
    {
        public const int NORECHARGE = 0;
        public const int RECHARGE = 1;
        public const int MAXRECHARGE = 2;
    }


    [Header("Shield Stats")]
    public GameObject prefab;
    public GameObject ChargingBar;
    public float shieldAmt;
    public float shieldDuration;
    public float RechargeTime; //In seconds only!!
    public float maxRechargeTime;


    //[HideInInspector]
    public float health;
    [HideInInspector]
    public int recharging = 0;
    [HideInInspector]
    public bool shieldUP = false;


    private GameObject sh;
    private PlayerController player;

    private void Start()
    {
        ChargingBar.SetActive(false);
        shieldAmt = PlayerStats.maxShield;
        health = shieldAmt;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            if (p.GetComponent<PlayerController>().isMine())
            {
                player = p.GetComponent<PlayerController>();
                break;
            }
        }

    }

    private void Update()
    {       
        if(health <= 0)
        {
            //Debug.Log("haha");
            health = 0;
            shieldUP = false;
            AudioManager.Play("ShieldBreak");
            //PhotonNetwork.Destroy(sh);
            Destroy(sh);
            player.Enable();
            recharging = States.MAXRECHARGE;
            StartCoroutine("Recharge");
            
            
        }
        ChargingBar.transform.GetChild(0).GetComponent<Image>().fillAmount = health / shieldAmt;
        if (ChargingBar.transform.GetChild(0).GetComponent<Image>().fillAmount >= 0.9999993f) ChargingBar.SetActive(false);
        else if (ChargingBar.transform.GetChild(0).GetComponent<Image>().fillAmount < 1.0f) ChargingBar.SetActive(true);
    }

    public void SpawnShield(Vector3 sp, Transform player)
    {
        if (recharging == States.MAXRECHARGE) return;
        if (shieldUP) return;

        StopAllCoroutines();

        int pID = -1;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p.GetComponent<PlayerController>().isMine())
            {
                pID = p.GetComponent<PhotonView>().ViewID;
                break;
            }
        }
        sh = PhotonNetwork.Instantiate(prefab.name, sp, prefab.transform.rotation);
        sh.transform.SetParent(player);
        sh.transform.localScale = 
            new Vector3(sh.transform.localScale.x, player.localScale.x > 0 ? sh.transform.localScale.y : -sh.transform.localScale.y, sh.transform.localScale.z);
        //Debug.Log(pID + " " + sh.GetComponent<PhotonView>().ViewID);
        photonView.RPC("SetShieldParent", RpcTarget.Others, pID, sh.GetComponent<PhotonView>().ViewID);
        shieldUP = true;
    }

    public void Despawn()
    {
        if (recharging == States.MAXRECHARGE) return;
        shieldUP = false;
        recharging = States.RECHARGE;
        PhotonNetwork.Destroy(sh);
        if (ChargingBar.transform.GetChild(0).GetComponent<Image>().fillAmount >= 1.0f) return;
        StartCoroutine("Recharge");
    }

    public void DamageShield(float amt)
    {
        if (!shieldUP) return;

        health -= amt;
    }

    IEnumerator Recharge()
    {
        
        ChargingBar.SetActive(true);

        float time = recharging == States.MAXRECHARGE ? maxRechargeTime : RechargeTime;
        if (recharging == States.MAXRECHARGE) ChargingBar.transform.GetChild(0).GetComponent<Image>().color = new Color(1, 0.2520669f, 0);
        else ChargingBar.transform.GetChild(0).GetComponent<Image>().color = new Color(0, 0.5254902f, 1);
        for (float i = (health/shieldAmt); health <= shieldAmt; i+= 0.01f)
        {
            health = i * shieldAmt;
            ChargingBar.transform.GetChild(0).GetComponent<Image>().fillAmount = i;
            yield return new WaitForSeconds(time / 100);
        }
        health = shieldAmt;
        ChargingBar.transform.GetChild(0).GetComponent<Image>().color = new Color(0, 0.5254902f, 1);
        recharging = States.NORECHARGE;
    }

    [PunRPC]
    public void SetShieldParent(int pViewID, int cViewID)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            if (p.GetComponent<PhotonView>().ViewID == pViewID)
            {
                //Debug.Log("Found player");
                GameObject[] shields = GameObject.FindGameObjectsWithTag("Shield");

                foreach (GameObject s in shields)
                {
                    //Debug.Log(s.GetComponent<PhotonView>().ViewID + " " + cViewID);
                    if (s.GetComponent<PhotonView>().ViewID == cViewID)
                    {
                        //Debug.Log("Found shield");
                        s.transform.SetParent(p.transform);
                        break;
                    }
                }
                break;
            }
        }     
    }
}


/**
 * get view id of player and shield
 * rpc goes through player list and compares view id and tag
 * set shield to parent
 */
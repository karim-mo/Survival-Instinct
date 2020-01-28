using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class Lilith : MonoBehaviourPun, IPunObservable
{
    [Header("Explosion")]
    public GameObject explo;
    public Transform exploPos;

    [Header("Shared Mechanics Objects")]
    public Transform middlePos;
    public Transform leftPos;
    public Transform rightPos;
    public GameObject homingMeteor;
    public GameObject meteor;

    [Header("Meteor spawns")]
    public Transform[] bossMeteorSpawns;
    public Transform[] topMeteorSpawns;
    public Transform[] leftMeteorSpawns;
    public Transform[] rightMeteorSpawns;
      

    [Header("Stats")]
    public float health;
    public float exploDamage;
    public float meteorDamage;
    public float homingDamage;

    [Header("Particles")]
    public GameObject onDeath;
    public GameObject fromPlayer;

    [Header("Firing & Powerups")]
    public GameObject bullet;
    public float fireRate = 10000f;
    public float projectileSpeed = 1f;
    public float projectileCount = 20f;
    public float spreadAngle = 8.0f;


    [Header("Boss HP Bar")]
    public GameObject bossBar;
    public GameObject bossName;
    public GameObject bossHp;
    public GameObject bossPerc;


    Transform player;

    SpriteRenderer sr;
    PolygonCollider2D pc;
    Animator anim;

    [HideInInspector]
    public float maxHealth = 0;

    bool canTP = false;
    bool canMove = false;
    bool canAttack = false;
    bool ready = false;

    bool mech90 = false;
    bool mech60 = false;
    bool mech30 = false;
    bool mech10 = false;
    bool mech10B = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        pc = GetComponent<PolygonCollider2D>();
        anim = GetComponent<Animator>();
        canTP = true;
        canMove = true;

        middlePos = GameObject.FindGameObjectWithTag("middlePos").gameObject.transform;
        leftPos = GameObject.FindGameObjectWithTag("leftPos").gameObject.transform;
        rightPos = GameObject.FindGameObjectWithTag("rightPos").gameObject.transform;

        bossBar = GameObject.FindGameObjectWithTag("bossBar").transform.GetChild(0).gameObject;
        bossBar.SetActive(true);
        bossHp = GameObject.FindGameObjectWithTag("bossHp");
        bossPerc = GameObject.FindGameObjectWithTag("bossPerc");
        bossName = GameObject.FindGameObjectWithTag("bossName");
        bossName.GetComponent<TextMeshProUGUI>().text = "Lilith";

        GameObject[] _leftMeteorSpawns = GameObject.FindGameObjectsWithTag("leftMeteorSpawns");
        Transform[] _leftMeteorPOS = new Transform[_leftMeteorSpawns.Length];
        int i = 0;
        foreach(GameObject p in _leftMeteorSpawns)
        {
            _leftMeteorPOS[i++] = p.transform;
        }
        leftMeteorSpawns = _leftMeteorPOS;

        GameObject[] _rightMeteorSpawns = GameObject.FindGameObjectsWithTag("rightMeteorSpawns");
        Transform[] _rightMeteorPOS = new Transform[_rightMeteorSpawns.Length];
        i = 0;
        foreach (GameObject p in _rightMeteorSpawns)
        {
            _rightMeteorPOS[i++] = p.transform;
        }
        rightMeteorSpawns = _rightMeteorPOS;

        GameObject[] _topMeteorSpawns = GameObject.FindGameObjectsWithTag("topMeteorSpawns");
        Transform[] _topMeteorPOS = new Transform[_topMeteorSpawns.Length];
        i = 0;
        foreach (GameObject p in _topMeteorSpawns)
        {
            _topMeteorPOS[i++] = p.transform;
        }
        topMeteorSpawns = _topMeteorPOS;
        maxHealth = health;

        StartCoroutine("BossPattern");
    }

    private void Update()
    {
        if (health <= 0)
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
            //if(PhotonNetwork.OfflineMode) FindObjectOfType<RandomSpawner>().bossBar.SetActive(false);
            bossBar.SetActive(false);
        }
        float hpPercent = health / maxHealth;
        bossHp.GetComponent<Image>().fillAmount = hpPercent;
        bossPerc.GetComponent<TextMeshProUGUI>().text = "" + Mathf.Ceil(hpPercent * 100) + "%";
        if (hpPercent >= 0.65f && hpPercent <= 0.9f && !mech90)
        {
            mech90 = true;
            Debug.Log("90% Phase");
            StopAllCoroutines();
            StartCoroutine(readyForMech(1, 90));
        }

        if (hpPercent >= 0.35f && hpPercent <= 0.6f && !mech60)
        {
            mech60 = true;
            Debug.Log("60% Phase");
            StopAllCoroutines();
            StartCoroutine(readyForMech(2, 60));
        }

        if (hpPercent >= 0.15f && hpPercent <= 0.3f && !mech30)
        {
            mech30 = true;
            Debug.Log("30% Phase");
            StopAllCoroutines();
            StartCoroutine(readyForMech(0, 30));
        }

        if (hpPercent >= 0f && hpPercent <= 0.1f && !mech10)
        {
            mech10 = true;
            Debug.Log("10% Phase");
            StopAllCoroutines();
            StartCoroutine(readyForMech(1, 10));
        }


        if (canTP)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            player = null;
            foreach (GameObject p in players)
            {
                float dist = Vector2.Distance(p.transform.position, transform.position);
                if (dist > 10)
                {
                    canTP = false;
                    player = p.transform;
                    StartCoroutine(Teleport(player));
                    break;
                }
            }
        }


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
                    break;
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(exploDamage);
            stream.SendNext(meteorDamage);
            stream.SendNext(homingDamage);
        }
        else
        {
            this.exploDamage = (float)stream.ReceiveNext();
            this.meteorDamage = (float)stream.ReceiveNext();
            this.homingDamage = (float)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void DestroyEnemyByID(int enemyID)
    {
        EnemyAI.PlayerKills++;
        Destroy(PhotonView.Find(enemyID).gameObject);
    }

    [PunRPC]
    public void SetBossHealth(int eViewID, float dmg)
    {
        PhotonView enemy = PhotonView.Find(eViewID);
        if (enemy == null) return;
        enemy.gameObject.GetComponent<Lilith>().health -= dmg;
    }

    [PunRPC]
    public void SendBullet(int bViewID, Vector2 dir)
    {
        PhotonView.Find(bViewID).gameObject.GetComponent<Rigidbody2D>().AddForce(dir * 0.02f);
    }

    [PunRPC]
    public void DisplayError(string error, float time)
    {
        FindObjectOfType<ERROR>().DisplayError(error, time);
    }



    public Transform findFurthest()
    {
        Transform _player = null;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float maxDist = -999999;
        foreach(GameObject p in players)
        {
            float dist = Vector2.Distance(this.transform.position, p.transform.position);
            if (dist > maxDist)
            {
                maxDist = dist;
                _player = p.transform;
            }
        }
        return _player;
    }


    IEnumerator BossPattern()
    {
        yield return new WaitForSeconds(5);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] meteors = new GameObject[4];
        for(int i = 0; i < bossMeteorSpawns.Length; i++)
        {
            GameObject _meteor = PhotonNetwork.Instantiate(homingMeteor.name, bossMeteorSpawns[i].position, Quaternion.identity);
            _meteor.GetComponent<Meteors>().damage = homingDamage;
            meteors[i] = _meteor;
        }

        yield return new WaitForSeconds(1);

        int k = 3;
        int j = 0;
        while (k >= 0)
        {
            meteors[k].GetComponent<HomingMissile>().target = players[j % players.Length].transform;
            j++;
            k--;
        }

        StartCoroutine(BossPattern());
    }

    IEnumerator readyForMech(int state, int mech)
    {
        canTP = false;
        canAttack = false;
        StopCoroutine("Teleport");
        StopCoroutine("BossPattern");
        if (mech == 10)
        {
            if (PhotonNetwork.OfflineMode) FindObjectOfType<ERROR>().DisplayError("Lilith  has  failed  to  capture  her  sacrifice  and  is  enraged", 3.5f);
            else photonView.RPC("DisplayError", RpcTarget.All, "Lilith  has  failed  to  capture  her  sacrifice  and  is  enraged", 3.5f);
            yield return new WaitForSeconds(3f);
        }
        else
        {
            if (PhotonNetwork.OfflineMode) FindObjectOfType<ERROR>().DisplayError("Lilith  calls  for  help  from  above", 3);
            else photonView.RPC("DisplayError", RpcTarget.All, "Lilith  calls  for  help  from  above", 3f);
        }
        if(!mech10) yield return new WaitForSeconds(2);

        
        pc.enabled = false;

        for (float i = 1.0f; i >= 0; i -= 0.1f)
        {
            Color _alpha = sr.color;
            _alpha.a = i;
            sr.color = _alpha;
            yield return new WaitForSeconds(0.03f);
        }

        Color alpha = sr.color;
        alpha.a = 0f;
        sr.color = alpha;

        switch (state)
        {
            case 0:
                transform.position = middlePos.position;
                break;
            case 1:
                transform.position = leftPos.position;
                break;
            case 2:
                transform.position = rightPos.position;
                break;
        }


        for (float i = 0; i <= 1.0; i += 0.1f)
        {
            Color _alpha = sr.color;
            _alpha.a = i;
            sr.color = _alpha;
            yield return new WaitForSeconds(0.02f);
        }

        alpha = sr.color;
        alpha.a = 1.0f;
        sr.color = alpha;
        
        anim.SetInteger("state", 1);
        //boss health bar communication goes here
        ready = false;

        StartCoroutine("_mech" + mech);
        while (!ready) yield return null;
        anim.SetInteger("state", 0);
        if (!mech10)
        {
            canTP = true;
            canAttack = true;
            StartCoroutine("BossPattern");
        }
        pc.enabled = true;
    }

    IEnumerator _mech90()
    {
        yield return new WaitForSeconds(3);

        int frequency = 10;
        while(frequency > 0)
        {
            for(int i = 0; i < leftMeteorSpawns.Length; i++)
            {
                StartCoroutine(spawn(Random.Range(0.2f, 0.8f), leftMeteorSpawns[i]));
            }
            frequency--;
            yield return new WaitForSeconds(0.5f);
        }

        if (!mech10)
        {
            if (PhotonNetwork.OfflineMode) FindObjectOfType<ERROR>().DisplayError("Lilith  is  locating  her  demon  sacrifice", 3.5f);
            else photonView.RPC("DisplayError", RpcTarget.All, "Lilith  is  locating  her  demon  sacrifice", 3.5f);
            yield return new WaitForSeconds(1.5f);
            Transform furth = findFurthest();
            furth.gameObject.GetComponent<PlayerController>().toggleMark();
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(TeleportImm(furth, true));
        }
        ready = true;
    }

    IEnumerator _mech60()
    {
        yield return new WaitForSeconds(3);

        int frequency = 10;
        while (frequency > 0)
        {
            for (int i = 0; i < rightMeteorSpawns.Length; i++)
            {
                StartCoroutine(spawn(Random.Range(0.2f, 0.8f), rightMeteorSpawns[i]));
            }
            frequency--;
            yield return new WaitForSeconds(0.5f);
        }

        if (!mech10)
        {
            if(PhotonNetwork.OfflineMode) FindObjectOfType<ERROR>().DisplayError("Lilith  is  locating  her  demon  sacrifice", 3.5f);
            else photonView.RPC("DisplayError", RpcTarget.All, "Lilith  is  locating  her  demon  sacrifice", 3.5f);
            yield return new WaitForSeconds(1.5f);
            Transform furth = findFurthest();
            furth.gameObject.GetComponent<PlayerController>().toggleMark();
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(TeleportImm(furth, true));
        }
        ready = true;
    }

    IEnumerator _mech30()
    {
        yield return new WaitForSeconds(3);

        int frequency = 10;
        while (frequency > 0)
        {
            for (int i = 0; i < topMeteorSpawns.Length; i++)
            {
                StartCoroutine(spawn(Random.Range(0.2f, 0.8f), topMeteorSpawns[i]));
            }
            frequency--;
            yield return new WaitForSeconds(0.5f);
        }

        //yield return new WaitForSeconds(2f);
        if (!mech10)
        {
            if(PhotonNetwork.OfflineMode) FindObjectOfType<ERROR>().DisplayError("Lilith  is  locating  her  demon  sacrifice", 3.5f);
            else photonView.RPC("DisplayError", RpcTarget.All, "Lilith  is  locating  her  demon  sacrifice", 3.5f);
            yield return new WaitForSeconds(1.5f);
            Transform furth = findFurthest();
            furth.gameObject.GetComponent<PlayerController>().toggleMark();
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(TeleportImm(furth, true));
        }
        ready = true;
        if (mech10) mech10B = true;
    }

    IEnumerator _mech10()
    {
        StopCoroutine("readyForMech");
        ready = false;
        
        //Left
        StartCoroutine(readyForMech(1, 90));
        while (!ready) yield return null;
        if(PhotonNetwork.OfflineMode) FindObjectOfType<ERROR>().DisplayError("Lilith  is  locating  her  demon  sacrifice", 3.5f);
        else photonView.RPC("DisplayError", RpcTarget.All, "Lilith  is  locating  her  demon  sacrifice", 3.5f);
        yield return new WaitForSeconds(1.5f);
        ready = false;
        Transform furthestPlayer = findFurthest();
        furthestPlayer.gameObject.GetComponent<PlayerController>().toggleMark();
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(TeleportImm(furthestPlayer, true));
        yield return new WaitForSeconds(0.6f);
        
        //Right
        StartCoroutine(readyForMech(2, 60));
        while (!ready) yield return null;
        if(PhotonNetwork.OfflineMode) FindObjectOfType<ERROR>().DisplayError("Lilith  is  locating  her  demon  sacrifice", 3.5f);
        else photonView.RPC("DisplayError", RpcTarget.All, "Lilith  is  locating  her  demon  sacrifice", 3.5f);
        yield return new WaitForSeconds(1.5f);
        ready = false;
        furthestPlayer = findFurthest();
        furthestPlayer.gameObject.GetComponent<PlayerController>().toggleMark();
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(TeleportImm(furthestPlayer, true));
        yield return new WaitForSeconds(0.6f);


        //Middle
        StartCoroutine(readyForMech(0, 30));
        while (!ready) yield return null;
        if(PhotonNetwork.OfflineMode) FindObjectOfType<ERROR>().DisplayError("Lilith  is  locating  her  demon  sacrifice", 3.5f);
        else photonView.RPC("DisplayError", RpcTarget.All, "Lilith  is  locating  her  demon  sacrifice", 3.5f);
        yield return new WaitForSeconds(1.5f);
        ready = false;
        furthestPlayer = findFurthest();
        furthestPlayer.gameObject.GetComponent<PlayerController>().toggleMark();
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(TeleportImm(furthestPlayer, true));
        yield return new WaitForSeconds(0.6f);

        while (!mech10B) yield return null;
        StartCoroutine("_mech10B");

        //ready = true;
        //yield return null;
    }

    IEnumerator _mech10B()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] meteors = new GameObject[4];
        for (int i = 0; i < bossMeteorSpawns.Length; i++)
        {
            GameObject _meteor = PhotonNetwork.Instantiate(homingMeteor.name, bossMeteorSpawns[i].position, Quaternion.identity);
            _meteor.GetComponent<Meteors>().damage = homingDamage;
            meteors[i] = _meteor;
        }

        yield return new WaitForSeconds(1);

        int k = 3;
        int j = 0;
        while (k >= 0)
        {
            meteors[k].GetComponent<HomingMissile>().target = players[j % players.Length].transform;
            j++;
            k--;
        }

        yield return new WaitForSeconds(1);

        if(PhotonNetwork.OfflineMode) FindObjectOfType<ERROR>().DisplayError("Lilith  is  locating  her  demon  sacrifice", 3.5f);
        else photonView.RPC("DisplayError", RpcTarget.All, "Lilith  is  locating  her  demon  sacrifice", 3.5f);
        yield return new WaitForSeconds(1.5f);
        Transform furthestPlayer = findFurthest();
        furthestPlayer.gameObject.GetComponent<PlayerController>().toggleMark();
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(TeleportImm(furthestPlayer, true));
        yield return new WaitForSeconds(0.6f);

        StartCoroutine("_mech10B");
    }

    IEnumerator spawn(float time, Transform _spawn)
    {
        yield return new WaitForSeconds(time);
        GameObject _meteor = PhotonNetwork.Instantiate(meteor.name, _spawn.position, Quaternion.identity);
        _meteor.GetComponent<Rigidbody2D>().velocity = _spawn.right * Random.Range(7, 15);
        _meteor.GetComponent<Meteors>().damage = meteorDamage;
    }

    IEnumerator Teleport(Transform p)
    {
        //yield return new WaitForSeconds(1f);

        for (float i = 1.0f; i >= 0; i -= 0.1f)
        {
            Color _alpha = sr.color;
            _alpha.a = i;
            sr.color = _alpha;
            yield return new WaitForSeconds(0.03f);
        }

        Color alpha = sr.color;
        alpha.a = 0f;
        sr.color = alpha;

        pc.enabled = false;

        
        transform.position = p.position;
        Vector3 pos = transform.position;
        pos.y += 2.0f;
        transform.position = pos;



        for (float i = 0; i <= 1.0; i += 0.1f)
        {
            Color _alpha = sr.color;
            _alpha.a = i;
            sr.color = _alpha;
            yield return new WaitForSeconds(0.02f);
        }

        alpha = sr.color;
        alpha.a = 1.0f;
        sr.color = alpha;

        pc.enabled = true;

        GameObject _explo = PhotonNetwork.Instantiate(explo.name, exploPos.position, Quaternion.identity);
        _explo.GetComponent<ExploDEATH>().damage = exploDamage;

        while (true)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            player = null;
            bool flag = false;
            foreach (GameObject pl in players)
            {
                float dist = Vector2.Distance(pl.transform.position, transform.position);
                if (dist > 10)
                {
                    flag = true;
                    player = pl.transform;
                    break;
                }
            }
            if (flag) break;
            yield return null;
        }
        StartCoroutine(Teleport(player));
    }

    IEnumerator TeleportImm(Transform p, bool attack)
    {
        //yield return new WaitForSeconds(1f);

        for (float i = 1.0f; i >= 0; i -= 0.1f)
        {
            Color _alpha = sr.color;
            _alpha.a = i;
            sr.color = _alpha;
            yield return new WaitForSeconds(0.03f);
        }

        Color alpha = sr.color;
        alpha.a = 0f;
        sr.color = alpha;

        pc.enabled = false;


        transform.position = p.position;
        Vector3 pos = transform.position;
        pos.y += 2.0f;
        transform.position = pos;



        for (float i = 0; i <= 1.0; i += 0.1f)
        {
            Color _alpha = sr.color;
            _alpha.a = i;
            sr.color = _alpha;
            yield return new WaitForSeconds(0.02f);
        }

        alpha = sr.color;
        alpha.a = 1.0f;
        sr.color = alpha;

        pc.enabled = true;

        GameObject _explo;
        if (attack)
        {
            _explo = PhotonNetwork.Instantiate(explo.name, exploPos.position, Quaternion.identity);
            _explo.GetComponent<ExploDEATH>().damage = exploDamage;
        }
        p.gameObject.GetComponent<PlayerController>().toggleMark();
    }
}

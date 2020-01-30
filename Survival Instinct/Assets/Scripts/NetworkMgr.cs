using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;
using TMPro;
using UnityEngine.UI;

public class NetworkMgr : MonoBehaviourPun
{
    
    public GameObject playerPrefab;
    public GameObject cam;
    public Joystick steck;

    //public GameObject enem;
    public GameObject spawnPoint;

     

    private void Awake()
    {
        Cursor.visible = false;
        if(Application.platform == RuntimePlatform.Android)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.AutomaticallySyncScene = true;

        GameObject Lyn = PhotonNetwork.Instantiate(this.playerPrefab.name, spawnPoint.transform.position, spawnPoint.transform.rotation);
        cam.GetComponent<CinemachineVirtualCamera>().Follow = Lyn.transform;
        cam.GetComponent<CinemachineVirtualCamera>().LookAt = Lyn.transform;
        Lyn.GetComponent<Lyn>().joystick = steck;
        //Debug.Log(Lyn.GetComponent<PlayerController>().health);

        if (!PhotonNetwork.OfflineMode)
            Lyn.GetComponent<PlayerController>().playerName.GetComponent<TextMeshProUGUI>().text = Lyn.GetComponent<PhotonView>().Owner.NickName;
        else
            Lyn.GetComponent<PlayerController>().playerName.SetActive(false);
    }


    private void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            if (!p.GetComponent<PlayerController>().isMine())
            {
                p.GetComponent<SpriteRenderer>().color = new Color(0.5647059f, 0.5647059f, 0.5647059f);
                PlayerController pc = p.GetComponent<PlayerController>();
                pc.playerName.GetComponent<TextMeshProUGUI>().text = p.GetComponent<PhotonView>().Owner.NickName;
                pc.hpBar.SetActive(true);
                pc.hpBar.transform.GetChild(0).gameObject.GetComponent<Image>().fillAmount = pc.health / pc.maxHp;                
            }
        }
    }

}

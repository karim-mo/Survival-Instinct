using System.Collections;
using UnityEngine;
using TMPro;
using Photon.Pun;


public class RandomSpawner : MonoBehaviourPun
{
    [System.Serializable]
    public class Wave
    {
        public string name;
        public Transform[] prefab;
        public int spawnCount;
        public float spawnRate;
    }

    [Header("Waves Configuration")]
    public Wave[] waves;
    public GameObject count;
    public Wave[] wavesMulti;
    public GameObject countMulti;

    [Header("Spawn Points")]
    public Transform[] spawnPointsF;
    public Transform[] spawnPointsG;
    public Transform[] bossSpawns;

    private float timer = 5f;   
    private bool go = false;
    private PlayerController player;
    [HideInInspector]
    public int waveCount = 0;
    [HideInInspector]
    public int SpawnCount;


    private int hpratio = 0;
    private int dmgratio = 0;

    void Start()
    {
        if (waves == null) return;
        waveCount = 8;
        StartCoroutine(awake());
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

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        //Debug.Log(GameObject.FindGameObjectsWithTag("Enemy").Length);
        if (waves != null)
        {
            if (go && GameObject.FindGameObjectWithTag("Enemy") == null && EnemyAI.PlayerKills >= SpawnCount)
            {
                go = false;
                EnemyAI.PlayerKills = 0;
                if (PhotonNetwork.OfflineMode)
                {
                    FindObjectOfType<StatsGUIConfirm>().Panel.SetActive(true);
                    Cursor.visible = true;
                    player.Disable();
                }
                else { photonView.RPC("StatsPanel", RpcTarget.All); }
                waveCount++;              
                StartCoroutine(awake());
               // return;
            }
        }
    }

    [PunRPC]
    public void StatsPanel()
    {
        FindObjectOfType<StatsGUIConfirm>().Panel.SetActive(true);
        Cursor.visible = true;
        player.Disable();
    }
    public void StopAllWaves()
    {
        StopAllCoroutines();
        go = false;
    }

    int inc = 0;
    void SpawnEnemy(Transform enemy, string name, int count)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Transform temp = null;
        if(name == "Flying")
        {
            SpawnCount = count;
            Transform spawn = spawnPointsF[Random.Range(0, 20) % spawnPointsF.Length];
            spawn.position = new Vector3(spawn.position.x, spawn.position.y, 0);

            temp = PhotonNetwork.Instantiate(enemy.name, spawn.position, spawn.rotation).transform;
            
        }
        else if(name == "Ground")
        {
            SpawnCount = count;
            Transform spawn = spawnPointsG[Random.Range(0, 20) % spawnPointsG.Length];
            spawn.position = new Vector3(spawn.position.x, spawn.position.y, 0);

            temp = PhotonNetwork.Instantiate(enemy.name, spawn.position, spawn.rotation).transform;
        }
        else if(name == "Flying&Ground")
        {
            SpawnCount = count * 2;
            if (inc == 0)
            {
                Transform spawn = spawnPointsG[Random.Range(0, 20) % spawnPointsG.Length];
                spawn.position = new Vector3(spawn.position.x, spawn.position.y, 0);

                temp = PhotonNetwork.Instantiate(enemy.name, spawn.position, spawn.rotation).transform;
            }
            else
            {
                Transform spawn = spawnPointsF[Random.Range(0, 20) % spawnPointsF.Length];
                spawn.position = new Vector3(spawn.position.x, spawn.position.y, 0);

                temp = PhotonNetwork.Instantiate(enemy.name, spawn.position, spawn.rotation).transform;
            }
            inc++;
            if (inc > 1) inc = 0;
        }
        else if(name == "Boss")
        {
            SpawnCount = count;
            Transform spawn = bossSpawns[Random.Range(0, 20) % bossSpawns.Length];
            spawn.position = new Vector3(spawn.position.x, spawn.position.y, 0);

            temp = PhotonNetwork.Instantiate(enemy.name, spawn.position, spawn.rotation).transform;
            if (waveCount % 9 == 0 && waveCount != 0)
            {
                temp.GetComponent<Boss1>().health += PhotonNetwork.OfflineMode == true ? 9000 : 17000 * 5;
                temp.GetComponent<Boss1>().damage += 0;
            }           
            return;
        }
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform pl = null;
        float currDist = 999999f;
        foreach (GameObject p in players)
        {

            if(Vector2.Distance(p.transform.position, temp.position) < currDist)
            {
                currDist = Vector2.Distance(p.transform.position, temp.position);
                pl = p.GetComponent<Transform>();
            }
        }
        temp.GetComponent<EnemyAI>().target = pl;
        temp.GetComponent<EnemyAI>().damage += dmgratio;
        temp.GetComponent<EnemyAI>().Health += hpratio;
        //temp.GetComponent<SpriteRenderer>().color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));        

    }

    IEnumerator SpawnWave(int wave)
    {
        Wave _wave = waves[wave];

        for(int i = 0; i < _wave.spawnCount; i++)
        {
            for (int j = 0; j < _wave.prefab.Length; j++)
            {
                SpawnEnemy(_wave.prefab[j], _wave.name, _wave.spawnCount);
                yield return new WaitForSeconds(_wave.spawnRate);
            }
        }
    }
    IEnumerator SpawnMultiWave(int wave)
    {
        Wave _wave = wavesMulti[wave];

        for (int i = 0; i < _wave.spawnCount; i++)
        {
            for (int j = 0; j < _wave.prefab.Length; j++)
            {
                SpawnEnemy(_wave.prefab[j], _wave.name, _wave.spawnCount);
                yield return new WaitForSeconds(_wave.spawnRate);
            }
        }
    }

    IEnumerator awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            while (FindObjectOfType<StatsGUIConfirm>().Panel.activeInHierarchy)
            {
                yield return null;
            }
            yield return new WaitForSeconds(2);
            go = true;
            if (PhotonNetwork.OfflineMode)
            {
                StartCoroutine(SpawnWave(waveCount % waves.Length));
                FindObjectOfType<ERROR>().DisplayError("Wave " + (waveCount % waves.Length + 1) + " has started!");
            }
            else
            {
                StartCoroutine(SpawnMultiWave(waveCount % wavesMulti.Length));
                photonView.RPC("DisplayError", RpcTarget.All, "Wave " + (waveCount % wavesMulti.Length + 1) + " has started!");
            }

            if (PhotonNetwork.OfflineMode) { hpratio += 8; dmgratio += 8; }
            else { hpratio += 25; dmgratio += 7; }
            
        }
    }

    [PunRPC]
    public void DisplayError(string erorr)
    {
        FindObjectOfType<ERROR>().DisplayError(erorr);
    }
}

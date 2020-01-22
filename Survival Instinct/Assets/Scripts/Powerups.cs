using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerups : MonoBehaviour
{

    [Header("Prefabs Management")]
    public Transform[] Prefabs;
    public float spawnRate;



    [Header("Spawn Locations")]
    public Transform[] SpawnLocs;

    public bool[] visited;
    private bool go = false;
    private int index = 0;
    private PlayerController player;
    public Transform[] instantiated;

    void Start()
    {
        if (SpawnLocs.Length != 0) visited = new bool[SpawnLocs.Length];
        instantiated = new Transform[1000];
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        StartCoroutine(awake());
    }

    
    void Update()
    {
        if(go && GameObject.FindGameObjectWithTag("Enemy") == null 
            && player.enemyKills == FindObjectOfType<RandomSpawner>().SpawnCount)
        {
            go = false;
            StopAllCoroutines();
            for(int i = 0; i < instantiated.Length; i++)
            {
                if (instantiated[i] == null) continue;

                Destroy(instantiated[i].gameObject);
                instantiated[i] = null;
            }
            for (int i = 0; i < visited.Length; i++) visited[i] = false;
            index = 0;
            StartCoroutine(awake());
        }
    }
    public void DestroyPowerup(int i)
    {
        Destroy(instantiated[i]);
        instantiated[i] = null;
    }

    void SpawnPrefab(Transform prefab)
    {
        if (EnemyAI.deaths < 4) return;

        int loc = checkVisited();
        if (loc == -1) return;
        FindObjectOfType<ERROR>().DisplayError("Powerup Spawned!");

        EnemyAI.deaths = 0;

        Transform sp = SpawnLocs[loc];
        visited[loc] = true;
        sp.position = new Vector3(sp.position.x, sp.position.y, 0);
        instantiated[index] = Instantiate(prefab, sp.position, sp.rotation);
        instantiated[index++].GetComponent<PowerUpDetails>().visitedIndex = loc;
    }

    int checkVisited()
    {
        int i;
        for(i = 0; i < visited.Length; i++)
        {
            if (visited[i]) continue;
            else break;
        }
        if (i == visited.Length) return -1;
        while (true)
        {
            int rnd = Random.Range(0, 100) % SpawnLocs.Length;
            if (visited[rnd] == false) return rnd;
        }
    }

    IEnumerator SpawnPowerups()
    {      
        while (true)
        {
            SpawnPrefab(Prefabs[Random.Range(0, 100) % Prefabs.Length]);
            yield return new WaitForSeconds(1);
        }
    }
    IEnumerator awake()
    {
        yield return new WaitForSeconds(10);
        go = true;
        StartCoroutine(SpawnPowerups());
    }
}

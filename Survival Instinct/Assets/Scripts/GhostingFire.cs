using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostingFire : MonoBehaviour
{

    [Header("Setup")]
    public GameObject Ghost;
    public Transform[] locs;
    public int count;
    public float spawnRate;

    private Transform player;
    private SpriteRenderer playerSprite;

    private bool Lock = false;

    private void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            Debug.Log(p);
            if (p.GetComponent<PlayerController>().isMine())
            {
                player = p.GetComponent<Transform>();
                break;
            }
        }
        playerSprite = GameObject.FindGameObjectWithTag("Player").GetComponent<SpriteRenderer>();

    }

    public void StartGhosting()
    {
        if (!Lock)
        {
            Lock = true;
            StartCoroutine(SpawnGhosts());
        }
    }


    IEnumerator SpawnGhosts()
    {
        float localY = player.localScale.x;
        Vector3 local = new Vector3(Ghost.transform.localScale.x, localY, 1);
        for (int i = 0; i < count; i++)
        {
            Vector3 spawn = locs[i].position;
            spawn.z = 0;
            GameObject sp = Instantiate(Ghost, spawn, Ghost.transform.rotation);
            sp.GetComponent<Transform>().localScale = local;
            StartCoroutine(FadeGhost(sp));
            yield return new WaitForSeconds(spawnRate);
        }
        Lock = false;
    }


    IEnumerator FadeGhost(GameObject ghost)
    {
        yield return new WaitForSeconds(0.3f);
        for (float i = 1f; i >= 0; i -= 0.05f)
        {
            Color temp = ghost.GetComponent<SpriteRenderer>().color;
            temp.a = i;
            ghost.GetComponent<SpriteRenderer>().color = temp;
            yield return new WaitForSeconds(0.04f);
        }
        Destroy(ghost);
    }
}

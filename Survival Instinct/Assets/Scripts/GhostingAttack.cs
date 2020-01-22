using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostingAttack : MonoBehaviour
{

    [Header("Setup")]
    public GameObject Ghost;
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
        yield return new WaitForSeconds(.3f);
        Vector3 spawn = player.transform.position;
        spawn.x = (player.localScale.x > 0 ? spawn.x + 1 : spawn.x - 1);
        float localX = player.localScale.x;
        Vector3 local = new Vector3(localX, player.localScale.y, 1);
        for (int i = 0; i < count; i++)
        {
            GameObject sp = Instantiate(Ghost, spawn, Quaternion.identity);
            sp.GetComponent<Transform>().localScale = local;
            sp.GetComponent<Rigidbody2D>().AddForce((localX > 0 ? Vector2.right : Vector2.left) * 100);
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
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(ghost);
    }
}

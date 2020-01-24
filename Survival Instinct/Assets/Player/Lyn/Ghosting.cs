using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghosting : MonoBehaviour
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
            //Debug.Log(p);
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
            StartCoroutine(SpawnGhosts(count));
        }          
    }

    public void StartGhosting(float count)
    {       
        if (!Lock)
        {
            Lock = true;
            StartCoroutine(SpawnGhosts(count));
        }
    }


    IEnumerator SpawnGhosts(float count)
    {
        for(int i = 0; i < count; i++)
        {
            GameObject sp = Instantiate(Ghost, player.transform.position, player.transform.rotation);
            sp.GetComponent<SpriteRenderer>().sprite = playerSprite.sprite;
            sp.GetComponent<Transform>().localScale = player.localScale;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1AI : MonoBehaviour
{
    public LayerMask Boundary;
    public float health;
    public GameObject onDeath;
    public GameObject fromPlayer;

    [Header("Firing & Powerups")]
    public GameObject bullet;
    public float fireRate = 10000f;
    public float projectileSpeed = 1f;
    public float projectileCount = 20f;
    public float spreadAngle = 20.0f;

    private Rigidbody2D rb;
    private PlayerController player;
    private bool left = false;
    bool canMove = true;

    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine("BossSequence");
    }

    private void Update()
    {
        if ((transform.position.x < -33f && left) || (transform.position.x > 14f && !left)) left = !left;
        if (health <= 0)
        {
            Instantiate(onDeath, transform.position, Quaternion.identity);
            int rnd = Random.Range(FindObjectOfType<LevelStats>().minExpRange, FindObjectOfType<LevelStats>().maxExpRange);
            FindObjectOfType<Experience>().experience += rnd;
            LevelStats.totalExp += rnd;
            PlayerPrefs.SetInt("Exp", FindObjectOfType<Experience>().experience);
            FindObjectOfType<TextOverlay>().Overlay("+" + rnd + " EXP");
            player.enemyKills++;
            EnemyAI.deaths++;
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        if (!left && canMove)
        {
            rb.AddForce(Vector2.right * 400 * Time.fixedDeltaTime);
        }
        else if (left && canMove)
        {
            rb.AddForce(Vector2.left * 400 * Time.fixedDeltaTime);
        }

    }

    
    
    IEnumerator Movement()
    {
        left = !left;
        yield return new WaitForSeconds(7);
        StartCoroutine("Movement");
    }

    IEnumerator BossSequence()
    {
        StartCoroutine("Movement");
        yield return new WaitForSeconds(Random.Range(3, 8));
        StopCoroutine("Movement");
        canMove = false;
        FindObjectOfType<ERROR>().DisplayError("Boss attack incoming. Start running!");
        yield return new WaitForSeconds(1f);
        StartCoroutine("Attack1");
        yield return new WaitForSeconds(6);
        StopCoroutine("Attack1");
        canMove = true;
        StartCoroutine("BossSequence");
    }

    IEnumerator Attack1()
    {
        Vector2 direction = Vector2.left;
        for (int i = 0; i < 50; i++)
        {
            GameObject projectile = Instantiate(bullet, transform.position, Quaternion.identity);
            int x = i;
            if (x > 5)
            {
                x = i * -1;
            }
            direction = Quaternion.Euler(0, 0, spreadAngle * x) * direction;
            projectile.GetComponent<Rigidbody2D>().AddForce(direction * 0.02f);
        }
        yield return new WaitForSeconds(1f);
        StartCoroutine("Attack1");
    }
}

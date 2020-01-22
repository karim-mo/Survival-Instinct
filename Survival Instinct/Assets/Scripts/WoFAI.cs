using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoFAI : MonoBehaviour
{
    private static class States
    {
        public const int IDLE = 0;
        public const int ATTACKING = 1;
        public const int ENROUTE = 2;
        public const int SEEKING_PATH = 3; // might not be used for now
    }


    [Header("Code Related")]
    public GameObject fromPlayer;

    [Header("Enemy Vars")]
    public float speed = 100f;
    public float health = 100f;
    public float jumpForce = 500f;
    public float distanceFromTarget = 2f;
    public float DistanceToTheGround = 0.1f;
    public Transform target;

    [HideInInspector]
    public bool endOfPath = false;
    public int currState = 0;



    private float iFrame;
    private bool iframed = false;


    void Start()
    {
        if (target == null) return;
        Physics2D.IgnoreLayerCollision(12, 12);
    }

    
    void Update()
    {
        iFrame -= Time.deltaTime;
        if (iFrame <= 0)
        {
            iFrame = GameObject.FindGameObjectWithTag("PlayerWeapon").GetComponent<Gun>().fireRate / 1000;
            iframed = false;
        }

    }


    void FixedUpdate()
    {
        //Debug.Log(IsGrounded());
        if (Vector2.Distance(transform.position, target.position) > distanceFromTarget && currState != 3)
        {
            currState = States.ENROUTE;
            Vector2 direction = transform.position - target.position;
            direction = new Vector2(direction.x, transform.position.y);
            direction.Normalize();
            //if(transform.position.y < target.position.y) direction.y = -transform.position.y;
            //else direction.y = transform.position.y;

            if (transform.position.x != target.position.x)
            {
                float movementDistance = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, target.position, movementDistance);
            }
            //GetComponent<Rigidbody2D>().velocity = -direction * speed * Time.fixedDeltaTime;

            if (transform.position.y + 0.1 < target.position.y  && IsGrounded())
            {
                Debug.Log("lol");
                GetComponent<Rigidbody2D>().AddForce(Vector2.up * Random.Range(jumpForce, jumpForce * 1.5f));

                //Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
                //velocity.y = jumpForce * Time.fixedDeltaTime;
                //GetComponent<Rigidbody2D>().velocity = velocity;
            }
            if(GetComponent<Rigidbody2D>().velocity.y < 0)
            {
                StartCoroutine(ResetGravity());
            }

        }
        else currState = States.IDLE; 
            

    }

    IEnumerator ResetGravity()
    {
        GetComponent<Rigidbody2D>().gravityScale = 5;
        yield return new WaitForSeconds(0.1f);
        GetComponent<Rigidbody2D>().gravityScale = 2;
    }

    public LayerMask groundLayer;

    bool IsGrounded()
    {
        Vector2 position = transform.position;
        Vector2 direction = Vector2.down;
        float distance = 0.5f;
        Debug.DrawRay(position, direction, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundLayer);
        if (hit.collider != null)
        {
            return true;
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            //GetComponent<Rigidbody2D>().AddForce(new Vector2(-transform.position.x * 2000 * Time.deltaTime, 0));
            collision.GetComponent<PlayerController>().health -= 10;
            GetComponent<Rigidbody2D>().AddForce(new Vector2(transform.position.x + Random.Range(-5, 5), 0));
        }
        if (collision.tag == "Bullet")
        {
            Vector3 a = transform.position;
            a.z = -0.4f;
            GameObject parti = Instantiate(fromPlayer, a, Quaternion.identity);
            if (!iframed)
            {
                iframed = true;
                health -= 10;
            }

            if (health <= 0) Destroy(gameObject);
        }
    }

    }

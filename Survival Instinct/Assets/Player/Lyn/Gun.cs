using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Gun : MonoBehaviour
{

    [Header("Code Related")]
    public Transform muzzle;
    public Transform player;
    public GameObject bullet;
    public Camera mainCamera;



    [Header("Firing & Powerups")]
    public float fireRate = 500f;
    public float projectileSpeed = 25f;
    public float projectileCount = 1f;
    public float spreadAngle = 3.0f;
    

    float nextShotTime;
    Vector3 originalCameraPosition;
    float shakeAmt = 0.03f;
    float shakeDuration = 0f;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    //PERHAPS DEPRECATED FUNCTION FOR CURRENT GAME USE
    public void Shoot()
    {

        if (Time.time > nextShotTime)
        {
            if (fireRate == 400f)
                AudioManager.Play("Shoot");
            else AudioManager.Play("Shoot2");
            originalCameraPosition = mainCamera.transform.position;
            Camera.main.transform.DOComplete();
            Camera.main.transform.DOShakePosition(.1f, .4f, 14, 90, false, true);
            //InvokeRepeating("CameraShake", 0, .01f);
            //Invoke("StopShaking", 0.2f);
            //ShakeOnce(0.2f, 0.7f);
            nextShotTime = Time.time + fireRate / 1000;
            Vector2 target = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
            Vector2 myPos = new Vector2(muzzle.position.x, muzzle.position.y);
            Vector2 direction = target - myPos;
            direction.Normalize();
            //Quaternion a = muzzle.rotation;
            for (int i = 0; i < projectileCount; i++)
            {
                GameObject projectile = Instantiate(bullet, myPos, Quaternion.identity);
                int x = i;
                if (x > 5)
                {
                    x = i * -1;
                }
                direction = Quaternion.Euler(0, 0, spreadAngle * x) * direction;
                projectile.GetComponent<Rigidbody2D>().velocity = direction * projectileSpeed;
            }
        }
    }

    public void Shoot(float speed, float count)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.1f, .4f, 14, 90, false, true);
        for (int i = 0; i < count; i++)
        {
            GameObject projectile = Instantiate(bullet, muzzle.position, Quaternion.identity);
            projectile.GetComponent<Rigidbody2D>().velocity = 
                (player.localScale.x > 0 ? Vector2.right : Vector2.left) * speed;
            projectile.transform.localScale = new Vector3(
                player.localScale.x > 0 ? player.localScale.x - 0.5f : player.localScale.x + 0.5f, player.localScale.y, player.localScale.z);
        }
    }

    public void ShakeOnce(float lenght, float strength)
    {
        shakeDuration = lenght;
        shakeAmt = strength;
    }

    void CameraShake()
    {
        if (shakeAmt > 0)
        {
            float quakeAmt = Random.value * shakeAmt * 2 - shakeAmt;
            Vector3 pp = mainCamera.transform.position;
            pp.y += quakeAmt;
            pp.x += quakeAmt;
            mainCamera.transform.position = pp;

            //Vector3 newPos = originalCameraPosition + Random.insideUnitSphere * shakeAmt;

            //mainCamera.localPosition = Vector3.SmoothDamp(mainCamera.localPosition, newPos, ref vel2, 0.05f);

            //shakeDuration -= Time.deltaTime;
            //shakeAmt = Mathf.SmoothDamp(shakeAmt, 0, ref vel, 0.7f);
        }
  //      else
  //      {
  //          mainCamera.localPosition = originalCameraPosition;
		//}
    }

    void StopShaking()
    {
        CancelInvoke("CameraShake");
        mainCamera.transform.position = originalCameraPosition;
    }
}
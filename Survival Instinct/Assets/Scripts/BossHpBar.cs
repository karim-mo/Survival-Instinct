using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHpBar : MonoBehaviour
{
    public GameObject boss;
    
    public string bossScript;
    // Update is called once per frame
    void Update()
    {
        if (boss != null)
        {
            gameObject.GetComponent<Image>().fillAmount = boss.GetComponent<Lilith>().health / boss.GetComponent<Lilith>().maxHealth;
        }
    }
}

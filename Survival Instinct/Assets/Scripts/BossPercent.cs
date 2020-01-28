using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossPercent : MonoBehaviour
{
    public GameObject boss;

    // Update is called once per frame
    void Update()
    {
        if(boss != null)
        {
            gameObject.GetComponent<TextMeshProUGUI>().text = "" + Mathf.Ceil((boss.GetComponent<Lilith>().health / boss.GetComponent<Lilith>().maxHealth) * 100) + "%";
        }
    }
}

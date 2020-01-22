using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Tracker : MonoBehaviour
{
    void Update()
    {
        PlayerController myLyn = transform.parent.gameObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<PlayerController>();

        gameObject.GetComponent<Image>().fillAmount = myLyn.health / myLyn.maxHp;
    }
}

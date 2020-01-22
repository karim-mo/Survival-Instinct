using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Experience : MonoBehaviour
{
    public int maxExp = 400;
    public GameObject Exp;
    public Image ExpBar;

    [HideInInspector]
    public int experience;


    private void Awake()
    {
        experience = PlayerPrefs.GetInt("Exp", 0);
    }

    private void Update()
    {
        if (experience >= maxExp)
        {
            experience = 0;
            PlayerPrefs.SetInt("PTS", ++GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().StatPoints);
        }
        Exp.GetComponent<TextMeshProUGUI>().text = "" + experience + "/" + maxExp;
        ExpBar.fillAmount = experience / (1.0f * maxExp);       
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatsGUIConfirm : MonoBehaviour
{
    public GameObject Panel;
    public GameObject Points;
    public GameObject[] values;
    
    [HideInInspector]
    public bool isActive = false;

    private PlayerController player;

    private void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            Debug.Log(p);
            if (p.GetComponent<PlayerController>().isMine())
            {
                player = p.GetComponent<PlayerController>();
                break;
            }
        }
        //Just a little extra response\
        Points.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("PTS").ToString();
        values[0].GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("HP").ToString();
        values[1].GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("DP").ToString();
        values[2].GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("LP").ToString();
        values[3].GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("MP").ToString();
        values[4].GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("SP").ToString();
        StartCoroutine("AutoSave");
    }

    private void Update()
    {
        if (Panel.activeInHierarchy && !isActive)
        {            
            isActive = true;
            Points.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("PTS").ToString();
            values[0].GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("HP").ToString();
            values[1].GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("DP").ToString();
            values[2].GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("LP").ToString();
            values[3].GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("MP").ToString();
            values[4].GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("SP").ToString();
        }
        if (!Panel.activeInHierarchy) { isActive = false; }        
    }


    IEnumerator AutoSave()
    {
        PlayerPrefs.Save();
        yield return new WaitForSeconds(60);
        StartCoroutine("AutoSave");
    }
}

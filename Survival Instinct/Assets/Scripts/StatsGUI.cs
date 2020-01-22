using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatsGUI : MonoBehaviour
{
    public GameObject Value;
    public GameObject Cost;
    public GameObject Points;
    public GameObject[] values = new GameObject[5];
    public string Key;
    public int PlusCost;
    public int MinusCost;


    private PlayerController player;

    
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    public void Increase()
    {
        AudioManager.Play("Click");
        int val = int.Parse(Value.GetComponent<TextMeshProUGUI>().text);
        if (val == 100) return;
        val++;
        if (val > PlayerPrefs.GetInt(Key))
        {
            int _cost = int.Parse(Cost.GetComponent<TextMeshProUGUI>().text) + PlusCost;
            Cost.GetComponent<TextMeshProUGUI>().text = "" + _cost;
        }
        else
        {
            int _cost = int.Parse(Cost.GetComponent<TextMeshProUGUI>().text) - MinusCost;
            Cost.GetComponent<TextMeshProUGUI>().text = "" + _cost;
        }
        Value.GetComponent<TextMeshProUGUI>().text = "" + val;
    }

    public void Decrease()
    {
        AudioManager.Play("Click");
        int val = int.Parse(Value.GetComponent<TextMeshProUGUI>().text);
        if (val == 0) return;
        val--;
        if(val < PlayerPrefs.GetInt(Key))
        {
            int _cost = int.Parse(Cost.GetComponent<TextMeshProUGUI>().text) + MinusCost;
            Cost.GetComponent<TextMeshProUGUI>().text = "" + _cost;
        }
        else
        {
            int _cost = int.Parse(Cost.GetComponent<TextMeshProUGUI>().text) - PlusCost;
            Cost.GetComponent<TextMeshProUGUI>().text = "" + _cost;
        }
        Value.GetComponent<TextMeshProUGUI>().text = "" + val;
    }

    public void Confirm()
    {
        AudioManager.Play("Click");
        int _Points = int.Parse(Points.GetComponent<TextMeshProUGUI>().text);
        int _Cost = int.Parse(Cost.GetComponent<TextMeshProUGUI>().text);

        if (_Cost == 0)
        {
            FindObjectOfType<ERROR>().DisplayError("You didn't update your stats!");
            return;
        }

        if(_Points >= _Cost)
        {
            _Points -= _Cost;
            Points.GetComponent<TextMeshProUGUI>().text = "" + _Points;
            Cost.GetComponent<TextMeshProUGUI>().text = "0";
            player.StatPoints = _Points;
            PlayerPrefs.SetInt("HP", int.Parse(values[0].GetComponent<TextMeshProUGUI>().text));
            PlayerPrefs.SetInt("DP", int.Parse(values[1].GetComponent<TextMeshProUGUI>().text));
            PlayerPrefs.SetInt("LP", int.Parse(values[2].GetComponent<TextMeshProUGUI>().text));
            PlayerPrefs.SetInt("MP", int.Parse(values[3].GetComponent<TextMeshProUGUI>().text));
            PlayerPrefs.SetInt("SP", int.Parse(values[4].GetComponent<TextMeshProUGUI>().text));
            PlayerPrefs.SetInt("PTS", int.Parse(Points.GetComponent<TextMeshProUGUI>().text));
            FindObjectOfType<PlayerStats>().Refresh();
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().UpdateStats();
        }
        else
        {
            FindObjectOfType<ERROR>().DisplayError("You don't have enough currency!");
            return;
        }
    }

    public void Exit()
    {
        Cursor.visible = false;
        AudioManager.Play("Click");
        GameObject.FindGameObjectWithTag("GM").GetComponent<StatsGUIConfirm>().Panel.SetActive(false);
        Cost.GetComponent<TextMeshProUGUI>().text = "0";
        player.Enable();
    }
}

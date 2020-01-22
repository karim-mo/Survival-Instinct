using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Photon.Pun;


public class HealthManager : MonoBehaviourPun
{
    public Image[] hearts;

    private PlayerController player;

    private float prevHealth;

    Dictionary<float, Image> _hearts = new Dictionary<float, Image>();
     

    private void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach(GameObject p in players)
        {
            Debug.Log(p);
            if (p.GetComponent<PlayerController>().isMine())
            {
                player = p.GetComponent<PlayerController>();
                break;
            }
        }

        //player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        for(int i = 0; i < hearts.Length; i++)
        {
            _hearts.Add((i+1) * 0.2f, hearts[i]);
        }
    }
 
    private void Update()
    {
        float hpstate = player.health / PlayerStats.maxHealth;
        int _hpstate = Mathf.CeilToInt(hpstate / 0.2f);
        float _hp = player.health;

        for (int i = 0; i < _hearts.Count; i++)
        {
            KeyValuePair<float, Image> entry = _hearts.ElementAt(i);

            if (hpstate <= entry.Key)
            {
                entry.Value.GetComponent<Image>().fillAmount = _hp / (PlayerStats.maxHealth / 5);
                break;
            }
            else
            {
                _hp -= PlayerStats.maxHealth / 5;
                entry.Value.GetComponent<Image>().fillAmount = 1;
            }
        }


        for (int i = _hpstate; i < _hearts.Count; i++)
        {
            KeyValuePair<float, Image> entry = _hearts.ElementAt(i);
            entry.Value.GetComponent<Image>().fillAmount = 0;
        }
    }



    #region mylegacycode:(
    //{
    //int state = Mathf.CeilToInt(player.health / 2 / 10) - 1;
    //    if (state >= 0) hearts[state].GetComponent<Image>().fillAmount = player.health / 10 % 2 == 0 ? 1f : 0.5f;
    //    if(state >= 0 && state<hearts.Length - 1 && (player.health / 10) % 2 == 0) hearts[state + 1].GetComponent<Image>().fillAmount = 0f;
    //    for(int i = 0; i<state; i++)
    //    {
    //        hearts[i].GetComponent<Image>().fillAmount = 1f;
    //    }
    //}
    #endregion
}

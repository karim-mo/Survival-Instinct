using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health Config.")]
    public float baseHP;
    public float hpCap;
    public float hpPtsSoft;
    public float hpPts;

    [Header("Damage Config.")]
    public float baseDmg;
    public float DmgCap;
    public float DmgPtsSoft;
    public float DmgPts;

    [Header("Speed Config.")]
    public float baseSpeed;
    public float SpeedCap;
    public float SpeedPtsSoft;
    public float SpeedPts;

    [Header("LifeSteal Config.")]
    public float baseLS;
    public float LifeStealCap;
    public float LSPtsSoft;
    public float LSPts;

    [Header("Shield Config.")]
    public float baseShield;
    public float ShieldCap;
    public float ShieldPtsSoft;
    public float ShieldPts;

    private PlayerController player;

    private int HP, DP, MP, LP, SP;
    public static float maxHealth, Damage, Speed, LSteal, maxShield;

    private void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            //Debug.Log(p);
            if (p.GetComponent<PlayerController>().isMine())
            {
                player = p.GetComponent<PlayerController>();
                break;
            }
        }

        Refresh();
    }

    public void Refresh()
    {
        HP = PlayerPrefs.GetInt("HP");
        DP = PlayerPrefs.GetInt("DP");
        MP = PlayerPrefs.GetInt("MP");
        LP = PlayerPrefs.GetInt("LP");
        SP = PlayerPrefs.GetInt("SP");

        maxHealth = baseHP + (HP <= hpCap ? HP * hpPtsSoft : ((HP - hpCap) * hpPts + hpCap * hpPtsSoft));
        Damage = baseDmg + (DP <= DmgCap ? DP * DmgPtsSoft : ((DP - DmgCap) * DmgPts + DmgCap * DmgPtsSoft));
        Speed = baseSpeed + (MP <= SpeedCap ? MP * SpeedPtsSoft : ((MP - SpeedCap) * SpeedPts + SpeedCap * SpeedPtsSoft));
        LSteal = baseLS + (LP <= LifeStealCap ? LP * LSPtsSoft : ((LP - LifeStealCap) * LSPts + LifeStealCap * LSPtsSoft));
        maxShield = baseShield + (SP <= ShieldCap ? SP * ShieldPtsSoft : ((SP - ShieldCap) * ShieldPts + ShieldCap * ShieldPtsSoft));
        PlayerPrefs.Save();
    }

}

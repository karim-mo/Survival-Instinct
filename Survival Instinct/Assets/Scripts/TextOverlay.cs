using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextOverlay : MonoBehaviour
{
    public GameObject Text;


    private PlayerController player;

    private void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            if (p.GetComponent<PlayerController>().isMine())
            {
                player = p.GetComponent<PlayerController>();
                break;
            }
        }
    }

    public void Overlay(string text)
    {
        GameObject canv = Instantiate(Text, player.transform.position, Quaternion.identity);
        canv.transform.SetParent(player.transform);
        canv.GetComponent<TextMeshPro>().text = text;

        StartCoroutine(TextEffect(canv));
    }


    public void Overlay(Transform sp, string text, bool parent)
    {
        if (sp == null) return;
        GameObject canv = Instantiate(Text, sp.position, Quaternion.identity);
        if(parent) canv.transform.SetParent(sp);
        canv.GetComponent<TextMeshPro>().text = text;
        canv.GetComponent<TextMeshPro>().color = new Color(1, 0, 0.03875828f);
        
        StartCoroutine(TextEffect(canv));
    }

    public void Overlay(Transform sp, string text, bool parent, Color color)
    {
        if (sp == null) return;
        GameObject canv = Instantiate(Text, sp.position, Quaternion.identity);
        if (parent) canv.transform.SetParent(sp);
        canv.GetComponent<TextMeshPro>().text = text;
        canv.GetComponent<TextMeshPro>().color = color;

        StartCoroutine(TextEffect(canv));
    }

    IEnumerator TextEffect(GameObject _Text)
    {
        if (_Text == null) StopCoroutine("TextEffect");
        for (int i = 0; i < 20; i++)
        {
            if (_Text != null)
            {
                Vector3 pos = _Text.GetComponent<RectTransform>().position;
                pos.y += 0.1f;
                _Text.GetComponent<RectTransform>().position = pos;
                Color c = _Text.GetComponent<TextMeshPro>().color;
                c.a -= 0.05f;
                _Text.GetComponent<TextMeshPro>().color = c;
            }
            yield return new WaitForSeconds(0.05f);
        }
        Destroy(_Text);
    }



}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ERROR : MonoBehaviour
{
    public GameObject ErrorText;

    public void DisplayError(string e)
    {
        StopAllCoroutines();
        ErrorText.GetComponent<TextMeshProUGUI>().text = e;
        ErrorText.SetActive(true);
        StartCoroutine(Despawn(2));
    }

    public void DisplayError(string e, float time)
    {
        StopAllCoroutines();
        ErrorText.GetComponent<TextMeshProUGUI>().text = e;
        ErrorText.SetActive(true);
        StartCoroutine(Despawn(time));
    }

    IEnumerator Despawn(float time)
    {
        yield return new WaitForSeconds(time);
        ErrorText.SetActive(false);
    }
}

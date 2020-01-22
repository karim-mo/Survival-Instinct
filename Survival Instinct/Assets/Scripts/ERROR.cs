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
        StartCoroutine("Despawn");
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(2);
        ErrorText.SetActive(false);
    }
}

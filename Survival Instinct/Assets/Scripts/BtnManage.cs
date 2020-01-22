using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BtnManage : MonoBehaviour
{
    public void HOVER()
    {
        AudioManager.Play("Hover");
    }
    public void CLICK()
    {
        AudioManager.Play("Click");
    }
    public void PLAYCLICK()
    {
        SceneManager.LoadScene(1);
    }
    public void QUITCLICK()
    {
        Application.Quit();
    }
}

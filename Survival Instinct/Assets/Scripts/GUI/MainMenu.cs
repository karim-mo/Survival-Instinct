using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MainMenu : MonoBehaviour
{
    [Header("General")]
    public GameObject mainPanel;
    public Animator anim;
    public GameObject DropDown;

    [Header("Control Panel")]
    public GameObject cntrlPanel;
    

    [Header("Options Panel")]
    public GameObject optPanel;
    public GameObject SelectedOption;

    [Header("Coop")]
    public GameObject coop;


    private int width;
    private int height;

    private void Awake()
    {
        Cursor.visible = true;
        if (SelectedOption != null)
        {
            TMP_Dropdown _Dropdown = DropDown.GetComponent<TMP_Dropdown>();
            List<string> options = new List<string>();
            int i;
            for (i = 0; i < 3; i++)
            {
                options.Add(_Dropdown.options[i].text);
            }

            string res = PlayerPrefs.GetInt("Width", 1280) + " x " + PlayerPrefs.GetInt("Height", 720);
            for (i = 0; i < 3; i++)
            {
                if (res.Equals(_Dropdown.options[i].text))
                {
                    break;
                }
            }
            options.RemoveAt(i);
            options.Insert(0, _Dropdown.options[i].text);
            _Dropdown.options.RemoveRange(0, _Dropdown.options.Capacity);
            for (i = 0; i < 3; i++)
            {
                _Dropdown.options.Add(new TMP_Dropdown.OptionData(options[i]));
            }
        }
    }


    //Main Panel
    public void btnControl()
    {
        AudioManager.Play("Click");
        cntrlPanel.SetActive(true);
        mainPanel.SetActive(false);
    }
    public void btnOption()
    {
        AudioManager.Play("Click");
        optPanel.SetActive(true);
        mainPanel.SetActive(false);
    }
    public void btnCoop()
    {
        AudioManager.Play("Click");
        coop.SetActive(true);
        mainPanel.SetActive(false);
    }

    //Start & Scene Transition
    public void Play()
    {
        PhotonNetwork.OfflineMode = true;
        AudioManager.Play("Click");
        anim.SetTrigger("FadeOut");
        PlayerPrefs.SetInt("scene", SceneManager.GetActiveScene().buildIndex + 2);
    }
    public void SceneTransition()
    {        
        SceneManager.LoadScene(1);
    }


    //Control Panel
    public void ctrlBack()
    {
        AudioManager.Play("Click");
        mainPanel.SetActive(true);
        cntrlPanel.SetActive(false);       
    }

    //Options Panel
    public void optBack()
    {
        AudioManager.Play("Click");
        mainPanel.SetActive(true);
        optPanel.SetActive(false);
    }
    public void optApply()
    {
        AudioManager.Play("Click");
        string res = SelectedOption.GetComponent<TextMeshProUGUI>().text;
        if (res.Equals("1280 x 720"))
        {
            width = 1280;
            height = 720;
        }
        else if (res.Equals("1680 x 1050"))
        {
            width = 1680;
            height = 1050;
        }
        else if (res.Equals("1920 x 1080"))
        {
            width = 1920;
            height = 1080;
        }

        PlayerPrefs.SetInt("Width", width);
        PlayerPrefs.SetInt("Height", height);
    }

    //Quit
    public void Quit()
    {
        AudioManager.Play("Click");
        PlayerPrefs.Save();
        Application.Quit();
    }


    

}

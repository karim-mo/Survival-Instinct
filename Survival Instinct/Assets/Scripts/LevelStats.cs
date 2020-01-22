using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStats : MonoBehaviour
{
    public int minExpRange;
    public int maxExpRange;

    public static float totalExp = 0;
    //To be continued..

    private void Awake()
    {
        Cursor.visible = false;
        Screen.SetResolution(PlayerPrefs.GetInt("Width", 1280), PlayerPrefs.GetInt("Height", 720), FullScreenMode.FullScreenWindow);
    }
}

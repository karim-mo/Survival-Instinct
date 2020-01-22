using UnityEngine;
using UnityEngine.UI;


using Photon.Pun;
using Photon.Realtime;


using System.Collections;
public class PIF : MonoBehaviour
{
    #region Private Constants
    InputField _inputField;

    // Store the PlayerPref Key to avoid typos
    const string playerNamePrefKey = "PlayerName";


    #endregion


    #region MonoBehaviour CallBacks


    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {


        string defaultName = string.Empty;
        _inputField = this.GetComponent<InputField>();
        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }


        PhotonNetwork.NickName = defaultName;
    }


    #endregion


    #region Public Methods


    /// <summary>
    /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
    /// </summary>
    /// <param name="value">The name of the Player</param>
    public void SetPlayerName()
    {
        // #Important
        if (string.IsNullOrEmpty(_inputField.text))
        {
            Debug.LogError("Player Name is null or empty");
            return;
        }
        PhotonNetwork.NickName = _inputField.text;


        PlayerPrefs.SetString(playerNamePrefKey, _inputField.text);
    }


    #endregion
}

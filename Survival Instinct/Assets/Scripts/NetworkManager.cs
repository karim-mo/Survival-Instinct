using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Player = Photon.Realtime.Player;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    string gameVersion = "1";
    bool isConnecting;
    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;
    private Dictionary<int, GameObject> playerListEntries;


    [Header("Menu")]
    public GameObject mainMenu;

    [Header("Name Phase")]
    public GameObject namePhase;
    public GameObject _textInput;
    public GameObject label;
    public GameObject Join_Create;

    [Header("Join/Create")]
    public GameObject Room;
    public GameObject RoomList;

    [Header("RoomList")]
    public GameObject RoomListEntryPrefab;
    public GameObject RoomListContent;

    [Header("Room")]
    public GameObject PlayerListEntryPrefab;
    public GameObject PlayerListContent;
    public GameObject startBtn;


    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();
        if(PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        PhotonNetwork.NetworkingClient.State = ClientState.Disconnected;
        
    }

    //private void Start()
    //{
    //    if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
    //    PhotonNetwork.NetworkingClient.State = ClientState.Disconnected;
    //}

    public void Connect()
    {
        AudioManager.Play("Click");
        if (isConnecting) return;
        if (_textInput.GetComponent<InputField>().text == "") return;
        isConnecting = true;
        label.SetActive(true);
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.NickName = _textInput.GetComponent<InputField>().text;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected!");
        label.GetComponent<Text>().text = "Connected Succesfully!";
        StartCoroutine("JoinOrCreate");
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("Disconnected with reason {0}", cause);
    }

    

    #region namephase
    public void nameBack()
    {
        AudioManager.Play("Click");
        isConnecting = false;
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        PhotonNetwork.NetworkingClient.State = ClientState.Disconnected;
        label.GetComponent<Text>().text = "Connecting to Server...";
        label.SetActive(false);
        mainMenu.SetActive(true);
        namePhase.SetActive(false);      
    }
    #endregion


    #region Join/Create

    public void JoinRoom()
    {
        AudioManager.Play("Click");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        RoomList.SetActive(true);
        Join_Create.SetActive(false);
    }
    public void CreateRoom()
    {
        AudioManager.Play("Click");
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };

        PhotonNetwork.CreateRoom(PhotonNetwork.NickName + "'s Room", options, null);
        
        Room.SetActive(true);
        Join_Create.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        Room.SetActive(true);
        RoomList.SetActive(false);
        startBtn.SetActive(PhotonNetwork.IsMasterClient && CheckAllReady());
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(PlayerListEntryPrefab);
            entry.transform.SetParent(PlayerListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<PlayerEntries>().Initialize(p.ActorNumber, p.NickName);



            playerListEntries.Add(p.ActorNumber, entry);
        }
        startBtn.gameObject.SetActive(CheckAllReady());
    }
    public override void OnLeftRoom()
    {
        foreach (GameObject entry in playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        startBtn.gameObject.SetActive(CheckAllReady());
        playerListEntries.Clear();
        playerListEntries = null;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        GameObject entry = Instantiate(PlayerListEntryPrefab);
        entry.transform.SetParent(PlayerListContent.transform);
        entry.transform.localScale = Vector3.one;
        entry.GetComponent<PlayerEntries>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

        playerListEntries.Add(newPlayer.ActorNumber, entry);
        startBtn.gameObject.SetActive(CheckAllReady());
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
        playerListEntries.Remove(otherPlayer.ActorNumber);
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        startBtn.gameObject.SetActive(CheckAllReady());
    }

    public override void OnJoinedLobby()
    {
        //Debug.Log("kms");
    }
    public void joincreateBack()
    {
        AudioManager.Play("Click");
        isConnecting = false;
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        PhotonNetwork.NetworkingClient.State = ClientState.Disconnected;
        mainMenu.SetActive(true);
        Join_Create.SetActive(false);
    }
    #endregion


    #region RoomList
    public void roomListBack()
    {
        AudioManager.Play("Click");
        isConnecting = false;
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        PhotonNetwork.NetworkingClient.State = ClientState.Disconnected;
        mainMenu.SetActive(true);
        RoomList.SetActive(false);
    }
    #endregion

    #region room

    public void check()
    {
        startBtn.gameObject.SetActive(CheckAllReady());
    }

    private bool CheckAllReady()
    {
        if (!PhotonNetwork.IsMasterClient) return false;

        foreach(Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            object isReady;
            if (player.CustomProperties.TryGetValue("PlayerReady", out isReady))
            {
                if (!(bool)isReady)
                {
                    return false;
                }
            }
            else
            {               
                return false;
            }

        }
        //Debug.Log("All ready");
        return true;
    }

    public void roomBack()
    {
        AudioManager.Play("Click");
        isConnecting = false;
        PhotonNetwork.LeaveRoom();
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        PhotonNetwork.NetworkingClient.State = ClientState.Disconnected;
        mainMenu.SetActive(true);
        Room.SetActive(false);
    }
    public void startGame()
    {
        AudioManager.Play("Click");
        PhotonNetwork.OfflineMode = false;
        PlayerPrefs.SetInt("scene", SceneManager.GetActiveScene().buildIndex + 2);
        PhotonNetwork.LoadLevel("LoadingScreen");
    }
    #endregion


    private void Update()
    {
       // Debug.Log(cachedRoomList.Count + " " + roomListEntries.Count);

    }

    private void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        roomListEntries.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }

            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    private void UpdateRoomListView()
    {
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = Instantiate(RoomListEntryPrefab);
            entry.transform.SetParent(RoomListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<RoomEntries>().Initialize(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

            roomListEntries.Add(info.Name, entry);
        }
    }


    IEnumerator JoinOrCreate()
    {
        yield return new WaitForSeconds(1.5f);
        label.GetComponent<Text>().text = "Connecting to Server...";
        label.SetActive(false);
        Join_Create.SetActive(true);
        namePhase.SetActive(false);
    }
}

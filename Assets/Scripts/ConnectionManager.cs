using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Security.Permissions;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    private Dictionary<string, RoomInfo> cachedRoomList = new();

    public static ConnectionManager instance;

    public static readonly string TOP_DUNGEON_DEFEATED = "t";
    public static readonly string BOTTOM_DUNGEON_DEFEATED = "b";
    public static readonly string BOSS_DEFEATED = "g";

    void Awake()
    {
        if (instance == null) instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Connect to Photon
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.InLobby && PhotonNetwork.NetworkClientState != ClientState.JoiningLobby)
            PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        cachedRoomList.Clear();
    }

    public void CreateRoom(string roomName)
    {
        if (!PhotonNetwork.InLobby)
        {
            if (MenuUIManager.instance != null)
                MenuUIManager.instance.ShowErrorDialog("Create Room Failed", "You are not connected to the server!");
            return;
        }
        RoomOptions roomOptions = new()
        {
            MaxPlayers = 4,
            PlayerTtl = 60000,
        };
        ExitGames.Client.Photon.Hashtable props = new()
        {
            { TOP_DUNGEON_DEFEATED, false },
            { BOTTOM_DUNGEON_DEFEATED, false },
            { BOSS_DEFEATED, false },
        };
        roomOptions.CustomRoomProperties = props;
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
        PhotonNetwork.LoadLevel("GameScene MP");
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (MenuUIManager.instance != null)
            MenuUIManager.instance.ShowErrorDialog("Failed to join", message);
        Debug.LogErrorFormat("Room creation failed with error code {0} and error message {1}", returnCode, message);
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }
        if (MenuUIManager.instance != null)
            MenuUIManager.instance.ListAllRooms(cachedRoomList);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
    }
    public override void OnLeftLobby()
    {
        cachedRoomList.Clear();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (SceneManager.GetActiveScene().name == "GameScene MP")
        {
            SceneManager.LoadScene("Menu");
        }
        if (cause != DisconnectCause.DisconnectByClientLogic)
        {
            if (PhotonNetwork.InLobby && PlayerPrefs.HasKey("RoomName"))
                PhotonNetwork.RejoinRoom(PlayerPrefs.GetString("RoomName"));
        }
        if (MenuUIManager.instance != null)
        {
            var errMessage = cause.ToString();
            if (cause == DisconnectCause.DnsExceptionOnConnect)
                errMessage = "You are not connected to the server!";
            MenuUIManager.instance.ShowErrorDialog("Disconnected", errMessage);
        }

        StartCoroutine(ReconnectWithCooldown(5));

        cachedRoomList.Clear();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (MenuUIManager.instance != null)
            MenuUIManager.instance.ShowErrorDialog("Create Room Failed", "Cause: " + message + " (" + returnCode + ")");
    }

    public IEnumerator ReconnectWithCooldown(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        PhotonNetwork.ConnectUsingSettings();
    }
}

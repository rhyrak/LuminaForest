using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;
using System.Collections;
using UI;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    private readonly Dictionary<string, RoomInfo> _cachedRoomList = new();

    public static ConnectionManager Instance;

    public static readonly string TOP_DUNGEON_DEFEATED = "t";
    public static readonly string BOTTOM_DUNGEON_DEFEATED = "b";
    public static readonly string BOSS_DEFEATED = "g";

    public void Awake()
    {
        if (Instance == null) Instance = this;
        DontDestroyOnLoad(this.gameObject);
        PhotonNetwork.AutomaticallySyncScene = true;
        
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
        _cachedRoomList.Clear();
    }

    public void CreateRoom(string roomName)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            if (MenuUIManager.Instance != null)
                MenuUIManager.Instance.ShowErrorDialog("Create Room Failed", "You are not connected to the server!");
            return;
        }
        RoomOptions roomOptions = new()
        {
            MaxPlayers = 4,
            PlayerTtl = 1000,
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
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (MenuUIManager.Instance != null)
            MenuUIManager.Instance.ShowErrorDialog("Failed to join", message);
        Debug.LogErrorFormat("Room creation failed with error code {0} and error message {1}", returnCode, message);
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (var info in roomList)
        {
            if (info.RemovedFromList)
                _cachedRoomList.Remove(info.Name);
            else
                _cachedRoomList[info.Name] = info;
        }

        if (MenuUIManager.Instance != null)
            MenuUIManager.Instance.ListAllRooms(_cachedRoomList);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
    }
    
    public override void OnLeftLobby()
    {
        _cachedRoomList.Clear();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (SceneManager.GetActiveScene().name == "GameScene MP")
        {
            SceneManager.LoadScene("Menu");
        }
        if (cause != DisconnectCause.DisconnectByClientLogic)
        {
            PhotonNetwork.ReconnectAndRejoin();
        }
        if (MenuUIManager.Instance != null)
        {
            var errMessage = cause.ToString();
            if (cause == DisconnectCause.DnsExceptionOnConnect)
                errMessage = "You are not connected to the server!";
            MenuUIManager.Instance.ShowErrorDialog("Disconnected", errMessage);
        }

        StartCoroutine(ReconnectWithCooldown(5));

        _cachedRoomList.Clear();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (MenuUIManager.Instance != null)
            MenuUIManager.Instance.ShowErrorDialog("Create Room Failed", "Cause: " + message + " (" + returnCode + ")");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created room");
    }

    private IEnumerator ReconnectWithCooldown(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        PhotonNetwork.ConnectUsingSettings();
    }
}

using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    private Dictionary<string, RoomInfo> cachedRoomList = new();

    public static ConnectionManager instance;

    public static readonly string TOP_DUNGEON_DEFEATED = "t";
    public static readonly string BOTTOM_DUNGEON_DEFEATED = "b";
    public static readonly string BOSS_DEFEATED = "g";

    void Start()
    {
        if (instance == null) instance = this;

        // Connect to Photon
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        cachedRoomList.Clear();
    }

    public void CreateRoom(string roomName)
    {
        RoomOptions roomOptions = new()
        {
            MaxPlayers = 4
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
        cachedRoomList.Clear();
    }
}

using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMP_InputField UserNameInputField;
    [SerializeField]
    private TMP_InputField RoomNameInputField;
    [SerializeField]
    private RectTransform RoomList;
    [SerializeField]
    private GameObject RoomTilePrefab;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    public static ConnectionManager instance;

    public static readonly string TOP_DUNGEON_DEFEATED = "t";
    public static readonly string BOTTOM_DUNGEON_DEFEATED = "b";
    public static readonly string BOSS_DEFEATED = "g";

    void Start()
    {
        if (instance == null) instance = this;
        // Check if username and room name are saved in PlayerPrefs
        if (PlayerPrefs.HasKey("Username"))
        {
            UserNameInputField.text = PlayerPrefs.GetString("Username");
        }

        if (PlayerPrefs.HasKey("RoomName"))
        {
            RoomNameInputField.text = PlayerPrefs.GetString("RoomName");
        }

        // Connect to Photon
        PhotonNetwork.ConnectUsingSettings();
    }

    public void SaveUsernameAndRoomName(string roomName)
    {
        // Save the username and room name locally using PlayerPrefs
        string username = UserNameInputField.text;

        // Check for empty username, if empty, set a random name
        SetPlayerName(username);

        // Check for empty room name, if empty, set a default name
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room_" + Random.Range(1000, 9999).ToString();
        }

        // Save the non-empty values
        PlayerPrefs.SetString("Username", PhotonNetwork.NickName); // Save the actual Photon username
        PlayerPrefs.SetString("RoomName", roomName);
        PlayerPrefs.Save();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        cachedRoomList.Clear();
    }

    public void CreateRoom()
    {
        if (PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.JoinedLobby)
        {
            Debug.LogError("Cannot create room. Client is not in the lobby or ready for matchmaking. Wait for OnJoinedLobby.");
            return;
        }

        SaveUsernameAndRoomName(RoomNameInputField.text);  // Save before creating the room

        Photon.Realtime.RoomOptions roomOptions = new();
        roomOptions.MaxPlayers = 4;
        ExitGames.Client.Photon.Hashtable props = new()
        {
            { TOP_DUNGEON_DEFEATED, false },
            { BOTTOM_DUNGEON_DEFEATED, false },
            { BOSS_DEFEATED, false },
        };
        roomOptions.CustomRoomProperties = props;
        PhotonNetwork.CreateRoom(RoomNameInputField.text, roomOptions);
    }

    public void JoinRoom(string roomName)
    {
        SaveUsernameAndRoomName(roomName);  // Save the username and room name locally
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
        foreach (Transform child in RoomList)
        {
            GameObject.Destroy(child.gameObject);
        }
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
        foreach (var room in cachedRoomList)
        {
            var tile = Instantiate(RoomTilePrefab);
            tile.GetComponent<RoomTile>().SetValues(room.Value);
            tile.transform.parent = RoomList;
            tile.transform.localScale = new Vector3(1, 1, 1);
        }
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

    public void SetPlayerName(string playerName)
    {
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.NickName = playerName;
        }
        else
        {
            PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);
        }
    }
}

using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField RoomNameInputField;
    [SerializeField] private TMP_InputField UserNameInputField;

    public static readonly string TOP_DUNGEON_DEFEATED = "t";
    public static readonly string BOTTOM_DUNGEON_DEFEATED = "b";
    public static readonly string BOSS_DEFEATED = "g";

    void Start()
    {
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

    public void SaveUsernameAndRoomName()
    {
        // Save the username and room name locally using PlayerPrefs
        string username = UserNameInputField.text;
        string roomName = RoomNameInputField.text;

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
    }

    public void CreateRoom()
    {
        SaveUsernameAndRoomName();  // Save before creating the room

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

    public void JoinRoom()
    {
        SaveUsernameAndRoomName();  // Save before joining the room
        PhotonNetwork.JoinRoom(RoomNameInputField.text);
    }


    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room");
        PhotonNetwork.LoadLevel("GameScene MP");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogErrorFormat("Room creation failed with error code {0} and error message {1}", returnCode, message);
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

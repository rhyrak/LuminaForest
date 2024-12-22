using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMP_InputField RoomNameInputField;

    public static readonly string TOP_DUNGEON_DEFEATED = "t";
    public static readonly string BOTTOM_DUNGEON_DEFEATED = "b";
    public static readonly string BOSS_DEFEATED = "g";

    public void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
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
}

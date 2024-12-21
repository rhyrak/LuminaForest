using UnityEngine;
using Photon.Pun;
using TMPro;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMP_InputField RoomNameInputField;

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
        PhotonNetwork.CreateRoom(RoomNameInputField.text);
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

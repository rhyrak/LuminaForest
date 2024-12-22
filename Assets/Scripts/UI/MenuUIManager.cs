using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class MenuUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField UserNameInputField;
    [SerializeField] private TMP_InputField RoomNameInputField;
    [SerializeField] private TMP_Text ConnectionInfo;
    [SerializeField] private RectTransform RoomList;
    [SerializeField] private GameObject RoomTilePrefab;
    [SerializeField] private GameObject MultiplayerWindow;

    private int updateConnectionInfoTimer = 0;

    public static MenuUIManager instance;

    public void Start()
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
    }

    public void FixedUpdate()
    {
        if (updateConnectionInfoTimer > 0)
        {
            updateConnectionInfoTimer--;
            return;
        }
        if (PhotonNetwork.InLobby)
        {
            ConnectionInfo.text = "Connected! Ping: " + PhotonNetwork.GetPing();
        }
        else
        {
            ConnectionInfo.text = "Connecting...";
        }
        updateConnectionInfoTimer++;
    }

    public void SaveUsernameAndRoomName()
    {
        // Save the username and room name locally using PlayerPrefs
        string username = UserNameInputField.text;
        string roomName = RoomNameInputField.text;

        // Check for empty username, if empty, set a random name
        if (!string.IsNullOrEmpty(username))
            PhotonNetwork.NickName = username;
        else
            PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);

        // Check for empty room name, if empty, set a default name
        if (string.IsNullOrEmpty(roomName))
            roomName = "Room_" + Random.Range(1000, 9999).ToString();

        // Save the non-empty values
        PlayerPrefs.SetString("Username", PhotonNetwork.NickName); // Save the actual Photon username
        PlayerPrefs.SetString("RoomName", roomName);
        PlayerPrefs.Save();
    }

    public void CreateRoom()
    {
        var roomName = RoomNameInputField.text;
        SaveUsernameAndRoomName();
        ConnectionManager.instance.CreateRoom(roomName);
    }

    public void JoinRoom(string roomName)
    {
        SaveUsernameAndRoomName();  // Save before joining the room
        ConnectionManager.instance.JoinRoom(roomName);
    }

    public void ListAllRooms(Dictionary<string, RoomInfo> rooms)
    {
        foreach (Transform child in RoomList)
        {
            GameObject.Destroy(child.gameObject);
        }
        int listHeight = 60;
        // TODO: Delete me
        // for (int i = 0; i < 2; i++)
        // {
        //     listHeight += 110;
        //     var dummyTile = Instantiate(RoomTilePrefab);
        //     dummyTile.transform.parent = RoomList;
        //     dummyTile.transform.localScale = new Vector3(1, 1, 1);
        // }
        foreach (var room in rooms)
        {
            listHeight += 110;
            var tile = Instantiate(RoomTilePrefab);
            tile.GetComponent<RoomTile>().SetValues(room.Value);
            tile.transform.parent = RoomList;
            tile.transform.localScale = new Vector3(1, 1, 1);
        }
        RoomList.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, listHeight);
    }

    public void ToggleMultiplayerWindow()
    {
        MultiplayerWindow.SetActive(!MultiplayerWindow.activeInHierarchy);
    }

    public void Quit()
    {
        Application.Quit();
    }
}

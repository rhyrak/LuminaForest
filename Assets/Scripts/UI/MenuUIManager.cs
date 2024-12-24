using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    public class MenuUIManager : MonoBehaviour
    {
        // Window elements for in lobby state
        [SerializeField] private TMP_InputField userNameInputField;
        [SerializeField] private TMP_InputField roomNameInputField;
        [SerializeField] private TMP_Text connectionInfo;
        [SerializeField] private RectTransform roomList;
        [SerializeField] private GameObject roomTilePrefab;
        [SerializeField] private GameObject multiplayerWindow;
        // Window elements for in room state
        [SerializeField] private GameObject roomLobbyWindow;
        [SerializeField] private TMP_Text roomName;
        [SerializeField] private TMP_Text roomInfo;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button startButton;
        [SerializeField] private RectTransform playerList;
        [SerializeField] private GameObject playerTilePrefab;
        // Error dialog elements
        [SerializeField] private GameObject errorDialog;
        [SerializeField] private TMP_Text errorTitle;
        [SerializeField] private TMP_Text errorText;

        private int _updateConnectionInfoTimer;
        private bool _showWindow;
        
        public static MenuUIManager Instance;

        public void Start()
        {
            if (Instance == null) Instance = this;

            // Check if username and room name are saved in PlayerPrefs
            if (PlayerPrefs.HasKey("Username"))
            {
                userNameInputField.text = PlayerPrefs.GetString("Username");
            }

            if (PlayerPrefs.HasKey("RoomName"))
            {
                roomNameInputField.text = PlayerPrefs.GetString("RoomName");
            }
            
            startButton.onClick.AddListener(() =>
            {
                if(PhotonNetwork.IsMasterClient)
                    PhotonNetwork.LoadLevel("GameScene MP");
                PhotonNetwork.CurrentRoom.IsOpen = false;
            });
            
            leaveButton.onClick.AddListener(() =>
            {
                PhotonNetwork.LeaveRoom(false);
            });
        }

        public void FixedUpdate()
        {
            if (_showWindow)
            {
                if (PhotonNetwork.InRoom)
                {
                    UpdateInRoomWindow();
                }
                else
                {
                    UpdateInLobbyWindow();
                }
            }
        }

        private void UpdateInLobbyWindow()
        {
            roomLobbyWindow.SetActive(false);
            multiplayerWindow.SetActive(true);
            UpdateConnectionInfo();
        }

        private void UpdateInRoomWindow()
        {
            multiplayerWindow.SetActive(false);
            roomLobbyWindow.SetActive(true);
            var isMaster = PhotonNetwork.IsMasterClient;
            startButton.enabled = isMaster;
            roomName.text = PhotonNetwork.CurrentRoom.Name;
            roomInfo.text = isMaster ? "You are the room owner. Start the game whenever you want."
                : "Waiting for the room owner to start the game.";
            
            foreach (Transform child in playerList)
            {
                GameObject.Destroy(child.gameObject);
            }
            var listHeight = 60;
            var players = PhotonNetwork.PlayerList;
            foreach (var player in players)
            {
                listHeight += 110;
                var playerType = player.IsMasterClient ? "Master" : "Client";
                var tile = Instantiate(playerTilePrefab, playerList, true);
                tile.GetComponent<PlayerTile>().SetValues(player.NickName, playerType);
                tile.transform.localScale = new Vector3(1, 1, 1);
            }
            playerList.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, listHeight);
        }

        private void UpdateConnectionInfo()
        {
            if (_updateConnectionInfoTimer > 0)
            {
                _updateConnectionInfoTimer--;
                return;
            }
            if (PhotonNetwork.InLobby)
            {
                connectionInfo.text = "Connected! Ping: " + PhotonNetwork.GetPing();
            }
            else
            {
                connectionInfo.text = "Connecting...";
            }
            _updateConnectionInfoTimer++;
        }

        private void SaveUsernameAndRoomName()
        {
            // Save the username and room name locally using PlayerPrefs
            var username = userNameInputField.text;
            var roomName = roomNameInputField.text;

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
            var roomName = roomNameInputField.text;
            SaveUsernameAndRoomName();
            ConnectionManager.Instance.CreateRoom(roomName);
        }

        public void JoinRoom(string roomName)
        {
            SaveUsernameAndRoomName();  // Save before joining the room
            ConnectionManager.Instance.JoinRoom(roomName);
        }

        public void ListAllRooms(Dictionary<string, RoomInfo> rooms)
        {
            foreach (Transform child in roomList)
            {
                GameObject.Destroy(child.gameObject);
            }
            var listHeight = 60;
            foreach (var room in rooms)
            {
                listHeight += 110;
                var tile = Instantiate(roomTilePrefab, roomList, true);
                tile.GetComponent<RoomTile>().SetValues(room.Value);
                tile.transform.localScale = new Vector3(1, 1, 1);
            }
            roomList.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, listHeight);
        }

        public void ShowErrorDialog(string errTitle, string errMessage)
        {
            errorTitle.text = errTitle;
            errorText.text = errMessage;
            errorDialog.SetActive(true);
        }

        public void CloseErrorDialog()
        {
            errorDialog.SetActive(false);
        }

        public void ToggleMultiplayerWindow()
        {
            _showWindow = !_showWindow;
            multiplayerWindow.SetActive(!multiplayerWindow.activeInHierarchy);
        }

        public void TransitionToTutorial()
        {
            SceneManager.LoadScene("GameScene");
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}

using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomTile : MonoBehaviour
{
    [SerializeField]
    private TMP_Text RoomName;
    [SerializeField]
    private TMP_Text PlayersInfo;
    [SerializeField]
    private Button JoinButton;

    public void SetValues(RoomInfo roomInfo)
    {
        RoomName.text = roomInfo.Name;
        PlayersInfo.text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
        JoinButton.onClick.AddListener(() =>
        {
            ConnectionManager.instance.JoinRoom(roomInfo.Name);
        });
        if (roomInfo.PlayerCount == roomInfo.MaxPlayers)
        {
            JoinButton.enabled = false;
        }
    }
}

using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RoomTile : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text roomName;
        [SerializeField]
        private TMP_Text playersInfo;
        [SerializeField]
        private Button joinButton;

        public void SetValues(RoomInfo roomInfo)
        {
            roomName.text = roomInfo.Name;
            if(roomInfo.IsOpen)
                playersInfo.text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
            else
                playersInfo.text = "Started";
            joinButton.onClick.AddListener(() =>
            {
                MenuUIManager.Instance.JoinRoom(roomInfo.Name);
            });
            if (roomInfo.PlayerCount == roomInfo.MaxPlayers)
            {
                joinButton.enabled = false;
            }
        }
    }
}

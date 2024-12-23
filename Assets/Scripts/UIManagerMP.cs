using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManagerMP : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectives;
    [SerializeField] private GameObject InfoPanel;
    [SerializeField] private RectTransform InfoPanelContent;
    [SerializeField] private GameObject InfoPanelTile;
    [SerializeField] private TMP_Text InfoPanelRoomName;

    public void Start()
    {
        InfoPanelRoomName.text = PhotonNetwork.CurrentRoom.Name;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            InfoPanel.SetActive(!InfoPanel.activeInHierarchy);
    }

    public void FixedUpdate()
    {
        UpdateObjectives();
        if (InfoPanel.activeInHierarchy)
            UpdateInfoPanel();
    }

    private void UpdateObjectives()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            objectives.text = "You are disconnected!";
            return;
        }
        var top = PhotonNetwork.CurrentRoom.CustomProperties[ConnectionManager.TOP_DUNGEON_DEFEATED];
        var bot = PhotonNetwork.CurrentRoom.CustomProperties[ConnectionManager.BOTTOM_DUNGEON_DEFEATED];
        var boss = PhotonNetwork.CurrentRoom.CustomProperties[ConnectionManager.BOSS_DEFEATED];

        objectives.text = "Defeat North Dungeon: ";
        if (top != null && (bool)top)
            objectives.text += "Completed\n";
        else
            objectives.text += "...\n";

        objectives.text += "Defeat South Dungeon: ";
        if (bot != null && (bool)bot)
            objectives.text += "Completed\n";
        else
            objectives.text += "...\n";

        objectives.text += "Defeat Boss: ";
        if (boss != null && (bool)boss)
            objectives.text += "Completed";
        else
            objectives.text += "...";
    }

    private void UpdateInfoPanel()
    {
        foreach (Transform child in InfoPanelContent)
        {
            GameObject.Destroy(child.gameObject);
        }
        var players = FindObjectsOfType<PlayerController>();
        List<InfoPanelTile.InfoTileData> playerData = new();
        foreach (var player in players)
        {
            InfoPanelTile.InfoTileData data = new()
            {
                ping = -1,
                nickname = player.Nickname,
                score = -1,
            };
            data.score = player.Score;
            data.ping = player.Ping;
            playerData.Add(data);
        }

        playerData.Sort((x, y) => y.score - x.score);
        foreach (var values in playerData)
        {
            var tile = Instantiate(InfoPanelTile);
            tile.GetComponent<InfoPanelTile>().SetValues(values);
            tile.transform.SetParent(InfoPanelContent);
            tile.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Menu");
    }
}

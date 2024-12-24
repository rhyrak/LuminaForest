using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManagerMP : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectives;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private RectTransform infoPanelContent;
    [SerializeField] private GameObject infoPanelTile;
    [SerializeField] private TMP_Text infoPanelRoomName;

    public void Start()
    {
        infoPanelRoomName.text = PhotonNetwork.CurrentRoom.Name;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            infoPanel.SetActive(!infoPanel.activeInHierarchy);
    }

    public void FixedUpdate()
    {
        UpdateObjectives();
        if (infoPanel.activeInHierarchy)
            UpdateinfoPanel();
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

    private void UpdateinfoPanel()
    {
        foreach (Transform child in infoPanelContent)
        {
            GameObject.Destroy(child.gameObject);
        }
        var players = FindObjectsOfType<PlayerController>();
        List<InfoPanelTile.InfoTileData> playerData = new();
        foreach (var player in players)
        {
            InfoPanelTile.InfoTileData data = new()
            {
                Ping = player.Ping,
                Nickname = player.Nickname,
                DeathCount = player.DeathCount,
                Score = player.Score,
            };
            playerData.Add(data);
        }

        playerData.Sort((x, y) => y.Score - x.Score);
        foreach (var values in playerData)
        {
            var tile = Instantiate(infoPanelTile, infoPanelContent, true);
            tile.GetComponent<InfoPanelTile>().SetValues(values);
            tile.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Menu");
    }
}

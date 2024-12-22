using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoPanelTile : MonoBehaviour
{
    [SerializeField]
    private TMP_Text Ping;
    [SerializeField]
    private TMP_Text Nickname;
    [SerializeField]
    private TMP_Text Score;

    public void SetValues(InfoTileData data)
    {
        Ping.text = "" + data.ping;
        Nickname.text = data.nickname;
        Score.text = "" + data.score;
    }

    public struct InfoTileData
    {
        public int ping;
        public string nickname;
        public int score;
    }
}

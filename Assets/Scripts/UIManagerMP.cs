using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManagerMP : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectives;

    void FixedUpdate()
    {
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
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private TextMeshProUGUI subtitle;
    [SerializeField] private RectTransform energyLevel;
    [SerializeField] private GameObject energyBar;

    private PlayerController playerController;
    private bool tutorialCompleted = false;

    private string[] tutorialTexts = new[] {
        "Use A and D to move left and right. Hold Shift to sprint. Press Space to jump.",
        "As you explore, you'll come across glowing blue plants. Stand close to these plants to regenerate your energy. Energy is vital for using abilities, so keep an eye out for these helpful plants!",
        "Click the Left Mouse Button to perform a quick dash in the direction you're facing. This move is perfect for killing slimes or quickly covering distance."
        };

    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        subtitle.text = tutorialTexts[0];
    }

    void Update()
    {
        Tutorial();
        var level = 280f * (playerController.CurrentEnergy / playerController.MaxEnergy);
        energyLevel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, level);
    }

    private void Tutorial()
    {
        if (tutorialCompleted)
            return;

        if (player.transform.position.x <= -4f)
        {
            subtitle.text = tutorialTexts[0];
        }
        else
        if (player.transform.position.x >= 0 && player.transform.position.x <= 5.5f)
        {
            subtitle.text = tutorialTexts[1];
            energyBar.SetActive(true);
        }
        else
        if (player.transform.position.x >= 5.5f && player.transform.position.x <= 12f)
        {
            subtitle.text = tutorialTexts[2];
        }
        else
        {
            subtitle.text = "";
        }
        if (player.transform.position.y <= -4f)
        {
            subtitle.text = "";
            tutorialCompleted = true;
        }
    }
}

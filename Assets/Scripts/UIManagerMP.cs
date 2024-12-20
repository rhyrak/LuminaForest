using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManagerMP : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private TextMeshProUGUI subtitle;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private RectTransform energyLevel;
    [SerializeField] private GameObject energyBar;

    private PlayerController playerController;

    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        energyBar.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
        if (playerController == null)
        {
            GameOver("You Died!");
            return;
        }

        var level = 280f * (playerController.CurrentEnergy / playerController.MaxEnergy);
        energyLevel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, level);

        if (playerController.IsBossKilled)
            GameOver("You Won!");
    }


    public void GameOver(string gameOverMessage)
    {

        gameOverText.text = gameOverMessage;
        subtitle.text = "Press R to play again.";
        energyBar.SetActive(false);
    }
}

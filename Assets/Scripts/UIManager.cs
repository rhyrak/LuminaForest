using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private TextMeshProUGUI subtitle;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private RectTransform energyLevel;
    [SerializeField] private GameObject energyBar;

    private TutPlayerController _playerController;
    private bool _tutorialCompleted = false;

    private readonly string[] _tutorialTexts = new[] {
        "Use A and D to move left and right. Hold Shift to sprint. Press Space to jump.",
        "As you explore, you'll come across glowing blue plants. Stand close to these plants to regenerate your energy. Energy is vital for using abilities, so keep an eye out for these helpful plants!",
        "Click the Left Mouse Button to perform a quick dash in the direction you're facing. This move is perfect for killing slimes or quickly covering distance."
        };

    public void Start()
    {
        _playerController = player.GetComponent<TutPlayerController>();
        subtitle.text = _tutorialTexts[0];
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            var currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
        if (_playerController == null)
        {
            GameOver("You Lost!");
            return;
        }

        Tutorial();
        var level = 280f * (_playerController.CurrentEnergy / _playerController.MaxEnergy);
        energyLevel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, level);

        if (_playerController.IsBossKilled)
            GameOver("You Won!");
    }

    private void Tutorial()
    {
        if (_tutorialCompleted)
            return;

        switch (player.transform.position.x)
        {
            case <= -4f:
                subtitle.text = _tutorialTexts[0];
                break;
            case >= 0 and <= 5.5f:
                subtitle.text = _tutorialTexts[1];
                energyBar.SetActive(true);
                break;
            case >= 5.5f and <= 12f:
                subtitle.text = _tutorialTexts[2];
                break;
            default:
                subtitle.text = "";
                break;
        }
        if (player.transform.position.y <= -4f)
        {
            subtitle.text = "";
            _tutorialCompleted = true;
        }
    }

    private void GameOver(string gameOverMessage)
    {
        gameOverText.text = gameOverMessage;
        subtitle.text = "Press R to play again.";
        energyBar.SetActive(false);
    }

    public void GoBackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}

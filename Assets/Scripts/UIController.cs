using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public GameObject endGamePanel;
    public TetrisManager tetrisManager;

    public void UpdateScore()
    {
        scoreText.text = $"Score: {tetrisManager.score.ToString():n0}";
    }

    public void UpdateGameOver()
    {
        endGamePanel.SetActive(tetrisManager.gameOver);
    }

    public void PlayAgain()
    {
        tetrisManager.SetGameOver(false);
    }
}
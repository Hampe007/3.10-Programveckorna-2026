using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultScreenManager : MonoBehaviour
{
    [SerializeField] string rematchSceneName;
    [SerializeField] string mainMenuSceneName;
    [SerializeField] string setupSceneName;
    [SerializeField] TextMeshProUGUI winnerText;

    void Awake()
    {
        int winner = 0;
        GameResult result = FindFirstObjectByType<GameResult>();
        if (result != null)
        {
            winner = result.winner;
            Destroy(result.gameObject);
        }
        winnerText.text = "Player " + (winner + 1) + "\nVictory";
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
    public void GoToSetup()
    {
        SceneManager.LoadScene(setupSceneName);
    }
    public void Rematch()
    {
        SceneManager.LoadScene(rematchSceneName);
    }
}

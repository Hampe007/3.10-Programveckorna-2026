using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultScreenManager : MonoBehaviour
{
    [SerializeField] string rematchSceneName;
    [SerializeField] string mainMenuSceneName;
    [SerializeField] string setupSceneName;
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

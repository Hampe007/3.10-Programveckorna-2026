using Unity.VisualScripting;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject container;
    public static PauseManager instance;

    void Awake()
    {
        instance = this;
    }

    public void Pause()
    {
        container.SetActive(true);
        BattleTimeManager.instance.SetPause(true);
    }

    public void ResumeBotton()
    {
        container.SetActive(false);
        BattleTimeManager.instance.SetPause(false);
    }

    public void MainmenuButton()
    {
        BattleTimeManager.instance?.SetPause(false);
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}

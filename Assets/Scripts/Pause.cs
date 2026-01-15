using UnityEngine;

public class Pause : MonoBehaviour
{
    public GameObject container;
  
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            container.SetActive(true);
            BattleTimeManager.instance.SetPause(true);
        }
    }

    public void ResumeBotton()
    {
        container.SetActive(false);
        BattleTimeManager.instance.SetPause(false);
    }

    public void MainmenuButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}

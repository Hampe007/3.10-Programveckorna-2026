using UnityEngine;

public class Pause : MonoBehaviour
{
    public GameObject container;
  
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            container.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void ResumeBotton()
    {
        container.SetActive(false);
        Time.timeScale = 1f;
    }

    public void MainmenuButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}

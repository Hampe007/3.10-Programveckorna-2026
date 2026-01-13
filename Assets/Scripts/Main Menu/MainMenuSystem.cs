using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LocalGame.Session;
public class MainMenuSystem : MonoBehaviour
{
     
    public void Play()
    {
        var startLocalGame = Object.FindFirstObjectByType<StartLocalGame>();
        if (startLocalGame == null)
        {
            Debug.LogError("Could not find StartLocalGame in the scene");
            return;
        }

        startLocalGame.StartLocalGameFlow();
    }
    public void Quit()
    {
        Application.Quit();
        Debug.Log("Användaren har lämnat spelet");
    }
}

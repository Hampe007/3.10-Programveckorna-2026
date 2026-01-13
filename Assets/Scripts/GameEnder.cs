using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnder : MonoBehaviour
{
    [SerializeField] float resultScreenDelay;
    [SerializeField] string resultSceneName;
    [SerializeField] GameObject dataPrefab;
    int winner;
    bool countDown = false;
    float timeLeft;
    void Awake()
    {
        timeLeft = resultScreenDelay;
    }
    void Update()
    {
        if (CharacterTracker.instance.characters[0].health <= 0)
        {
            winner = 0;
            countDown = true;
        }
        else if(CharacterTracker.instance.characters[1].health <= 0)
        {
            winner = 1;
            countDown = true;
        }
        if (countDown)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                GameObject data = Instantiate(dataPrefab);
                DontDestroyOnLoad(data);
                data.GetComponent<GameResult>().winner = winner;
                SceneManager.LoadScene(resultSceneName);
            }
        }
    }
}

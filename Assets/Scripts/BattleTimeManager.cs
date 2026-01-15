using UnityEngine;

public class BattleTimeManager : MonoBehaviour
{
    bool paused = false;
    float hitStopLeft;
    public static BattleTimeManager instance;
    public void SetPause(bool paused)
    {
        this.paused = paused;
    }
    public void HitPause(float time)
    {
        if(time > hitStopLeft)
        {
            hitStopLeft = time;
        }
    }

    void Awake()
    {
        if (instance != null) //Ensures there is always only time manager, accessible through the static ScreenShake.Instance.
        {
            Destroy(gameObject);
            Debug.LogWarning("Destroyed a BattleTimeManager as there was already a BattleTimeManager instance");
        }
        else
        {
            instance = this;
        }
    }

    public void Update()
    {
        if(paused)
        {
            Time.timeScale = 0;
        }
        else
        {
            if(hitStopLeft > 0)
            {
                Time.timeScale = 0;
                hitStopLeft -= Time.unscaledDeltaTime;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
    }
}

using UnityEngine;

public class StarVisuals : MonoBehaviour
{
    float standardSize;
    public float growthFactor;
    public float timeElapsed;
    public float cycleTime;
    Vector2 minScale => Vector2.one * standardSize;
    Vector2 maxScale => Vector2.one * standardSize * growthFactor;

    void Update()
    {
        timeElapsed += Time.deltaTime;

        while (timeElapsed > cycleTime)
        {
            timeElapsed -= cycleTime;
        }

        Vector2 newScale = Vector2.zero;
        newScale.x = Mathf.Lerp(minScale.x, maxScale.x, TimeFractionToLerpFactor(timeElapsed / cycleTime));
        newScale.y = Mathf.Lerp(minScale.y, maxScale.y, TimeFractionToLerpFactor(timeElapsed / cycleTime));
        transform.localScale = newScale;
    }

    void Start()
    {
        standardSize = transform.localScale.x;
    }
    float TimeFractionToLerpFactor(float time)
    {
        if (time <= 0.5f)
        {
            return time * 2;
        }
        else
        {
            return 1 - 2 * (time - 0.5f);
        }
    }
}

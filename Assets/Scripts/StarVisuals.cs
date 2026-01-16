using UnityEngine;

public class StarVisuals : MonoBehaviour
{
    float standardSize;
    public float growthFactor;
    public float timeElapsed;
    public float cycleTime;
    Vector2 minScale => Vector2.one * standardSize;
    Vector2 maxScale => Vector2.one * standardSize * growthFactor;
    [Header("Drift")]
    [SerializeField] Vector2 driftAmplitudeRange = new Vector2(6f, 18f);
    [SerializeField] Vector2 driftSpeedRange = new Vector2(0.48f, 0.75f);
    RectTransform rectTransform;
    Vector2 baseAnchoredPosition;
    Vector2 driftAmplitude;
    Vector2 driftSpeed;
    float driftOffset;

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

        if (rectTransform != null && driftAmplitude.sqrMagnitude > 0f)
        {
            float t = Time.time + driftOffset;
            Vector2 drift = new Vector2(Mathf.Sin(t * driftSpeed.x), Mathf.Cos(t * driftSpeed.y));
            rectTransform.anchoredPosition = baseAnchoredPosition + Vector2.Scale(drift, driftAmplitude);
        }
    }

    void Start()
    {
        standardSize = transform.localScale.x;
        rectTransform = transform as RectTransform;
        if (rectTransform != null)
        {
            baseAnchoredPosition = rectTransform.anchoredPosition;
            driftOffset = Random.Range(0f, Mathf.PI * 2f);
            float amplitude = Random.Range(driftAmplitudeRange.x, driftAmplitudeRange.y);
            float speed = Random.Range(driftSpeedRange.x, driftSpeedRange.y);
            driftAmplitude = new Vector2(amplitude, amplitude * Random.Range(0.5f, 1f));
            driftSpeed = new Vector2(speed, speed * Random.Range(0.6f, 1.3f));
        }
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

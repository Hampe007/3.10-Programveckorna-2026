using UnityEngine;

public class StarGenerator : MonoBehaviour
{
    [SerializeField] GameObject star;

    [Header("Star spawning")]
    [SerializeField] float starBandSize;
    [SerializeField] float starBandWidth;
    [SerializeField] float starsPerWidthUnit;
    [SerializeField] float starConcentration;
    [SerializeField] float starBaseRate;

    [Header("Star appearance")]
    [SerializeField] float minScale;
    [SerializeField] float maxScale;
    [SerializeField] float minGrowthFactor;
    [SerializeField] float maxGrowthFactor;
    [SerializeField] float minCycleTime;
    [SerializeField] float maxCycleTime;
    void Awake()
    {
        GenerateStars();
    }

    void GenerateStars()
    {
        int starCount = (int)(starBandWidth * starsPerWidthUnit);
        for (int i = 0; i < starCount; i++)
        {
            GenerateStar();
        }
    }

    void GenerateStar()
    {
        GameObject newStar = Instantiate(star);
        newStar.transform.SetParent(transform, false);
        float xPos = Random.Range(-starBandWidth / 2, starBandWidth / 2);
        float height;
        if(Random.Range(0,1f) >= starBaseRate)
        {
            height = GetRandomStarHeight();
        }
        else
        {
            height = GetPureRandomStarHeight();
        }
        newStar.transform.localPosition = new Vector2(xPos, height * starBandSize);

        StarVisuals starScript = newStar.GetComponent<StarVisuals>();
        starScript.growthFactor = Random.Range(minGrowthFactor, maxGrowthFactor);
        starScript.cycleTime = Random.Range(minCycleTime, maxCycleTime);
        starScript.timeElapsed = Random.Range(0, 1);
        newStar.transform.rotation = Quaternion.Euler(0, 0, -transform.rotation.z);
        newStar.transform.localScale = Vector2.one * Random.Range(minScale, maxScale);
    }

    float GetRandomStarHeight()
    {
        if(Random.Range(0,2) == 1) // 50/50
        {
            return 1 - (Mathf.Pow(Random.Range(0f, 1f), starConcentration));
        }
        else
        {
            return - (1 - (Mathf.Pow(Random.Range(0f, 1f), starConcentration)));
        }
        
    }
    float GetPureRandomStarHeight()
    {
        return (Random.Range(-1f, 1f));
    }
}

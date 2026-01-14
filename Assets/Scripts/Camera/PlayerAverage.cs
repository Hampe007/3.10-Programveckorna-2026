using System.Collections.Generic;
using UnityEngine;

public class PlayerAverage : MonoBehaviour
{
    public List<Transform> targets = new List<Transform>();
    public void Initialize()
    {
        targets.Add(CharacterTracker.instance.characters[0].transform);
        targets.Add(CharacterTracker.instance.characters[1].transform);
    }

    void Update()
    {
        float avgX = 0;
        float avgY = 0;
        foreach (Transform target in targets)
        {
            avgX += target.position.x;
            avgY += target.position.y;
        }
        if (targets.Count > 0)
        {
            avgX /= targets.Count;
            avgY /= targets.Count;
        }
        transform.position = new Vector2(avgX, avgY);
    }
}

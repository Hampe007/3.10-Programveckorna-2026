using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class OneWayManager : MonoBehaviour
{
    List<OneWayPlatform> platforms = new();
    public static OneWayManager instance;
    void Awake()
    {
        if (instance != null) { Debug.LogWarning("Multiple OneWayManagers exist."); }
        instance = this;

        foreach (OneWayPlatform plat in FindObjectsByType<OneWayPlatform>(FindObjectsSortMode.None))
        {
            platforms.Add(plat);
        }
    }

    public void AddObject(GameObject gameObject)
    {
        foreach (OneWayPlatform plat in platforms)
        {
            plat.AddCollision(gameObject);
        }
    }

    public void RemoveObject(GameObject gameObject)
    {
        foreach (OneWayPlatform plat in platforms)
        {
            plat.RemoveCollision(gameObject);
        }
    }
}

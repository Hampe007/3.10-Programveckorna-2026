using System.Collections.Generic;
using UnityEngine;

public class OneWayManager : MonoBehaviour
{
    List<OneWayPlatform> platforms = new();
    public static OneWayManager instance;
    List<GameObject> pausedObjects = new();
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
        if(pausedObjects.Contains(gameObject))
        {
            pausedObjects.Remove(gameObject);
            return;
        }

        foreach (OneWayPlatform plat in platforms)
        {
            plat.RemoveCollision(gameObject);
        }
    }

    public void PauseObject(GameObject gameObject)
    {
        pausedObjects.Add(gameObject);
        foreach (OneWayPlatform plat in platforms)
        {
            plat.RemoveCollision(gameObject);
        }
    }

    public void UnPauseObject(GameObject gameObject)
    {
        pausedObjects.Remove(gameObject);
        foreach (OneWayPlatform plat in platforms)
        {
            plat.AddCollision(gameObject);
        }
    }
}

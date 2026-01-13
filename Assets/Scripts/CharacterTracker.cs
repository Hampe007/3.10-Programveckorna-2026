using System;
using UnityEngine;

public class CharacterTracker : MonoBehaviour
{
    public static CharacterTracker instance;
    public Character[] characters = new Character[2];
    void Awake()
    {
        if (instance != null) { Debug.LogWarning("Multiple CharacterTrackers exist."); }
        instance = this;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class CharacterCreationManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    List<OneWayPlatform> platforms = new();
    void Start()
    {
        foreach (OneWayPlatform plat in FindObjectsByType<OneWayPlatform>(FindObjectsSortMode.None))
        {
            platforms.Add(plat);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreatePlayers();
        }
    }

    void CreatePlayers()
    {
        for (int i = 0; i < 2; i++) //0 & 1
        {
            CreatePlayer(i, new Vector2(i * 3, 4));
        }
        foreach (ControllerSender controller in InputManager.instance.activeControllers)
        {
            if (controller != null)
            {
                controller.UpdateConnections();
            }
        }
    }

    void CreatePlayer(int index, Vector2 position)
    {
        GameObject newPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
        newPlayer.GetComponent<CharacterInputHandler>().playerIndex = index;
        newPlayer.GetComponent<Character>().playerIndex = index;
        foreach (OneWayPlatform plat in platforms)
        {
            plat.AddCollision(newPlayer);
        }
    }
}

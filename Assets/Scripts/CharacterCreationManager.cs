using LocalGame.Session;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCreationManager : MonoBehaviour
{
    [SerializeField] List<GameObject> playerPrefabs;
    List<OneWayPlatform> platforms = new();
    public Vector2 characterCreationPos;
    Vector2[] characterCreationPositions = new Vector2[2];
    void Awake()
    {
        characterCreationPositions[0] = characterCreationPos;
        characterCreationPositions[1] = characterCreationPos * Vector2.left;
        foreach (OneWayPlatform plat in FindObjectsByType<OneWayPlatform>(FindObjectsSortMode.None))
        {
            platforms.Add(plat);
        }
    }

    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreatePlayers();
        }
        */
    }

    public void CreatePlayers()
    {
        int[] characters = new int[2];
        characters[0] = GameSession.Instance.P1CharacterId;
        characters[1] = GameSession.Instance.P2CharacterId;
        for (int i = 0; i < 2; i++) //0 & 1
        {
            CreatePlayer(i, characterCreationPositions[i], characters[i]);
        }
        foreach (ControllerSender controller in InputManager.instance.activeControllers)
        {
            if (controller != null)
            {
                controller.UpdateConnections();
            }
        }
    }

    void CreatePlayer(int index, Vector2 position, int character)
    {
        GameObject newPlayer = Instantiate(playerPrefabs[character], position, Quaternion.identity);
        newPlayer.GetComponent<CharacterInputHandler>().playerIndex = index;
        newPlayer.GetComponent<Character>().playerIndex = index;
        OneWayManager.instance.AddObject(newPlayer);
        CharacterTracker.instance.characters[index] = newPlayer.GetComponent<Character>();
    }
}

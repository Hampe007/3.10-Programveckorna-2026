using System.Collections.Generic;
using UnityEngine;

public class CharacterCreationManager : MonoBehaviour
{
    [SerializeField] GameObject defaultPlayerPrefab;
    [SerializeField] Transform[] spawnPoints = new Transform[2];
    List<OneWayPlatform> platforms = new();

    void Start()
    {
        foreach (OneWayPlatform plat in FindObjectsByType<OneWayPlatform>(FindObjectsSortMode.None))
        {
            platforms.Add(plat);
        }

        if (MatchSetupRuntime.HasSelections)
        {
            SpawnFromSelections();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SpawnDebugPlayers();
        }
    }

    void SpawnFromSelections()
    {
        for (int i = 0; i < 2; i++)
        {
            MatchSetupRuntime.PlayerSelection selection = MatchSetupRuntime.GetSelection(i);
            CharacterDefinition definition = selection != null ? selection.character : null;
            GameObject prefab = definition != null && definition.FighterPrefab != null
                ? definition.FighterPrefab
                : defaultPlayerPrefab;
            Vector3 spawnPos = GetSpawnPosition(i);
            CreatePlayer(prefab, i, spawnPos);
        }
        NotifyControllers();
        MatchSetupRuntime.Clear();
    }

    void SpawnDebugPlayers()
    {
        for (int i = 0; i < 2; i++)
        {
            Vector3 spawnPos = GetSpawnPosition(i);
            CreatePlayer(defaultPlayerPrefab, i, spawnPos);
        }
        NotifyControllers();
    }

    void CreatePlayer(GameObject prefab, int index, Vector3 position)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Cannot create player, prefab missing.");
            return;
        }
        GameObject newPlayer = Instantiate(prefab, position, Quaternion.identity);
        CharacterInputHandler inputHandler = newPlayer.GetComponent<CharacterInputHandler>();
        if (inputHandler != null)
        {
            inputHandler.playerIndex = index;
        }
        Character character = newPlayer.GetComponent<Character>();
        if (character != null)
        {
            character.playerIndex = index;
        }
        foreach (OneWayPlatform plat in platforms)
        {
            plat.AddCollision(newPlayer);
        }
    }

    Vector3 GetSpawnPosition(int index)
    {
        if (spawnPoints != null && index < spawnPoints.Length && spawnPoints[index] != null)
        {
            return spawnPoints[index].position;
        }
        return new Vector3(index * 3f, 4f, 0f);
    }

    void NotifyControllers()
    {
        if (InputManager.instance == null)
        {
            return;
        }
        foreach (ControllerSender controller in InputManager.instance.activeControllers)
        {
            controller?.UpdateConnections();
        }
    }
}

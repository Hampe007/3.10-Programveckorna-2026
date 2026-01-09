using UnityEngine;
using UnityEngine.InputSystem;

public static class MatchSetupRuntime
{
    public class PlayerSelection
    {
        public string characterId;
        public CharacterDefinition character;
        public LobbyInputType inputType;
        public int gamepadDeviceId = -1;
    }

    static readonly PlayerSelection[] cachedSelections = new PlayerSelection[2];

    public static bool HasSelections =>
        cachedSelections[0]?.character != null && cachedSelections[1]?.character != null;

    public static void StoreSelection(int slotIndex, CharacterDefinition character, LobbyInputType inputType, Gamepad pad = null)
    {
        if (slotIndex < 0 || slotIndex >= cachedSelections.Length)
        {
            return;
        }

        cachedSelections[slotIndex] = new PlayerSelection
        {
            character = character,
            characterId = character != null ? character.CharacterId : string.Empty,
            inputType = inputType,
            gamepadDeviceId = pad != null ? pad.deviceId : -1
        };
    }

    public static PlayerSelection GetSelection(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= cachedSelections.Length)
        {
            return null;
        }
        return cachedSelections[slotIndex];
    }

    public static void Clear()
    {
        for (int i = 0; i < cachedSelections.Length; i++)
        {
            cachedSelections[i] = null;
        }
    }
}

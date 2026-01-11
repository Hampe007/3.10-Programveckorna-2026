using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

namespace LocalGame.Session
{
    #region Summary
    /// <summary>
    /// Persisted Session across scenes.
    /// Stores ONLY the final match payload:
    /// - P1 device identity
    /// - P2 device identity
    /// - P1CharacterId (0..15)
    /// - P2CharacterId (0..15)
    /// </summary>
    #endregion

    #region Code
    public sealed class GameSession : MonoBehaviour
    {
        private const string LogPrefix = "[GameSession]";
        private const string SessionObjectName = "Session";

        public static GameSession Instance { get; private set; } // EXPOSED (other scripts): global runtime session singleton

        [Serializable]
        public struct PlayerDeviceInfo
        {
            [Tooltip("InputSystem deviceId (stable during runtime). -1 means unassigned.")]
            public int deviceId; // EXPOSED (other scripts): device identifier used to resolve InputDevice

            [Tooltip("For UI/debug only (e.g. \"Xbox Controller\").")]
            public string deviceName; // EXPOSED (other scripts): friendly label for UI/debug

            [Tooltip("For UI/debug only: N in \"Gamepad N\" (connection order). 0 means unknown.")]
            public int gamepadIndex;  // EXPOSED (other scripts): UI-only "Gamepad N" index (runtime convenience)

            public bool IsAssigned => deviceId >= 0; // EXPOSED (other scripts): true if this player has a device claimed

            public static PlayerDeviceInfo Unassigned => new PlayerDeviceInfo
            {
                deviceId = -1,
                deviceName = string.Empty,
                gamepadIndex = 0
            }; // EXPOSED (other scripts): "no device" value
        }

        [Header("Controller Binding Data (final payload)")]
        [SerializeField] private PlayerDeviceInfo p1Device = PlayerDeviceInfo.Unassigned;
        [SerializeField] private PlayerDeviceInfo p2Device = PlayerDeviceInfo.Unassigned;

        [Header("Character Pick Data (final payload)")]
        [SerializeField, Range(0, 15)] private byte p1CharacterId = 0;
        [SerializeField, Range(0, 15)] private byte p2CharacterId = 0;

        public PlayerDeviceInfo P1Device => p1Device; // EXPOSED (other scripts): P1 claimed device payload (deviceId/name/index)
        public PlayerDeviceInfo P2Device => p2Device; // EXPOSED (other scripts): P2 claimed device payload (deviceId/name/index)

        public byte P1CharacterId => p1CharacterId; // EXPOSED (other scripts): P1 locked character ID (0..15)
        public byte P2CharacterId => p2CharacterId; // EXPOSED (other scripts): P2 locked character ID (0..15)

        public void ResetToDefaults()
        {
            p1Device = PlayerDeviceInfo.Unassigned;
            p2Device = PlayerDeviceInfo.Unassigned;
            p1CharacterId = 0;
            p2CharacterId = 0;
        } // EXPOSED (other scripts): resets session for a fresh setup flow

        public void SetP1Device(InputDevice device) => p1Device = BuildDeviceInfoSafe(device, "P1"); // EXPOSED: claim/assign P1 device
        public void SetP2Device(InputDevice device) => p2Device = BuildDeviceInfoSafe(device, "P2"); // EXPOSED: claim/assign P2 device

        public void ClearP1Device() => p1Device = PlayerDeviceInfo.Unassigned; // EXPOSED: unclaim P1 device
        public void ClearP2Device() => p2Device = PlayerDeviceInfo.Unassigned; // EXPOSED: unclaim P2 device

        public void SetP1CharacterId(byte id) => p1CharacterId = Clamp4Bit(id, "P1CharacterId"); // EXPOSED: set P1 character id (clamped 0..15)
        public void SetP2CharacterId(byte id) => p2CharacterId = Clamp4Bit(id, "P2CharacterId"); // EXPOSED: set P2 character id (clamped 0..15)

        public InputDevice ResolveDevice(PlayerDeviceInfo info)
        {
            if (!info.IsAssigned)
                return null;

            try
            {
                foreach (var d in InputSystem.devices)
                {
                    if (d != null && d.deviceId == info.deviceId)
                        return d;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} ResolveDevice failed: {ex.GetType().Name}: {ex.Message}", this);
            }

            return null;
        } // EXPOSED (other scripts): convert stored deviceId -> current InputDevice reference

        public static GameSession EnsureExists()
        {
            if (Instance != null)
                return Instance;

            var existing = FindFirstObjectByType<GameSession>();
            if (existing != null)
            {
                Instance = existing;
                Instance.gameObject.name = SessionObjectName;
                DontDestroyOnLoad(Instance.gameObject);
                return Instance;
            }

            var go = new GameObject(SessionObjectName);
            var session = go.AddComponent<GameSession>();
            return session;
        } // EXPOSED (other scripts): creates or finds the runtime Session singleton safely

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"{LogPrefix} Duplicate detected, destroying newest instance.", this);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            gameObject.name = SessionObjectName;
            DontDestroyOnLoad(gameObject);
        }

        private static byte Clamp4Bit(byte id, string fieldName)
        {
            if (id <= 15)
                return id;

            Debug.LogError($"{LogPrefix} {fieldName} out of range ({id}). Clamping to 15.");
            return 15;
        }

        private PlayerDeviceInfo BuildDeviceInfoSafe(InputDevice device, string label)
        {
            if (device == null)
            {
                Debug.LogError($"{LogPrefix} Tried to assign null device to {label}.", this);
                return PlayerDeviceInfo.Unassigned;
            }

            try
            {
                int gamepadIndex = 0;
                if (device is Gamepad gp)
                {
                    int idx = IndexOf(Gamepad.all, gp);
                    gamepadIndex = idx >= 0 ? idx + 1 : 0;
                }

                return new PlayerDeviceInfo
                {
                    deviceId = device.deviceId,
                    deviceName = device.displayName ?? device.name ?? "Unknown Device",
                    gamepadIndex = gamepadIndex
                };
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"{LogPrefix} Failed building device info for {label}: {ex.GetType().Name}: {ex.Message}",
                    this);
                return PlayerDeviceInfo.Unassigned;
            }
        }

        private static int IndexOf<T>(System.Collections.Generic.IReadOnlyList<T> list, T value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (Equals(list[i], value))
                    return i;
            }
            return -1;
        }
    }
#endregion

    #region  Documentation (More Detailed)
    /// <summary>
    /// Warning!
    /// 
    /// GPT Written documentation:
    /// 
    /// GameSession (DontDestroyOnLoad runtime singleton)
    /// -------------------------------------------------
    /// Purpose:
    ///   This component is the single persistent "session payload" that lives across scene loads.
    ///   It stores ONLY the information required to start the match/game scene:
    ///
    ///     - Which input device is Player 1 using?
    ///     - Which input device is Player 2 using?
    ///     - Which character ID did Player 1 lock in? (0..15, 4-bit)
    ///     - Which character ID did Player 2 lock in? (0..15, 4-bit)
    ///
    /// Why do we need this object?
    ///   In Unity, when you load a new scene, scene objects are destroyed by default.
    ///   We want selections from the setup flow (controller claim, character select) to persist into
    ///   the match scene. The standard Unity approach is a persistent object:
    ///
    ///     DontDestroyOnLoad(gameObject);
    ///
    /// Design constraints:
    ///   - Keep this payload minimal and "data-like" (no gameplay logic).
    ///   - The match scene reads this data once and uses it to:
    ///       (A) Resolve character IDs to prefabs via the roster database
    ///       (B) Bind correct device/input to each player's spawned character
    ///
    /// Important note about devices:
    ///   We store InputSystem device identity primarily via "deviceId".
    ///   deviceId is stable during the app runtime, but is NOT meant to persist across app restarts.
    ///   That's okay: this Session is only expected to live in memory during a single play session.
    ///
    /// Typical usage flow:
    ///   Scene 1 (Main Menu)
    ///     - GameSession.EnsureExists()
    ///     - GameSession.ResetToDefaults()
    ///     - Load Setup Scene
    ///
    ///   Scene 2 (Setup)
    ///     - Controller Claim menu calls SetP1Device / SetP2Device
    ///     - Character Select menu calls SetP1CharacterId / SetP2CharacterId
    ///
    ///   Scene 3 (Match/Game)
    ///     - Reads P1Device/P2Device and P1CharacterId/P2CharacterId
    ///     - Spawns prefabs and binds inputs
    ///
    /// Minimal API (what other scripts typically call):
    ///   - EnsureExists()
    ///   - ResetToDefaults()
    ///   - SetP1Device(device), SetP2Device(device)
    ///   - ClearP1Device(), ClearP2Device()
    ///   - SetP1CharacterId(id), SetP2CharacterId(id)
    ///   - ResolveDevice(PlayerDeviceInfo) => InputDevice
    ///
    /// Common pitfalls this avoids:
    ///   - Loading Setup Scene directly from editor: EnsureExists() recreates Session safely.
    ///   - Duplicate Session objects: Awake() enforces singleton and destroys duplicates.
    ///
    /// </summary>
    #endregion

}
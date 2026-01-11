using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

namespace LocalGame.Session
{
    /// <summary>
    /// Persisted Session across scenes.
    /// Stores ONLY the final match payload:
    /// - P1 device identity
    /// - P2 device identity
    /// - P1CharacterId (0..15)
    /// - P2CharacterId (0..15)
    /// </summary>
    public sealed class GameSession : MonoBehaviour
    {
        private const string LogPrefix = "[GameSession]";
        private const string SessionObjectName = "Session";

        public static GameSession Instance { get; private set; }

        [Serializable]
        public struct PlayerDeviceInfo
        {
            [Tooltip("InputSystem deviceId (stable during runtime). -1 means unassigned.")]
            public int deviceId;

            [Tooltip("For UI/debug only (e.g. \"Xbox Controller\").")]
            public string deviceName;

            [Tooltip("For UI/debug only: N in \"Gamepad N\" (connection order). 0 means unknown.")]
            public int gamepadIndex;

            public bool IsAssigned => deviceId >= 0;

            public static PlayerDeviceInfo Unassigned => new PlayerDeviceInfo
            {
                deviceId = -1,
                deviceName = string.Empty,
                gamepadIndex = 0
            };
        }

        [Header("Controller Binding Data (final payload)")]
        [SerializeField] private PlayerDeviceInfo p1Device = PlayerDeviceInfo.Unassigned;
        [SerializeField] private PlayerDeviceInfo p2Device = PlayerDeviceInfo.Unassigned;

        [Header("Character Pick Data (final payload)")]
        [SerializeField, Range(0, 15)] private byte p1CharacterId = 0;
        [SerializeField, Range(0, 15)] private byte p2CharacterId = 0;

        public PlayerDeviceInfo P1Device => p1Device;
        public PlayerDeviceInfo P2Device => p2Device;

        public byte P1CharacterId => p1CharacterId;
        public byte P2CharacterId => p2CharacterId;

        public void ResetToDefaults()
        {
            p1Device = PlayerDeviceInfo.Unassigned;
            p2Device = PlayerDeviceInfo.Unassigned;
            p1CharacterId = 0;
            p2CharacterId = 0;
        }

        public void SetP1Device(InputDevice device) => p1Device = BuildDeviceInfoSafe(device, "P1");
        public void SetP2Device(InputDevice device) => p2Device = BuildDeviceInfoSafe(device, "P2");

        public void ClearP1Device() => p1Device = PlayerDeviceInfo.Unassigned;
        public void ClearP2Device() => p2Device = PlayerDeviceInfo.Unassigned;

        public void SetP1CharacterId(byte id) => p1CharacterId = Clamp4Bit(id, "P1CharacterId");
        public void SetP2CharacterId(byte id) => p2CharacterId = Clamp4Bit(id, "P2CharacterId");

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
        }

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
        }

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
}
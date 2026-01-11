using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using LocalGame.Session;

namespace LocalGame.SetupScene
{
    /// <summary>
    /// Menu 2: Controls View (Ready-up)
    /// - Each player presses ANY digital button on their assigned device to become READY.
    /// - When both are READY -> advance to Character Select menu.
    ///
    /// Digital here means: face buttons, shoulders, start/select, stick buttons, dpad directions.
    /// (No stick movement axes, no triggers-as-axes.)
    /// </summary>
    public sealed class ControlsViewMenuController : MonoBehaviour
    {
        private const string LogPrefix = "[ControlsViewMenu]";

        [Header("Scene refs")]
        [SerializeField] private SetupSceneUIRoot uiRoot;

        [Header("P1 UI")]
        [SerializeField] private CanvasGroup p1PanelCanvasGroup;
        [SerializeField] private TMP_Text p1DeviceText;
        [SerializeField] private TMP_Text p1ReadyText;

        [Header("P2 UI")]
        [SerializeField] private CanvasGroup p2PanelCanvasGroup;
        [SerializeField] private TMP_Text p2DeviceText;
        [SerializeField] private TMP_Text p2ReadyText;

        [Header("Ready Visuals")]
        [SerializeField, Range(0.05f, 1f)] private float readyDimAlpha = 0.35f;
        [SerializeField] private string readyLabel = "READY";

        private GameSession _session;

        private bool _p1Ready;
        private bool _p2Ready;

        private Gamepad _p1Pad;
        private Gamepad _p2Pad;

        private void Awake()
        {
            try
            {
                _session = GameSession.EnsureExists();

                if (uiRoot == null)
                    Debug.LogError($"{LogPrefix} uiRoot is not assigned.", this);

                RefreshDeviceRefs();
                ResetReadyState();
                RefreshUI();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Awake failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void OnEnable()
        {
            try
            {
                _session = GameSession.EnsureExists();
                RefreshDeviceRefs();
                ResetReadyState();
                RefreshUI();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} OnEnable failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void Update()
        {
            try
            {
                // If devices got unplugged or changed, refresh best-effort.
                if (_p1Pad == null || _p2Pad == null)
                    RefreshDeviceRefs();

                if (!_p1Ready && _p1Pad != null && AnyDigitalPressedThisFrame(_p1Pad))
                {
                    _p1Ready = true;
                    uiRoot?.ShowToast("P1 READY");
                    RefreshUI();
                }

                if (!_p2Ready && _p2Pad != null && AnyDigitalPressedThisFrame(_p2Pad))
                {
                    _p2Ready = true;
                    uiRoot?.ShowToast("P2 READY");
                    RefreshUI();
                }

                if (_p1Ready && _p2Ready)
                {
                    uiRoot?.ActivateCharacterSelect();
                    gameObject.SetActive(false); // prevent double-advance
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Update failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void RefreshDeviceRefs()
        {
            try
            {
                _session ??= GameSession.EnsureExists();

                // Resolve stored device info back to an InputDevice, then cast to Gamepad.
                // (Controller claim menu currently only assigns gamepads, so this is expected.)
                var p1Device = _session.ResolveDevice(_session.P1Device);
                var p2Device = _session.ResolveDevice(_session.P2Device);

                _p1Pad = p1Device as Gamepad;
                _p2Pad = p2Device as Gamepad;

                if (_p1Pad == null && _session.P1Device.IsAssigned)
                    Debug.LogWarning($"{LogPrefix} P1 assigned device is not a Gamepad (or missing).", this);

                if (_p2Pad == null && _session.P2Device.IsAssigned)
                    Debug.LogWarning($"{LogPrefix} P2 assigned device is not a Gamepad (or missing).", this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} RefreshDeviceRefs failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void ResetReadyState()
        {
            _p1Ready = false;
            _p2Ready = false;
        }

        private void RefreshUI()
        {
            try
            {
                // Device labels
                if (p1DeviceText != null)
                {
                    p1DeviceText.text = _session.P1Device.IsAssigned
                        ? $"Gamepad {_session.P1Device.gamepadIndex} — {_session.P1Device.deviceName}"
                        : "Unassigned";
                }

                if (p2DeviceText != null)
                {
                    p2DeviceText.text = _session.P2Device.IsAssigned
                        ? $"Gamepad {_session.P2Device.gamepadIndex} — {_session.P2Device.deviceName}"
                        : "Unassigned";
                }

                // Ready labels
                if (p1ReadyText != null) p1ReadyText.text = _p1Ready ? readyLabel : string.Empty;
                if (p2ReadyText != null) p2ReadyText.text = _p2Ready ? readyLabel : string.Empty;

                // Dim panels when ready
                if (p1PanelCanvasGroup != null) p1PanelCanvasGroup.alpha = _p1Ready ? readyDimAlpha : 1f;
                if (p2PanelCanvasGroup != null) p2PanelCanvasGroup.alpha = _p2Ready ? readyDimAlpha : 1f;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} RefreshUI failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private static bool AnyDigitalPressedThisFrame(Gamepad pad)
        {
            // Face
            if (pad.buttonSouth.wasPressedThisFrame) return true;
            if (pad.buttonEast.wasPressedThisFrame) return true;
            if (pad.buttonWest.wasPressedThisFrame) return true;
            if (pad.buttonNorth.wasPressedThisFrame) return true;

            // D-pad
            if (pad.dpad.up.wasPressedThisFrame) return true;
            if (pad.dpad.down.wasPressedThisFrame) return true;
            if (pad.dpad.left.wasPressedThisFrame) return true;
            if (pad.dpad.right.wasPressedThisFrame) return true;

            // Shoulders
            if (pad.leftShoulder.wasPressedThisFrame) return true;
            if (pad.rightShoulder.wasPressedThisFrame) return true;

            // Menu buttons
            if (pad.startButton.wasPressedThisFrame) return true;
            if (pad.selectButton.wasPressedThisFrame) return true;

            // Stick buttons (digital)
            if (pad.leftStickButton.wasPressedThisFrame) return true;
            if (pad.rightStickButton.wasPressedThisFrame) return true;

            // NOTE: We intentionally do NOT count:
            // - stick movement (axes)
            // - triggers (they're analog in Input System)
            return false;
        }
    }
}
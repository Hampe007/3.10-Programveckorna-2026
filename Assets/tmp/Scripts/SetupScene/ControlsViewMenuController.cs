using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using LocalGame.Session;
using LocalGame.InputFx;

namespace LocalGame.SetupScene
{
    /// <summary>
    /// Menu 2: Controls View
    /// - Press any gameplay-relevant button to become READY.
    /// - When both ready -> advance to Character Select
    /// - B/Circle backs out to Controller Claim (only if nobody ready yet).
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

        [Serializable]
        private sealed class RumbleSettings
        {
            public bool enableRumble = true;

            [Tooltip("Pulse when a player becomes READY.")]
            [Range(0f, 1f)] public float rumbleReadyLow = 0.25f;
            [Range(0f, 1f)] public float rumbleReadyHigh = 0.55f;
            [Min(0.01f)] public float rumbleReadySeconds = 0.12f;

            [Tooltip("Pulse when backing out to Controller Claim.")]
            [Range(0f, 1f)] public float rumbleBackLow = 0.18f;
            [Range(0f, 1f)] public float rumbleBackHigh = 0.12f;
            [Min(0.01f)] public float rumbleBackSeconds = 0.10f;

            [Tooltip("Pulse when advancing to Character Select.")]
            [Range(0f, 1f)] public float rumbleAdvanceLow = 0.30f;
            [Range(0f, 1f)] public float rumbleAdvanceHigh = 0.65f;
            [Min(0.01f)] public float rumbleAdvanceSeconds = 0.14f;
        }

        [Header("Rumble (Gamepad)")]
        [SerializeField] private RumbleSettings rumble = new RumbleSettings();

        // Forwarders so the rest of the script can stay unchanged.
        private bool enableRumble => rumble != null && rumble.enableRumble;

        private GameSession _session;

        private bool _p1Ready;
        private bool _p2Ready;

        private Gamepad _p1Pad;
        private Gamepad _p2Pad;

        private float rumbleReadyLow => rumble?.rumbleReadyLow ?? 0f;
        private float rumbleReadyHigh => rumble?.rumbleReadyHigh ?? 0f;
        private float rumbleReadySeconds => rumble?.rumbleReadySeconds ?? 0.01f;

        private float rumbleBackLow => rumble?.rumbleBackLow ?? 0f;
        private float rumbleBackHigh => rumble?.rumbleBackHigh ?? 0f;
        private float rumbleBackSeconds => rumble?.rumbleBackSeconds ?? 0.01f;

        private float rumbleAdvanceLow => rumble?.rumbleAdvanceLow ?? 0f;
        private float rumbleAdvanceHigh => rumble?.rumbleAdvanceHigh ?? 0f;
        private float rumbleAdvanceSeconds => rumble?.rumbleAdvanceSeconds ?? 0.01f;

        private void Awake()
        {
            try
            {
                _session = GameSession.EnsureExists();

                if (uiRoot == null)
                    Debug.LogError($"{LogPrefix} uiRoot is not assigned.", this);

                ResolvePads();
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
                ResolvePads();
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
                    ResolvePads();

                // BACK out of Controls View:
                // Only when BOTH are not ready yet (prevents accidental back after someone readies).
                bool p1Back = _p1Pad != null && _p1Pad.buttonEast.wasPressedThisFrame;
                bool p2Back = _p2Pad != null && _p2Pad.buttonEast.wasPressedThisFrame;

                if ((p1Back || p2Back) && CanBackOutToControllerClaim())
                {
                    if (enableRumble)
                    {
                        if (p1Back && _p1Pad != null)
                            GamepadRumble.Pulse(this, _p1Pad, rumbleBackLow, rumbleBackHigh, rumbleBackSeconds);
                        if (p2Back && _p2Pad != null)
                            GamepadRumble.Pulse(this, _p2Pad, rumbleBackLow, rumbleBackHigh, rumbleBackSeconds);
                    }

                    BackToControllerClaim();
                    return;
                }

                if (!_p1Ready && _p1Pad != null && AnyDigitalPressedThisFrame(_p1Pad))
                {
                    _p1Ready = true;
                    uiRoot?.ShowToast("P1 READY");
                    RefreshUI();

                    if (enableRumble)
                        GamepadRumble.Pulse(this, _p1Pad, rumbleReadyLow, rumbleReadyHigh, rumbleReadySeconds);
                }

                if (!_p2Ready && _p2Pad != null && AnyDigitalPressedThisFrame(_p2Pad))
                {
                    _p2Ready = true;
                    uiRoot?.ShowToast("P2 READY");
                    RefreshUI();

                    if (enableRumble)
                        GamepadRumble.Pulse(this, _p2Pad, rumbleReadyLow, rumbleReadyHigh, rumbleReadySeconds);
                }

                // Advance when both ready.
                if (_p1Ready && _p2Ready)
                {
                    if (enableRumble)
                    {
                        if (_p1Pad != null)
                            GamepadRumble.Pulse(this, _p1Pad, rumbleAdvanceLow, rumbleAdvanceHigh, rumbleAdvanceSeconds);
                        if (_p2Pad != null)
                            GamepadRumble.Pulse(this, _p2Pad, rumbleAdvanceLow, rumbleAdvanceHigh, rumbleAdvanceSeconds);
                    }

                    uiRoot?.ActivateCharacterSelect();
                    gameObject.SetActive(false); // prevent double-advance
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Update failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void ResolvePads()
        {
            try
            {
                _session ??= GameSession.EnsureExists();
                _p1Pad = _session.ResolveDevice(_session.P1Device) as Gamepad;
                _p2Pad = _session.ResolveDevice(_session.P2Device) as Gamepad;

                if (_p1Pad == null) Debug.LogWarning($"{LogPrefix} P1 gamepad missing/unresolved.", this);
                if (_p2Pad == null) Debug.LogWarning($"{LogPrefix} P2 gamepad missing/unresolved.", this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} ResolvePads failed: {ex.GetType().Name}: {ex.Message}", this);
            }
        }

        private void ResetReadyState()
        {
            _p1Ready = false;
            _p2Ready = false;
        }

        private void RefreshUI()
        {
            if (p1ReadyText != null)
                p1ReadyText.text = _p1Ready ? readyLabel : string.Empty;

            if (p2ReadyText != null)
                p2ReadyText.text = _p2Ready ? readyLabel : string.Empty;
        }

        private static bool AnyDigitalPressedThisFrame(Gamepad pad)
        {
            // "Meaningful press" definition: face buttons, shoulders, start/select, stick buttons, dpad directions.
            return pad.buttonSouth.wasPressedThisFrame ||
                pad.buttonEast.wasPressedThisFrame ||
                pad.buttonWest.wasPressedThisFrame ||
                pad.buttonNorth.wasPressedThisFrame ||
                pad.leftShoulder.wasPressedThisFrame ||
                pad.rightShoulder.wasPressedThisFrame ||
                pad.startButton.wasPressedThisFrame ||
                pad.selectButton.wasPressedThisFrame ||
                pad.leftStickButton.wasPressedThisFrame ||
                pad.rightStickButton.wasPressedThisFrame ||
                pad.dpad.up.wasPressedThisFrame ||
                pad.dpad.down.wasPressedThisFrame ||
                pad.dpad.left.wasPressedThisFrame ||
                pad.dpad.right.wasPressedThisFrame;
        }

        private bool CanBackOutToControllerClaim()
        {
            // Only allow backing out if nobody is ready yet.
            return !_p1Ready && !_p2Ready;
        }

        private void BackToControllerClaim()
        {
            try
            {
                                ResetReadyState();
                RefreshUI();

                uiRoot?.ActivateControllerClaim();
                gameObject.SetActive(false);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} BackToControllerClaim failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void OnDisable()
        {
            try
            {
                if (!enableRumble)
                    return;

                GamepadRumble.Stop(this, _p1Pad);
                GamepadRumble.Stop(this, _p2Pad);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{LogPrefix} OnDisable rumble stop failed: {ex.GetType().Name}: {ex.Message}", this);
            }
        }
    }
}
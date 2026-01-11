using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using LocalGame.Session;
using LocalGame.InputFx;

namespace LocalGame.SetupScene
{
    /// <summary>
    /// Menu 1: Controller Claim
    /// - D-pad Left claims P1
    /// - D-pad Right claims P2
    /// - X / Square unclaims your assigned slot
    /// - B / Circle goes back to Scene 1 (Main Menu)
    /// - Slot taken -> toast 2s
    /// - If already assigned to one slot and try to claim the other -> move, unclaim old, toast
    /// - When both claimed -> switch to ControlsViewMenu
    /// </summary>
    public sealed class ControllerClaimMenuController : MonoBehaviour
    {
        private const string LogPrefix = "[ControllerClaimMenu]";

        [Header("Scene refs")]
        [SerializeField] private SetupSceneUIRoot uiRoot;

        [Header("P1 Panel Text")]
        [SerializeField] private TMP_Text p1MainText;
        [SerializeField] private TMP_Text p1SubText;

        [Header("P2 Panel Text")]
        [SerializeField] private TMP_Text p2MainText;
        [SerializeField] private TMP_Text p2SubText;
        
        [Serializable]
        private sealed class RumbleSettings
        {
            public bool enableRumble = true;

            [Tooltip("Pulse when successfully claiming/moving to a slot.")]
            [Range(0f, 1f)] public float rumbleClaimLow = 0.25f;
            [Range(0f, 1f)] public float rumbleClaimHigh = 0.45f;
            [Min(0.01f)] public float rumbleClaimSeconds = 0.10f;

            [Tooltip("Pulse when unclaiming your slot.")]
            [Range(0f, 1f)] public float rumbleUnclaimLow = 0.18f;
            [Range(0f, 1f)] public float rumbleUnclaimHigh = 0.12f;
            [Min(0.01f)] public float rumbleUnclaimSeconds = 0.08f;

            [Tooltip("Pulse when an action is blocked (slot taken, etc.).")]
            [Range(0f, 1f)] public float rumbleErrorLow = 0.35f;
            [Range(0f, 1f)] public float rumbleErrorHigh = 0.35f;
            [Min(0.01f)] public float rumbleErrorSeconds = 0.10f;

            [Tooltip("Pulse when advancing to Controls View (both claimed).")]
            [Range(0f, 1f)] public float rumbleAdvanceLow = 0.30f;
            [Range(0f, 1f)] public float rumbleAdvanceHigh = 0.55f;
            [Min(0.01f)] public float rumbleAdvanceSeconds = 0.12f;
        }

        [Header("Rumble (Gamepad)")]
        [SerializeField] private RumbleSettings rumble = new RumbleSettings();

        // Forwarders so the rest of the script can stay unchanged.
        private bool enableRumble => rumble != null && rumble.enableRumble;

        private GameSession _session;

        private Gamepad _p1Pad;
        private Gamepad _p2Pad;

        private float rumbleClaimLow => rumble?.rumbleClaimLow ?? 0f;
        private float rumbleClaimHigh => rumble?.rumbleClaimHigh ?? 0f;
        private float rumbleClaimSeconds => rumble?.rumbleClaimSeconds ?? 0.01f;

        private float rumbleUnclaimLow => rumble?.rumbleUnclaimLow ?? 0f;
        private float rumbleUnclaimHigh => rumble?.rumbleUnclaimHigh ?? 0f;
        private float rumbleUnclaimSeconds => rumble?.rumbleUnclaimSeconds ?? 0.01f;

        private float rumbleErrorLow => rumble?.rumbleErrorLow ?? 0f;
        private float rumbleErrorHigh => rumble?.rumbleErrorHigh ?? 0f;
        private float rumbleErrorSeconds => rumble?.rumbleErrorSeconds ?? 0.01f;

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

                if (p1MainText == null || p1SubText == null || p2MainText == null || p2SubText == null)
                    Debug.LogError($"{LogPrefix} One or more TMP text references are not assigned.", this);

                RefreshAssignedPads();
                RefreshUI();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Awake failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void OnEnable()
        {
            RefreshAssignedPads();
            RefreshUI();
        }

        private void Update()
        {
            try
            {
                _session ??= GameSession.EnsureExists();

                // --- Back to Scene 1 ---
                if (TryGetBackPad(out var backPad))
                {
                    if (enableRumble && backPad != null)
                    {
                        // Small "back" pulse so the user feels the action.
                        GamepadRumble.Pulse(this, backPad, rumbleUnclaimLow, rumbleUnclaimHigh, rumbleUnclaimSeconds);
                        GamepadRumble.Stop(this, backPad);
                    }

                    uiRoot?.ReturnToMainMenu();
                    gameObject.SetActive(false);
                    return;
                }

                // Poll all connected gamepads (Input System safe)
                var pads = Gamepad.all;
                for (int i = 0; i < pads.Count; i++)
                {
                    var pad = pads[i];
                    if (pad == null)
                        continue;

                    if (pad.dpad.left.wasPressedThisFrame)
                        TryClaimP1(pad);

                    if (pad.dpad.right.wasPressedThisFrame)
                        TryClaimP2(pad);

                    if (pad.buttonWest.wasPressedThisFrame) // X / Square
                        TryUnclaim(pad);
                }

                RefreshAssignedPads();
                
                // If both are claimed, advance to Controls View.
                if (_session.P1Device.IsAssigned && _session.P2Device.IsAssigned)
                {
                    if (enableRumble)
                    {
                        if (_p1Pad != null)
                            GamepadRumble.Pulse(this, _p1Pad, rumbleAdvanceLow, rumbleAdvanceHigh, rumbleAdvanceSeconds);
                        if (_p2Pad != null)
                            GamepadRumble.Pulse(this, _p2Pad, rumbleAdvanceLow, rumbleAdvanceHigh, rumbleAdvanceSeconds);

                        GamepadRumble.Stop(this, _p1Pad);
                        GamepadRumble.Stop(this, _p2Pad);
                    }
                    uiRoot?.ActivateControlsView();
                    gameObject.SetActive(false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Update failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

                private bool TryGetBackPad(out Gamepad backPad)
        {
            backPad = null;
            try
            {
                var pads = Gamepad.all;
                for (int i = 0; i < pads.Count; i++)
                {
                    var pad = pads[i];
                    if (pad == null) continue;

                    if (pad.buttonEast.wasPressedThisFrame) // B / Circle
                    {
                        backPad = pad;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Back detection failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }

            return false;
        }

        private void TryClaimP1(Gamepad pad)
        {
            try
            {
                int deviceId = pad.deviceId;

                bool isP1 = _session.P1Device.IsAssigned && _session.P1Device.deviceId == deviceId;
                bool isP2 = _session.P2Device.IsAssigned && _session.P2Device.deviceId == deviceId;

                // Already P1: do nothing.
                if (isP1)
                    return;

                // If controller is currently P2 and tries to claim P1 -> move it.
                if (isP2)
                {
                    _session.SetP1Device(pad);
                    _session.ClearP2Device();
                    RefreshAssignedPads();
                    RefreshUI();

                    uiRoot?.ShowToast("Controller moved to P1 — P2 unclaimed");

                    if (enableRumble)
                        GamepadRumble.Pulse(this, pad, rumbleClaimLow, rumbleClaimHigh, rumbleClaimSeconds);

                    return;
                }

                // Unassigned controller trying to claim P1:
                if (_session.P1Device.IsAssigned)
                {
                    uiRoot?.ShowToast("P1 already claimed");

                    if (enableRumble)
                        GamepadRumble.Pulse(this, pad, rumbleErrorLow, rumbleErrorHigh, rumbleErrorSeconds);

                    return;
                }

                _session.SetP1Device(pad);
                RefreshAssignedPads();
                RefreshUI();
                
                if (enableRumble)
                    GamepadRumble.Pulse(this, pad, rumbleClaimLow, rumbleClaimHigh, rumbleClaimSeconds);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} TryClaimP1 failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void TryClaimP2(Gamepad pad)
        {
            try
            {
                int deviceId = pad.deviceId;

                bool isP1 = _session.P1Device.IsAssigned && _session.P1Device.deviceId == deviceId;
                bool isP2 = _session.P2Device.IsAssigned && _session.P2Device.deviceId == deviceId;

                // Already P2: do nothing.
                if (isP2)
                    return;

                // If controller is currently P1 and tries to claim P2 -> move it.
                if (isP1)
                {
                    _session.SetP2Device(pad);
                    _session.ClearP1Device();
                    RefreshAssignedPads();
                    RefreshUI();

                    uiRoot?.ShowToast("Controller moved to P2 — P1 unclaimed");

                    if (enableRumble)
                        GamepadRumble.Pulse(this, pad, rumbleClaimLow, rumbleClaimHigh, rumbleClaimSeconds);

                    return;
                }

                // Unassigned controller trying to claim P2:
                if (_session.P2Device.IsAssigned)
                {
                    uiRoot?.ShowToast("P2 already claimed");

                    if (enableRumble)
                        GamepadRumble.Pulse(this, pad, rumbleErrorLow, rumbleErrorHigh, rumbleErrorSeconds);

                    return;
                }

                _session.SetP2Device(pad);
                RefreshAssignedPads();
                RefreshUI();
                
                if (enableRumble)
                    GamepadRumble.Pulse(this, pad, rumbleClaimLow, rumbleClaimHigh, rumbleClaimSeconds);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} TryClaimP2 failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void TryUnclaim(Gamepad pad)
        {
            try
            {
                int deviceId = pad.deviceId;

                bool isP1 = _session.P1Device.IsAssigned && _session.P1Device.deviceId == deviceId;
                bool isP2 = _session.P2Device.IsAssigned && _session.P2Device.deviceId == deviceId;

                if (!isP1 && !isP2)
                    return;

                if (isP1)
                    _session.ClearP1Device();

                if (isP2)
                    _session.ClearP2Device();

                RefreshAssignedPads();
                RefreshUI();
                
                if (enableRumble)
                    GamepadRumble.Pulse(this, pad, rumbleUnclaimLow, rumbleUnclaimHigh, rumbleUnclaimSeconds);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} TryUnclaim failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void RefreshAssignedPads()
        {
            try
            {
                _session ??= GameSession.EnsureExists();
                _p1Pad = _session.ResolveDevice(_session.P1Device) as Gamepad;
                _p2Pad = _session.ResolveDevice(_session.P2Device) as Gamepad;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{LogPrefix} RefreshAssignedPads failed: {ex.GetType().Name}: {ex.Message}", this);
            }
        }

        private void RefreshUI()
        {
            try
            {
                if (_session == null)
                    return;

                if (p1MainText != null)
                    p1MainText.text = _session.P1Device.IsAssigned ? $"P1 — Gamepad {_session.P1Device.gamepadIndex}" : "P1 — Unclaimed";
                if (p1SubText != null)
                    p1SubText.text = _session.P1Device.IsAssigned ? _session.P1Device.deviceName : string.Empty;

                if (p2MainText != null)
                    p2MainText.text = _session.P2Device.IsAssigned ? $"P2 — Gamepad {_session.P2Device.gamepadIndex}" : "P2 — Unclaimed";
                if (p2SubText != null)
                    p2SubText.text = _session.P2Device.IsAssigned ? _session.P2Device.deviceName : string.Empty;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} RefreshUI failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void OnDisable()
        {
            try
            {
                if (!enableRumble)
                    return;

                foreach (var pad in Gamepad.all)
                {
                    if (pad != null)
                        GamepadRumble.Stop(this, pad);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{LogPrefix} OnDisable rumble stop failed: {ex.GetType().Name}: {ex.Message}", this);
            }
        }
    }
}
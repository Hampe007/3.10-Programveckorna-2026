using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using LocalGame.Session;

namespace LocalGame.SetupScene
{
    /// <summary>
    /// Menu 1: Controller Claim
    /// - D-pad Left claims P1
    /// - D-pad Right claims P2
    /// - B/Circle unclaims your assigned slot
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

        private GameSession _session;

        private void Awake()
        {
            try
            {
                _session = GameSession.EnsureExists();

                if (uiRoot == null)
                    Debug.LogError($"{LogPrefix} uiRoot is not assigned.", this);

                if (p1MainText == null || p1SubText == null || p2MainText == null || p2SubText == null)
                    Debug.LogError($"{LogPrefix} One or more TMP text references are not assigned.", this);

                RefreshUI();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Awake failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        private void Update()
        {
            // Poll all connected gamepads (Input System safe).
            try
            {
                if (_session == null)
                    _session = GameSession.EnsureExists();

                // No gamepads connected: nothing to do here (we'll handle keyboard later if needed).
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

                    if (pad.buttonEast.wasPressedThisFrame) // B / Circle
                        TryUnclaim(pad);
                }

                // If both are claimed, advance to Controls View.
                if (_session.P1Device.IsAssigned && _session.P2Device.IsAssigned)
                {
                    // We only want to advance once, so disable this menu after switching.
                    uiRoot?.ActivateControlsView();
                    gameObject.SetActive(false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Update failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
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
                    RefreshUI();
                    uiRoot?.ShowToast("Controller moved to P1 — P2 unclaimed");
                    return;
                }

                // Unassigned controller trying to claim P1:
                if (_session.P1Device.IsAssigned)
                {
                    uiRoot?.ShowToast("P1 already claimed");
                    return;
                }

                _session.SetP1Device(pad);
                RefreshUI();
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
                    RefreshUI();
                    uiRoot?.ShowToast("Controller moved to P2 — P1 unclaimed");
                    return;
                }

                // Unassigned controller trying to claim P2:
                if (_session.P2Device.IsAssigned)
                {
                    uiRoot?.ShowToast("P2 already claimed");
                    return;
                }

                _session.SetP2Device(pad);
                RefreshUI();
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

                RefreshUI();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} TryUnclaim failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
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
    }
}
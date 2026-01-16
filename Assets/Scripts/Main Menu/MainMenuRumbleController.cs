using System;
using LocalGame.InputFx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MainMenuRumbleController : MonoBehaviour
{
    private const string LogPrefix = "[MainMenuRumble]";

    [Header("Scene Refs")]
    [SerializeField] private EventSystem eventSystem;

    [Serializable]
    private sealed class RumbleSettings
    {
        public bool enableRumble = true;

        [Tooltip("Small tick when navigating between UI elements.")]
        [Range(0f, 1f)] public float rumbleNavLow = 0.08f;
        [Range(0f, 1f)] public float rumbleNavHigh = 0.14f;
        [Min(0.01f)] public float rumbleNavSeconds = 0.04f;

        [Tooltip("Confirm/submit pulse.")]
        [Range(0f, 1f)] public float rumbleConfirmLow = 0.20f;
        [Range(0f, 1f)] public float rumbleConfirmHigh = 0.35f;
        [Min(0.01f)] public float rumbleConfirmSeconds = 0.08f;

        [Tooltip("Cancel/back pulse.")]
        [Range(0f, 1f)] public float rumbleCancelLow = 0.18f;
        [Range(0f, 1f)] public float rumbleCancelHigh = 0.12f;
        [Min(0.01f)] public float rumbleCancelSeconds = 0.08f;
    }

    [Header("Rumble (Gamepad)")]
    [SerializeField] private RumbleSettings rumble = new RumbleSettings();
    private bool enableRumble => rumble != null && rumble.enableRumble;

    private GameObject lastSelected;

    private void Awake()
    {
        if (eventSystem == null)
            eventSystem = EventSystem.current;
    }

    private void Update()
    {
        if (!enableRumble)
            return;

        if (eventSystem == null)
            eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            Debug.LogWarning($"{LogPrefix} No EventSystem found.", this);
            return;
        }

        var selected = eventSystem.currentSelectedGameObject;
        if (selected != null && selected != lastSelected)
            PulseNavigation();

        lastSelected = selected;

        if (TryGetPadPress(pad => pad.buttonSouth.wasPressedThisFrame, out var confirmPad))
            PulseConfirm(confirmPad);

        if (TryGetPadPress(pad => pad.buttonEast.wasPressedThisFrame, out var cancelPad))
            PulseCancel(cancelPad);
    }

    private void PulseNavigation()
    {
        var pad = ResolveNavigationPad();
        if (pad == null)
            return;

        Pulse(pad, rumble.rumbleNavLow, rumble.rumbleNavHigh, rumble.rumbleNavSeconds);
    }

    private void PulseConfirm(Gamepad pad)
    {
        if (pad == null)
            return;

        Pulse(pad, rumble.rumbleConfirmLow, rumble.rumbleConfirmHigh, rumble.rumbleConfirmSeconds);
    }

    private void PulseCancel(Gamepad pad)
    {
        if (pad == null)
            return;

        Pulse(pad, rumble.rumbleCancelLow, rumble.rumbleCancelHigh, rumble.rumbleCancelSeconds);
    }

    private void Pulse(Gamepad pad, float low, float high, float seconds)
    {
        float gain = GetGainForPad(pad);
        if (gain <= 0f)
            return;

        GamepadRumble.Pulse(this, pad, low * gain, high * gain, seconds);
    }

    private bool TryGetPadPress(Func<Gamepad, bool> predicate, out Gamepad pad)
    {
        pad = null;
        var pads = Gamepad.all;
        for (int i = 0; i < pads.Count; i++)
        {
            var candidate = pads[i];
            if (candidate != null && predicate(candidate))
            {
                pad = candidate;
                return true;
            }
        }

        return false;
    }

    private Gamepad ResolveNavigationPad()
    {
        if (Gamepad.current != null)
            return Gamepad.current;

        var pads = Gamepad.all;
        if (pads.Count == 1)
            return pads[0];

        return null;
    }

    private float GetGainForPad(Gamepad pad)
    {
        var pads = Gamepad.all;
        for (int i = 0; i < pads.Count; i++)
        {
            if (pads[i] == pad)
                return RumblePreferences.GetGainForPlayer(i);
        }

        return 1f;
    }
}

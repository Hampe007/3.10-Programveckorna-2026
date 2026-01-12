using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LocalGame.InputFx
{
    /// <summary>
    /// Lightweight rumble helper that supports:
    /// - Continuous rumble (typically set every frame while holding).
    /// - Pulses (short feedback on events) that temporarily override continuous rumble.
    ///
    /// Integration quick-start:
    /// - UI feedback: Pulse on highlight, confirm, or error states from your menu/controller.
    /// - Combat: Pulse on hit/parry/critical; SetContinuous while charging heavy attacks.
    /// - Movement: Pulse on dash/land; SetContinuous based on speed, traction, or boost.
    /// - Cutscenes: low continuous rumble for tension, then Stop on exit/pause.
    ///
    /// Example (UI confirm):
    /// <code>
    /// GamepadRumble.Pulse(this, Gamepad.current, 0.05f, 0.2f, 0.06f);
    /// </code>
    /// Example (charge attack each frame):
    /// <code>
    /// GamepadRumble.SetContinuous(Gamepad.current, charge * 0.2f, charge * 0.6f);
    /// </code>
    ///
    /// Important detail:
    /// If a pulse is active, SetContinuous() will only update the "desired" value and NOT
    /// immediately overwrite the motors. When the pulse finishes, the motors revert to the
    /// latest desired continuous value (usually 0,0).
    ///
    /// Global tuning:
    /// - GlobalGain boosts ALL rumble intensities in one place.
    /// - GlobalLowGain / GlobalHighGain lets you bias motors.
    /// - GlobalExponent remaps intensity (v -> v^exp). If exp < 1, small values become stronger.
    /// </summary>
    public static class GamepadRumble
    {

        // Global tuning
        
        /// <summary>
        /// Master multiplier applied to ALL rumble values.
        /// Example: 1.8f makes everything 80% stronger.
        /// </summary>
        public static float GlobalGain { get; set; } = 1.0f;

        /// <summary>
        /// Extra multiplier only for the low-frequency motor.
        /// </summary>
        public static float GlobalLowGain { get; set; } = 1.0f;

        /// <summary>
        /// Extra multiplier only for the high-frequency motor.
        /// </summary>
        public static float GlobalHighGain { get; set; } = 1.0f;

        /// <summary>
        /// Intensity curve exponent (v -> v^exp).
        /// - 1.0 = linear
        /// - < 1.0 boosts small values (recommended for Xbox; e.g. 0.75)
        /// - > 1.0 reduces small values
        /// </summary>
        public static float GlobalExponent { get; set; } = 1.0f;

        private struct Desired
        {
            public float low;
            public float high;
        }

        // Keyed by Gamepad.deviceId so reconnects are handled cleanly.
        private static readonly Dictionary<int, Desired> DesiredByDevice = new();
        private static readonly Dictionary<int, Coroutine> PulseByDevice = new();

        public static void SetContinuous(Gamepad pad, float lowFrequency, float highFrequency)
        {
            if (pad == null)
                return;

            lowFrequency = Mathf.Clamp01(lowFrequency);
            highFrequency = Mathf.Clamp01(highFrequency);

            // Store DESIRED in final tuned space so pulse-revert matches what player would feel.
            ApplyGlobalTuning(ref lowFrequency, ref highFrequency);

            DesiredByDevice[pad.deviceId] = new Desired { low = lowFrequency, high = highFrequency };

            // If a pulse is currently playing, don't overwrite the pulse.
            if (PulseByDevice.ContainsKey(pad.deviceId))
                return;

            SafeSetMotorsRaw(pad, lowFrequency, highFrequency);
        }

        public static void Pulse(MonoBehaviour host, Gamepad pad, float lowFrequency, float highFrequency, float seconds)
        {
            if (host == null || pad == null)
                return;

            lowFrequency = Mathf.Clamp01(lowFrequency);
            highFrequency = Mathf.Clamp01(highFrequency);
            seconds = Mathf.Max(0.01f, seconds);

            ApplyGlobalTuning(ref lowFrequency, ref highFrequency);

            // Stop any previous pulse on this pad.
            StopPulse(host, pad);

            var co = host.StartCoroutine(PulseRoutine(pad, lowFrequency, highFrequency, seconds));
            PulseByDevice[pad.deviceId] = co;
        }

        public static void Stop(MonoBehaviour host, Gamepad pad)
        {
            if (pad == null)
                return;

            // Clear desired.
            DesiredByDevice[pad.deviceId] = new Desired { low = 0f, high = 0f };

            // Stop any running pulse coroutine.
            StopPulse(host, pad);

            SafeSetMotorsRaw(pad, 0f, 0f);
        }

        private static void StopPulse(MonoBehaviour host, Gamepad pad)
        {
            if (host == null || pad == null)
                return;

            if (PulseByDevice.TryGetValue(pad.deviceId, out var co) && co != null)
            {
                host.StopCoroutine(co);
            }

            PulseByDevice.Remove(pad.deviceId);
        }

        private static IEnumerator PulseRoutine(Gamepad pad, float low, float high, float seconds)
        {
            SafeSetMotorsRaw(pad, low, high);

            // Use unscaled time so pulses still work if timeScale changes.
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            PulseByDevice.Remove(pad.deviceId);

            // Revert to latest desired continuous.
            if (DesiredByDevice.TryGetValue(pad.deviceId, out var desired))
            {
                SafeSetMotorsRaw(pad, desired.low, desired.high);
            }
            else
            {
                SafeSetMotorsRaw(pad, 0f, 0f);
            }
        }

        private static void SafeSetMotors(Gamepad pad, float low, float high)
        {
            // Some platforms/devices can throw if haptics aren't supported; fail safely.
            try
            {
                ApplyGlobalTuning(ref low, ref high);
                pad.SetMotorSpeeds(low, high);
            }
            catch
            {
                // Intentionally ignored.
            }
        }

        private static void SafeSetMotorsRaw(Gamepad pad, float low, float high)
        {
            // Some platforms/devices can throw if haptics aren't supported; fail safely.
            try
            {
                low = Mathf.Clamp01(low);
                high = Mathf.Clamp01(high);
                pad.SetMotorSpeeds(low, high);
            }
            catch
            {
                // Intentionally ignored.
            }
        }

        private static float ApplyGlobalCurve(float v)
        {
            v = Mathf.Clamp01(v);

            float exp = Mathf.Max(0.01f, GlobalExponent);

            // Keep exact same output when exp is effectively 1 (micro-optimization + avoids tiny float drift).
            if (Mathf.Abs(exp - 1f) < 0.0001f)
                return v;

            return Mathf.Pow(v, exp);
        }

        private static void ApplyGlobalTuning(ref float low, ref float high)
        {
            low = Mathf.Clamp01(low);
            high = Mathf.Clamp01(high);

            // Apply curve before gain (so gain still scales the final amplitude).
            low = ApplyGlobalCurve(low);
            high = ApplyGlobalCurve(high);

            // Apply global gain + per-motor gains.
            low *= GlobalGain * GlobalLowGain;
            high *= GlobalGain * GlobalHighGain;

            // Clamp to supported range.
            low = Mathf.Clamp01(low);
            high = Mathf.Clamp01(high);
        }
    }
}

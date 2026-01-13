using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using LocalGame.Session;

/// <summary>
/// Binds persisted devices from GameSession to runtime PlayerInput instances.
/// - Ensures P1/P2 are paired to the exact devices chosen in the setup flow.
/// - Creates players through PlayerInputManager if they do not exist.
/// - Optionally disables further joins after binding.
/// </summary>
public sealed class SessionPlayerInputBinder : MonoBehaviour
{
    private const string LogPrefix = "[SessionPlayerInputBinder]";

    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private bool disableJoiningAfterBind = true;

    /// <summary>
    /// Delay one frame so PlayerInputManager/InputManager have initialized, then bind.
    /// </summary>
    public void CreatePlayerInputs()
    {
        if (playerInputManager == null)
            playerInputManager = GetComponent<PlayerInputManager>();

        BindFromSession();
    }

    /// <summary>
    /// Bind P1/P2 devices from the current session to PlayerInput instances.
    /// </summary>
    private void BindFromSession()
    {
        try
        {
            if (playerInputManager == null)
            {
                Debug.LogError($"{LogPrefix} PlayerInputManager is not assigned.", this);
                return;
            }

            var session = GameSession.EnsureExists();
            BindPlayer(session, session.P1Device, 0, "P1");
            BindPlayer(session, session.P2Device, 1, "P2");

            if (disableJoiningAfterBind)
                playerInputManager.DisableJoining();
        }
        catch (Exception ex)
        {
            Debug.LogError($"{LogPrefix} BindFromSession failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
        }
    }

    /// <summary>
    /// Bind a single player index to its assigned device, or join a new player if missing.
    /// </summary>
    private void BindPlayer(GameSession session, GameSession.PlayerDeviceInfo info, int playerIndex, string label)
    {
        if (!info.IsAssigned)
        {
            Debug.LogWarning($"{LogPrefix} {label} device is not assigned.", this);
            return;
        }

        var device = session.ResolveDevice(info);
        if (device == null)
        {
            Debug.LogWarning($"{LogPrefix} {label} device could not be resolved.", this);
            return;
        }

        var playerInput = FindPlayerByIndex(playerIndex);
        if (playerInput == null)
        {
            string scheme = device is Gamepad ? "Gamepad" : null;
            playerInput = playerInputManager.JoinPlayer(playerIndex, -1, scheme, device);
        }

        if (playerInput == null)
        {
            Debug.LogError($"{LogPrefix} Failed to create PlayerInput for {label}.", this);
            return;
        }

        BindDevices(playerInput, device);
    }

    /// <summary>
    /// Returns the PlayerInput matching the given player index, if it exists.
    /// </summary>
    private static PlayerInput FindPlayerByIndex(int playerIndex)
    {
        foreach (var player in PlayerInput.all)
        {
            if (player.playerIndex == playerIndex)
                return player;
        }

        return null;
    }

    /// <summary>
    /// Clears previous pairings and pairs the target device (plus mouse if keyboard).
    /// </summary>
    private static void BindDevices(PlayerInput playerInput, InputDevice device)
    {
        playerInput.user.UnpairDevices();
        InputUser.PerformPairingWithDevice(device, playerInput.user);

        if (device is Keyboard && Mouse.current != null)
            InputUser.PerformPairingWithDevice(Mouse.current, playerInput.user);
    }
}

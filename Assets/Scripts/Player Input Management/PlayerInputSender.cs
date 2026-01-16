using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;

public class ControllerSender : MonoBehaviour
{
    PlayerInput myInput;
    List<CharacterInputHandler> connectedHandlers = new List<CharacterInputHandler>();
    int connectedIndex;

    private void Awake()
    {
        InputManager.instance.AddController(this);
        myInput = GetComponent<PlayerInput>();
        connectedIndex = myInput.playerIndex;
        UpdateConnections();
    }

    public void OnJump(CallbackContext ctx)
    {
        foreach (CharacterInputHandler handler in connectedHandlers)
        {
            handler.OnJump(ctx);
        }
    }

    public void OnHorizontal(CallbackContext ctx)
    {
        foreach (CharacterInputHandler handler in connectedHandlers)
        {
            handler.OnHorizontal(ctx);
        }
    }

    public void OnAbility1(CallbackContext ctx)
    {
        foreach (CharacterInputHandler handler in connectedHandlers)
        {
            handler.OnAbility1(ctx);
        }
    }
    public void OnAbility2(CallbackContext ctx)
    {
        foreach (CharacterInputHandler handler in connectedHandlers)
        {
            handler.OnAbility2(ctx);
        }
    }
    public void OnAbility3(CallbackContext ctx)
    {
        foreach (CharacterInputHandler handler in connectedHandlers)
        {
            handler.OnAbility3(ctx);
        }
    }

    public void UpdateConnections()
    {
        CharacterInputHandler[] handlers = FindObjectsByType<CharacterInputHandler>(FindObjectsSortMode.None);
        connectedHandlers = new List<CharacterInputHandler>();
        foreach (CharacterInputHandler handler in handlers)
        {
            if (handler.playerIndex == connectedIndex)
            {
                connectedHandlers.Add(handler);
            }
        }
    }

    public bool TryGetGamepad(out Gamepad pad)
    {
        pad = null;
        if (myInput == null)
            return false;

        var devices = myInput.devices;
        for (int i = 0; i < devices.Count; i++)
        {
            if (devices[i] is Gamepad gamepad)
            {
                pad = gamepad;
                return true;
            }
        }

        return false;
    }

    public int PlayerIndex => connectedIndex;
}

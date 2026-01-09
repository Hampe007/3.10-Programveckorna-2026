using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public List<ControllerSender> controllers = new List<ControllerSender>();
    public ControllerSender[] activeControllers = new ControllerSender[2];
    public static InputManager instance;

    void Awake()
    {
        if (instance != null) { Debug.LogWarning("Multiple InputManagers exist."); }
        instance = this;
    }

    public void AddController(ControllerSender controller)
    {
        controllers.Add(controller);
        controller.transform.parent = transform;
        if (activeControllers[0] == null)
        {
            activeControllers[0] = controller;
        }
        else if (activeControllers[1] == null)
        {
            activeControllers[1] = controller;
        }
    }

    public void AssignDeviceToSlot(InputDevice device, int slotIndex)
    {
        if (device == null || slotIndex < 0)
        {
            return;
        }
        foreach (ControllerSender sender in controllers)
        {
            if (sender != null && sender.OwnsDevice(device))
            {
                sender.OverrideIndex(slotIndex);
                if (slotIndex < activeControllers.Length)
                {
                    activeControllers[slotIndex] = sender;
                }
                return;
            }
        }
    }

    public void ClearDeviceOverride(InputDevice device)
    {
        if (device == null)
        {
            return;
        }
        foreach (ControllerSender sender in controllers)
        {
            if (sender != null && sender.OwnsDevice(device))
            {
                sender.ClearOverride();
                return;
            }
        }
    }
}

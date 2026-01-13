using System.Collections.Generic;
using UnityEngine;

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
}

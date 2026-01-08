using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class CharacterInputHandler : MonoBehaviour
{
    public bool leftHeld;
    public bool rightHeld;
    public int horizontalDirection;
    public Vector2 LStick;

    public bool jumpPressed;
    public bool jumpReleased;
    bool jumpPressedQ;
    bool jumpRelaseseQ;

    public bool ability1Pressed;
    public bool ability1Released;
    bool ability1PressedQ;
    bool ability1ReleasedQ;

    public bool ability2Pressed;
    public bool ability2Released;
    bool ability2PressedQ;
    bool ability2ReleasedQ;

    public bool ability3Pressed;
    public bool ability3Released;
    bool ability3PressedQ;
    bool ability3ReleasedQ;

    public int playerIndex;


    void Update()
    {
        if (LStick.x < 0)
        {
            horizontalDirection = -1;
            rightHeld = false;
            leftHeld = true;
        }
        else if (LStick.x > 0)
        {
            horizontalDirection = 1;
            rightHeld = true;
            leftHeld = false;
        }
        else
        {
            horizontalDirection = 0;
            rightHeld = false;
            leftHeld = false;
        }


        if (jumpPressedQ)
        {
            jumpPressed = true;
            jumpPressedQ = false;
        }
        else
        {
            jumpPressed = false;
        }

        if (jumpRelaseseQ)
        {
            jumpReleased = true;
            jumpRelaseseQ = false;
        }
        else
        {
            jumpReleased = false;
        }

        if (ability1PressedQ)
        {
            ability1Pressed = true;
            ability1PressedQ = false;
        }
        else
        {
            ability1Pressed = false;
        }
        if (ability1ReleasedQ)
        {
            ability1Released = true;
            ability1ReleasedQ = false;
        }
        else
        {
            ability1Released = false;
        }

        if (ability2PressedQ)
        {
            ability2Pressed = true;
            ability2PressedQ = false;
        }
        else
        {
            ability2Pressed = false;
        }
        if (ability2ReleasedQ)
        {
            ability2Released = true;
            ability2ReleasedQ = false;
        }
        else
        {
            ability2Released = false;
        }

        if (ability3PressedQ)
        {
            ability3Pressed = true;
            ability3PressedQ = false;
        }
        else
        {
            ability3Pressed = false;
        }
        if (ability3ReleasedQ)
        {
            ability3Released = true;
            ability3ReleasedQ = false;
        }
        else
        {
            ability3Released = false;
        }
    }

    public void OnJump(CallbackContext Context)
    {
        if (Context.canceled)
        {
            jumpRelaseseQ = true;
        }
        if (Context.performed)
        {
            jumpPressedQ = true;
        }
    }

    public void OnHorizontal(CallbackContext Context)
    {
        LStick = Context.ReadValue<Vector2>();
    }

    public void OnDisconnect(CallbackContext Context)
    {
        Destroy(gameObject);
    }

    public void OnAbility1(CallbackContext Context)
    {
        if (Context.canceled)
        {
            ability1ReleasedQ = true;
        }
        if (Context.performed)
        {
            ability1PressedQ = true;
        }
    }
    public void OnAbility2(CallbackContext Context)
    {
        if (Context.canceled)
        {
            ability2ReleasedQ = true;
        }
        if (Context.performed)
        {
            ability2PressedQ = true;
        }
    }
    public void OnAbility3(CallbackContext Context)
    {
        if (Context.canceled)
        {
            ability3ReleasedQ = true;
        }
        if (Context.performed)
        {
            ability3PressedQ = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using LocalGame.InputFx;

public class Character : MonoBehaviour
{
    [Serializable]
    private sealed class RumbleSettings
    {
        public bool enableRumble = true;

        [Tooltip("Pulse when this player lands a hit.")]
        [Range(0f, 1f)] public float rumbleAttackLow = 0.20f;
        [Range(0f, 1f)] public float rumbleAttackHigh = 0.35f;
        [Min(0.01f)] public float rumbleAttackSeconds = 0.08f;

        [Tooltip("Pulse when this player takes damage.")]
        [Range(0f, 1f)] public float rumbleHitLow = 0.35f;
        [Range(0f, 1f)] public float rumbleHitHigh = 0.60f;
        [Min(0.01f)] public float rumbleHitSeconds = 0.12f;
    }

    [Header("Rumble (Gamepad)")]
    [SerializeField] private RumbleSettings rumble = new RumbleSettings();
    private bool enableRumble => rumble != null && rumble.enableRumble;

    private float rumbleAttackLow => rumble?.rumbleAttackLow ?? 0f;
    private float rumbleAttackHigh => rumble?.rumbleAttackHigh ?? 0f;
    private float rumbleAttackSeconds => rumble?.rumbleAttackSeconds ?? 0.01f;

    private float rumbleHitLow => rumble?.rumbleHitLow ?? 0f;
    private float rumbleHitHigh => rumble?.rumbleHitHigh ?? 0f;
    private float rumbleHitSeconds => rumble?.rumbleHitSeconds ?? 0.01f;

    public CharacterInputHandler input;
    [NonSerialized] public bool facingLeft;
    public int facingMultiplier => !facingLeft ? 1 : -1;
    public int maxHealth;
    [NonSerialized] public int health;
    public float runSpeed;
    public float airSpeed;
    public float jumpStartup;
    public float jumpForce;
    public float webTime = 1.4f;
    public GameObject jumpEffect;
    public GameObject landEffect;
    public GameObject takeHitEffect;
    public GameObject deathEffect;

    public CharacterState state { get; private set; }
    [NonSerialized] public Rigidbody rb;
    public string stateName => GetStateName();
    public int horizontalInputAdjusted => facingLeft ? input.horizontalDirection : -input.horizontalDirection;

    List<Collider> grounds = new List<Collider>();
    public FakeGravity gravity;
    
    protected bool grounded => grounds.Count > 0;
    bool platdropping;
    public int playerIndex;
    public string GetStateName()
    {
        string fullName = state.GetType().Name;
        if (!fullName.Contains("State"))
        {
            Debug.LogError("State name did not contain \"State\"");
            return "Error";
        }
        return fullName.Remove(fullName.Length - 5);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        StartState();
        health = maxHealth;
    }

    protected virtual void StartState()
    {
        Debug.LogError("Base StartState has ran. The character " + gameObject.name + " is missing an override.");
    }
    protected virtual void Die()
    {
        Debug.LogError("Base Die has ran. The character " + gameObject.name + " is missing an override.");
    }

    protected virtual void GetWebbed()
    {
        Debug.LogError("Base GetWebbed has ran. The character " + gameObject.name + " is missing an override.");
    }

    protected virtual void Update()
    {
        state.ElapseTime();
        state.OnTimeElapsed(Time.deltaTime);
        int newHorizontalInput = input.horizontalDirection;
        if (input.horizontalDirection == 0)
        {
            state.OnDirectionStop();
        }
        else
        {
            bool dirSwitch = input.horizontalDirection == -1 ^ facingLeft; //Is the new direction different
            state.OnDirectionStart(dirSwitch);
        }

        if (input.jumpPressed)
        {
            state.OnJumpHeld();
        }

        if (input.jumpReleased)
        {
            state.OnJumpReleased();
        }

        if (input.ability1Pressed)
        {
            state.OnAbility1Held();
        }
        if (input.ability1Released)
        {
            state.OnAbility1Released();
        }
        if (input.ability2Pressed)
        {
            state.OnAbility2Held();
        }
        if (input.ability2Released)
        {
            state.OnAbility2Released();
        }
        if (input.ability3Pressed)
        {
            state.OnAbility3Held();
        }
        if (input.ability3Released)
        {
            state.OnAbility3Released();
        }

        if(input.downHeld)
        {
            if(!platdropping)
            {
                platdropping = true;
                OneWayManager.instance.PauseObject(gameObject);
            }
        }
        else if(!input.downHeld)
        {
            if(platdropping)
            {
                platdropping = false;
                OneWayManager.instance.UnPauseObject(gameObject);
            }
        }
    }


    public void SwitchState(Type newState)
    {
        if(state != null)
        {
            state.TrueOnEnd();
        }
        object[] parameters = new object[1];
        parameters[0] = this;
        state = (CharacterState)Activator.CreateInstance(newState, parameters);
        state.TrueOnStart();
        //Debug.Log("New state is " + stateName);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Ground>(out _))
        {
            bool groundedBefore = grounded;
            grounds.Add(collision.collider);
            if (groundedBefore ^ grounded)
            {
                state.OnLand();
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Ground>(out _))
        {
            bool groundedBefore = grounded;
            grounds.Remove(collision.collider);
            if (groundedBefore ^ grounded)
            {
                state.OnLeaveGround();
            }
        }
    }

    public void TakeHit(int damage)
    {
        CameraControl.instance.ShakeCam(0.20f, 0.7f);
        ActivateEffect(takeHitEffect, transform.position);
        RumbleTakeHit();
        health -= damage;
        if (health <= 0)
        {
            ActivateEffect(deathEffect, transform.position);
            Die();
        }
    }

    public void WebHit()
    {
        if(state.interruptible)
        {
            state.OnInterruption();
            GetWebbed();
        }
    }

    public bool HitEnemies(List<RaycastHit> hits, int damage)
    {
        return HitEnemies(hits, damage, this);
    }
    public static bool HitEnemies(List<RaycastHit> hits, int damage, Character owner)
    {
        bool hasHit = false;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.TryGetComponent(out Character hitCharacter))
            {
                if (hitCharacter == owner)
                {
                    continue;
                }
                hasHit = true;
                hitCharacter.TakeHit(damage);
            }
        }
        if (hasHit && owner != null)
            owner.RumbleAttackHit();
        return hasHit;
    }

    public void ActivateEffect(GameObject effect, Vector2 position)
    {
        if(effect == null)
        {
            return;
        }
        Instantiate(effect, position, Quaternion.identity);
    }

    public void RumbleAttackHit()
    {
        if (!enableRumble)
            return;

        var pad = ResolveGamepad();
        if (pad == null)
            return;

        float gain = RumblePreferences.GetGainForPlayer(playerIndex);
        if (gain <= 0f)
            return;

        GamepadRumble.Pulse(this, pad, rumbleAttackLow * gain, rumbleAttackHigh * gain, rumbleAttackSeconds);
    }

    private void RumbleTakeHit()
    {
        if (!enableRumble)
            return;

        var pad = ResolveGamepad();
        if (pad == null)
            return;

        float gain = RumblePreferences.GetGainForPlayer(playerIndex);
        if (gain <= 0f)
            return;

        GamepadRumble.Pulse(this, pad, rumbleHitLow * gain, rumbleHitHigh * gain, rumbleHitSeconds);
    }

    private Gamepad ResolveGamepad()
    {
        var manager = InputManager.instance;
        if (manager == null)
            return null;

        if (manager.controllers != null)
        {
            for (int i = 0; i < manager.controllers.Count; i++)
            {
                var controller = manager.controllers[i];
                if (controller == null || controller.PlayerIndex != playerIndex)
                    continue;

                return controller.TryGetGamepad(out var controllerPad) ? controllerPad : null;
            }
        }

        if (manager.activeControllers == null ||
            playerIndex < 0 ||
            playerIndex >= manager.activeControllers.Length)
            return null;

        var activeController = manager.activeControllers[playerIndex];
        if (activeController == null)
            return null;

        return activeController.TryGetGamepad(out var activePad) ? activePad : null;
    }
}

public class Cooldown
{
    public float duration;
    public float timeLeft;
    public bool ready => timeLeft <= 0;
    public Cooldown(float time, bool startOnCooldown = false)
    {
        duration = time;
        if(startOnCooldown)
        {
            timeLeft = duration;
        }
    }
    public void AddTime(float time)
    {
        timeLeft -= time;
    }
    public void Reset()
    {
        timeLeft = duration;
    }
}

public abstract class CharacterState
{
    protected Character owner;
    protected float timeElapsed = 0;
    protected float expirationTime = -1;
    public bool interruptible = true;
    public bool gravity = true;
    bool expired = false;

    public CharacterState(Character owner)
    {
        this.owner = owner;
    }

    public void ElapseTime()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= expirationTime && !expired)
        {
            expired = true;
            OnExpiration();
        }
    }
    public void TrueOnStart()
    {
        if(!gravity)
        {
            owner.gravity.active = false;
        }
        OnStart();
    }
    public void TrueOnEnd()
    {
        if (!gravity)
        {
            owner.gravity.active = true;
        }
    }
    public virtual void OnStart() { }
    public virtual void OnTimeElapsed(float time) { }
    public virtual void OnExpiration() { }
    public virtual void OnInterruption() { }
    public virtual void OnDirectionStart(bool dirSwitch) { }
    public virtual void OnDirectionStop() { }
    public virtual void OnJumpHeld() { }
    public virtual void OnJumpReleased() { }
    public virtual void OnLand() { }
    public virtual void OnLeaveGround() { }
    public virtual void OnAbility1Held() { }
    public virtual void OnAbility1Released() { }
    public virtual void OnAbility2Held() { }
    public virtual void OnAbility2Released() { }
    public virtual void OnAbility3Held() { }
    public virtual void OnAbility3Released() { }
}

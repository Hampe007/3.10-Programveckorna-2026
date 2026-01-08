using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Character : MonoBehaviour
{
    [SerializeField] CharacterInputHandler input;
    [NonSerialized] public bool facingLeft;
    public int facingMultiplier => !facingLeft ? 1 : -1;
    public int health;
    public CharacterState state { get; private set; }
    [NonSerialized] public Rigidbody rb;
    public string stateName => GetStateName();

    public int horizontalInputAdjusted => facingLeft ? input.horizontalDirection : -input.horizontalDirection;

    List<Collider> grounds = new List<Collider>();
    bool grounded => grounds.Count > 0;
    
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

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartState();
    }
    protected virtual void StartState()
    {
        Debug.LogError("Base StartState has ran. The character " + gameObject.name + " is missing an override.");
    }
    protected virtual void Die()
    {
        Debug.LogError("Base Die has ran. The character " + gameObject.name + " is missing an override.");
    }

    void Update()
    {
        state.ElapseTime();
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
    }

    public void SwitchState(Type newState)
    {
        object[] parameters = new object[1];
        parameters[0] = this;
        state = (CharacterState)Activator.CreateInstance(newState, parameters);
        state.OnStart();
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
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }
    public void HitEnemies(List<RaycastHit> hits, int damage)
    {
        HitEnemies(hits, damage, this);
    }
    public static void HitEnemies(List<RaycastHit> hits, int damage, Character owner)
    {
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.TryGetComponent(out Character hitCharacter))
            {
                if (hitCharacter == owner)
                {
                    continue;
                }
                hitCharacter.TakeHit(damage);
            }
        }
    }
}

public abstract class CharacterState
{
    protected Character owner;
    protected float timeElapsed = 0;
    protected float expirationTime = -1;
    public bool interruptible = true;
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

    public virtual void OnStart() { }
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

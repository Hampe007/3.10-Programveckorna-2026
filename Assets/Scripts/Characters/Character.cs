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

    public float runSpeed;
    public float airSpeed;
    public float jumpStartup;
    public float jumpForce;
    public float punchStartup;
    public float punchEndlag;
    public int punchDamage;

    public int horizontalInputAdjusted => facingLeft ? input.horizontalDirection : -input.horizontalDirection;

    List<Collider> grounds = new List<Collider>();
    bool grounded => grounds.Count > 0;

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
        state = new AirStillState(this);
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
            SwitchState(typeof(InactiveState));
        }
    }

    public void HitEnemies(List<RaycastHit> hits, int damage)
    {
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.TryGetComponent(out Character hitCharacter))
            {
                if (hitCharacter == this)
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
public class InactiveState : CharacterState
{
    public InactiveState(Character owner) : base(owner)
    {

    }

    public override void OnStart()
    {
        owner.rb.linearVelocity = Vector2.zero;
    }
}
public class IdleState : CharacterState
{
    public IdleState(Character owner) : base(owner)
    {

    }

    public override void OnStart()
    {
        owner.rb.linearVelocity = Vector2.zero;
    }
    public override void OnDirectionStart(bool dirSwitch)
    {
        if (dirSwitch) { owner.facingLeft = !owner.facingLeft; }
        owner.SwitchState(typeof(RunState));
    }
    public override void OnJumpHeld()
    {
        owner.SwitchState(typeof(JumpSquatState));
    }
    public override void OnAbility1Held()
    {
        owner.SwitchState(typeof(PunchStartupState));
    }
}

public class RunState : CharacterState
{
    public RunState(Character owner) : base(owner)
    {

    }

    public override void OnStart()
    {
        owner.rb.linearVelocity = Vector3.right * owner.runSpeed * owner.facingMultiplier;
    }

    public override void OnDirectionStop()
    {
        owner.SwitchState(typeof(IdleState));
    }

    public override void OnDirectionStart(bool dirSwitch)
    {
        if (dirSwitch)
        {
            owner.facingLeft = !owner.facingLeft;
            owner.SwitchState(typeof(RunState));
        }
    }

    public override void OnJumpHeld()
    {
        owner.SwitchState(typeof(JumpSquatState));
    }

    public override void OnLeaveGround()
    {
        owner.SwitchState(typeof(AirMoveState));
    }

    public override void OnAbility1Held()
    {
        owner.SwitchState(typeof(PunchStartupState));
    }
}

public class JumpSquatState : CharacterState
{
    public JumpSquatState(Character owner) : base(owner)
    {
        expirationTime = owner.jumpStartup;
    }

    public override void OnStart()
    {
        owner.rb.linearVelocity = Vector2.zero;
    }

    public override void OnExpiration()
    {
        owner.rb.linearVelocity = Vector3.up *  owner.jumpForce;
        switch (owner.horizontalInputAdjusted)
        {
            case 0:
                owner.SwitchState(typeof(AirStillState));
                break;
            case 1:
                owner.SwitchState(typeof(AirMoveState));
                break;
            case -1:
                owner.facingLeft = !owner.facingLeft;
                owner.SwitchState(typeof(AirMoveState));
                break;
        }
    }
}

public class AirStillState : CharacterState
{
    public AirStillState(Character owner) : base(owner)
    {

    }
    public override void OnStart()
    {
        owner.rb.linearVelocity = new Vector3(0, owner.rb.linearVelocity.y, 0);
    }
    public override void OnDirectionStart(bool dirSwitch)
    {
        if (dirSwitch) { owner.facingLeft = !owner.facingLeft; }
        owner.SwitchState(typeof(AirMoveState));
    }
    public override void OnLand()
    {
        owner.SwitchState(typeof(IdleState));
    }
}

public class AirMoveState : CharacterState
{
    public AirMoveState(Character owner) : base(owner)
    {

    }

    public override void OnStart()
    {
        owner.rb.linearVelocity = new Vector3(owner.airSpeed * owner.facingMultiplier, owner.rb.linearVelocity.y, 0);
    }

    public override void OnDirectionStop()
    {
        owner.SwitchState(typeof(AirStillState));
    }

    public override void OnDirectionStart(bool dirSwitch)
    {
        if (dirSwitch)
        {
            owner.facingLeft = !owner.facingLeft;
            owner.SwitchState(typeof(AirMoveState));
        }
    }

    public override void OnLand()
    {
        owner.SwitchState(typeof(RunState));
    }
}

public class PunchStartupState : CharacterState
{
    public PunchStartupState(Character owner) : base(owner)
    {

    }
    public override void OnStart()
    {
        owner.rb.linearVelocity = Vector2.zero;
        expirationTime = owner.punchStartup;
    }

    public override void OnExpiration()
    {
        owner.HitEnemies(Physics.BoxCastAll((Vector2)owner.transform.position + Vector2.up * 0.5f + Vector2.right * owner.facingMultiplier, Vector2.one * 0.5f, Vector3.right, Quaternion.identity, 0).ToList(), owner.punchDamage);

        owner.SwitchState(typeof(PunchEndlagState));
    }
}

public class PunchEndlagState : CharacterState
{
    public PunchEndlagState(Character owner) : base(owner)
    {

    }
    public override void OnStart()
    {
        expirationTime = owner.punchEndlag;
    }
    public override void OnExpiration()
    {
        owner.SwitchState(typeof(IdleState));
    }
}
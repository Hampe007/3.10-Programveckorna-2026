using System;
using System.Linq;
using UnityEngine;

public class Fatboy : Character
{
    public float chargeStartup;
    public float chargeSpeed;
    public float chargeEndlag;

    public float dashCost;
    public float dashDuration;
    public float dashSpeed;
    public float dashAngle;

    public float slamCost;
    public float slamStartupTime;
    public float slamStartupSpeed;
    public float slamSpeed;
    public float slamEndlag;

    [NonSerialized] public float charge;

    public ParticleSystem deathParticles;


    protected override void StartState()
    {
        SwitchState(typeof(AirStillState));
    }

    protected override void Die()
    {
        deathParticles.Play();
        SwitchState(typeof(InactiveState));
    }

    protected override void GetWebbed()
    {
        if (grounded)
        {
            SwitchState(typeof(WebGroundState));
        }
        else
        {
            SwitchState(typeof(WebAirState));
        }
    }
    public void AddCharge(float charge)
    {
        this.charge += charge;
        this.charge = Mathf.Clamp(this.charge, 0, 100);
    }
    public bool ExpendCharge(float charge)
    {
        if (this.charge >= charge)
        {
            this.charge -= charge;
            return true;
        }
        return false;
    }

    class InactiveState : CharacterState
    {
        public InactiveState(Character owner) : base(owner)
        {
            interruptible = false;
        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector2.zero;
        }
    }
    class IdleState : CharacterState
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
            owner.SwitchState(typeof(ChargeStartupState));
        }
        public override void OnAbility3Held()
        {
            if (((Fatboy)owner).ExpendCharge(((Fatboy)owner).dashCost))
            {
                owner.SwitchState(typeof(DashState));
            }
        }
    }

    class RunState : CharacterState
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
            owner.SwitchState(typeof(ChargeStartupState));
        }
        public override void OnAbility3Held()
        {
            if (((Fatboy)owner).ExpendCharge(((Fatboy)owner).dashCost))
            {
                owner.SwitchState(typeof(DashState));
            }
        }
    }

    class JumpSquatState : CharacterState
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
            owner.rb.linearVelocity = Vector3.up * owner.jumpForce;
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

    class AirStillState : CharacterState
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
        public override void OnAbility2Held()
        {
            if (((Fatboy)owner).ExpendCharge(((Fatboy)owner).slamCost))
            {
                owner.SwitchState(typeof(SlamStartupState));
            }
        }
        public override void OnAbility3Held()
        {
            if (((Fatboy)owner).ExpendCharge(((Fatboy)owner).dashCost))
            {
                owner.SwitchState(typeof(DashState));
            }
        }
    }

    class AirMoveState : CharacterState
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

        public override void OnAbility2Held()
        {
            if (((Fatboy)owner).ExpendCharge(((Fatboy)owner).slamCost))
            {
                owner.SwitchState(typeof(SlamStartupState));
            }
        }
        public override void OnAbility3Held()
        {
            if (((Fatboy)owner).ExpendCharge(((Fatboy)owner).dashCost))
            {
                owner.SwitchState(typeof(DashState));
            }
        }
    }

    class ChargeStartupState : CharacterState
    {
        public ChargeStartupState(Character owner) : base(owner)
        {

        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector3.zero;
        }

        public override void OnExpiration()
        {
            if(owner.input.ability1Held)
            {
                owner.SwitchState(typeof(ChargeActiveState));
            }
            else
            {
                owner.SwitchState(typeof(ChargeEndlagState));
            }
        }
    }

    class ChargeActiveState : CharacterState
    {
        public ChargeActiveState(Character owner) : base(owner)
        {
            
        }

        public override void OnTimeElapsed(float time)
        {
            ((Fatboy)owner).AddCharge(time * ((Fatboy)owner).chargeSpeed);
        }

        public override void OnAbility1Released()
        {
            owner.SwitchState(typeof(ChargeEndlagState));
        }
    }

    class ChargeEndlagState : CharacterState
    {
        public ChargeEndlagState(Character owner) : base(owner)
        {
            expirationTime = ((Fatboy)owner).chargeEndlag;
        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector3.zero;
        }

        public override void OnExpiration()
        {
            owner.SwitchState(typeof(IdleState));
        }
    }

    class SlamStartupState : CharacterState
    {
        public SlamStartupState(Character owner) : base(owner)
        {
            expirationTime = ((Fatboy) owner).slamStartupTime;
            gravity = false;
        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector3.up * ((Fatboy)owner).slamStartupSpeed;
        }

        public override void OnExpiration()
        {
            owner.SwitchState(typeof(SlamFallState));
        }
    }

    class SlamFallState : CharacterState
    {
        public SlamFallState (Character owner) : base(owner)
        {
            gravity = false;
        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector3.down * ((Fatboy)owner).slamSpeed;
        }

        public override void OnLand()
        {
            owner.SwitchState(typeof(SlamEndlagState));
        }
    }

    class SlamEndlagState : CharacterState
    {
        public SlamEndlagState(Character owner) : base(owner)
        {
            expirationTime = ((Fatboy)owner).slamEndlag;
        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector3.zero;
        }

        public override void OnExpiration()
        {
            owner.SwitchState(typeof(IdleState));
        }
    }

    class DashState : CharacterState
    {
        public DashState(Character owner) : base(owner)
        {
            expirationTime = ((Fatboy)owner).dashDuration;
            gravity = false;
        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = new Vector2(Mathf.Cos(Mathf.Deg2Rad * ((Fatboy)owner).dashAngle), Mathf.Sin(Mathf.Deg2Rad * ((Fatboy)owner).dashAngle)) * owner.facingMultiplier * ((Fatboy)owner).dashSpeed;
        }

        public override void OnExpiration()
        {
            owner.rb.linearVelocity = Vector2.zero;
            owner.SwitchState(typeof(AirStillState));
        }
    }

    class WebGroundState : CharacterState
    {
        public WebGroundState(Character owner) : base(owner)
        {
            expirationTime = owner.webTime;
        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector3.zero;
        }

        public override void OnExpiration()
        {
            owner.SwitchState(typeof(IdleState));
        }
    }

    class WebAirState : CharacterState
    {
        public WebAirState(Character owner) : base(owner)
        {
            expirationTime = owner.webTime;
            gravity = false;
        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector3.zero;
        }

        public override void OnExpiration()
        {
            owner.SwitchState(typeof(IdleState));
        }
    }
}

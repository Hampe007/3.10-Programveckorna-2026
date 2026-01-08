using System.Linq;
using UnityEngine;

public class TestCharacter : Character
{
    public float runSpeed;
    public float airSpeed;
    public float jumpStartup;
    public float jumpForce;
    public float punchStartup;
    public float punchEndlag;
    public int punchDamage;


    protected override void StartState()
    {
        SwitchState(typeof(AirStillState));
    }

    protected override void Die()
    {
        SwitchState(typeof(InactiveState));
    }

    class InactiveState : CharacterState
    {
        public InactiveState(Character owner) : base(owner)
        {

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
            owner.SwitchState(typeof(PunchStartupState));
        }
    }

    class RunState : CharacterState
    {
        public RunState(Character owner) : base(owner)
        {

        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector3.right * ((TestCharacter) owner).runSpeed * owner.facingMultiplier;
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

    class JumpSquatState : CharacterState
    {
        public JumpSquatState(Character owner) : base(owner)
        {
            expirationTime = ((TestCharacter)owner).jumpStartup;
        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector2.zero;
        }

        public override void OnExpiration()
        {
            owner.rb.linearVelocity = Vector3.up * ((TestCharacter)owner).jumpForce;
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
    }

    class AirMoveState : CharacterState
    {
        public AirMoveState(Character owner) : base(owner)
        {

        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = new Vector3(((TestCharacter)owner).airSpeed * owner.facingMultiplier, owner.rb.linearVelocity.y, 0);
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

    class PunchStartupState : CharacterState
    {
        public PunchStartupState(Character owner) : base(owner)
        {

        }
        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector2.zero;
            expirationTime = ((TestCharacter)owner).punchStartup;
        }

        public override void OnExpiration()
        {
            owner.HitEnemies(Physics.BoxCastAll((Vector2)owner.transform.position + Vector2.up * 0.5f + Vector2.right * owner.facingMultiplier, Vector2.one * 0.5f, Vector3.right, Quaternion.identity, 0).ToList(), ((TestCharacter)owner).punchDamage);

            owner.SwitchState(typeof(PunchEndlagState));
        }
    }

    class PunchEndlagState : CharacterState
    {
        public PunchEndlagState(Character owner) : base(owner)
        {

        }
        public override void OnStart()
        {
            expirationTime = ((TestCharacter)owner).punchEndlag;
        }
        public override void OnExpiration()
        {
            owner.SwitchState(typeof(IdleState));
        }
    }
}

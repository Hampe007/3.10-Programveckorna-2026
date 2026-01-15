using System.Linq;
using UnityEngine;

public class TestCharacter : Character
{
    public float punchStartup;
    public float punchEndlag;
    public int punchDamage;
    public float projectileStartup;
    public float projectileEndlag;
    public float ascensionSpeed;
    public float maxAscensionTime;

    bool ascendAvailable;

    public GameObject projectilePrefab;
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
        if(grounded)
        {
            SwitchState(typeof(WebGroundState));
        }
        else
        {
            SwitchState(typeof(WebAirState));
        }
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
            owner.SwitchState(typeof(PunchStartupState));
        }
        public override void OnAbility2Held()
        {
            owner.SwitchState(typeof(ProjectileStartupState));
        }

        public override void OnAbility3Held()
        {
            if (((TestCharacter)owner).ascendAvailable)
            {
                owner.SwitchState(typeof(AscendState));
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
            owner.SwitchState(typeof(PunchStartupState));
        }

        public override void OnAbility2Held()
        {
            owner.SwitchState(typeof(ProjectileStartupState));
        }
        public override void OnAbility3Held()
        {
            if (((TestCharacter)owner).ascendAvailable)
            {
                owner.SwitchState(typeof(AscendState));
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
            ((TestCharacter)owner).ascendAvailable = true;
            owner.SwitchState(typeof(IdleState));
        }
        public override void OnAbility3Held()
        {
            if(((TestCharacter)owner).ascendAvailable)
            {
                owner.SwitchState(typeof(AscendState));
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
            ((TestCharacter)owner).ascendAvailable = true;
            owner.SwitchState(typeof(RunState));
        }

        public override void OnAbility3Held()
        {
            if (((TestCharacter)owner).ascendAvailable)
            {
                owner.SwitchState(typeof(AscendState));
            }
        }
    }

    class PunchStartupState : CharacterState
    {
        public PunchStartupState(Character owner) : base(owner)
        {
            expirationTime = ((TestCharacter)owner).punchStartup;
        }
        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector2.zero;
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
            expirationTime = ((TestCharacter)owner).punchEndlag;
        }
        public override void OnExpiration()
        {
            owner.SwitchState(typeof(IdleState));
        }
    }

    class ProjectileStartupState : CharacterState
    {
        public ProjectileStartupState(Character owner) : base(owner)
        {
            expirationTime = ((TestCharacter)owner).projectileStartup;
        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector2.zero;
        }

        public override void OnExpiration()
        {
            TestProjectile projectile = Instantiate(((TestCharacter)owner).projectilePrefab, owner.transform.position + Vector3.up * 0.5f, Quaternion.identity).GetComponent<TestProjectile>();
            projectile.direction = owner.facingMultiplier;
            projectile.ownerId = owner.playerIndex;
            owner.SwitchState(typeof(ProjectileEndlagState));
        }
    }

    class ProjectileEndlagState : CharacterState
    {
        public ProjectileEndlagState(Character owner) : base(owner)
        {
            expirationTime = ((TestCharacter)owner).projectileEndlag;
        }

        public override void OnExpiration()
        {
            owner.SwitchState(typeof(IdleState));
        }
    }

    class AscendState : CharacterState
    {
        public AscendState(Character owner) : base(owner)
        {
            expirationTime = ((TestCharacter)owner).maxAscensionTime;
        }
        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector3.up * ((TestCharacter)owner).ascensionSpeed;
            ((TestCharacter)owner).ascendAvailable = false;
            ((TestCharacter)owner).gravity.active = false;
        }
        public override void OnAbility3Released()
        {
            owner.rb.linearVelocity = owner.rb.linearVelocity / 2;
            ((TestCharacter)owner).gravity.active = true;
            owner.SwitchState(typeof(AirStillState));
        }
        public override void OnInterruption()
        {
            ((TestCharacter)owner).gravity.active = true;
        }
        public override void OnExpiration()
        {
            owner.rb.linearVelocity = owner.rb.linearVelocity / 2;
            ((TestCharacter)owner).gravity.active = true;
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

        public override void OnInterruption()
        {
            
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

        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector3.zero;
        }

        public override void OnLand()
        {
            owner.SwitchState(typeof(WebGroundState));
        }
    }
}

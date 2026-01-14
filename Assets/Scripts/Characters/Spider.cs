using System.Linq;
using UnityEngine;

public class Spider : Character
{
    public float biteStartup;
    public float biteEndlag;
    public int biteDamage;
    public float webBallStartup;
    public float webBallEndlag;
    public float teleportStartup;
    public float teleportEndlag;
    public float teleportActivateEndlag;
    public float projectileCooldown;

    public GameObject projectilePrefab;
    public GameObject teleportPrefab;
    public ParticleSystem deathParticles;

    Vector2 teleportPos;

    public SpiderProjectlie.LaunchAngles projectileAngle;

    public Cooldown webCooldown;
    public Cooldown teleportCooldown;

    void Start()
    {
        webCooldown = new Cooldown(projectileCooldown);
        teleportCooldown = new Cooldown(projectileCooldown);
    }
    protected override void Update()
    {
        base.Update();
        webCooldown.AddTime(Time.deltaTime);
        teleportCooldown.AddTime(Time.deltaTime);
    }
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

    public void TeleportActivate(Vector2 position, bool walled)
    {
        if(!state.interruptible)
        {
            return;
        }
        transform.position = position;
        state.OnInterruption();
        if(walled)
        {
            SwitchState(typeof(TeleportBallActivationAirState));
        }
        else
        {
            SwitchState(typeof(TeleportBallActivationGroundState));
        }
    }

    public SpiderProjectlie.LaunchAngles GetLaunchAngle()
    {
        if(input.upHeld)
        {
            return SpiderProjectlie.LaunchAngles.High;
        }
        if(input.downHeld)
        {
            return SpiderProjectlie.LaunchAngles.Low;
        }
        return SpiderProjectlie.LaunchAngles.Mid;
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
            if(((Spider)owner).webCooldown.ready)
            {
                owner.SwitchState(typeof(WebBallStartupState));
                ((Spider)owner).webCooldown.Reset();
            }
        }
        public override void OnAbility2Held()
        {
            owner.SwitchState(typeof(BiteStartupState));
        }

        public override void OnAbility3Held()
        {
            if (((Spider)owner).teleportCooldown.ready)
            {
                owner.SwitchState(typeof(TeleportBallStartupState));
                ((Spider)owner).teleportCooldown.Reset();
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
            if (((Spider)owner).webCooldown.ready)
            {
                owner.SwitchState(typeof(WebBallStartupState));
                ((Spider)owner).webCooldown.Reset();
            }
        }
        public override void OnAbility2Held()
        {
            owner.SwitchState(typeof(BiteStartupState));
        }

        public override void OnAbility3Held()
        {
            if (((Spider)owner).teleportCooldown.ready)
            {
                owner.SwitchState(typeof(TeleportBallStartupState));
                ((Spider)owner).teleportCooldown.Reset();
            }
        }
    }

    class JumpSquatState : CharacterState
    {
        public JumpSquatState(Character owner) : base(owner)
        {
            expirationTime = owner.jumpStartup;
            gravity = false;
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
    }

    class BiteStartupState : CharacterState
    {
        public BiteStartupState(Character owner) : base(owner)
        {
            expirationTime = ((Spider)owner).biteStartup;
            gravity = false;
        }
        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector2.zero;
        }

        public override void OnExpiration()
        {
            owner.HitEnemies(Physics.BoxCastAll((Vector2)owner.transform.position + Vector2.up * 0.5f + Vector2.right * owner.facingMultiplier, Vector2.one * 0.5f, Vector3.right, Quaternion.identity, 0).ToList(), ((Spider)owner).biteDamage);

            owner.SwitchState(typeof(BiteEndlagState));
        }
    }

    class BiteEndlagState : CharacterState
    {
        public BiteEndlagState(Character owner) : base(owner)
        {
            expirationTime = ((Spider)owner).biteEndlag;
            gravity = false;
        }
        public override void OnExpiration()
        {
            owner.SwitchState(typeof(IdleState));
        }
    }

    class WebBallStartupState : CharacterState
    {
        public WebBallStartupState(Character owner) : base(owner)
        {
            expirationTime = ((Spider)owner).webBallStartup;
            gravity = false;
        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector2.zero;
            ((Spider)owner).projectileAngle = ((Spider)owner).GetLaunchAngle();
        }

        public override void OnExpiration()
        {
            WebProjectile projectile = Instantiate(((Spider)owner).projectilePrefab, owner.transform.position + Vector3.up * 1 + Vector3.forward * 0.5f * owner.facingMultiplier, Quaternion.identity).GetComponent<WebProjectile>();
            projectile.Launch(((Spider)owner).projectileAngle, owner.facingMultiplier);
            projectile.owner = (Spider)owner;
            owner.SwitchState(typeof(WebBallEndlagState));
        }
    }

    class WebBallEndlagState : CharacterState
    {
        public WebBallEndlagState(Character owner) : base(owner)
        {
            expirationTime = ((Spider)owner).webBallEndlag;
            gravity = false;
        }

        public override void OnExpiration()
        {
            owner.SwitchState(typeof(IdleState));
        }
    }

    class WebGroundState : CharacterState
    {
        public WebGroundState(Character owner) : base(owner)
        {
            expirationTime = owner.webTime;
            gravity = false;
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

    class TeleportBallStartupState : CharacterState
    {
        public TeleportBallStartupState(Character owner) : base(owner)
        {
            expirationTime = ((Spider)owner).teleportStartup;
            gravity = false;
        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector2.zero;
            ((Spider)owner).projectileAngle = ((Spider)owner).GetLaunchAngle();
        }

        public override void OnExpiration()
        {
            TeleportProjectile projectile = Instantiate(((Spider)owner).teleportPrefab, owner.transform.position + Vector3.up * 1 + Vector3.forward * 0.5f * owner.facingMultiplier, Quaternion.identity).GetComponent<TeleportProjectile>();
            projectile.Launch(((Spider)owner).projectileAngle, owner.facingMultiplier);
            projectile.owner = (Spider) owner;
            owner.SwitchState(typeof(TeleportBallEndlagState));
        }
    }

    class TeleportBallEndlagState : CharacterState
    {
        public TeleportBallEndlagState(Character owner) : base(owner)
        {
            expirationTime = ((Spider)owner).teleportEndlag;
            gravity = false;
        }

        public override void OnExpiration()
        {
            owner.SwitchState(typeof(IdleState));
        }
    }

    class TeleportBallActivationGroundState : CharacterState
    {
        public TeleportBallActivationGroundState(Character owner) : base(owner)
        {
            expirationTime = ((Spider)owner).teleportActivateEndlag;
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
    class TeleportBallActivationAirState : CharacterState
    {
        public TeleportBallActivationAirState(Character owner) : base(owner)
        {
            expirationTime = ((Spider)owner).teleportActivateEndlag;
            owner.gravity.active = false;
        }

        public override void OnStart()
        {
            owner.rb.linearVelocity = Vector3.zero;
        }

        public override void OnInterruption()
        {
            owner.gravity.active = true;
        }

        public override void OnExpiration()
        {
            owner.gravity.active = true;
            owner.SwitchState(typeof(AirStillState));
        }
    }
}

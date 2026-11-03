using UnityEngine;
using UnityEngine.InputSystem;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class AkioPrototypeController : MonoBehaviour
    {
        public enum DefenseResult
        {
            Hit,
            Guarded,
            Parried,
            Evaded
        }

        private enum ActionState
        {
            Free,
            Dashing,
            Attacking,
            Parrying,
            Guarding,
            Stunned,
            Dead
        }

        private enum AttackKind
        {
            None,
            Light,
            Heavy
        }

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float acceleration = 60f;
        [SerializeField] private float deceleration = 70f;
        [SerializeField] private float airControl = 0.85f;
        [SerializeField] private float jumpVelocity = 13.6f;
        [SerializeField] private float riseGravity = 8f;
        [SerializeField] private float jumpCutGravity = 34f;
        [SerializeField] private float extraFallGravity = 24f;
        [SerializeField] private float coyoteTime = 0.10f;
        [SerializeField] private float inputBuffer = 0.15f;
        [SerializeField] private float groundCheckRadius = 0.20f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float killHeight = -14f;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 24f;
        [SerializeField] private float dashDuration = 0.18f;
        [SerializeField] private float dashCooldown = 0.32f;
        [SerializeField] private float dashInvulnerability = 0.11f;

        [Header("Defense")]
        [SerializeField] private float parryStartup = 0.03f;
        [SerializeField] private float parryWindow = 0.16f;
        [SerializeField] private float parryRecovery = 0.28f;
        [SerializeField] private float guardDamageMultiplier = 0.20f;
        [SerializeField] private float guardBreakDuration = 0.75f;
        [SerializeField] private float hitStunDuration = 0.20f;
        [SerializeField] private float hitInvulnerability = 0.55f;
        [SerializeField] private float counterWindow = 0.6f;
        [SerializeField] private float counterMultiplier = 1.6f;

        [Header("Vitals")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxPosture = 100f;
        [SerializeField] private float postureRegenDelay = 1.25f;
        [SerializeField] private float postureRegenPerSecond = 30f;

        [Header("Combat")]
        [SerializeField] private float attackHeight = 1.65f;
        [SerializeField] private float attackDepth = 1.4f;
        [SerializeField] private float lightRange = 2.15f;
        [SerializeField] private float heavyRange = 2.45f;

        private static readonly Collider[] GroundBuffer = new Collider[12];

        private Rigidbody body;
        private CapsuleCollider capsule;
        private Transform visualRoot;
        private SamuraiVisualRig rig;
        private Vector3 spawnPoint;

        private ActionState state = ActionState.Free;
        private AttackKind attackKind = AttackKind.None;

        private float moveInput;
        private float facing = 1f;
        private bool grounded;
        private float lastGroundedTime = -10f;

        // Input buffers (unscaled time, so they survive hit-stop).
        private float lightBuffer = -10f;
        private float heavyBuffer = -10f;
        private float parryBuffer = -10f;
        private float dashBuffer = -10f;
        private float jumpBuffer = -10f;

        private float dashElapsed;
        private float dashCooldownRemaining;
        private float parryElapsed;
        private float stateTimer;
        private bool stunIsGuardBreak;
        private float externalX;
        private float invulnerableUntil;
        private float counterUntil;

        private float attackElapsed;
        private float attackStartup;
        private float attackActive;
        private float attackRecovery;
        private float attackDamage;
        private float attackPostureDamage;
        private float attackRange;
        private float attackLunge;
        private float attackHitStop;
        private bool attackResolved;
        private bool queuedLight;
        private int comboIndex;
        private float comboResetRemaining;

        private float health;
        private float posture;
        private float lastPostureDamageTime;
        private float deathTimer;

        public float Health => health;
        public float MaxHealth => maxHealth;
        public float Posture => posture;
        public float MaxPosture => maxPosture;
        public bool IsDead => state == ActionState.Dead;
        public float Facing => facing;
        public int ComboIndex => comboIndex;
        public string StateLabel => state.ToString();
        public float DashReady01 => 1f - Mathf.Clamp01(dashCooldownRemaining / Mathf.Max(0.01f, dashCooldown));
        public bool CounterReady => Time.time < counterUntil;
        public float LastDamageTime { get; private set; } = -10f;
        public float LastParryTime { get; private set; } = -10f;
        public float LastGuardBreakTime { get; private set; } = -10f;
        public float DeathProgress => state == ActionState.Dead ? Mathf.Clamp01(1f - deathTimer / 1.8f) : 0f;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();

            health = maxHealth;
            posture = maxPosture;

            body.useGravity = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.constraints = RigidbodyConstraints.FreezePositionZ |
                               RigidbodyConstraints.FreezeRotationX |
                               RigidbodyConstraints.FreezeRotationY |
                               RigidbodyConstraints.FreezeRotationZ;
        }

        public void Initialize(Transform visuals, Vector3 initialSpawnPoint)
        {
            visualRoot = visuals;
            rig = visuals != null ? visuals.GetComponent<SamuraiVisualRig>() : null;
            spawnPoint = initialSpawnPoint;
            health = maxHealth;
            posture = maxPosture;
            UpdateFacingVisual();
        }

        // ------------------------------------------------------------------
        // Update loop
        // ------------------------------------------------------------------

        private void Update()
        {
            float dt = Time.deltaTime;

            UpdateGrounded();

            dashCooldownRemaining = Mathf.Max(0f, dashCooldownRemaining - dt);
            comboResetRemaining = Mathf.Max(0f, comboResetRemaining - dt);

            if (Time.time >= lastPostureDamageTime + postureRegenDelay && posture < maxPosture && state != ActionState.Dead)
            {
                posture = Mathf.Min(maxPosture, posture + postureRegenPerSecond * dt);
            }

            ReadMoveInput();
            BufferInputs();

            if (transform.position.y < killHeight && state != ActionState.Dead)
            {
                FallRespawn();
            }

            RunStateMachine(dt);
            PushStateToRig();
        }

        private void RunStateMachine(float dt)
        {
            switch (state)
            {
                case ActionState.Dead:
                    deathTimer -= dt;
                    if (deathTimer <= 0f)
                    {
                        Respawn();
                    }

                    return;

                case ActionState.Stunned:
                    stateTimer -= dt;
                    if (stateTimer <= 0f)
                    {
                        state = ActionState.Free;
                        stunIsGuardBreak = false;
                    }

                    return;

                case ActionState.Dashing:
                    dashElapsed += dt;
                    if (dashElapsed >= dashDuration)
                    {
                        state = ActionState.Free;
                    }

                    return;

                case ActionState.Parrying:
                    parryElapsed += dt;
                    if (parryElapsed >= parryStartup + parryWindow + parryRecovery)
                    {
                        rig?.EndMove();
                        state = IsGuardHeld() ? ActionState.Guarding : ActionState.Free;
                    }

                    return;

                case ActionState.Attacking:
                    if (Buffered(lightBuffer) && attackKind == AttackKind.Light && comboIndex < 3)
                    {
                        queuedLight = true;
                        lightBuffer = -10f;
                    }

                    // Dash-cancel out of attack recovery keeps combat fluid.
                    bool inRecovery = attackElapsed >= attackStartup + attackActive;
                    if (inRecovery && Buffered(dashBuffer) && dashCooldownRemaining <= 0f)
                    {
                        rig?.EndMove();
                        dashBuffer = -10f;
                        StartDash();
                        return;
                    }

                    AdvanceAttack(dt);
                    return;
            }

            // Free or Guarding.
            if (Mathf.Abs(moveInput) > 0.08f)
            {
                facing = Mathf.Sign(moveInput);
            }

            UpdateFacingVisual();

            if (Buffered(parryBuffer))
            {
                parryBuffer = -10f;
                StartParry();
                return;
            }

            if (Buffered(dashBuffer) && dashCooldownRemaining <= 0f)
            {
                dashBuffer = -10f;
                StartDash();
                return;
            }

            if (Buffered(jumpBuffer) && (grounded || Time.unscaledTime - lastGroundedTime <= coyoteTime))
            {
                jumpBuffer = -10f;
                Jump();
            }

            if (Buffered(heavyBuffer))
            {
                heavyBuffer = -10f;
                StartHeavyAttack();
                return;
            }

            if (Buffered(lightBuffer))
            {
                lightBuffer = -10f;
                int nextCombo = comboResetRemaining > 0f && comboIndex > 0 && comboIndex < 3
                    ? comboIndex + 1
                    : 1;
                StartLightAttack(nextCombo);
                return;
            }

            state = IsGuardHeld() ? ActionState.Guarding : ActionState.Free;
        }

        private void FixedUpdate()
        {
            Vector3 velocity = body.linearVelocity;

            if (state == ActionState.Dead)
            {
                body.linearVelocity = new Vector3(0f, Mathf.Min(0f, velocity.y), 0f);
                return;
            }

            float dt = Time.fixedDeltaTime;

            if (state == ActionState.Dashing)
            {
                body.linearVelocity = new Vector3(facing * dashSpeed, 0f, 0f);
                return;
            }

            float movementMultiplier = state switch
            {
                ActionState.Guarding => 0.35f,
                ActionState.Attacking => 0.18f,
                ActionState.Parrying => 0f,
                ActionState.Stunned => 0f,
                _ => 1f
            };

            float targetSpeed = moveInput * moveSpeed * movementMultiplier;
            float rate = Mathf.Abs(targetSpeed) > Mathf.Abs(velocity.x) ? acceleration : deceleration;
            if (!grounded)
            {
                rate *= airControl;
            }

            float horizontal = Mathf.MoveTowards(velocity.x, targetSpeed, rate * dt);

            // Attack lunge: a short burst of forward momentum through startup and active frames.
            if (state == ActionState.Attacking && attackElapsed < attackStartup + attackActive)
            {
                float t = Mathf.Clamp01(attackElapsed / Mathf.Max(0.01f, attackStartup + attackActive));
                float lunge = attackLunge * (1f - t * 0.6f);
                horizontal = facing * Mathf.Max(Mathf.Abs(horizontal * 0.5f), lunge);
            }

            // Knockback decays quickly and is layered on top of locomotion.
            externalX = Mathf.MoveTowards(externalX, 0f, 38f * dt);
            horizontal += externalX;

            float vertical = velocity.y;
            if (vertical < 0f)
            {
                vertical -= extraFallGravity * dt;
            }
            else if (vertical > 0f)
            {
                vertical -= (IsJumpHeld() ? riseGravity : jumpCutGravity) * dt;
            }

            body.linearVelocity = new Vector3(horizontal, vertical, 0f);
        }

        private void PushStateToRig()
        {
            if (rig == null)
            {
                return;
            }

            Vector3 velocity = body.linearVelocity;
            rig.SetLocomotion(velocity.x, velocity.y, grounded);
            rig.SetGuard(state == ActionState.Guarding);
            rig.SetDashing(state == ActionState.Dashing);
            rig.SetStunned(state == ActionState.Stunned && stunIsGuardBreak);
            rig.SetDead(state == ActionState.Dead);
        }

        // ------------------------------------------------------------------
        // Ground detection (ignores the player's own collider)
        // ------------------------------------------------------------------

        private void UpdateGrounded()
        {
            bool touching = false;
            Vector3 origin = transform.position + Vector3.up * 0.05f;

            int count = Physics.OverlapSphereNonAlloc(
                origin,
                groundCheckRadius,
                GroundBuffer,
                groundMask,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Collider other = GroundBuffer[i];
                if (other == null || other == capsule || other.attachedRigidbody == body)
                {
                    continue;
                }

                // Enemies are not ground: standing on their capsule would let Akio jump off them.
                if (other.GetComponentInParent<PrototypeCombatTarget>() != null)
                {
                    continue;
                }

                touching = true;
                break;
            }

            grounded = touching && body.linearVelocity.y <= 0.5f;

            if (grounded)
            {
                lastGroundedTime = Time.unscaledTime;
            }
        }

        // ------------------------------------------------------------------
        // Input
        // ------------------------------------------------------------------

        private bool Buffered(float time)
        {
            return Time.unscaledTime - time <= inputBuffer;
        }

        private void BufferInputs()
        {
            float now = Time.unscaledTime;

            if (IsLightPressed()) lightBuffer = now;
            if (IsHeavyPressed()) heavyBuffer = now;
            if (IsParryPressed()) parryBuffer = now;
            if (IsDashPressed()) dashBuffer = now;
            if (IsJumpPressed()) jumpBuffer = now;
        }

        private void ReadMoveInput()
        {
            float keyboardInput = 0f;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    keyboardInput -= 1f;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    keyboardInput += 1f;
                }
            }

            float gamepadInput = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue().x : 0f;
            if (Mathf.Abs(gamepadInput) < 0.2f)
            {
                gamepadInput = 0f;
            }

            moveInput = Mathf.Abs(gamepadInput) > Mathf.Abs(keyboardInput) ? gamepadInput : keyboardInput;
        }

        private void Jump()
        {
            Vector3 velocity = body.linearVelocity;
            velocity.y = jumpVelocity;
            body.linearVelocity = velocity;
            grounded = false;
            lastGroundedTime = -10f;
            CombatFeedback.Instance.Dust(transform.position + Vector3.up * 0.05f, 4);
        }

        // ------------------------------------------------------------------
        // Actions
        // ------------------------------------------------------------------

        private void FaceInputOrTarget()
        {
            if (Mathf.Abs(moveInput) > 0.08f)
            {
                facing = Mathf.Sign(moveInput);
            }
            else
            {
                // Soft aim assist: swing towards a nearby enemy when no direction is held.
                PrototypeCombatTarget nearest = PrototypeCombatTarget.FindNearest(transform.position, 3.4f);
                if (nearest != null)
                {
                    float direction = nearest.transform.position.x - transform.position.x;
                    if (Mathf.Abs(direction) > 0.1f)
                    {
                        facing = Mathf.Sign(direction);
                    }
                }
            }

            UpdateFacingVisual();
        }

        private void StartDash()
        {
            if (Mathf.Abs(moveInput) > 0.08f)
            {
                facing = Mathf.Sign(moveInput);
                UpdateFacingVisual();
            }

            state = ActionState.Dashing;
            attackKind = AttackKind.None;
            dashElapsed = 0f;
            dashCooldownRemaining = dashCooldown;
            comboResetRemaining = 0f;
            queuedLight = false;
            CombatFeedback.Instance.Dust(transform.position + Vector3.up * 0.05f, 5);
        }

        private void StartParry()
        {
            FaceInputOrTarget();
            state = ActionState.Parrying;
            parryElapsed = 0f;
            comboResetRemaining = 0f;
            rig?.BeginMove(SamuraiVisualRig.Move.Parry, parryStartup, parryWindow, parryRecovery);
        }

        private void StartLightAttack(int step)
        {
            FaceInputOrTarget();
            comboIndex = Mathf.Clamp(step, 1, 3);
            attackKind = AttackKind.Light;
            queuedLight = false;

            switch (comboIndex)
            {
                case 1:
                    BeginAttack(SamuraiVisualRig.Move.Light1, 0.10f, 0.10f, 0.18f, 10f, 8f, lightRange, 3.2f, 0.045f);
                    break;
                case 2:
                    BeginAttack(SamuraiVisualRig.Move.Light2, 0.11f, 0.10f, 0.19f, 11f, 9f, lightRange, 3.6f, 0.045f);
                    break;
                default:
                    BeginAttack(SamuraiVisualRig.Move.Light3, 0.14f, 0.12f, 0.28f, 16f, 14f, lightRange + 0.15f, 8.5f, 0.085f);
                    break;
            }
        }

        private void StartHeavyAttack()
        {
            FaceInputOrTarget();
            comboIndex = 0;
            comboResetRemaining = 0f;
            attackKind = AttackKind.Heavy;
            queuedLight = false;
            BeginAttack(SamuraiVisualRig.Move.Heavy, 0.35f, 0.14f, 0.38f, 28f, 32f, heavyRange, 5.5f, 0.10f);
        }

        private void BeginAttack(
            SamuraiVisualRig.Move move,
            float startup,
            float active,
            float recovery,
            float damage,
            float postureDamage,
            float range,
            float lunge,
            float hitStop)
        {
            bool counter = Time.time < counterUntil;
            if (counter)
            {
                counterUntil = 0f;
            }

            state = ActionState.Attacking;
            attackElapsed = 0f;
            attackStartup = startup;
            attackActive = active;
            attackRecovery = recovery;
            attackDamage = damage * (counter ? counterMultiplier : 1f);
            attackPostureDamage = postureDamage * (counter ? counterMultiplier : 1f);
            attackRange = range;
            attackLunge = lunge;
            attackHitStop = hitStop * (counter ? 1.4f : 1f);
            attackResolved = false;

            rig?.BeginMove(move, startup, active, recovery);
        }

        private void AdvanceAttack(float dt)
        {
            attackElapsed += dt;

            if (!attackResolved && attackElapsed >= attackStartup)
            {
                attackResolved = true;
                ResolveAttack();
            }

            float totalDuration = attackStartup + attackActive + attackRecovery;
            if (attackElapsed < totalDuration)
            {
                return;
            }

            if (queuedLight && attackKind == AttackKind.Light && comboIndex < 3)
            {
                StartLightAttack(comboIndex + 1);
                return;
            }

            if (attackKind == AttackKind.Light)
            {
                comboResetRemaining = 0.35f;
            }
            else
            {
                comboIndex = 0;
            }

            attackKind = AttackKind.None;
            rig?.EndMove();
            state = ActionState.Free;
        }

        private void ResolveAttack()
        {
            Vector3 center = transform.position +
                             Vector3.up * (attackHeight * 0.55f) +
                             Vector3.right * facing * (attackRange * 0.52f);

            Vector3 halfExtents = new(
                attackRange * 0.5f,
                attackHeight * 0.5f,
                attackDepth * 0.5f);

            Collider[] hits = Physics.OverlapBox(
                center,
                halfExtents,
                Quaternion.identity,
                ~0,
                QueryTriggerInteraction.Ignore);

            System.Collections.Generic.HashSet<PrototypeCombatTarget> resolvedTargets = new();
            bool anyHit = false;
            PrototypeCombatTarget.HitResult strongest = PrototypeCombatTarget.HitResult.Ignored;
            Vector3 contact = center;

            foreach (Collider hit in hits)
            {
                PrototypeCombatTarget target = hit.GetComponentInParent<PrototypeCombatTarget>();
                if (target == null || !resolvedTargets.Add(target))
                {
                    continue;
                }

                PrototypeCombatTarget.HitResult result = target.ApplyHit(attackDamage, attackPostureDamage, facing);
                if (result == PrototypeCombatTarget.HitResult.Ignored)
                {
                    continue;
                }

                anyHit = true;
                contact = target.transform.position + Vector3.up * 1.3f - Vector3.right * facing * 0.25f;
                contact.z = transform.position.z - 0.3f;

                if (result > strongest)
                {
                    strongest = result;
                }
            }

            if (anyHit)
            {
                OnAttackConnected(strongest, contact);
            }
        }

        private void OnAttackConnected(PrototypeCombatTarget.HitResult result, Vector3 contact)
        {
            CombatFeedback fx = CombatFeedback.Instance;
            bool heavy = attackKind == AttackKind.Heavy || comboIndex == 3;
            float hitStop = attackHitStop;
            float shake = heavy ? 0.16f : 0.07f;
            int sparks = heavy ? 18 : 9;

            switch (result)
            {
                case PrototypeCombatTarget.HitResult.PostureBroken:
                    hitStop = 0.14f;
                    shake = 0.22f;
                    sparks = 26;
                    fx.Pulse(contact, new Color(1f, 0.85f, 0.7f, 0.9f), 0.6f, 3.4f, 0.3f);
                    fx.ZoomPunch(0.2f);
                    break;
                case PrototypeCombatTarget.HitResult.Killed:
                    hitStop = 0.22f;
                    shake = 0.3f;
                    sparks = 40;
                    fx.Pulse(contact, new Color(1f, 0.2f, 0.12f, 1f), 0.8f, 5f, 0.4f);
                    fx.ZoomPunch(0.45f);
                    break;
            }

            fx.Sparks(contact, new Vector3(facing, 0.5f, 0f), sparks, heavy ? 15f : 11f, false);
            fx.LightFlash(contact + Vector3.back * 0.8f, new Color(1f, 0.35f, 0.15f), heavy ? 8f : 5f, 5.5f, 0.12f);
            fx.HitStop(hitStop);
            fx.Shake(shake, 0.16f);
        }

        // ------------------------------------------------------------------
        // Defence
        // ------------------------------------------------------------------

        public DefenseResult ReceiveIncomingAttack(float damage, float postureDamage, float attackerX, bool unblockable = false)
        {
            if (state == ActionState.Dead || Time.time < invulnerableUntil)
            {
                return DefenseResult.Evaded;
            }

            if (state == ActionState.Dashing && dashElapsed <= dashInvulnerability)
            {
                OnEvaded();
                return DefenseResult.Evaded;
            }

            float toAttacker = attackerX - transform.position.x;
            bool attackerInFront = toAttacker * facing >= -0.3f;
            float away = toAttacker >= 0f ? -1f : 1f;
            Vector3 contact = transform.position + Vector3.up * 1.3f - Vector3.right * away * 0.45f;
            contact.z = transform.position.z - 0.3f;

            bool parryActive = state == ActionState.Parrying &&
                               attackerInFront &&
                               parryElapsed >= parryStartup &&
                               parryElapsed <= parryStartup + parryWindow;

            if (parryActive)
            {
                OnParried(contact);
                return DefenseResult.Parried;
            }

            if (!unblockable && state == ActionState.Guarding && attackerInFront)
            {
                health -= damage * guardDamageMultiplier;
                DamagePosture(postureDamage);
                externalX = away * 5f;

                CombatFeedback fx = CombatFeedback.Instance;
                fx.Sparks(contact, new Vector3(-away, 0.4f, 0f), 12, 9f, false);
                fx.LightFlash(contact + Vector3.back * 0.8f, new Color(1f, 0.55f, 0.2f), 4f, 4.5f, 0.1f);
                fx.HitStop(0.05f);
                fx.Shake(0.08f, 0.12f);

                if (posture <= 0f)
                {
                    BreakGuard();
                }

                CheckDeath();
                return DefenseResult.Guarded;
            }

            health -= damage;
            DamagePosture(postureDamage * 0.35f);
            LastDamageTime = Time.unscaledTime;
            invulnerableUntil = Time.time + hitInvulnerability;
            externalX = away * 8f;

            CombatFeedback hitFx = CombatFeedback.Instance;
            hitFx.Sparks(contact, new Vector3(-away, 0.5f, 0f), 14, 10f, false);
            hitFx.HitStop(0.07f);
            hitFx.Shake(0.15f, 0.2f);
            rig?.TriggerHurt();

            if (health > 0f && state != ActionState.Stunned)
            {
                // Hit-stun interrupts whatever Akio was doing.
                rig?.EndMove();
                attackKind = AttackKind.None;
                queuedLight = false;
                comboIndex = 0;
                comboResetRemaining = 0f;
                state = ActionState.Stunned;
                stunIsGuardBreak = false;
                stateTimer = hitStunDuration;
            }

            CheckDeath();
            return DefenseResult.Hit;
        }

        private void OnParried(Vector3 contact)
        {
            posture = Mathf.Min(maxPosture, posture + 15f);
            LastParryTime = Time.unscaledTime;
            counterUntil = Time.time + counterWindow;

            // Reward: cut the parry recovery short so Akio can counter immediately.
            parryElapsed = Mathf.Max(parryElapsed, parryStartup + parryWindow + parryRecovery - 0.06f);

            CombatFeedback fx = CombatFeedback.Instance;
            fx.Sparks(contact, new Vector3(facing, 0.2f, 0f), 30, 16f, true);
            fx.Pulse(contact, new Color(1f, 0.95f, 0.85f, 1f), 0.5f, 4.2f, 0.28f);
            fx.LightFlash(contact + Vector3.back * 0.8f, new Color(1f, 0.9f, 0.8f), 10f, 7f, 0.16f);
            fx.HitStop(0.12f);
            fx.Shake(0.18f, 0.18f);
            fx.ZoomPunch(0.3f);
            rig?.Flash(new Color(3f, 2.4f, 1.8f), 0.2f);
        }

        private void OnEvaded()
        {
            posture = Mathf.Min(maxPosture, posture + 8f);
            CombatFeedback fx = CombatFeedback.Instance;
            fx.Pulse(transform.position + Vector3.up * 1.2f, new Color(0.9f, 0.95f, 1f, 0.8f), 0.6f, 2.8f, 0.25f);
            rig?.Flash(new Color(1.6f, 1.8f, 2.2f), 0.14f);
        }

        private void DamagePosture(float amount)
        {
            posture = Mathf.Max(0f, posture - amount);
            lastPostureDamageTime = Time.time;
        }

        private void BreakGuard()
        {
            posture = 0f;
            state = ActionState.Stunned;
            stunIsGuardBreak = true;
            stateTimer = guardBreakDuration;
            LastGuardBreakTime = Time.unscaledTime;
            rig?.EndMove();

            CombatFeedback fx = CombatFeedback.Instance;
            fx.Pulse(transform.position + Vector3.up * 1.3f, new Color(1f, 0.2f, 0.1f, 0.9f), 0.5f, 3.4f, 0.35f);
            fx.HitStop(0.1f);
            fx.Shake(0.2f, 0.25f);
        }

        private void CheckDeath()
        {
            if (health > 0f)
            {
                return;
            }

            health = 0f;
            state = ActionState.Dead;
            deathTimer = 1.8f;
            body.linearVelocity = Vector3.zero;
            rig?.EndMove();

            CombatFeedback fx = CombatFeedback.Instance;
            fx.HitStop(0.2f);
            fx.ZoomPunch(0.6f);
            fx.Shake(0.28f, 0.4f);
        }

        private void Respawn()
        {
            transform.position = spawnPoint;
            body.linearVelocity = Vector3.zero;
            health = maxHealth;
            posture = maxPosture;
            comboIndex = 0;
            comboResetRemaining = 0f;
            externalX = 0f;
            invulnerableUntil = Time.time + 1f;
            state = ActionState.Free;
        }

        private void FallRespawn()
        {
            transform.position = spawnPoint;
            body.linearVelocity = Vector3.zero;
            health = Mathf.Max(1f, health - 15f);
            LastDamageTime = Time.unscaledTime;
            invulnerableUntil = Time.time + 1f;
            state = ActionState.Free;
        }

        private void UpdateFacingVisual()
        {
            if (visualRoot == null)
            {
                return;
            }

            Vector3 scale = visualRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * facing;
            visualRoot.localScale = scale;
        }

        // ------------------------------------------------------------------
        // Input helpers
        // ------------------------------------------------------------------

        private static bool IsJumpPressed()
        {
            return (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
                   (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
        }

        private static bool IsJumpHeld()
        {
            return (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) ||
                   (Gamepad.current != null && Gamepad.current.buttonSouth.isPressed);
        }

        private static bool IsDashPressed()
        {
            return (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame) ||
                   (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);
        }

        private static bool IsLightPressed()
        {
            bool keyboard = Keyboard.current != null &&
                            (Keyboard.current.jKey.wasPressedThisFrame ||
                             Keyboard.current.enterKey.wasPressedThisFrame);

            bool mouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool gamepad = Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;
            return keyboard || mouse || gamepad;
        }

        private static bool IsHeavyPressed()
        {
            bool keyboard = Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame;
            bool mouse = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
            bool gamepad = Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame;
            return keyboard || mouse || gamepad;
        }

        private static bool IsParryPressed()
        {
            return (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) ||
                   (Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame);
        }

        private static bool IsGuardHeld()
        {
            bool keyboard = Keyboard.current != null && Keyboard.current.lKey.isPressed;
            bool gamepad = Gamepad.current != null && Gamepad.current.leftTrigger.ReadValue() > 0.45f;
            return keyboard || gamepad;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            float previewRange = attackKind == AttackKind.Heavy ? heavyRange : lightRange;
            Vector3 center = transform.position +
                             Vector3.up * (attackHeight * 0.55f) +
                             Vector3.right * facing * (previewRange * 0.52f);

            Gizmos.DrawWireCube(center, new Vector3(previewRange, attackHeight, attackDepth));
        }
    }
}

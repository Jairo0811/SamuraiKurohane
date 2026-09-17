using System.Collections.Generic;
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
        [SerializeField] private float acceleration = 45f;
        [SerializeField] private float deceleration = 55f;
        [SerializeField] private float jumpVelocity = 10.5f;
        [SerializeField] private float extraFallGravity = 24f;
        [SerializeField] private float groundCheckRadius = 0.22f;
        [SerializeField] private LayerMask groundMask = ~0;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 24f;
        [SerializeField] private float dashDuration = 0.18f;
        [SerializeField] private float dashCooldown = 0.32f;
        [SerializeField] private float dashInvulnerability = 0.08f;

        [Header("Defense")]
        [SerializeField] private float parryStartup = 0.03f;
        [SerializeField] private float parryWindow = 0.14f;
        [SerializeField] private float parryRecovery = 0.28f;
        [SerializeField] private float guardDamageMultiplier = 0.20f;
        [SerializeField] private float guardBreakDuration = 0.75f;

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

        private Rigidbody body;
        private CapsuleCollider capsule;
        private Transform visualRoot;
        private Vector3 spawnPoint;

        private ActionState state = ActionState.Free;
        private AttackKind attackKind = AttackKind.None;

        private float moveInput;
        private float facing = 1f;
        private bool grounded;

        private float dashElapsed;
        private float dashCooldownRemaining;
        private float parryElapsed;
        private float stateTimer;

        private float attackElapsed;
        private float attackStartup;
        private float attackActive;
        private float attackRecovery;
        private float attackDamage;
        private float attackPostureDamage;
        private float attackRange;
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
            spawnPoint = initialSpawnPoint;
            health = maxHealth;
            posture = maxPosture;
            UpdateFacingVisual();
        }

        private void Update()
        {
            grounded = Physics.CheckSphere(
                transform.position + Vector3.up * 0.08f,
                groundCheckRadius,
                groundMask,
                QueryTriggerInteraction.Ignore);

            dashCooldownRemaining = Mathf.Max(0f, dashCooldownRemaining - Time.deltaTime);
            comboResetRemaining = Mathf.Max(0f, comboResetRemaining - Time.deltaTime);

            if (Time.time >= lastPostureDamageTime + postureRegenDelay && posture < maxPosture && state != ActionState.Dead)
            {
                posture = Mathf.Min(maxPosture, posture + postureRegenPerSecond * Time.deltaTime);
            }

            ReadMoveInput();

            if (Mathf.Abs(moveInput) > 0.08f && state != ActionState.Dashing)
            {
                facing = Mathf.Sign(moveInput);
                UpdateFacingVisual();
            }

            if (state == ActionState.Dead)
            {
                deathTimer -= Time.deltaTime;
                if (deathTimer <= 0f)
                {
                    Respawn();
                }

                return;
            }

            if (state == ActionState.Stunned)
            {
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    state = ActionState.Free;
                }

                return;
            }

            if (state == ActionState.Dashing)
            {
                dashElapsed += Time.deltaTime;
                if (dashElapsed >= dashDuration)
                {
                    state = ActionState.Free;
                }

                return;
            }

            if (state == ActionState.Parrying)
            {
                parryElapsed += Time.deltaTime;
                if (parryElapsed >= parryStartup + parryWindow + parryRecovery)
                {
                    state = IsGuardHeld() ? ActionState.Guarding : ActionState.Free;
                }

                return;
            }

            bool lightPressed = IsLightPressed();

            if (state == ActionState.Attacking)
            {
                if (lightPressed && attackKind == AttackKind.Light && comboIndex < 3)
                {
                    queuedLight = true;
                }

                AdvanceAttack();
                return;
            }

            if (IsParryPressed())
            {
                StartParry();
                return;
            }

            if (IsDashPressed() && dashCooldownRemaining <= 0f)
            {
                StartDash();
                return;
            }

            if (IsJumpPressed() && grounded)
            {
                Jump();
            }

            if (IsHeavyPressed())
            {
                StartHeavyAttack();
                return;
            }

            if (lightPressed)
            {
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
                body.linearVelocity = Vector3.zero;
                return;
            }

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
            float horizontal = Mathf.MoveTowards(velocity.x, targetSpeed, rate * Time.fixedDeltaTime);

            float vertical = velocity.y;
            if (vertical < 0f)
            {
                vertical -= extraFallGravity * Time.fixedDeltaTime;
            }

            body.linearVelocity = new Vector3(horizontal, vertical, 0f);
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
            moveInput = Mathf.Abs(gamepadInput) > Mathf.Abs(keyboardInput) ? gamepadInput : keyboardInput;
        }

        private void Jump()
        {
            Vector3 velocity = body.linearVelocity;
            velocity.y = jumpVelocity;
            body.linearVelocity = velocity;
        }

        private void StartDash()
        {
            state = ActionState.Dashing;
            dashElapsed = 0f;
            dashCooldownRemaining = dashCooldown;
            comboResetRemaining = 0f;
        }

        private void StartParry()
        {
            state = ActionState.Parrying;
            parryElapsed = 0f;
            comboResetRemaining = 0f;
        }

        private void StartLightAttack(int step)
        {
            comboIndex = Mathf.Clamp(step, 1, 3);
            attackKind = AttackKind.Light;
            queuedLight = false;

            switch (comboIndex)
            {
                case 1:
                    BeginAttack(0.10f, 0.10f, 0.18f, 10f, 8f, lightRange);
                    break;
                case 2:
                    BeginAttack(0.11f, 0.10f, 0.19f, 11f, 9f, lightRange);
                    break;
                default:
                    BeginAttack(0.14f, 0.12f, 0.28f, 16f, 14f, lightRange + 0.15f);
                    break;
            }
        }

        private void StartHeavyAttack()
        {
            comboIndex = 0;
            comboResetRemaining = 0f;
            attackKind = AttackKind.Heavy;
            queuedLight = false;
            BeginAttack(0.35f, 0.14f, 0.38f, 28f, 32f, heavyRange);
        }

        private void BeginAttack(
            float startup,
            float active,
            float recovery,
            float damage,
            float postureDamage,
            float range)
        {
            state = ActionState.Attacking;
            attackElapsed = 0f;
            attackStartup = startup;
            attackActive = active;
            attackRecovery = recovery;
            attackDamage = damage;
            attackPostureDamage = postureDamage;
            attackRange = range;
            attackResolved = false;
        }

        private void AdvanceAttack()
        {
            attackElapsed += Time.deltaTime;

            if (!attackResolved &&
                attackElapsed >= attackStartup &&
                attackElapsed <= attackStartup + attackActive)
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

            HashSet<PrototypeCombatTarget> resolvedTargets = new();

            foreach (Collider hit in hits)
            {
                PrototypeCombatTarget target = hit.GetComponentInParent<PrototypeCombatTarget>();
                if (target == null || !resolvedTargets.Add(target))
                {
                    continue;
                }

                target.ApplyHit(attackDamage, attackPostureDamage, facing);
            }
        }

        public DefenseResult ReceiveIncomingAttack(float damage, float postureDamage)
        {
            if (state == ActionState.Dead)
            {
                return DefenseResult.Evaded;
            }

            if (state == ActionState.Dashing && dashElapsed <= dashInvulnerability)
            {
                return DefenseResult.Evaded;
            }

            bool parryActive = state == ActionState.Parrying &&
                               parryElapsed >= parryStartup &&
                               parryElapsed <= parryStartup + parryWindow;

            if (parryActive)
            {
                posture = Mathf.Min(maxPosture, posture + 15f);
                return DefenseResult.Parried;
            }

            if (state == ActionState.Guarding || IsGuardHeld())
            {
                health -= damage * guardDamageMultiplier;
                DamagePosture(postureDamage);

                if (posture <= 0f)
                {
                    BreakGuard();
                }

                CheckDeath();
                return DefenseResult.Guarded;
            }

            health -= damage;
            DamagePosture(postureDamage * 0.35f);
            CheckDeath();
            return DefenseResult.Hit;
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
            stateTimer = guardBreakDuration;
        }

        private void CheckDeath()
        {
            if (health > 0f)
            {
                return;
            }

            health = 0f;
            state = ActionState.Dead;
            deathTimer = 1.5f;
            body.linearVelocity = Vector3.zero;
        }

        private void Respawn()
        {
            transform.position = spawnPoint;
            body.linearVelocity = Vector3.zero;
            health = maxHealth;
            posture = maxPosture;
            comboIndex = 0;
            comboResetRemaining = 0f;
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

        private static bool IsJumpPressed()
        {
            return (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
                   (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
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

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            const float width = 360f;
            Rect panel = new(18f, 18f, width, 126f);
            GUI.Box(panel, string.Empty);

            GUI.Label(new Rect(32f, 28f, width - 20f, 22f), "AKIO KUROHANE — GAMEPLAY PROTOTYPE v0.1");
            GUI.Label(new Rect(32f, 52f, width - 20f, 22f), $"HP {health:0}/{maxHealth:0}    POSTURE {posture:0}/{maxPosture:0}");
            GUI.Label(new Rect(32f, 76f, width - 20f, 22f), $"STATE: {state}    COMBO: {comboIndex}");
            GUI.Label(new Rect(32f, 100f, width - 20f, 38f), "A/D Move  |  Space Jump  |  Shift Dash  |  J/LMB Light  |  K/RMB Heavy  |  Q Parry  |  L Guard");
        }
    }
}

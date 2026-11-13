using System.Collections.Generic;
using UnityEngine;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    /// <summary>
    /// Imperial Guard training enemy. Faces and paces Akio, mixes three telegraphed attacks
    /// (blockable slash, unblockable overhead that must be dodged, gap-closing thrust) and uses
    /// the posture system: parry or break its posture to open it up for a punish.
    /// </summary>
    public sealed class PrototypeCombatTarget : MonoBehaviour
    {
        public enum HitResult
        {
            Ignored = 0,
            Hit = 1,
            PostureBroken = 2,
            Killed = 3
        }

        private enum TargetState
        {
            Idle,
            Telegraphing,
            Striking,
            Recovering,
            PostureBroken,
            Dead
        }

        private struct AttackDef
        {
            public SamuraiVisualRig.Move Move;
            public float Telegraph;
            public float Strike;
            public float Recovery;
            public float Damage;
            public float Posture;
            public float Range;
            public float LungeSpeed;
            public bool Unblockable;
            public Color Glow;
        }

        private static readonly List<PrototypeCombatTarget> Registry = new();

        public static PrototypeCombatTarget FindNearest(Vector3 position, float maxDistance)
        {
            PrototypeCombatTarget best = null;
            float bestDistance = maxDistance;

            foreach (PrototypeCombatTarget target in Registry)
            {
                if (target == null || target.IsDead)
                {
                    continue;
                }

                float distance = Mathf.Abs(target.transform.position.x - position.x);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
            }

            return best;
        }

        public static IReadOnlyList<PrototypeCombatTarget> All => Registry;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Registry.Clear();
        }

        [Header("Identity")]
        [SerializeField] private string displayName = "IMPERIAL GUARD";

        [Header("Vitals")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxPosture = 80f;
        [SerializeField] private float postureRegenDelay = 2.5f;
        [SerializeField] private float postureRegenPerSecond = 10f;

        [Header("Behaviour")]
        [SerializeField] private float detectionRange = 9f;
        [SerializeField] private float walkSpeed = 2.8f;
        [SerializeField] private float preferredMinDistance = 1.7f;
        [SerializeField] private float preferredMaxDistance = 2.5f;
        [SerializeField] private float attackInterval = 1.9f;
        [SerializeField] private float leashRadius = 11f;
        [SerializeField] private float parryPostureDamage = 34f;

        [Header("Recovery")]
        [SerializeField] private float postureBrokenDuration = 1.3f;
        [SerializeField] private float respawnDelay = 2.4f;

        private AkioPrototypeController player;
        private SamuraiVisualRig rig;
        private Rigidbody body;
        private Collider bodyCollider;

        private TargetState state = TargetState.Idle;
        private AttackDef currentAttack;
        private bool followUpPending;
        private float health;
        private float posture;
        private float stateTimer;
        private float attackTimer;
        private float lastPostureDamageTime;
        private float facing = -1f;
        private float walkVelocity;
        private float knockback;
        private float lungeVelocity;
        private Vector3 homePosition;
        private float levelMinX = -35f;
        private float levelMaxX = 35f;

        public string DisplayName => displayName;
        public float Health => health;
        public float MaxHealth => maxHealth;
        public float Posture => posture;
        public float MaxPosture => maxPosture;
        public bool IsDead => state == TargetState.Dead;
        public bool IsTelegraphing => state == TargetState.Telegraphing;
        public bool TelegraphIsUnblockable => state == TargetState.Telegraphing && currentAttack.Unblockable;
        public bool IsPostureBroken => state == TargetState.PostureBroken;
        public float HeadHeight => 3.1f;

        public void SetDisplayName(string value)
        {
            displayName = value;
        }

        private void OnEnable()
        {
            if (!Registry.Contains(this))
            {
                Registry.Add(this);
            }
        }

        private void OnDisable()
        {
            Registry.Remove(this);
        }

        private void Awake()
        {
            bodyCollider = GetComponent<Collider>();
            health = maxHealth;
            posture = maxPosture;
            attackTimer = attackInterval;
            homePosition = transform.position;
        }

        public void Initialize(AkioPrototypeController targetPlayer, SamuraiVisualRig visualRig, float minX, float maxX)
        {
            player = targetPlayer;
            rig = visualRig;
            levelMinX = minX;
            levelMaxX = maxX;
            homePosition = transform.position;

            body = GetComponent<Rigidbody>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody>();
            }

            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            FacePlayer();
        }

        // ------------------------------------------------------------------
        // Update
        // ------------------------------------------------------------------

        private void Update()
        {
            if (player == null || rig == null)
            {
                return;
            }

            float dt = Time.deltaTime;

            if (state != TargetState.Dead &&
                state != TargetState.PostureBroken &&
                Time.time >= lastPostureDamageTime + postureRegenDelay &&
                posture < maxPosture)
            {
                posture = Mathf.Min(maxPosture, posture + postureRegenPerSecond * dt);
            }

            walkVelocity = 0f;

            switch (state)
            {
                case TargetState.Dead:
                    stateTimer -= dt;
                    if (stateTimer <= 0f)
                    {
                        Respawn();
                    }

                    break;

                case TargetState.PostureBroken:
                    stateTimer -= dt;
                    if (stateTimer <= 0f)
                    {
                        posture = maxPosture * 0.55f;
                        state = TargetState.Idle;
                        attackTimer = 0.9f;
                        rig.SetStunned(false);
                    }

                    break;

                case TargetState.Telegraphing:
                    stateTimer -= dt;
                    if (stateTimer <= 0f)
                    {
                        BeginStrike();
                    }

                    break;

                case TargetState.Striking:
                    stateTimer -= dt;
                    lungeVelocity = currentAttack.LungeSpeed * facing;
                    if (stateTimer <= 0f)
                    {
                        ResolveStrike();
                    }

                    break;

                case TargetState.Recovering:
                    lungeVelocity = 0f;
                    stateTimer -= dt;
                    if (stateTimer <= 0f)
                    {
                        rig.EndMove();
                        state = TargetState.Idle;
                        attackTimer = followUpPending ? 0.12f : attackInterval * Random.Range(0.8f, 1.25f);
                    }

                    break;

                default:
                    UpdateIdle(dt);
                    break;
            }

            rig.SetLocomotion(walkVelocity * facing, 0f, true);
        }

        private void FixedUpdate()
        {
            if (body == null || state == TargetState.Dead)
            {
                return;
            }

            float dt = Time.fixedDeltaTime;
            float velocity = walkVelocity * facing + knockback + (state == TargetState.Striking ? lungeVelocity : 0f);
            knockback = Mathf.MoveTowards(knockback, 0f, 18f * dt);

            if (Mathf.Abs(velocity) < 0.001f)
            {
                return;
            }

            Vector3 position = body.position;
            float minX = Mathf.Max(levelMinX, homePosition.x - leashRadius);
            float maxX = Mathf.Min(levelMaxX, homePosition.x + leashRadius);
            position.x = Mathf.Clamp(position.x + velocity * dt, minX, maxX);
            body.MovePosition(position);
        }

        private void UpdateIdle(float dt)
        {
            FacePlayer();

            if (player.IsDead)
            {
                return;
            }

            float signedDistance = player.transform.position.x - transform.position.x;
            float distance = Mathf.Abs(signedDistance);

            if (distance > detectionRange)
            {
                return;
            }

            // Spacing: close the gap, hold the preferred band, back off if crowded.
            if (distance > preferredMaxDistance)
            {
                walkVelocity = walkSpeed;
            }
            else if (distance < preferredMinDistance)
            {
                walkVelocity = -walkSpeed * 0.7f;
            }

            if (followUpPending)
            {
                // Chain the quick follow-up as soon as the short recovery ends.
                followUpPending = false;
                StartAttack(QuickSlash());
                return;
            }

            attackTimer -= dt;
            if (attackTimer > 0f)
            {
                return;
            }

            if (distance <= 3.1f)
            {
                StartAttack(Random.value < 0.58f ? Slash() : Overhead());
            }
            else if (distance <= 6.5f && Random.value < 0.65f)
            {
                StartAttack(Thrust());
            }
            else
            {
                attackTimer = 0.35f;
            }
        }

        // ------------------------------------------------------------------
        // Attacks
        // ------------------------------------------------------------------

        private static AttackDef Slash() => new()
        {
            Move = SamuraiVisualRig.Move.Slash,
            Telegraph = 0.5f, Strike = 0.12f, Recovery = 0.7f,
            Damage = 12f, Posture = 26f, Range = 2.8f, LungeSpeed = 1.5f,
            Unblockable = false, Glow = new Color(3.2f, 1.1f, 0.1f)
        };

        private static AttackDef QuickSlash() => new()
        {
            Move = SamuraiVisualRig.Move.Slash,
            Telegraph = 0.34f, Strike = 0.12f, Recovery = 0.85f,
            Damage = 10f, Posture = 22f, Range = 2.8f, LungeSpeed = 1.5f,
            Unblockable = false, Glow = new Color(3.2f, 1.1f, 0.1f)
        };

        private static AttackDef Overhead() => new()
        {
            Move = SamuraiVisualRig.Move.Overhead,
            Telegraph = 0.85f, Strike = 0.14f, Recovery = 1.0f,
            Damage = 22f, Posture = 40f, Range = 3.0f, LungeSpeed = 3f,
            Unblockable = true, Glow = new Color(4.5f, 0.12f, 0.05f)
        };

        private static AttackDef Thrust() => new()
        {
            Move = SamuraiVisualRig.Move.Thrust,
            Telegraph = 0.62f, Strike = 0.17f, Recovery = 0.85f,
            Damage = 14f, Posture = 28f, Range = 2.7f, LungeSpeed = 16f,
            Unblockable = false, Glow = new Color(3.2f, 1.1f, 0.1f)
        };

        private void StartAttack(AttackDef attack)
        {
            FacePlayer();
            currentAttack = attack;
            state = TargetState.Telegraphing;
            stateTimer = attack.Telegraph;

            rig.BeginMove(attack.Move, attack.Telegraph, attack.Strike, attack.Recovery);
            rig.SetGlow(attack.Glow);

            Vector3 tip = rig.BladeTip != null ? rig.BladeTip.position : transform.position + Vector3.up * 1.5f;
            tip.z = transform.position.z - 0.4f;
            Color warning = attack.Unblockable ? new Color(1f, 0.1f, 0.05f, 0.95f) : new Color(1f, 0.6f, 0.15f, 0.9f);

            CombatFeedback fx = CombatFeedback.Instance;
            fx.Pulse(tip, warning, 0.4f, attack.Unblockable ? 2.6f : 1.8f, attack.Telegraph * 0.8f);
            fx.LightFlash(tip + Vector3.back * 0.6f, new Color(warning.r, warning.g, warning.b), 5f, 5f, 0.25f);
        }

        private void BeginStrike()
        {
            state = TargetState.Striking;
            stateTimer = currentAttack.Strike;
            rig.SetGlow(Color.black);
        }

        private void ResolveStrike()
        {
            lungeVelocity = 0f;

            float signedDistance = player.transform.position.x - transform.position.x;
            bool inFront = signedDistance * facing > -0.4f;
            float distance = Mathf.Abs(signedDistance);

            bool parried = false;
            if (!player.IsDead && inFront && distance <= currentAttack.Range)
            {
                AkioPrototypeController.DefenseResult result = player.ReceiveIncomingAttack(
                    currentAttack.Damage,
                    currentAttack.Posture,
                    transform.position.x,
                    currentAttack.Unblockable);

                if (result == AkioPrototypeController.DefenseResult.Parried)
                {
                    parried = true;
                    ApplyHit(0f, parryPostureDamage, 0f);
                }
            }

            if (state == TargetState.PostureBroken || state == TargetState.Dead)
            {
                return;
            }

            state = TargetState.Recovering;
            stateTimer = currentAttack.Recovery * (parried ? 0.5f : 1f);

            // Occasional aggressive follow-up after a plain slash that was not parried.
            followUpPending = !parried &&
                              currentAttack.Move == SamuraiVisualRig.Move.Slash &&
                              currentAttack.Telegraph > 0.4f &&
                              Random.value < 0.3f;

            if (followUpPending)
            {
                stateTimer = 0.28f;
            }
        }

        // ------------------------------------------------------------------
        // Damage
        // ------------------------------------------------------------------

        public HitResult ApplyHit(float damage, float postureDamage, float facingDirection)
        {
            if (state == TargetState.Dead)
            {
                return HitResult.Ignored;
            }

            float damageMultiplier = state == TargetState.PostureBroken ? 1.8f : 1f;
            health = Mathf.Max(0f, health - damage * damageMultiplier);
            posture = Mathf.Max(0f, posture - postureDamage);
            lastPostureDamageTime = Time.time;

            bool armored = state == TargetState.Telegraphing || state == TargetState.Striking;
            if (Mathf.Abs(facingDirection) > 0.01f)
            {
                knockback += Mathf.Sign(facingDirection) * (armored ? 1.2f : 4.2f);
            }

            if (rig != null)
            {
                rig.Flash(new Color(2.6f, 0.9f, 0.7f), 0.1f);
                if (!armored && damage > 0f)
                {
                    rig.TriggerHurt();
                }
            }

            if (health <= 0f)
            {
                Die();
                return HitResult.Killed;
            }

            if (posture <= 0f && state != TargetState.PostureBroken)
            {
                BreakPosture();
                return HitResult.PostureBroken;
            }

            return HitResult.Hit;
        }

        private void BreakPosture()
        {
            state = TargetState.PostureBroken;
            stateTimer = postureBrokenDuration;
            followUpPending = false;
            lungeVelocity = 0f;
            rig.EndMove();
            rig.SetGlow(Color.black);
            rig.SetStunned(true);

            Vector3 center = transform.position + Vector3.up * 1.5f;
            CombatFeedback.Instance.Pulse(center, new Color(1f, 0.85f, 0.6f, 0.9f), 0.5f, 3.2f, 0.35f);
        }

        private void Die()
        {
            state = TargetState.Dead;
            stateTimer = respawnDelay;
            posture = 0f;
            followUpPending = false;
            lungeVelocity = 0f;

            if (bodyCollider != null)
            {
                bodyCollider.enabled = false;
            }

            rig.EndMove();
            rig.SetGlow(Color.black);
            rig.SetStunned(false);
            rig.SetDead(true);
        }

        private void Respawn()
        {
            transform.position = homePosition;
            if (body != null)
            {
                body.position = homePosition;
            }

            health = maxHealth;
            posture = maxPosture;
            state = TargetState.Idle;
            attackTimer = attackInterval;
            knockback = 0f;

            if (bodyCollider != null)
            {
                bodyCollider.enabled = true;
            }

            rig.SetDead(false);
            rig.SetStunned(false);
        }

        private void FacePlayer()
        {
            if (player == null)
            {
                return;
            }

            float direction = player.transform.position.x - transform.position.x;
            if (Mathf.Abs(direction) > 0.15f)
            {
                facing = Mathf.Sign(direction);
            }

            if (rig != null)
            {
                Vector3 scale = rig.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * facing;
                rig.transform.localScale = scale;
            }
        }
    }
}

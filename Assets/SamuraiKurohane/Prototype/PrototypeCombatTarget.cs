using UnityEngine;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    public sealed class PrototypeCombatTarget : MonoBehaviour
    {
        private enum TargetState
        {
            Idle,
            Telegraphing,
            Recovering,
            PostureBroken,
            Dead
        }

        [Header("Vitals")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxPosture = 80f;

        [Header("Attack")]
        [SerializeField] private float detectionRange = 7f;
        [SerializeField] private float attackRange = 2.7f;
        [SerializeField] private float attackInterval = 2.2f;
        [SerializeField] private float telegraphDuration = 0.5f;
        [SerializeField] private float recoveryDuration = 0.7f;
        [SerializeField] private float attackDamage = 12f;
        [SerializeField] private float attackPostureDamage = 26f;
        [SerializeField] private float parryPostureDamage = 30f;

        [Header("Recovery")]
        [SerializeField] private float postureBrokenDuration = 1.15f;
        [SerializeField] private float respawnDelay = 2.0f;

        private AkioPrototypeController player;
        private Collider bodyCollider;
        private Renderer[] renderers;
        private MaterialPropertyBlock propertyBlock;

        private TargetState state = TargetState.Idle;
        private float health;
        private float posture;
        private float stateTimer;
        private float attackTimer;
        private float flashTimer;
        private Vector3 initialPosition;

        private static readonly Color IdleColor = new(0.20f, 0.035f, 0.04f);
        private static readonly Color TelegraphColor = new(1f, 0.26f, 0.03f);
        private static readonly Color HitColor = new(1f, 0.03f, 0.02f);
        private static readonly Color BrokenColor = new(0.34f, 0.10f, 0.11f);
        private static readonly Color DeadColor = new(0.025f, 0.015f, 0.018f);

        public float Health => health;
        public float Posture => posture;
        public bool IsDead => state == TargetState.Dead;

        private void Awake()
        {
            bodyCollider = GetComponent<Collider>();
            renderers = GetComponentsInChildren<Renderer>();
            propertyBlock = new MaterialPropertyBlock();

            health = maxHealth;
            posture = maxPosture;
            attackTimer = attackInterval;
            initialPosition = transform.position;
        }

        public void Initialize(AkioPrototypeController targetPlayer)
        {
            player = targetPlayer;
            initialPosition = transform.position;
            ApplyVisual(IdleColor);
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }

            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f)
                {
                    RefreshVisualForState();
                }
            }

            switch (state)
            {
                case TargetState.Dead:
                    stateTimer -= Time.deltaTime;
                    if (stateTimer <= 0f)
                    {
                        Respawn();
                    }
                    return;

                case TargetState.PostureBroken:
                    stateTimer -= Time.deltaTime;
                    if (stateTimer <= 0f)
                    {
                        posture = maxPosture * 0.5f;
                        state = TargetState.Idle;
                        attackTimer = 0.8f;
                        RefreshVisualForState();
                    }
                    return;

                case TargetState.Telegraphing:
                    stateTimer -= Time.deltaTime;
                    if (stateTimer <= 0f)
                    {
                        ResolveAttack();
                    }
                    return;

                case TargetState.Recovering:
                    stateTimer -= Time.deltaTime;
                    if (stateTimer <= 0f)
                    {
                        state = TargetState.Idle;
                        attackTimer = attackInterval;
                        RefreshVisualForState();
                    }
                    return;
            }

            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f || player.IsDead)
            {
                return;
            }

            float distance = Mathf.Abs(player.transform.position.x - transform.position.x);
            if (distance <= detectionRange)
            {
                StartTelegraph();
            }
            else
            {
                attackTimer = 0.35f;
            }
        }

        private void StartTelegraph()
        {
            state = TargetState.Telegraphing;
            stateTimer = telegraphDuration;
            ApplyVisual(TelegraphColor);
        }

        private void ResolveAttack()
        {
            float distance = Mathf.Abs(player.transform.position.x - transform.position.x);
            if (distance <= attackRange)
            {
                AkioPrototypeController.DefenseResult result =
                    player.ReceiveIncomingAttack(attackDamage, attackPostureDamage);

                if (result == AkioPrototypeController.DefenseResult.Parried)
                {
                    ApplyHit(0f, parryPostureDamage, 0f);
                }
            }

            if (state != TargetState.PostureBroken && state != TargetState.Dead)
            {
                state = TargetState.Recovering;
                stateTimer = recoveryDuration;
                RefreshVisualForState();
            }
        }

        public void ApplyHit(float damage, float postureDamage, float facingDirection)
        {
            if (state == TargetState.Dead)
            {
                return;
            }

            float damageMultiplier = state == TargetState.PostureBroken ? 1.8f : 1f;
            health = Mathf.Max(0f, health - damage * damageMultiplier);
            posture = Mathf.Max(0f, posture - postureDamage);

            if (Mathf.Abs(facingDirection) > 0.01f)
            {
                transform.position += Vector3.right * Mathf.Sign(facingDirection) * 0.08f;
            }

            flashTimer = 0.10f;
            ApplyVisual(HitColor);

            if (health <= 0f)
            {
                Die();
                return;
            }

            if (posture <= 0f)
            {
                BreakPosture();
            }
        }

        private void BreakPosture()
        {
            state = TargetState.PostureBroken;
            stateTimer = postureBrokenDuration;
            ApplyVisual(BrokenColor);
        }

        private void Die()
        {
            state = TargetState.Dead;
            stateTimer = respawnDelay;
            posture = 0f;

            if (bodyCollider != null)
            {
                bodyCollider.enabled = false;
            }

            ApplyVisual(DeadColor);
        }

        private void Respawn()
        {
            transform.position = initialPosition;
            health = maxHealth;
            posture = maxPosture;
            state = TargetState.Idle;
            attackTimer = attackInterval;

            if (bodyCollider != null)
            {
                bodyCollider.enabled = true;
            }

            ApplyVisual(IdleColor);
        }

        private void RefreshVisualForState()
        {
            Color color = state switch
            {
                TargetState.Telegraphing => TelegraphColor,
                TargetState.PostureBroken => BrokenColor,
                TargetState.Dead => DeadColor,
                _ => IdleColor
            };

            ApplyVisual(color);
        }

        private void ApplyVisual(Color color)
        {
            if (renderers == null)
            {
                return;
            }

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_BaseColor", color);
                propertyBlock.SetColor("_Color", color);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Vector3 screen = Camera.main != null
                ? Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f)
                : Vector3.zero;

            if (screen.z <= 0f)
            {
                return;
            }

            float y = Screen.height - screen.y;
            Rect labelRect = new(screen.x - 85f, y - 12f, 170f, 24f);
            GUI.Label(labelRect, $"TRAINING SAMURAI  HP {health:0}  POSTURE {posture:0}");
        }
    }
}

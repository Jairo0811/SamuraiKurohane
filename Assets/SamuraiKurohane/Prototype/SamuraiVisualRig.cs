using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    /// <summary>
    /// Procedural samurai model + code-driven animation. It replaces the old capsule blockout
    /// with a readable side-view silhouette (kasa, layered armour, flowing cape, katana) and
    /// poses every gameplay state (idle, run, jump, dash, guard, parry, attacks, stagger, death)
    /// without needing authored animation clips.
    ///
    /// Angle convention (side view, character facing +X, camera looking along +Z):
    /// a limb hanging along -Y rotated by a positive Z angle swings FORWARD (+X).
    /// "blade" is the absolute world angle of the katana (0 = forward, 90 = up).
    /// </summary>
    public sealed class SamuraiVisualRig : MonoBehaviour
    {
        public enum Archetype
        {
            Akio,
            ImperialGuard
        }

        public enum Move
        {
            None,
            Light1,
            Light2,
            Light3,
            Heavy,
            Parry,
            Slash,
            Overhead,
            Thrust
        }

        private struct Pose
        {
            public float ThighF, ShinF, ThighB, ShinB;
            public float ShoulderF, ElbowF, ShoulderB, ElbowB;
            public float Blade, Lean, Twist, Lunge;

            public static Pose Lerp(Pose a, Pose b, float t)
            {
                Pose p;
                p.ThighF = Mathf.Lerp(a.ThighF, b.ThighF, t);
                p.ShinF = Mathf.Lerp(a.ShinF, b.ShinF, t);
                p.ThighB = Mathf.Lerp(a.ThighB, b.ThighB, t);
                p.ShinB = Mathf.Lerp(a.ShinB, b.ShinB, t);
                p.ShoulderF = Mathf.Lerp(a.ShoulderF, b.ShoulderF, t);
                p.ElbowF = Mathf.Lerp(a.ElbowF, b.ElbowF, t);
                p.ShoulderB = Mathf.Lerp(a.ShoulderB, b.ShoulderB, t);
                p.ElbowB = Mathf.Lerp(a.ElbowB, b.ElbowB, t);
                p.Blade = Mathf.LerpAngle(a.Blade, b.Blade, t);
                p.Lean = Mathf.Lerp(a.Lean, b.Lean, t);
                p.Twist = Mathf.Lerp(a.Twist, b.Twist, t);
                p.Lunge = Mathf.Lerp(a.Lunge, b.Lunge, t);
                return p;
            }
        }

        // Both hands hold the katana, so the rear arm mirrors the sword arm closely.
        private static Pose Held(float thighF, float shinF, float thighB, float shinB,
            float shoulder, float elbow, float blade, float lean, float twist, float lunge)
        {
            return new Pose
            {
                ThighF = thighF, ShinF = shinF, ThighB = thighB, ShinB = shinB,
                ShoulderF = shoulder, ElbowF = elbow,
                ShoulderB = shoulder - 8f, ElbowB = elbow + 10f,
                Blade = blade, Lean = lean, Twist = twist, Lunge = lunge
            };
        }

        private static readonly Pose Ready = Held(14, -10, -16, -8, 35, 60, 30, 3, 0, 0);
        private static readonly Pose GuardPose = Held(18, -24, -20, -14, 60, 75, 85, -4, 8, 0);
        private static readonly Pose AirUp = Held(35, -55, -10, -45, 50, 40, 20, 4, 0, 0);
        private static readonly Pose AirDown = Held(18, -20, -22, -10, 70, 20, 40, 0, 0, 0);
        private static readonly Pose DashPose = Held(55, -35, -50, -12, -20, 20, 190, 26, 0, 0);
        private static readonly Pose StunPose = Held(10, -30, -8, -30, -15, 20, -100, 16, -10, 0);
        private static readonly Pose HurtPose = Held(8, -14, -12, -8, 10, 30, 60, -14, 0, 0);
        private static readonly Pose DeadPose = Held(5, -10, -5, -10, -20, 15, -90, 0, 0, 0);

        private static readonly Pose Light1Wind = Held(10, -12, -22, -10, 120, 70, 130, -8, -18, 0);
        private static readonly Pose Light1Strike = Held(42, -28, -30, -6, 40, 20, -40, 12, 20, 0.35f);
        private static readonly Pose Light2Wind = Held(30, -30, -18, -10, -20, 15, -150, 8, 15, 0);
        private static readonly Pose Light2Strike = Held(44, -20, -34, -8, 110, 60, 70, -6, -18, 0.40f);
        private static readonly Pose Light3Wind = Held(18, -20, -34, -10, 10, 110, 12, -10, -20, 0);
        private static readonly Pose Light3Strike = Held(58, -25, -48, -4, 80, 5, 0, 20, 25, 0.80f);
        private static readonly Pose HeavyWind = Held(12, -20, -28, -18, 150, 30, 100, -14, -10, 0);
        private static readonly Pose HeavyStrike = Held(52, -50, -42, -22, 70, 20, -55, 24, 20, 0.60f);
        private static readonly Pose ParryWind = Held(16, -22, -20, -12, 70, 90, 140, -8, 14, 0);
        private static readonly Pose ParryStrike = Held(20, -22, -22, -12, 50, 50, -10, 6, -6, 0);

        private const float ThighLength = 0.5f;
        private const float ShinLength = 0.5f;

        // ---- Hierarchy ----------------------------------------------------
        private Transform rigRoot;
        private Transform hips;
        private Transform thighF, shinF, thighB, shinB;
        private Transform torso, head;
        private Transform shoulderF, elbowF, shoulderB, elbowB;
        private Transform swordPivot;
        private Light bladeLightComponent;
        private readonly List<Transform> capeSegments = new();

        private Archetype archetype;
        private float scaleFactor = 1f;
        private readonly List<Material> ownedMaterials = new();

        // ---- Materials ----------------------------------------------------
        private Material matLacquer, matArmor, matCloth, matAccentCloth, matCape, matGlow, matSteel, matWrap, matBrass, matSkin;

        // ---- Flash / glow ---------------------------------------------------
        private sealed class Tracked
        {
            public Renderer Renderer;
            public Color BaseEmission;
        }

        private readonly List<Tracked> tracked = new();
        private MaterialPropertyBlock block;
        private Color flashColor;
        private float flashAge;
        private float flashDuration;
        private Color glowColor;
        private bool emissionDirty;
        private bool emissionApplied;

        // ---- Animation state ------------------------------------------------
        private Pose current;
        private Pose moveStartPose;
        private float velocityX;
        private float velocityY;
        private float prevVelocityY;
        private bool grounded = true;
        private bool wasGrounded = true;
        private bool guarding;
        private bool dashing;
        private bool stunned;
        private bool dead;
        private float hurtTimer;
        private float runPhase;
        private float idleTime;
        private float squash;
        private float deathProgress;
        private float dashSparkTimer;

        private Move move = Move.None;
        private float moveElapsed;
        private float moveStartup, moveActive, moveRecovery;
        private bool arcSpawned;

        private float capeAngle;
        private float capeVelocity;

        public float Facing => transform.localScale.x >= 0f ? 1f : -1f;
        public Transform BladeTip { get; private set; }
        public Transform Hips => hips;

        // ===================================================================
        // Construction
        // ===================================================================

        public static SamuraiVisualRig Build(Transform parent, Archetype archetype)
        {
            GameObject root = new(archetype == Archetype.Akio ? "Akio Visuals" : "Imperial Guard Visuals");
            root.transform.SetParent(parent, false);
            SamuraiVisualRig rig = root.AddComponent<SamuraiVisualRig>();
            rig.Construct(archetype);
            return rig;
        }

        private void Construct(Archetype style)
        {
            archetype = style;
            scaleFactor = style == Archetype.Akio ? 1f : 1.06f;
            block = new MaterialPropertyBlock();

            CreateMaterials();

            rigRoot = NewPivot("Rig Root", transform, Vector3.zero);
            rigRoot.localScale = Vector3.one * scaleFactor;

            hips = NewPivot("Hips", rigRoot, new Vector3(0f, 1f, 0f));
            BuildLegs();
            torso = NewPivot("Torso", hips, new Vector3(0f, 0.08f, 0f));
            BuildTorso();
            BuildHead();
            BuildArms();
            BuildSword();
            BuildScabbard();
            BuildCape();

            current = Ready;
            ApplyPose(current, 0f);
        }

        private void CreateMaterials()
        {
            if (archetype == Archetype.Akio)
            {
                matLacquer = Own(PrototypeMaterials.CreateLit("Akio Lacquer", new Color(0.058f, 0.058f, 0.068f), 0.35f, 0.80f));
                matArmor = Own(PrototypeMaterials.CreateLit("Akio Armor", new Color(0.165f, 0.165f, 0.185f), 0.7f, 0.62f));
                matCloth = Own(PrototypeMaterials.CreateLit("Akio Cloth", new Color(0.105f, 0.098f, 0.115f), 0f, 0.20f));
                matAccentCloth = Own(PrototypeMaterials.CreateLit("Akio Crimson Cloth", new Color(0.30f, 0.012f, 0.02f), 0f, 0.25f));
                matCape = Own(PrototypeMaterials.CreateLit("Akio Cape", new Color(0.165f, 0.026f, 0.036f), 0f, 0.14f));
                matGlow = Own(PrototypeMaterials.CreateLit("Akio Crimson Glow", new Color(0.35f, 0.01f, 0.015f), 0.1f, 0.2f,
                    new Color(5.0f, 0.03f, 0.025f)));
            }
            else
            {
                matLacquer = Own(PrototypeMaterials.CreateLit("Guard Lacquer", new Color(0.06f, 0.055f, 0.05f), 0.4f, 0.7f));
                matArmor = Own(PrototypeMaterials.CreateLit("Guard Armor", new Color(0.30f, 0.31f, 0.34f), 0.75f, 0.5f));
                matCloth = Own(PrototypeMaterials.CreateLit("Guard Cloth", new Color(0.13f, 0.11f, 0.09f), 0f, 0.15f));
                matAccentCloth = Own(PrototypeMaterials.CreateLit("Guard Tabard", new Color(0.22f, 0.025f, 0.03f), 0f, 0.2f));
                matCape = Own(PrototypeMaterials.CreateLit("Guard Banner Cloth", new Color(0.17f, 0.03f, 0.035f), 0f, 0.12f));
                matGlow = Own(PrototypeMaterials.CreateLit("Guard Amber Glow", new Color(0.5f, 0.2f, 0.02f), 0.1f, 0.2f,
                    new Color(5.5f, 1.7f, 0.15f)));
            }

            matSteel = Own(PrototypeMaterials.CreateLit("Katana Steel", new Color(0.78f, 0.80f, 0.86f), 0.95f, 0.92f));
            matWrap = Own(PrototypeMaterials.CreateLit("Handle Wrap", new Color(0.05f, 0.018f, 0.02f), 0f, 0.3f));
            matBrass = Own(PrototypeMaterials.CreateLit("Tsuba Brass", new Color(0.45f, 0.34f, 0.14f), 0.9f, 0.6f));
            matSkin = Own(PrototypeMaterials.CreateLit("Face Cloth", new Color(0.05f, 0.045f, 0.05f), 0f, 0.1f));
        }

        private Material Own(Material material)
        {
            ownedMaterials.Add(material);
            return material;
        }

        private void OnDestroy()
        {
            foreach (Material material in ownedMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }

        // ---- Body parts -----------------------------------------------------

        private void BuildLegs()
        {
            bool akio = archetype == Archetype.Akio;
            thighF = NewPivot("Thigh Front", hips, new Vector3(0.02f, -0.02f, -0.11f));
            thighB = NewPivot("Thigh Back", hips, new Vector3(0.02f, -0.02f, 0.11f));
            shinF = NewPivot("Shin Front", thighF, new Vector3(0f, -ThighLength + 0.02f, 0f));
            shinB = NewPivot("Shin Back", thighB, new Vector3(0f, -ThighLength + 0.02f, 0f));

            foreach (Transform thigh in new[] { thighF, thighB })
            {
                // Wide hakama trouser leg + knee guard.
                Part("Hakama Thigh", thigh, PrimitiveType.Cube, new Vector3(0.02f, -0.24f, 0f), new Vector3(0.30f, 0.50f, 0.26f), matCloth);
                Part("Knee Guard", thigh, PrimitiveType.Cube, new Vector3(0.10f, -0.47f, 0f), new Vector3(0.10f, 0.14f, 0.22f), matArmor);
            }

            foreach (Transform shin in new[] { shinF, shinB })
            {
                Part("Hakama Shin", shin, PrimitiveType.Cube, new Vector3(0f, -0.22f, 0f), new Vector3(0.26f, 0.44f, 0.24f), matCloth);
                Part("Greave", shin, PrimitiveType.Cube, new Vector3(0.06f, -0.20f, 0f), new Vector3(0.10f, 0.30f, 0.22f), matArmor);
                Part("Foot", shin, PrimitiveType.Cube, new Vector3(0.07f, -0.46f, 0f), new Vector3(0.36f, 0.08f, 0.22f), matLacquer);

                if (akio)
                {
                    Part("Ankle Cord", shin, PrimitiveType.Cube, new Vector3(0f, -0.38f, 0f), new Vector3(0.27f, 0.025f, 0.25f), matAccentCloth);
                }
            }
        }

        private void BuildTorso()
        {
            bool akio = archetype == Archetype.Akio;

            Part("Pelvis", hips, PrimitiveType.Cube, new Vector3(0f, 0.02f, 0f), new Vector3(0.46f, 0.22f, 0.34f), matCloth);
            Part("Obi Sash", torso, PrimitiveType.Cube, new Vector3(0f, 0.08f, 0f), new Vector3(0.52f, 0.15f, 0.38f), matAccentCloth);
            Part("Cuirass", torso, PrimitiveType.Cube, new Vector3(0f, 0.45f, 0f), new Vector3(0.58f, 0.56f, 0.40f), matArmor);
            Part("Cuirass Spine Plate", torso, PrimitiveType.Cube, new Vector3(-0.30f, 0.45f, 0f), new Vector3(0.05f, 0.50f, 0.30f), matLacquer);

            // Hip plates (kusazuri).
            Part("Kusazuri Front", torso, PrimitiveType.Cube, new Vector3(0.27f, -0.10f, 0f), new Vector3(0.14f, 0.30f, 0.34f), matArmor, Vector3.forward * -4f);
            Part("Kusazuri Back", torso, PrimitiveType.Cube, new Vector3(-0.27f, -0.10f, 0f), new Vector3(0.14f, 0.30f, 0.34f), matArmor, Vector3.forward * 4f);

            // Lacing lines on the camera-facing side.
            for (int i = 0; i < 3; i++)
            {
                Part("Lacing", torso, PrimitiveType.Cube, new Vector3(0f, 0.30f + i * 0.14f, -0.205f), new Vector3(0.54f, 0.022f, 0.012f),
                    akio ? matAccentCloth : matGlow);
            }

            // Clan emblem: stylised black feather (kurohane) built from three thin bars.
            Part("Emblem Spine", torso, PrimitiveType.Cube, new Vector3(0.02f, 0.46f, -0.212f), new Vector3(0.30f, 0.035f, 0.012f), matGlow, Vector3.forward * 52f);
            Part("Emblem Barb A", torso, PrimitiveType.Cube, new Vector3(-0.03f, 0.50f, -0.212f), new Vector3(0.15f, 0.022f, 0.012f), matGlow, Vector3.forward * 20f);
            Part("Emblem Barb B", torso, PrimitiveType.Cube, new Vector3(0.07f, 0.40f, -0.212f), new Vector3(0.15f, 0.022f, 0.012f), matGlow, Vector3.forward * 20f);

            Part("Gorget", torso, PrimitiveType.Cube, new Vector3(0.02f, 0.74f, 0f), new Vector3(0.36f, 0.10f, 0.34f), matArmor);
            Part("Neck", torso, PrimitiveType.Cube, new Vector3(0.02f, 0.80f, 0f), new Vector3(0.14f, 0.10f, 0.14f), matSkin);

            // Layered shoulder guards (sode) fixed to the torso.
            foreach (float z in new[] { -0.33f, 0.33f })
            {
                for (int i = 0; i < 3; i++)
                {
                    float width = 0.34f + i * 0.045f;
                    Part("Sode Plate", torso, PrimitiveType.Cube,
                        new Vector3(0.02f, 0.70f - i * 0.075f, z), new Vector3(width, 0.07f, 0.30f + i * 0.02f),
                        matArmor, Vector3.forward * (i * -3f));
                }

                Part("Sode Trim", torso, PrimitiveType.Cube, new Vector3(0.02f, 0.47f, z * 1.02f), new Vector3(0.40f, 0.02f, 0.30f), akio ? matAccentCloth : matGlow);
            }

            if (!akio)
            {
                // Imperial guard tabard hanging from the waist (front and back panels).
                Part("Tabard Front", torso, PrimitiveType.Cube, new Vector3(0.32f, -0.28f, 0f), new Vector3(0.05f, 0.46f, 0.30f), matAccentCloth, Vector3.forward * -5f);
                Part("Tabard Back", torso, PrimitiveType.Cube, new Vector3(-0.32f, -0.28f, 0f), new Vector3(0.05f, 0.50f, 0.30f), matAccentCloth, Vector3.forward * 5f);
                Part("Tabard Emblem", torso, PrimitiveType.Cube, new Vector3(0.345f, -0.20f, 0f), new Vector3(0.02f, 0.10f, 0.10f), matGlow, Vector3.right * 45f);
            }
        }

        private void BuildHead()
        {
            bool akio = archetype == Archetype.Akio;
            head = NewPivot("Head", torso, new Vector3(0.02f, 0.82f, 0f));

            Part("Head Wrap", head, PrimitiveType.Cube, new Vector3(0f, 0.12f, 0f), new Vector3(0.24f, 0.26f, 0.24f), matSkin);
            Part("Menpo Mask", head, PrimitiveType.Cube, new Vector3(0.12f, 0.07f, 0f), new Vector3(0.06f, 0.15f, 0.22f), matLacquer);
            Part("Eye Slit", head, PrimitiveType.Cube, new Vector3(0.10f, 0.15f, -0.118f), new Vector3(0.11f, 0.028f, 0.012f), matGlow);
            Part("Eye Slit Far", head, PrimitiveType.Cube, new Vector3(0.10f, 0.15f, 0.118f), new Vector3(0.11f, 0.028f, 0.012f), matGlow);

            if (akio)
            {
                // Wide conical kasa with a crimson rim and chin cord.
                Renderer kasa = Part("Kasa", head, PrimitiveType.Cube, new Vector3(0f, 0.20f, 0f), Vector3.one, matLacquer);
                kasa.GetComponent<MeshFilter>().sharedMesh = KasaMesh();

                Part("Kasa Rim", head, PrimitiveType.Cylinder, new Vector3(0f, 0.205f, 0f), new Vector3(1.58f, 0.012f, 1.58f), matAccentCloth);
                Part("Kasa Crown Cap", head, PrimitiveType.Sphere, new Vector3(0f, 0.545f, 0f), new Vector3(0.10f, 0.07f, 0.10f), matBrass);
                Part("Chin Cord", head, PrimitiveType.Cube, new Vector3(0.0f, 0.02f, -0.125f), new Vector3(0.24f, 0.016f, 0.01f), matAccentCloth, Vector3.forward * -12f);
            }
            else
            {
                // Kabuto: dome, neck guard and a crescent crest (maedate).
                Part("Kabuto Dome", head, PrimitiveType.Sphere, new Vector3(0f, 0.24f, 0f), new Vector3(0.40f, 0.30f, 0.38f), matArmor);
                Part("Kabuto Brim", head, PrimitiveType.Cube, new Vector3(0.10f, 0.20f, 0f), new Vector3(0.30f, 0.04f, 0.36f), matLacquer, Vector3.forward * -8f);
                Part("Shikoro Neck Guard", head, PrimitiveType.Cube, new Vector3(-0.17f, 0.06f, 0f), new Vector3(0.08f, 0.26f, 0.38f), matArmor, Vector3.forward * 14f);
                Part("Crest Horn A", head, PrimitiveType.Cube, new Vector3(0.17f, 0.50f, 0f), new Vector3(0.04f, 0.36f, 0.03f), matGlow, Vector3.forward * -34f);
                Part("Crest Horn B", head, PrimitiveType.Cube, new Vector3(0.02f, 0.52f, 0f), new Vector3(0.04f, 0.36f, 0.03f), matGlow, Vector3.forward * 22f);
            }
        }

        private void BuildArms()
        {
            shoulderF = NewPivot("Shoulder Front", torso, new Vector3(0.02f, 0.64f, -0.30f));
            shoulderB = NewPivot("Shoulder Back", torso, new Vector3(0.02f, 0.64f, 0.30f));
            elbowF = NewPivot("Elbow Front", shoulderF, new Vector3(0f, -0.38f, 0f));
            elbowB = NewPivot("Elbow Back", shoulderB, new Vector3(0f, -0.38f, 0f));

            foreach (Transform shoulder in new[] { shoulderF, shoulderB })
            {
                Part("Upper Arm", shoulder, PrimitiveType.Cube, new Vector3(0f, -0.19f, 0f), new Vector3(0.16f, 0.38f, 0.16f), matCloth);
            }

            foreach (Transform elbow in new[] { elbowF, elbowB })
            {
                Part("Forearm", elbow, PrimitiveType.Cube, new Vector3(0f, -0.17f, 0f), new Vector3(0.14f, 0.34f, 0.15f), matCloth);
                Part("Kote Bracer", elbow, PrimitiveType.Cube, new Vector3(0f, -0.15f, 0f), new Vector3(0.18f, 0.22f, 0.19f), matArmor);
                Part("Hand", elbow, PrimitiveType.Cube, new Vector3(0f, -0.36f, 0f), new Vector3(0.12f, 0.12f, 0.12f), matLacquer);
            }
        }

        private void BuildSword()
        {
            float bladeLength = archetype == Archetype.Akio ? 1.30f : 1.75f;

            // Sword pivot sits at the front hand; the blade extends along +X.
            swordPivot = NewPivot("Katana", elbowF, new Vector3(0f, -0.36f, -0.01f));

            Part("Tsuka Handle", swordPivot, PrimitiveType.Cube, new Vector3(-0.12f, 0f, 0f), new Vector3(0.34f, 0.055f, 0.055f), matWrap);
            Part("Kashira", swordPivot, PrimitiveType.Cube, new Vector3(-0.30f, 0f, 0f), new Vector3(0.04f, 0.07f, 0.07f), matBrass);
            Part("Tsuba Guard", swordPivot, PrimitiveType.Cylinder, new Vector3(0.06f, 0f, 0f), new Vector3(0.15f, 0.012f, 0.15f), matBrass, Vector3.forward * 90f);
            Part("Blade", swordPivot, PrimitiveType.Cube, new Vector3(0.08f + bladeLength * 0.5f, 0f, 0f), new Vector3(bladeLength, 0.052f, 0.012f), matSteel);
            Part("Blade Energy Edge", swordPivot, PrimitiveType.Cube, new Vector3(0.10f + bladeLength * 0.5f, 0.024f, 0f), new Vector3(bladeLength - 0.04f, 0.012f, 0.016f), matGlow);

            GameObject tip = new("Blade Tip");
            tip.transform.SetParent(swordPivot, false);
            tip.transform.localPosition = new Vector3(0.08f + bladeLength, 0f, 0f);
            BladeTip = tip.transform;

            GameObject lightObject = new("Blade Light");
            lightObject.transform.SetParent(swordPivot, false);
            lightObject.transform.localPosition = new Vector3(0.08f + bladeLength * 0.5f, 0f, -0.35f);
            bladeLightComponent = lightObject.AddComponent<Light>();
            bladeLightComponent.type = LightType.Point;
            bladeLightComponent.range = 3.2f;
            bladeLightComponent.intensity = 1.1f;
            bladeLightComponent.color = archetype == Archetype.Akio ? new Color(1f, 0.1f, 0.06f) : new Color(1f, 0.55f, 0.15f);
            bladeLightComponent.shadows = LightShadows.None;
        }

        private void BuildScabbard()
        {
            Transform saya = Part("Saya", hips, PrimitiveType.Cube, new Vector3(-0.26f, -0.06f, 0.20f), new Vector3(1.0f, 0.06f, 0.07f), matLacquer, Vector3.forward * -24f).transform;
            Part("Saya Cord", saya, PrimitiveType.Cube, new Vector3(0.30f, 0f, 0f), new Vector3(0.06f, 0.075f, 0.085f), matAccentCloth);
            Part("Saya Tip", saya, PrimitiveType.Cube, new Vector3(-0.5f, 0f, 0f), new Vector3(0.05f, 0.075f, 0.075f), matBrass);
        }

        private void BuildCape()
        {
            Transform previous = NewPivot("Cape Root", torso, new Vector3(-0.30f, 0.68f, 0f));
            bool akio = archetype == Archetype.Akio;
            int segments = akio ? 4 : 2;
            float segmentLength = akio ? 0.36f : 0.34f;

            for (int i = 0; i < segments; i++)
            {
                Transform segment = NewPivot($"Cape Segment {i}", previous, i == 0 ? Vector3.zero : new Vector3(0f, -segmentLength, 0f));
                float width = akio ? Mathf.Lerp(0.34f, 0.22f, i / (float)(segments - 1)) : 0.20f;
                Material material = i % 2 == 0 ? matCape : matAccentCloth;

                Part("Cape Panel", segment, PrimitiveType.Cube, new Vector3(-width * 0.2f, -segmentLength * 0.5f, 0f),
                    new Vector3(width, segmentLength, 0.035f), material);

                if (akio && i == 0)
                {
                    Part("Cape Emblem Spine", segment, PrimitiveType.Cube, new Vector3(-0.06f, -0.20f, -0.024f), new Vector3(0.24f, 0.03f, 0.01f), matGlow, Vector3.forward * 62f);
                    Part("Cape Emblem Barb", segment, PrimitiveType.Cube, new Vector3(-0.09f, -0.14f, -0.024f), new Vector3(0.12f, 0.02f, 0.01f), matGlow, Vector3.forward * 24f);
                }

                if (akio && i == segments - 1)
                {
                    // Torn hem: ragged strips of different length.
                    float[] lengths = { 0.22f, 0.34f, 0.16f };
                    float[] offsets = { 0.08f, -0.02f, -0.12f };
                    for (int s = 0; s < lengths.Length; s++)
                    {
                        Part("Cape Tatter", segment, PrimitiveType.Cube,
                            new Vector3(offsets[s] - width * 0.2f, -segmentLength - lengths[s] * 0.5f + 0.02f, 0f),
                            new Vector3(0.07f, lengths[s], 0.03f), matCape, Vector3.forward * (s - 1) * 4f);
                    }
                }

                capeSegments.Add(segment);
                previous = segment;
            }
        }

        // ---- Meshes ---------------------------------------------------------

        private static Mesh kasaMesh;
        private static readonly Dictionary<PrimitiveType, Mesh> PrimitiveMeshes = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            kasaMesh = null;
            PrimitiveMeshes.Clear();
        }

        private static Mesh KasaMesh()
        {
            if (kasaMesh == null)
            {
                kasaMesh = BuildCone(24, 0.78f, 0.36f);
            }

            return kasaMesh;
        }

        /// <summary>Open cone (kasa hat): apex up, flat underside. Triangles are wound away from the cone's core.</summary>
        private static Mesh BuildCone(int segments, float radius, float height)
        {
            List<Vector3> vertices = new();
            List<int> triangles = new();
            Vector3 apex = new(0f, height, 0f);
            Vector3 core = new(0f, height * 0.33f, 0f);

            void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
            {
                Vector3 normal = Vector3.Cross(b - a, c - a);
                Vector3 centroid = (a + b + c) / 3f;

                if (Vector3.Dot(normal, centroid - core) < 0f)
                {
                    (b, c) = (c, b);
                }

                int start = vertices.Count;
                vertices.Add(a);
                vertices.Add(b);
                vertices.Add(c);
                triangles.Add(start);
                triangles.Add(start + 1);
                triangles.Add(start + 2);
            }

            for (int i = 0; i < segments; i++)
            {
                float a0 = i / (float)segments * Mathf.PI * 2f;
                float a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                Vector3 p0 = new(Mathf.Cos(a0) * radius, 0f, Mathf.Sin(a0) * radius);
                Vector3 p1 = new(Mathf.Cos(a1) * radius, 0f, Mathf.Sin(a1) * radius);

                AddTriangle(apex, p0, p1);
                AddTriangle(Vector3.zero, p0, p1);
            }

            Mesh mesh = new() { name = "Prototype Kasa" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh PrimitiveMesh(PrimitiveType type)
        {
            if (PrimitiveMeshes.TryGetValue(type, out Mesh cached) && cached != null)
            {
                return cached;
            }

            GameObject temp = GameObject.CreatePrimitive(type);
            Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(temp);
            PrimitiveMeshes[type] = mesh;
            return mesh;
        }

        private static Transform NewPivot(string name, Transform parent, Vector3 localPosition)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go.transform;
        }

        private Renderer Part(
            string name,
            Transform parent,
            PrimitiveType type,
            Vector3 localPosition,
            Vector3 size,
            Material material,
            Vector3? euler = null)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(euler ?? Vector3.zero);
            go.transform.localScale = size;

            go.AddComponent<MeshFilter>().sharedMesh = PrimitiveMesh(type);
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;

            Color baseEmission = Color.black;
            if (material.HasProperty("_EmissionColor"))
            {
                baseEmission = material.GetColor("_EmissionColor");
            }

            tracked.Add(new Tracked { Renderer = renderer, BaseEmission = baseEmission });
            return renderer;
        }

        // ===================================================================
        // Public state API (driven by the controller / enemy AI)
        // ===================================================================

        public void SetLocomotion(float velX, float velY, bool isGrounded)
        {
            velocityX = velX;
            velocityY = velY;
            grounded = isGrounded;
        }

        public void SetGuard(bool value) => guarding = value;
        public void SetStunned(bool value) => stunned = value;

        public void SetDashing(bool value)
        {
            if (value && !dashing)
            {
                dashSparkTimer = 0f;
            }

            dashing = value;
        }

        public void SetDead(bool value)
        {
            dead = value;
            if (!value)
            {
                deathProgress = 0f;
            }
        }

        public void TriggerHurt()
        {
            hurtTimer = 0.22f;
            Flash(new Color(3.5f, 0.2f, 0.1f), 0.14f);
        }

        public void BeginMove(Move newMove, float startup, float active, float recovery)
        {
            move = newMove;
            moveElapsed = 0f;
            moveStartup = Mathf.Max(0.04f, startup);
            moveActive = Mathf.Max(0.04f, active);
            moveRecovery = Mathf.Max(0.04f, recovery);
            moveStartPose = current;
            arcSpawned = false;
        }

        public void EndMove()
        {
            move = Move.None;
        }

        public void Flash(Color color, float duration)
        {
            flashColor = color;
            flashDuration = Mathf.Max(0.02f, duration);
            flashAge = 0f;
            emissionDirty = true;
        }

        /// <summary>Continuous glow, e.g. the enemy attack telegraph. Pass Color.black to clear.</summary>
        public void SetGlow(Color color)
        {
            if (glowColor != color)
            {
                glowColor = color;
                emissionDirty = true;
            }
        }

        // ===================================================================
        // Animation
        // ===================================================================

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            idleTime += dt;
            hurtTimer = Mathf.Max(0f, hurtTimer - dt);

            DetectLanding();

            Pose target = ComputeTargetPose(dt, out bool snap);

            if (snap)
            {
                current = target;
            }
            else
            {
                float rate = dashing ? 22f : 16f;
                current = Pose.Lerp(current, target, 1f - Mathf.Exp(-rate * dt));
            }

            ApplyPose(current, dt);
            UpdateRigRoot(dt);
            UpdateCape(dt);
            UpdateEmission(dt);
            UpdateDashSparks(dt);

            if (bladeLightComponent != null)
            {
                float intensity = move == Move.None ? 1.1f : 3.2f;
                bladeLightComponent.intensity = Mathf.Lerp(bladeLightComponent.intensity, intensity, 0.35f);
            }

            prevVelocityY = velocityY;
            wasGrounded = grounded;
        }

        private void DetectLanding()
        {
            if (grounded && !wasGrounded && prevVelocityY < -5f)
            {
                squash = Mathf.Clamp01(-prevVelocityY / 18f) * 0.22f;
                CombatFeedback.Instance.Dust(transform.position + Vector3.up * 0.05f, 6);
            }
        }

        private Pose ComputeTargetPose(float dt, out bool snap)
        {
            snap = false;
            Pose pose;

            if (dead)
            {
                deathProgress = Mathf.Min(1f, deathProgress + dt * 2.4f);
                return DeadPose;
            }

            if (move != Move.None)
            {
                snap = true;
                return ComputeMovePose(dt);
            }

            if (stunned)
            {
                pose = StunPose;
                pose.Lean += Mathf.Sin(idleTime * 14f) * 2f;
                return pose;
            }

            if (hurtTimer > 0f)
            {
                return HurtPose;
            }

            if (dashing)
            {
                return DashPose;
            }

            if (guarding)
            {
                pose = GuardPose;
                pose.Lean += Mathf.Sin(idleTime * 2.2f) * 0.8f;
                return pose;
            }

            if (!grounded)
            {
                return velocityY > 1f ? AirUp : AirDown;
            }

            float localSpeed = Mathf.Abs(velocityX);
            if (localSpeed > 0.6f)
            {
                runPhase += dt * localSpeed * 1.55f;
                return RunPose(localSpeed);
            }

            pose = Ready;
            float breath = Mathf.Sin(idleTime * 1.9f);
            pose.Lean += breath * 1.2f;
            pose.ShoulderF += breath * 2f;
            pose.ShoulderB += breath * 2f;
            pose.Blade += breath * 1.5f;
            return pose;
        }

        private Pose RunPose(float speed)
        {
            float s = Mathf.Sin(runPhase);
            float c = Mathf.Cos(runPhase);
            float sb = Mathf.Sin(runPhase + Mathf.PI);
            float cb = Mathf.Cos(runPhase + Mathf.PI);

            Pose pose;
            pose.ThighF = s * 48f + 6f;
            pose.ThighB = sb * 48f + 6f;
            pose.ShinF = -(10f + Mathf.Max(0f, c) * 70f);
            pose.ShinB = -(10f + Mathf.Max(0f, cb) * 70f);
            pose.ShoulderF = -32f + sb * 10f;
            pose.ElbowF = 16f;
            pose.ShoulderB = s * 40f;
            pose.ElbowB = 30f + Mathf.Max(0f, s) * 20f;
            pose.Blade = -168f + sb * 6f;
            pose.Lean = 9f + Mathf.Clamp01(speed / 7f) * 5f;
            pose.Twist = s * 6f;
            pose.Lunge = 0f;
            return pose;
        }

        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t) * (1f - t);
        private static float EaseInOut(float t) => t * t * (3f - 2f * t);

        private Pose ComputeMovePose(float dt)
        {
            moveElapsed += dt;
            GetMoveKeys(move, out Pose wind, out Pose strike);

            float windEnd = moveStartup * 0.6f;
            float strikeEnd = moveStartup + moveActive * 0.6f;
            float total = moveStartup + moveActive + moveRecovery;

            if (moveElapsed < windEnd)
            {
                return Pose.Lerp(moveStartPose, wind, EaseOut(Mathf.Clamp01(moveElapsed / Mathf.Max(0.001f, windEnd))));
            }

            if (moveElapsed < strikeEnd)
            {
                if (!arcSpawned)
                {
                    arcSpawned = true;
                    SpawnSlashArc();
                }

                float u = (moveElapsed - windEnd) / Mathf.Max(0.001f, strikeEnd - windEnd);
                return Pose.Lerp(wind, strike, EaseOut(Mathf.Clamp01(u)));
            }

            float r = (moveElapsed - strikeEnd) / Mathf.Max(0.001f, total - strikeEnd);
            return Pose.Lerp(strike, Ready, EaseInOut(Mathf.Clamp01(r)));
        }

        private static void GetMoveKeys(Move m, out Pose wind, out Pose strike)
        {
            switch (m)
            {
                case Move.Light1:
                case Move.Slash:
                    wind = Light1Wind; strike = Light1Strike; break;
                case Move.Light2:
                    wind = Light2Wind; strike = Light2Strike; break;
                case Move.Light3:
                case Move.Thrust:
                    wind = Light3Wind; strike = Light3Strike; break;
                case Move.Heavy:
                case Move.Overhead:
                    wind = HeavyWind; strike = HeavyStrike; break;
                default:
                    wind = ParryWind; strike = ParryStrike; break;
            }
        }

        private void SpawnSlashArc()
        {
            float facing = Facing;
            Color color = archetype == Archetype.Akio
                ? new Color(1f, 0.22f, 0.15f, 0.95f)
                : new Color(1f, 0.6f, 0.2f, 0.95f);

            Vector3 origin = transform.position + new Vector3(facing * 0.25f, 1.45f * scaleFactor, -0.2f);
            CombatFeedback fx = CombatFeedback.Instance;

            switch (move)
            {
                case Move.Light1:
                case Move.Slash:
                    fx.SlashArc(origin, facing, 15f, 1.95f, true, color, 0.17f);
                    break;
                case Move.Light2:
                    fx.SlashArc(origin, facing, 25f, 1.95f, false, color, 0.17f);
                    break;
                case Move.Light3:
                case Move.Thrust:
                    fx.SlashArc(origin, facing, 0f, 2.5f, false, color, 0.2f);
                    break;
                case Move.Heavy:
                case Move.Overhead:
                    fx.SlashArc(origin, facing, 35f, 2.5f, true, new Color(color.r, color.g, color.b, 1f), 0.26f);
                    fx.Dust(transform.position + new Vector3(facing * 1.4f, 0.05f, 0f), 5);
                    break;
                case Move.Parry:
                    fx.SlashArc(origin, facing, 40f, 1.5f, true, new Color(1f, 0.9f, 0.8f, 0.9f), 0.14f);
                    break;
            }
        }

        private void ApplyPose(Pose p, float dt)
        {
            // Plant the feet: the lowest foot defines the hip height while grounded.
            float depthF = ThighLength * Mathf.Cos(p.ThighF * Mathf.Deg2Rad) + ShinLength * Mathf.Cos((p.ThighF + p.ShinF) * Mathf.Deg2Rad);
            float depthB = ThighLength * Mathf.Cos(p.ThighB * Mathf.Deg2Rad) + ShinLength * Mathf.Cos((p.ThighB + p.ShinB) * Mathf.Deg2Rad);
            float hipHeight = Mathf.Max(depthF, depthB);

            if (!grounded && !dead)
            {
                hipHeight = Mathf.Max(hipHeight, 0.9f);
            }

            Vector3 hipPosition = hips.localPosition;
            hipPosition.y = Mathf.Lerp(hipPosition.y, hipHeight, dt <= 0f ? 1f : 1f - Mathf.Exp(-30f * dt));
            hipPosition.x = Mathf.Lerp(hipPosition.x, p.Lunge, dt <= 0f ? 1f : 1f - Mathf.Exp(-22f * dt));
            hips.localPosition = hipPosition;

            thighF.localRotation = Quaternion.Euler(0f, 0f, p.ThighF);
            shinF.localRotation = Quaternion.Euler(0f, 0f, p.ShinF);
            thighB.localRotation = Quaternion.Euler(0f, 0f, p.ThighB);
            shinB.localRotation = Quaternion.Euler(0f, 0f, p.ShinB);

            float lean = p.Lean + (hurtTimer > 0f ? -10f : 0f);
            torso.localRotation = Quaternion.Euler(0f, p.Twist, lean);
            head.localRotation = Quaternion.Euler(0f, -p.Twist * 0.6f, -lean * 0.55f + (stunned ? -10f : 0f));

            shoulderF.localRotation = Quaternion.Euler(0f, 0f, p.ShoulderF);
            elbowF.localRotation = Quaternion.Euler(0f, 0f, p.ElbowF);
            shoulderB.localRotation = Quaternion.Euler(0f, 0f, p.ShoulderB);
            elbowB.localRotation = Quaternion.Euler(0f, 0f, p.ElbowB);

            // The blade angle is absolute: counter the arm chain (and the torso lean) so it lands on p.Blade.
            float wrist = p.Blade - (p.ShoulderF + p.ElbowF + lean);
            swordPivot.localRotation = Quaternion.Euler(0f, 0f, wrist);
        }

        private void UpdateRigRoot(float dt)
        {
            squash = Mathf.MoveTowards(squash, 0f, dt * 0.9f);

            float stretch = (!grounded && velocityY > 2f) ? 0.05f : 0f;
            float scaleY = scaleFactor * (1f - squash + stretch);
            float scaleX = scaleFactor * (1f + squash * 0.6f - stretch * 0.5f);
            rigRoot.localScale = new Vector3(scaleX, scaleY, scaleFactor);

            Vector3 position = Vector3.zero;
            if (hurtTimer > 0f)
            {
                position.x = -0.14f * (hurtTimer / 0.22f);
            }

            Quaternion rotation = Quaternion.identity;
            if (dead)
            {
                float t = EaseOut(deathProgress);
                rotation = Quaternion.Euler(0f, 0f, 84f * t);
                position.x = -0.5f * t;
                position.y = 0.28f * t;
            }

            rigRoot.localPosition = position;
            rigRoot.localRotation = rotation;
        }

        private void UpdateCape(float dt)
        {
            if (capeSegments.Count == 0)
            {
                return;
            }

            // Local forward speed: moving forward pushes the cape back (negative angle).
            float forwardSpeed = velocityX * Facing;
            float rise = Mathf.Max(0f, -velocityY) * 2.4f;
            float targetAngle = -(8f + Mathf.Abs(forwardSpeed) * 3.4f + rise);

            if (dashing)
            {
                targetAngle = -78f;
            }

            if (dead)
            {
                targetAngle = -4f;
            }

            targetAngle = Mathf.Clamp(targetAngle, -100f, 0f);
            capeAngle = Mathf.SmoothDamp(capeAngle, targetAngle, ref capeVelocity, 0.14f, Mathf.Infinity, dt);

            float flutter = 0.6f + Mathf.Clamp01(Mathf.Abs(velocityX) / 7f) * 1.4f;
            for (int i = 0; i < capeSegments.Count; i++)
            {
                float wave = Mathf.Sin(idleTime * 4.2f - i * 0.9f) * 3.5f * flutter;
                float angle = capeAngle * (i == 0 ? 0.7f : 0.45f) + wave;
                capeSegments[i].localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void UpdateDashSparks(float dt)
        {
            if (!dashing)
            {
                return;
            }

            dashSparkTimer -= dt;
            if (dashSparkTimer > 0f)
            {
                return;
            }

            dashSparkTimer = 0.03f;
            Vector3 feet = transform.position + new Vector3(0f, 0.15f, -0.1f);
            CombatFeedback.Instance.Sparks(feet, new Vector3(-Facing, 0.3f, 0f), 2, 6f, false);
        }

        // ---- Emission ---------------------------------------------------------

        private void UpdateEmission(float dt)
        {
            float flashStrength = 0f;
            if (flashAge < flashDuration)
            {
                flashAge += dt;
                flashStrength = Mathf.Clamp01(1f - flashAge / flashDuration);
                emissionDirty = true;
            }
            else if (flashColor != Color.black)
            {
                flashColor = Color.black;
                emissionDirty = true;
            }

            if (!emissionDirty)
            {
                return;
            }

            emissionDirty = false;
            bool anyEffect = flashStrength > 0f || glowColor != Color.black;

            if (!anyEffect)
            {
                if (emissionApplied)
                {
                    foreach (Tracked t in tracked)
                    {
                        t.Renderer.SetPropertyBlock(null);
                    }

                    emissionApplied = false;
                }

                return;
            }

            Color add = flashColor * flashStrength + glowColor;
            foreach (Tracked t in tracked)
            {
                t.Renderer.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", t.BaseEmission + add);
                t.Renderer.SetPropertyBlock(block);
            }

            emissionApplied = true;
        }
    }
}

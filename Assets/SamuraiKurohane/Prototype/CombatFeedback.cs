using System.Collections.Generic;
using UnityEngine;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    /// <summary>
    /// Central "game feel" service: hit-stop, camera shake, sparks, light flashes,
    /// impact rings and slash arcs. Created on demand; nothing needs to be wired in the scene.
    /// </summary>
    public sealed class CombatFeedback : MonoBehaviour
    {
        private sealed class Flash
        {
            public Light Light;
            public float Age;
            public float Duration;
            public float Peak;
        }

        private sealed class Visual
        {
            public Transform Transform;
            public Renderer Renderer;
            public float Age;
            public float Duration;
            public float StartScale;
            public float EndScale;
            public Color Color;
            public Vector3 BaseScale;
            public bool IsArc;
        }

        private static CombatFeedback instance;

        public static CombatFeedback Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new("__CombatFeedback");
                    instance = go.AddComponent<CombatFeedback>();
                }

                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
        }

        private const float HitStopTimeScale = 0.03f;

        private readonly List<Flash> flashes = new();
        private readonly List<Visual> visuals = new();
        private readonly Stack<Flash> flashPool = new();

        private ParticleSystem hitSparks;
        private ParticleSystem parrySparks;
        private ParticleSystem dustPuffs;
        private Material ringMaterial;
        private Material arcMaterial;
        private Mesh arcMesh;
        private MaterialPropertyBlock block;
        private float hitStopUntil;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            block = new MaterialPropertyBlock();

            hitSparks = CreateParticles("Hit Sparks", new Color(1f, 0.38f, 0.12f), 0.10f, 0.30f, 0.9f, true);
            parrySparks = CreateParticles("Parry Sparks", new Color(1f, 0.92f, 0.85f), 0.13f, 0.38f, 0.5f, true);
            dustPuffs = CreateParticles("Dust", new Color(0.55f, 0.18f, 0.18f), 0.35f, 0.55f, -0.05f, false);

            ringMaterial = PrototypeMaterials.CreateVfx("Impact Ring", Color.white, true);
            arcMaterial = PrototypeMaterials.CreateVfx("Slash Arc", Color.white, true);
            arcMesh = BuildArcMesh();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                Time.timeScale = 1f;
                instance = null;
            }
        }

        private void Update()
        {
            if (Time.timeScale < 1f && Time.unscaledTime >= hitStopUntil)
            {
                Time.timeScale = 1f;
            }

            UpdateFlashes(Time.unscaledDeltaTime);
            UpdateVisuals(Time.unscaledDeltaTime);
        }

        // ------------------------------------------------------------------
        // Public API
        // ------------------------------------------------------------------

        public void HitStop(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            float until = Time.unscaledTime + duration;
            if (until > hitStopUntil)
            {
                hitStopUntil = until;
            }

            Time.timeScale = HitStopTimeScale;
        }

        public void Shake(float amplitude, float duration)
        {
            PrototypeCameraRig.Current?.AddShake(amplitude, duration);
        }

        public void ZoomPunch(float amount)
        {
            PrototypeCameraRig.Current?.AddZoomPunch(amount);
        }

        public void Sparks(Vector3 position, Vector3 direction, int count, float speed, bool parry)
        {
            ParticleSystem system = parry ? parrySparks : hitSparks;
            if (system == null)
            {
                return;
            }

            ParticleSystem.EmitParams emit = new();
            Vector3 baseDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.up;

            for (int i = 0; i < count; i++)
            {
                Vector3 random = Random.insideUnitSphere * 0.95f;
                random.z *= 0.15f;
                Vector3 velocity = (baseDirection + random).normalized * Random.Range(speed * 0.45f, speed);

                emit.position = position;
                emit.velocity = velocity;
                emit.startLifetime = Random.Range(0.14f, 0.34f);
                emit.startSize = Random.Range(0.05f, 0.12f);
                system.Emit(emit, 1);
            }
        }

        public void Dust(Vector3 position, int count)
        {
            if (dustPuffs == null)
            {
                return;
            }

            ParticleSystem.EmitParams emit = new();
            for (int i = 0; i < count; i++)
            {
                emit.position = position + new Vector3(Random.Range(-0.4f, 0.4f), 0f, 0f);
                emit.velocity = new Vector3(Random.Range(-1.6f, 1.6f), Random.Range(0.3f, 1.2f), 0f);
                emit.startLifetime = Random.Range(0.25f, 0.5f);
                emit.startSize = Random.Range(0.25f, 0.5f);
                dustPuffs.Emit(emit, 1);
            }
        }

        public void LightFlash(Vector3 position, Color color, float peakIntensity, float range, float duration)
        {
            Flash flash = flashPool.Count > 0 ? flashPool.Pop() : CreateFlash();
            flash.Light.transform.position = position;
            flash.Light.color = color;
            flash.Light.range = range;
            flash.Light.intensity = peakIntensity;
            flash.Light.enabled = true;
            flash.Age = 0f;
            flash.Duration = Mathf.Max(0.02f, duration);
            flash.Peak = peakIntensity;
            flashes.Add(flash);
        }

        /// <summary>Expanding glow ring used for parries, posture breaks and telegraphs.</summary>
        public void Pulse(Vector3 position, Color color, float startSize, float endSize, float duration)
        {
            Visual visual = CreateQuadVisual("Pulse", position, ringMaterial, color, duration);
            visual.StartScale = startSize;
            visual.EndScale = endSize;
            visual.Transform.localScale = Vector3.one * startSize;
        }

        /// <summary>
        /// Crescent slash. <paramref name="centerAngle"/> is the arc's centre direction for a
        /// character facing right (degrees, 0 = forward, 90 = up). <paramref name="clockwise"/>
        /// is the sweep direction for a character facing right.
        /// </summary>
        public void SlashArc(
            Vector3 origin,
            float facing,
            float centerAngle,
            float radius,
            bool clockwise,
            Color color,
            float duration)
        {
            bool right = facing >= 0f;
            float worldAngle = right ? centerAngle : 180f - centerAngle;
            bool worldClockwise = right ? clockwise : !clockwise;

            GameObject go = new("Slash Arc");
            go.transform.SetParent(transform, false);
            go.transform.position = origin;
            go.transform.rotation = Quaternion.Euler(0f, 0f, worldAngle);

            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = arcMesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = arcMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            Vector3 baseScale = new(radius, worldClockwise ? -radius : radius, 1f);
            go.transform.localScale = baseScale * 0.88f;

            visuals.Add(new Visual
            {
                Transform = go.transform,
                Renderer = renderer,
                Duration = Mathf.Max(0.04f, duration),
                Color = color,
                BaseScale = baseScale,
                IsArc = true,
                StartScale = 0.88f,
                EndScale = 1.06f
            });
        }

        // ------------------------------------------------------------------
        // Internals
        // ------------------------------------------------------------------

        private ParticleSystem CreateParticles(
            string name,
            Color color,
            float minSize,
            float maxSize,
            float gravity,
            bool stretched)
        {
            GameObject go = new(name);
            go.transform.SetParent(transform, false);

            ParticleSystem system = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = 0.3f;
            main.startSpeed = 0f;
            main.startSize = Mathf.Lerp(minSize, maxSize, 0.5f);
            main.startColor = Color.white;
            main.maxParticles = 500;
            main.gravityModifier = gravity;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = false;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = system.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = stretched ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard;
            renderer.lengthScale = 3.2f;
            renderer.velocityScale = 0.05f;
            renderer.material = PrototypeMaterials.CreateVfx(name, color * 2.2f, stretched);

            system.Play();
            return system;
        }

        private Flash CreateFlash()
        {
            GameObject go = new("Flash Light");
            go.transform.SetParent(transform, false);
            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.shadows = LightShadows.None;
            return new Flash { Light = light };
        }

        private void UpdateFlashes(float dt)
        {
            for (int i = flashes.Count - 1; i >= 0; i--)
            {
                Flash flash = flashes[i];
                flash.Age += dt;
                float t = flash.Age / flash.Duration;

                if (t >= 1f)
                {
                    flash.Light.enabled = false;
                    flashes.RemoveAt(i);
                    flashPool.Push(flash);
                    continue;
                }

                flash.Light.intensity = flash.Peak * (1f - t) * (1f - t);
            }
        }

        private Visual CreateQuadVisual(string name, Vector3 position, Material material, Color color, float duration)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.position = position;

            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            Visual visual = new()
            {
                Transform = go.transform,
                Renderer = renderer,
                Duration = Mathf.Max(0.04f, duration),
                Color = color,
                BaseScale = Vector3.one
            };

            visuals.Add(visual);
            return visual;
        }

        private void UpdateVisuals(float dt)
        {
            for (int i = visuals.Count - 1; i >= 0; i--)
            {
                Visual visual = visuals[i];

                if (visual.Transform == null)
                {
                    visuals.RemoveAt(i);
                    continue;
                }

                visual.Age += dt;
                float t = visual.Age / visual.Duration;

                if (t >= 1f)
                {
                    Destroy(visual.Transform.gameObject);
                    visuals.RemoveAt(i);
                    continue;
                }

                float eased = 1f - (1f - t) * (1f - t);
                float scale = Mathf.Lerp(visual.StartScale, visual.EndScale, eased);

                // Quads (pulses) scale uniformly; arcs keep their sign-flipped base scale.
                if (visual.IsArc)
                {
                    visual.Transform.localScale = visual.BaseScale * scale;
                }
                else
                {
                    visual.Transform.localScale = new Vector3(scale, scale, 1f);
                }

                Color color = visual.Color;
                color.a *= (1f - t) * (1f - t * 0.4f);

                visual.Renderer.GetPropertyBlock(block);
                PrototypeMaterials.SetTint(block, color);
                visual.Renderer.SetPropertyBlock(block);
            }
        }

        /// <summary>
        /// Crescent in the XY plane, radius 1, spanning -60..+60 degrees around +X.
        /// Vertex alpha fades from the tail (start angle) to the head (end angle).
        /// </summary>
        private static Mesh BuildArcMesh()
        {
            const int segments = 18;
            const float halfAngle = 60f;
            const float thickness = 0.34f;

            Vector3[] vertices = new Vector3[(segments + 1) * 2];
            Color[] colors = new Color[vertices.Length];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[segments * 6];

            for (int i = 0; i <= segments; i++)
            {
                float u = i / (float)segments;
                float angle = Mathf.Lerp(-halfAngle, halfAngle, u) * Mathf.Deg2Rad;
                float inner = 1f - thickness * Mathf.Sin(u * Mathf.PI);
                Vector3 dir = new(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

                int outerIndex = i * 2;
                int innerIndex = outerIndex + 1;

                vertices[outerIndex] = dir;
                vertices[innerIndex] = dir * inner;

                float alpha = Mathf.Pow(u, 1.6f);
                colors[outerIndex] = new Color(1f, 1f, 1f, alpha);
                colors[innerIndex] = new Color(1f, 1f, 1f, alpha * 0.1f);

                // Sample the opaque centre of the glow texture.
                uv[outerIndex] = new Vector2(0.5f, 0.5f);
                uv[innerIndex] = new Vector2(0.5f, 0.5f);
            }

            for (int i = 0; i < segments; i++)
            {
                int a = i * 2;
                int b = a + 1;
                int c = a + 2;
                int d = a + 3;
                int t = i * 6;

                triangles[t] = a;
                triangles[t + 1] = c;
                triangles[t + 2] = b;
                triangles[t + 3] = b;
                triangles[t + 4] = c;
                triangles[t + 5] = d;
            }

            Mesh mesh = new() { name = "Prototype Slash Arc" };
            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}

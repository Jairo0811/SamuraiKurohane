using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    /// <summary>
    /// Procedural environment blockout for the Imperial District prototype.
    /// The goal is to establish Samurai Kurohane's 2.5D visual language before
    /// committing to final modular environment art.
    /// </summary>
    public sealed class ImperialDistrictPrototype : MonoBehaviour
    {
        private const string GeneratedRootName = "__ImperialDistrict_Generated";

        [Header("Prototype")]
        [SerializeField] private int seed = 1108;
        [SerializeField] private bool buildOnAwake = true;

        private readonly List<Material> runtimeMaterials = new();

        private Transform generatedRoot;
        private Material stone;
        private Material blackMetal;
        private Material darkWood;
        private Material roof;
        private Material crimson;
        private Material emissiveRed;
        private Material lanternGlow;
        private Material moonMaterial;
        private Material waterMaterial;
        private Material silhouette;

        private void Awake()
        {
            if (buildOnAwake)
            {
                Build();
            }
        }

        [ContextMenu("Build Imperial District")]
        public void Build()
        {
            Random.InitState(seed);
            ClearGenerated();
            ConfigureScene();
            CreateMaterials();

            GameObject root = new(GeneratedRootName);
            root.transform.SetParent(transform, false);
            generatedRoot = root.transform;

            BuildSkyAndDistantCity();
            BuildImperialPalace();
            BuildMainCombatBridge();
            BuildSideArchitecture();
            BuildLanternNetwork();
            BuildToriiAndBanners();
            BuildCrimsonGardens();
            BuildAtmosphericDepth();
            BuildRain();
            BuildPrototypeSpawn();

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.006f;
            RenderSettings.fogColor = new Color(0.11f, 0.018f, 0.025f);
        }

        private void ConfigureScene()
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = 8.7f;
                camera.transform.position = new Vector3(0f, 2.2f, -28f);
                camera.transform.rotation = Quaternion.identity;
                camera.backgroundColor = new Color(0.055f, 0.012f, 0.018f);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 160f;
                camera.allowHDR = true;
            }

            Light directional = FindFirstObjectByType<Light>();
            if (directional != null && directional.type == LightType.Directional)
            {
                directional.color = new Color(0.62f, 0.15f, 0.12f);
                directional.intensity = 1.85f;
                directional.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
                directional.shadows = LightShadows.Soft;
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.16f, 0.035f, 0.045f);
        }

        private void CreateMaterials()
        {
            stone = CreateMaterial("Stone", new Color(0.11f, 0.095f, 0.10f), 0.15f, 0.72f);
            blackMetal = CreateMaterial("Black Metal", new Color(0.055f, 0.05f, 0.055f), 0.72f, 0.70f);
            darkWood = CreateMaterial("Dark Wood", new Color(0.095f, 0.025f, 0.025f), 0.08f, 0.50f);
            roof = CreateMaterial("Roof", new Color(0.065f, 0.055f, 0.06f), 0.38f, 0.66f);
            crimson = CreateMaterial("Kurohane Crimson", new Color(0.38f, 0.012f, 0.018f), 0.22f, 0.34f);
            emissiveRed = CreateMaterial("Neon Crimson", new Color(0.42f, 0.01f, 0.012f), 0.18f, 0.2f,
                new Color(5.2f, 0.035f, 0.025f));
            lanternGlow = CreateMaterial("Lantern Glow", new Color(1f, 0.20f, 0.025f), 0f, 0.12f,
                new Color(6.5f, 0.6f, 0.05f));
            moonMaterial = CreateMaterial("Blood Moon", new Color(0.55f, 0.02f, 0.025f), 0f, 0.1f,
                new Color(2.7f, 0.02f, 0.03f));
            waterMaterial = CreateMaterial("Canal", new Color(0.025f, 0.012f, 0.017f), 0.66f, 0.12f,
                new Color(0.28f, 0.006f, 0.008f));
            silhouette = CreateMaterial("Distant Silhouette", new Color(0.035f, 0.018f, 0.022f), 0f, 0.55f);
        }

        private Material CreateMaterial(string name, Color color, float metallic, float smoothness, Color? emission = null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new(shader) { name = name };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else
            {
                material.color = color;
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }
            else if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            if (emission.HasValue && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
            }

            runtimeMaterials.Add(material);
            return material;
        }

        private void BuildSkyAndDistantCity()
        {
            CreateSphere("Blood Moon", new Vector3(9.2f, 8.1f, 48f), new Vector3(5.6f, 5.6f, 1.1f), moonMaterial, false);

            Transform skyline = NewGroup("Distant Skyline");
            for (int i = 0; i < 12; i++)
            {
                float x = -27f + i * 5.1f + Random.Range(-0.8f, 0.8f);
                float h = Random.Range(3.5f, 8f);
                BuildPagoda(skyline, x, -2f + h * 0.5f, h, Random.Range(3.5f, 6.5f), 31f + Random.Range(-3f, 5f), silhouette, false);
            }

            for (int i = 0; i < 6; i++)
            {
                float x = -20f + i * 8f;
                CreateCube("Distant Tower", new Vector3(x, 2.8f, 36f), new Vector3(1.3f, 9f, 2.5f), silhouette, false);
                CreateRoof(x, 7.25f, 3.3f, 2.8f, 36f, silhouette, false);
            }
        }

        private void BuildImperialPalace()
        {
            Transform palace = NewGroup("Upper Imperial Palace");

            CreateCube("Palace Terrace", new Vector3(5.5f, 1.3f, 15f), new Vector3(24f, 1.4f, 7f), stone, false);

            for (int level = 0; level < 3; level++)
            {
                float width = 18f - level * 2.2f;
                float y = 3.2f + level * 2.35f;
                CreateCube($"Palace Floor {level + 1}", new Vector3(5.5f, y, 17f),
                    new Vector3(width, 1.8f, 5f), darkWood, false, palace);
                CreateRoof(5.5f, y + 1.05f, width + 2f, 6f, 17f, roof, false, palace);

                int windows = Mathf.Max(4, Mathf.RoundToInt(width / 2f));
                for (int i = 0; i < windows; i++)
                {
                    float t = windows == 1 ? 0f : i / (float)(windows - 1);
                    float x = Mathf.Lerp(5.5f - width * 0.42f, 5.5f + width * 0.42f, t);
                    CreateCube("Palace Window", new Vector3(x, y, 14.42f), new Vector3(0.55f, 0.8f, 0.08f), lanternGlow, false, palace);
                }
            }

            CreateTorii(palace, 5.5f, 0.2f, 12f, 13.4f, false);
        }

        private void BuildMainCombatBridge()
        {
            Transform bridge = NewGroup("Main Combat Bridge");

            CreateCube("Main Stone Deck", new Vector3(0f, -2.35f, 0f), new Vector3(39f, 1.15f, 5.8f), stone, true, bridge);
            CreateCube("Wet Surface", new Vector3(0f, -1.72f, -0.25f), new Vector3(39f, 0.12f, 5.2f), blackMetal, false, bridge);\n\n            // Thin emissive strips fake long rain reflections in the blockout.\n            for (int i = 0; i < 13; i++)\n            {\n                float x = -17f + i * 2.8f;\n                CreateCube("Wet Crimson Reflection", new Vector3(x, -1.63f, -1.1f), new Vector3(0.10f, 0.035f, 2.8f), emissiveRed, false, bridge);\n            }

            CreateCube("Lower Canal", new Vector3(0f, -7.4f, 5f), new Vector3(45f, 0.18f, 9f), waterMaterial, false, bridge);

            for (int i = 0; i < 18; i++)
            {
                float x = -18f + i * 2.15f;
                CreateCube("Bridge Rail Post", new Vector3(x, -0.9f, 1.85f), new Vector3(0.16f, 1.65f, 0.16f), blackMetal, false, bridge);
                CreateCube("Bridge Rail Post Rear", new Vector3(x, -0.9f, 2.2f), new Vector3(0.16f, 1.65f, 0.16f), blackMetal, false, bridge);
            }

            CreateCube("Bridge Rail", new Vector3(0f, -0.75f, 1.85f), new Vector3(38f, 0.13f, 0.13f), blackMetal, false, bridge);
            CreateCube("Bridge Rail Rear", new Vector3(0f, -0.75f, 2.2f), new Vector3(38f, 0.13f, 0.13f), blackMetal, false, bridge);

            CreateCube("Left Upper Platform", new Vector3(-14.4f, 2.6f, 1.5f), new Vector3(7.6f, 0.75f, 4.4f), stone, true, bridge);
            CreateCube("Left Mid Platform", new Vector3(-12.8f, 0.45f, 1.3f), new Vector3(3.2f, 0.5f, 3.8f), stone, true, bridge);

            for (int i = 0; i < 5; i++)
            {
                float x = 4.1f + i * 1.15f;
                float y = -1.4f + i * 0.62f;
                CreateCube("Upper Palace Step", new Vector3(x, y, 4.3f), new Vector3(1.3f, 0.42f, 4f), stone, true, bridge);
            }
        }

        private void BuildSideArchitecture()
        {
            Transform architecture = NewGroup("Side Architecture");

            BuildPagoda(architecture, -18f, 2f, 10f, 7f, 6f, darkWood, false);
            BuildPagoda(architecture, 18.3f, 1.5f, 9f, 6.5f, 7f, darkWood, false);

            CreateCube("Right Fortress Wall", new Vector3(18.2f, 0.5f, 2.2f), new Vector3(4.7f, 8f, 6.5f), darkWood, false, architecture);
            CreateRoof(18.2f, 4.65f, 6.1f, 7f, 2.2f, roof, false, architecture);

            CreateCube("Fortress Door", new Vector3(16.7f, -1.1f, -1.12f), new Vector3(2.5f, 4.3f, 0.18f), blackMetal, false, architecture);
            CreateCube("Door Neon Seam", new Vector3(16.7f, -1.05f, -1.23f), new Vector3(0.11f, 3.6f, 0.06f), emissiveRed, false, architecture);

            for (int i = 0; i < 5; i++)
            {
                float x = -17.2f;
                float y = -1.25f + i * 1.25f;
                CreateCube("Left Tower Slat", new Vector3(x, y, 2.35f), new Vector3(3.8f, 0.13f, 0.18f), crimson, false, architecture);
            }
        }

        private void BuildLanternNetwork()
        {
            Transform lanterns = NewGroup("Lanterns");

            float[] positions = { -16f, -11.5f, -6.8f, -2.2f, 2.6f, 7.4f, 12f, 15.5f };
            foreach (float x in positions)
            {
                BuildLantern(lanterns, new Vector3(x, -0.5f, -1.8f), x > -3f && x < 10f);
            }

            BuildLantern(lanterns, new Vector3(-15.5f, 4.2f, 0.2f), true);
            BuildLantern(lanterns, new Vector3(13.2f, 3.2f, 3.5f), true);
        }

        private void BuildToriiAndBanners()
        {
            Transform dressing = NewGroup("Torii and Clan Banners");

            CreateTorii(dressing, -9.5f, 2.9f, 2f, 7.5f, false);
            CreateTorii(dressing, 13.5f, 2.0f, 3.8f, 7.0f, false);

            float[] bannerX = { -12.8f, -4.5f, 9.6f, 18f };
            foreach (float x in bannerX)
            {
                CreateCube("Kurohane Banner", new Vector3(x, 2.6f, 0.2f), new Vector3(1.5f, 4.2f, 0.1f), crimson, false, dressing);
                CreateCube("Banner Spine", new Vector3(x, 4.8f, 0.15f), new Vector3(1.9f, 0.12f, 0.15f), blackMetal, false, dressing);
            }
        }

        private void BuildCrimsonGardens()
        {
            Transform gardens = NewGroup("Crimson Gardens");

            float[] treeX = { -16.5f, -10.8f, 11.4f, 16.2f };
            foreach (float x in treeX)
            {
                float z = x < 0f ? 5.2f : 6.5f;
                CreateCube("Crimson Tree Trunk", new Vector3(x, 0.3f, z), new Vector3(0.45f, 5.5f, 0.45f), darkWood, false, gardens);

                for (int i = 0; i < 9; i++)
                {
                    Vector3 crown = new Vector3(
                        x + Random.Range(-2.2f, 2.2f),
                        3.1f + Random.Range(-1.1f, 1.6f),
                        z + Random.Range(-0.8f, 0.8f));

                    CreateSphere("Crimson Foliage", crown, new Vector3(
                        Random.Range(1.3f, 2.5f),
                        Random.Range(0.8f, 1.6f),
                        Random.Range(0.7f, 1.3f)), crimson, false, gardens);
                }
            }
        }

        private void BuildAtmosphericDepth()
        {
            Transform atmosphere = NewGroup("Atmospheric Depth");

            // Large translucent-looking dark forms are used as foreground framing
            // until final VFX and authored parallax art replace this blockout.
            CreateCube("Foreground Left Frame", new Vector3(-20.5f, 1.5f, -7.5f), new Vector3(5.5f, 17f, 2f), silhouette, false, atmosphere);
            CreateCube("Foreground Right Frame", new Vector3(20.5f, 2f, -7.5f), new Vector3(5f, 18f, 2f), silhouette, false, atmosphere);

            for (int i = 0; i < 8; i++)
            {
                float x = -15f + i * 4.5f;
                CreateCube("Distant Crimson Beacon", new Vector3(x, Random.Range(0f, 6f), 24f), new Vector3(0.08f, Random.Range(1.5f, 4f), 0.08f), emissiveRed, false, atmosphere);
            }
        }

        private void BuildRain()
        {
            GameObject rainObject = new("Rain");
            rainObject.transform.SetParent(generatedRoot, false);
            rainObject.transform.position = new Vector3(0f, 10f, -4f);

            ParticleSystem rain = rainObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = rain.main;
            main.loop = true;
            main.startLifetime = 2.4f;
            main.startSpeed = 21f;
            main.startSize = 0.035f;
            main.maxParticles = 2200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.2f;

            ParticleSystem.EmissionModule emission = rain.emission;
            emission.rateOverTime = 650f;

            ParticleSystem.ShapeModule shape = rain.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(42f, 1f, 12f);

            rainObject.transform.rotation = Quaternion.Euler(8f, 0f, 6f);

            ParticleSystemRenderer renderer = rain.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 5f;
            renderer.velocityScale = 0.12f;
            renderer.material = CreateMaterial("Rain", new Color(0.45f, 0.12f, 0.12f, 0.7f), 0f, 0f);
        }

        private void BuildPrototypeSpawn()
        {
            Transform spawn = NewGroup("Akio Prototype Spawn");

            GameObject body = CreateCapsule("Akio Blockout", new Vector3(-7.2f, -0.55f, -0.8f), new Vector3(0.65f, 1.25f, 0.65f), silhouette, false, spawn);
            body.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            CreateCube("Kasa", new Vector3(-7.2f, 0.95f, -0.8f), new Vector3(1.5f, 0.12f, 1.2f), roof, false, spawn);
            GameObject blade = CreateCube("Prototype Katana", new Vector3(-6.3f, -0.15f, -0.95f), new Vector3(1.5f, 0.07f, 0.08f), emissiveRed, false, spawn);
            blade.transform.rotation = Quaternion.Euler(0f, 0f, -18f);
        }

        private void BuildPagoda(Transform parent, float x, float centerY, float height, float width, float z, Material material, bool collision)
        {
            int floors = Mathf.Clamp(Mathf.RoundToInt(height / 2.4f), 2, 4);
            float floorHeight = height / floors;

            for (int i = 0; i < floors; i++)
            {
                float taper = 1f - i * 0.12f;
                float y = centerY - height * 0.5f + floorHeight * (i + 0.5f);
                CreateCube("Pagoda Floor", new Vector3(x, y, z), new Vector3(width * taper, floorHeight * 0.82f, 3.4f), material, collision, parent);
                CreateRoof(x, y + floorHeight * 0.46f, width * taper + 1.6f, 4f, z, roof ?? material, collision, parent);
            }
        }

        private void CreateRoof(float x, float y, float width, float depth, float z, Material material, bool collision, Transform parent = null)
        {
            Transform targetParent = parent != null ? parent : generatedRoot;

            CreateCube("Roof Main", new Vector3(x, y, z), new Vector3(width, 0.34f, depth), material, collision, targetParent);

            GameObject left = CreateCube("Roof Eave L", new Vector3(x - width * 0.49f, y + 0.18f, z), new Vector3(1.15f, 0.25f, depth), material, collision, targetParent);
            left.transform.rotation = Quaternion.Euler(0f, 0f, 12f);

            GameObject right = CreateCube("Roof Eave R", new Vector3(x + width * 0.49f, y + 0.18f, z), new Vector3(1.15f, 0.25f, depth), material, collision, targetParent);
            right.transform.rotation = Quaternion.Euler(0f, 0f, -12f);
        }

        private void CreateTorii(Transform parent, float x, float y, float z, float width, bool collision)
        {
            CreateCube("Torii Left Post", new Vector3(x - width * 0.38f, y, z), new Vector3(0.38f, 5.3f, 0.55f), darkWood, collision, parent);
            CreateCube("Torii Right Post", new Vector3(x + width * 0.38f, y, z), new Vector3(0.38f, 5.3f, 0.55f), darkWood, collision, parent);
            CreateCube("Torii Lower Beam", new Vector3(x, y + 1.65f, z), new Vector3(width * 0.9f, 0.38f, 0.7f), darkWood, collision, parent);
            CreateCube("Torii Upper Beam", new Vector3(x, y + 2.35f, z), new Vector3(width * 1.08f, 0.46f, 0.78f), crimson, collision, parent);

            CreateCube("Torii Neon L", new Vector3(x - width * 0.38f, y, z - 0.31f), new Vector3(0.06f, 4.7f, 0.05f), emissiveRed, false, parent);
            CreateCube("Torii Neon R", new Vector3(x + width * 0.38f, y, z - 0.31f), new Vector3(0.06f, 4.7f, 0.05f), emissiveRed, false, parent);
        }

        private void BuildLantern(Transform parent, Vector3 position, bool withLight)
        {
            CreateCube("Lantern Post", position + new Vector3(0f, -1.2f, 0f), new Vector3(0.13f, 2.5f, 0.13f), blackMetal, false, parent);
            CreateCube("Lantern Housing", position, new Vector3(0.65f, 0.95f, 0.65f), blackMetal, false, parent);
            CreateCube("Lantern Core", position, new Vector3(0.42f, 0.68f, 0.42f), lanternGlow, false, parent);
            CreateRoof(position.x, position.y + 0.58f, 0.95f, 0.95f, position.z, roof, false, parent);

            if (withLight)
            {
                GameObject lightObject = new("Lantern Point Light");
                lightObject.transform.SetParent(parent, false);
                lightObject.transform.position = position + new Vector3(0f, 0.1f, -0.25f);
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 7f;
                light.intensity = 3.4f;
                light.color = new Color(1f, 0.12f, 0.025f);
                light.shadows = LightShadows.None;
            }
        }

        private Transform NewGroup(string name)
        {
            GameObject group = new(name);
            group.transform.SetParent(generatedRoot, false);
            return group.transform;
        }

        private GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material, bool collision, Transform parent = null)
        {
            return CreatePrimitive(PrimitiveType.Cube, name, position, scale, material, collision, parent);
        }

        private GameObject CreateSphere(string name, Vector3 position, Vector3 scale, Material material, bool collision, Transform parent = null)
        {
            return CreatePrimitive(PrimitiveType.Sphere, name, position, scale, material, collision, parent);
        }

        private GameObject CreateCapsule(string name, Vector3 position, Vector3 scale, Material material, bool collision, Transform parent = null)
        {
            return CreatePrimitive(PrimitiveType.Capsule, name, position, scale, material, collision, parent);
        }

        private GameObject CreatePrimitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material, bool collision, Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent != null ? parent : generatedRoot, false);
            go.transform.position = position;
            go.transform.localScale = scale;

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }

            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = collision;
            }

            return go;
        }

        private void ClearGenerated()
        {
            Transform old = transform.Find(GeneratedRootName);
            if (old != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(old.gameObject);
                }
                else
                {
                    DestroyImmediate(old.gameObject);
                }
            }

            foreach (Material material in runtimeMaterials)
            {
                if (material == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(material);
                }
                else
                {
                    DestroyImmediate(material);
                }
            }

            runtimeMaterials.Clear();
        }
    }
}

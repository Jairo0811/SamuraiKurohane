using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    /// <summary>
    /// Procedural environment for the Imperial District prototype (core: orchestration, scene
    /// configuration, materials and primitive helpers).
    ///
    /// Layout (world X): Outer Gate (-38..-20)  |  Canal Bridge (-20..20)  |  Palace Approach (20..38)
    /// Depth layers (back to front): sky, far skyline, mid skyline, palace, canal backdrop,
    /// gameplay lane (z = -0.8), foreground silhouettes. Layers scroll with parallax.
    ///
    /// Split across partial files:
    ///   ImperialDistrictPrototype.cs              core / helpers
    ///   ImperialDistrictPrototype.Architecture.cs buildings, bridge, platforms, props
    ///   ImperialDistrictPrototype.Atmosphere.cs   sky, light, mist, rain
    ///   ImperialDistrictPrototype.Characters.cs   Akio, enemies, camera and HUD wiring
    /// </summary>
    public sealed partial class ImperialDistrictPrototype : MonoBehaviour
    {
        private const string GeneratedRootName = "__ImperialDistrict_Generated";

        // Gameplay lane and level metrics.
        private const float LaneZ = -0.8f;
        private const float DeckTop = -1.775f;
        private const float HalfLength = 38f;
        private const float CameraRange = 22f;
        private const float HalfView = 21f;
        private const float CameraBaseY = 2.2f;
        private const float OrthoSize = 8.7f;

        [Header("Prototype")]
        [SerializeField] private int seed = 1108;
        [SerializeField] private bool buildOnAwake = true;

        [Header("Atmosphere")]
        [SerializeField] private bool rain = true;
        [SerializeField] private bool petalsAndEmbers = true;

        private readonly List<Material> runtimeMaterials = new();
        private readonly List<GameObject> cameraAttached = new();
        // Unity prohibe crear un MaterialPropertyBlock en un inicializador de campo
        // de MonoBehaviour: se crea de forma diferida en el primer uso.
        private MaterialPropertyBlock propertyBlock;
        private MaterialPropertyBlock Block => propertyBlock ??= new MaterialPropertyBlock();

        private Transform generatedRoot;
        private Camera sceneCamera;
        private bool shadowsEnabled = true;

        // Lit materials.
        private Material stone;
        private Material darkStone;
        private Material blackMetal;
        private Material darkWood;
        private Material roof;
        private Material crimson;
        private Material emissiveRed;
        private Material lanternGlow;
        private Material warmWindow;
        private Material moonMaterial;
        private Material waterMaterial;
        private Material silhouetteNear;
        private Material silhouetteMid;
        private Material silhouetteFar;

        // Transparent / unlit VFX materials.
        private Material glowRed;
        private Material glowAmber;
        private Material glowDark;
        private Material beamMaterial;

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

            // Background layers (no shadows, parallax).
            shadowsEnabled = false;
            BuildSkyAndMoon();
            BuildSkylineLayers();
            BuildImperialPalace();
            BuildCanalBackdrop();

            // Gameplay world.
            shadowsEnabled = true;
            BuildMainCombatBridge();
            BuildLanePlatforms();
            BuildSideArchitecture();
            BuildOuterGateZone();
            BuildPalaceApproach();
            BuildLanternNetwork();
            BuildGameplayReadabilityLights();
            BuildToriiAndBanners();
            BuildCrimsonGardens();
            BuildLevelBounds();

            // Atmosphere and foreground.
            shadowsEnabled = false;
            BuildMistAndLightShafts();
            BuildForegroundLayer();
            BuildWeather();
            shadowsEnabled = true;

            // Characters, camera, HUD.
            AkioPrototypeController player = BuildPrototypeSpawn();
            BuildEnemies(player);
            BuildHeroFocusLight(player.transform);
            BuildCameraRig(player);
            BuildHud(player);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0032f;
            RenderSettings.fogColor = new Color(0.19f, 0.04f, 0.05f);
        }

        // ------------------------------------------------------------------
        // Scene configuration
        // ------------------------------------------------------------------

        private void ConfigureScene()
        {
            sceneCamera = Camera.main;
            if (sceneCamera != null)
            {
                sceneCamera.orthographic = true;
                sceneCamera.orthographicSize = OrthoSize;
                sceneCamera.transform.position = new Vector3(0f, CameraBaseY, -28f);
                sceneCamera.transform.rotation = Quaternion.identity;
                sceneCamera.backgroundColor = new Color(0.095f, 0.02f, 0.03f);
                sceneCamera.clearFlags = CameraClearFlags.SolidColor;
                sceneCamera.nearClipPlane = 0.1f;
                sceneCamera.farClipPlane = 160f;
                sceneCamera.allowHDR = true;
            }

            Light directional = FindFirstObjectByType<Light>();
            if (directional != null && directional.type == LightType.Directional)
            {
                directional.color = new Color(0.62f, 0.15f, 0.12f);
                directional.intensity = 2.2f;
                directional.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
                directional.shadows = LightShadows.Soft;
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.32f, 0.085f, 0.095f);
        }

        private void CreateMaterials()
        {
            stone = Track(PrototypeMaterials.CreateLit("Stone", new Color(0.36f, 0.28f, 0.29f), 0.12f, 0.62f,
                null, PrototypeMaterials.GetPattern(PrototypePattern.StoneBrick)));
            darkStone = Track(PrototypeMaterials.CreateLit("Dark Stone", new Color(0.10f, 0.075f, 0.085f), 0.1f, 0.6f));
            blackMetal = Track(PrototypeMaterials.CreateLit("Black Metal", new Color(0.14f, 0.12f, 0.13f), 0.72f, 0.88f));
            darkWood = Track(PrototypeMaterials.CreateLit("Dark Wood", new Color(0.27f, 0.085f, 0.08f), 0.08f, 0.5f,
                null, PrototypeMaterials.GetPattern(PrototypePattern.WoodPlanks)));
            roof = Track(PrototypeMaterials.CreateLit("Roof", new Color(0.24f, 0.21f, 0.24f), 0.34f, 0.7f,
                null, PrototypeMaterials.GetPattern(PrototypePattern.RoofTiles)));
            crimson = Track(PrototypeMaterials.CreateLit("Kurohane Crimson", new Color(0.38f, 0.012f, 0.018f), 0.22f, 0.34f));
            emissiveRed = Track(PrototypeMaterials.CreateLit("Neon Crimson", new Color(0.42f, 0.01f, 0.012f), 0.18f, 0.2f,
                new Color(5.2f, 0.035f, 0.025f)));
            lanternGlow = Track(PrototypeMaterials.CreateLit("Lantern Glow", new Color(1f, 0.20f, 0.025f), 0f, 0.12f,
                new Color(6.5f, 0.6f, 0.05f)));
            warmWindow = Track(PrototypeMaterials.CreateLit("Warm Window", new Color(1f, 0.45f, 0.12f), 0f, 0.1f,
                new Color(3.4f, 1.0f, 0.18f)));
            moonMaterial = Track(PrototypeMaterials.CreateLit("Blood Moon", new Color(0.55f, 0.02f, 0.025f), 0f, 0.1f,
                new Color(2.7f, 0.02f, 0.03f)));
            waterMaterial = Track(PrototypeMaterials.CreateLit("Canal", new Color(0.04f, 0.015f, 0.022f), 0.72f, 0.5f,
                new Color(0.22f, 0.012f, 0.02f)));

            // Atmospheric perspective: further layers are lighter and redder (the fog does not reach them).
            silhouetteNear = Track(PrototypeMaterials.CreateLit("Silhouette Near", new Color(0.055f, 0.022f, 0.032f), 0f, 0.3f));
            silhouetteMid = Track(PrototypeMaterials.CreateLit("Silhouette Mid", new Color(0.11f, 0.032f, 0.048f), 0f, 0.3f,
                new Color(0.05f, 0.006f, 0.01f)));
            silhouetteFar = Track(PrototypeMaterials.CreateLit("Silhouette Far", new Color(0.19f, 0.045f, 0.065f), 0f, 0.3f,
                new Color(0.12f, 0.012f, 0.02f)));

            glowRed = Track(PrototypeMaterials.CreateVfx("Glow Red", Color.white, true));
            glowAmber = Track(PrototypeMaterials.CreateVfx("Glow Amber", Color.white, true));
            glowDark = Track(PrototypeMaterials.CreateVfx("Glow Dark", Color.white, false));
            beamMaterial = Track(PrototypeMaterials.CreateVfx("Light Beam", Color.white, true, PrototypeMaterials.SoftBeam));
        }

        private Material Track(Material material)
        {
            runtimeMaterials.Add(material);
            return material;
        }

        // ------------------------------------------------------------------
        // Layer + primitive helpers
        // ------------------------------------------------------------------

        private Transform NewGroup(string name, Transform parent = null)
        {
            GameObject group = new(name);
            group.transform.SetParent(parent != null ? parent : generatedRoot, false);
            return group.transform;
        }

        /// <summary>
        /// A parallax group. factor 0 = world locked, 1 = camera locked, negative = foreground.
        /// Content is authored as if the camera were centred on x = 0.
        /// </summary>
        private Transform NewLayer(string name, float horizontalFactor, float verticalFactor)
        {
            Transform layer = NewGroup(name);

            if (sceneCamera != null)
            {
                PrototypeParallaxLayer parallax = layer.gameObject.AddComponent<PrototypeParallaxLayer>();
                parallax.Configure(sceneCamera.transform, horizontalFactor, verticalFactor, 0f, CameraBaseY);
            }

            return layer;
        }

        /// <summary>Half width a layer must cover so the screen never runs out of content.</summary>
        private static float LayerHalfSpan(float factor)
        {
            return HalfView + (1f - factor) * CameraRange + 3f;
        }

        private GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material, bool collision,
            Transform parent = null, float tile = 0f)
        {
            return CreatePrimitive(PrimitiveType.Cube, name, position, scale, material, collision, parent, tile);
        }

        private GameObject CreateSphere(string name, Vector3 position, Vector3 scale, Material material, bool collision,
            Transform parent = null)
        {
            return CreatePrimitive(PrimitiveType.Sphere, name, position, scale, material, collision, parent, 0f);
        }

        private GameObject CreateCylinder(string name, Vector3 position, Vector3 scale, Material material, bool collision,
            Transform parent = null)
        {
            return CreatePrimitive(PrimitiveType.Cylinder, name, position, scale, material, collision, parent, 0f);
        }

        private GameObject CreatePrimitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material,
            bool collision, Transform parent, float tile)
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
                renderer.shadowCastingMode = shadowsEnabled ? ShadowCastingMode.On : ShadowCastingMode.Off;
                renderer.receiveShadows = shadowsEnabled;

                if (tile > 0f)
                {
                    // Texture repeats once per `tile` world units instead of stretching over the whole cube.
                    renderer.GetPropertyBlock(Block);
                    Block.SetVector("_BaseMap_ST", new Vector4(Mathf.Max(0.25f, scale.x / tile), Mathf.Max(0.25f, scale.y / tile), 0f, 0f));
                    renderer.SetPropertyBlock(Block);
                }
            }

            Collider collider = go.GetComponent<Collider>();
            if (collider != null && !collision)
            {
                DestroyObject(collider);
            }

            return go;
        }

        /// <summary>Soft camera-facing glow card (halos, mist, clouds, light shafts).</summary>
        private GameObject CreateGlow(string name, Vector3 position, Vector2 size, Material material, Color tint,
            Transform parent, float zRotation = 0f)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(parent != null ? parent : generatedRoot, false);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyObject(collider);
            }

            Renderer renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            renderer.GetPropertyBlock(Block);
            PrototypeMaterials.SetTint(Block, tint);
            renderer.SetPropertyBlock(Block);
            return go;
        }

        private static void AddDrift(GameObject go, float amplitude, float speed, float phase)
        {
            PrototypeDrift drift = go.AddComponent<PrototypeDrift>();
            drift.Configure(amplitude, speed, phase);
        }

        // ------------------------------------------------------------------
        // Cleanup
        // ------------------------------------------------------------------

        private void ClearGenerated()
        {
            Transform old = transform.Find(GeneratedRootName);
            if (old != null)
            {
                DestroyObject(old.gameObject);
            }

            foreach (GameObject attached in cameraAttached)
            {
                if (attached != null)
                {
                    DestroyObject(attached);
                }
            }

            cameraAttached.Clear();

            foreach (Material material in runtimeMaterials)
            {
                if (material != null)
                {
                    DestroyObject(material);
                }
            }

            runtimeMaterials.Clear();
        }

        private static void DestroyObject(Object target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}

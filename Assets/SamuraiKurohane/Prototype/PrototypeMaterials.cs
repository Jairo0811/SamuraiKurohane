using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    public enum PrototypePattern
    {
        StoneBrick,
        WoodPlanks,
        RoofTiles
    }

    /// <summary>
    /// Runtime material and texture factory shared by the environment, the character
    /// rigs and the combat VFX. Everything is generated in code so the prototype keeps
    /// working without authored assets.
    /// </summary>
    public static class PrototypeMaterials
    {
        private static readonly Dictionary<PrototypePattern, Texture2D> PatternCache = new();
        private static Texture2D softGlow;
        private static Texture2D softBeam;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            PatternCache.Clear();
            softGlow = null;
            softBeam = null;
        }

        // ------------------------------------------------------------------
        // Lit materials
        // ------------------------------------------------------------------

        public static Material CreateLit(
            string name,
            Color color,
            float metallic,
            float smoothness,
            Color? emission = null,
            Texture2D baseMap = null)
        {
            // Prefer the shader referenced by the active render pipeline asset;
            // Shader.Find can return null in Player builds when a shader was stripped.
            Shader shader = GraphicsSettings.currentRenderPipeline?.defaultShader;

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                throw new System.InvalidOperationException(
                    $"No compatible runtime shader is available for prototype material '{name}'.");
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

            if (baseMap != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", baseMap);
                }
                else if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", baseMap);
                }
            }

            // Emission is always enabled (black by default) so a MaterialPropertyBlock can
            // drive hit flashes and telegraph glows without touching shader keywords.
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission ?? Color.black);
            }

            return material;
        }

        // ------------------------------------------------------------------
        // VFX materials (transparent, unlit)
        // ------------------------------------------------------------------

        public static Material CreateVfx(string name, Color tint, bool additive, Texture2D texture = null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            Material material = new(shader) { name = name };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", tint);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", tint);
            }

            Texture2D map = texture != null ? texture : SoftGlow;
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", map);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", map);
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", additive ? 2f : 0f);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.SetOverrideTag("RenderType", "Transparent");
            }

            if (material.HasProperty("_SrcBlendAlpha"))
            {
                material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            }

            if (material.HasProperty("_DstBlendAlpha"))
            {
                material.SetFloat("_DstBlendAlpha", additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
            }

            if (material.HasProperty("_Cull"))
            {
                material.SetFloat("_Cull", 0f);
            }

            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        public static void SetTint(MaterialPropertyBlock block, Color color)
        {
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
        }

        // ------------------------------------------------------------------
        // Procedural textures
        // ------------------------------------------------------------------

        public static Texture2D SoftGlow
        {
            get
            {
                if (softGlow == null)
                {
                    softGlow = BuildRadial(128, 1.6f);
                    softGlow.name = "Prototype Soft Glow";
                }

                return softGlow;
            }
        }

        /// <summary>Soft vertical beam: bright at the top, fading to the bottom and the sides.</summary>
        public static Texture2D SoftBeam
        {
            get
            {
                if (softBeam == null)
                {
                    const int size = 128;
                    Texture2D tex = NewTexture(size, size, TextureWrapMode.Clamp);
                    Color[] pixels = new Color[size * size];

                    for (int y = 0; y < size; y++)
                    {
                        float v = y / (size - 1f);
                        for (int x = 0; x < size; x++)
                        {
                            float u = Mathf.Abs((x + 0.5f) / size * 2f - 1f);
                            float side = Mathf.Pow(1f - u, 1.8f);
                            float vertical = Mathf.Pow(v, 1.3f);
                            pixels[y * size + x] = new Color(1f, 1f, 1f, side * vertical);
                        }
                    }

                    tex.SetPixels(pixels);
                    tex.Apply(false, true);
                    tex.name = "Prototype Soft Beam";
                    softBeam = tex;
                }

                return softBeam;
            }
        }

        public static Texture2D GetPattern(PrototypePattern pattern)
        {
            if (PatternCache.TryGetValue(pattern, out Texture2D cached) && cached != null)
            {
                return cached;
            }

            Texture2D tex = pattern switch
            {
                PrototypePattern.StoneBrick => BuildStoneBrick(),
                PrototypePattern.WoodPlanks => BuildWoodPlanks(),
                _ => BuildRoofTiles()
            };

            tex.name = $"Prototype {pattern}";
            PatternCache[pattern] = tex;
            return tex;
        }

        private static Texture2D NewTexture(int width, int height, TextureWrapMode wrap)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, true)
            {
                wrapMode = wrap,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 4
            };
        }

        private static Texture2D BuildRadial(int size, float power)
        {
            Texture2D tex = NewTexture(size, size, TextureWrapMode.Clamp);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Pow(Mathf.Clamp01(1f - d), power);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, true);
            return tex;
        }

        private static float Hash(int x, int y, int salt)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + salt * 2147483647;
                h = (h ^ (h >> 13)) * 1274126177;
                h ^= h >> 16;
                return (h & 0xFFFF) / 65535f;
            }
        }

        private static Texture2D BuildStoneBrick()
        {
            const int size = 256;
            const int rows = 8;
            const int cols = 4;
            Texture2D tex = NewTexture(size, size, TextureWrapMode.Repeat);
            Color[] pixels = new Color[size * size];

            int brickH = size / rows;
            int brickW = size / cols;

            for (int y = 0; y < size; y++)
            {
                int row = y / brickH;
                int offset = (row % 2 == 0) ? 0 : brickW / 2;

                for (int x = 0; x < size; x++)
                {
                    int xx = (x + offset) % size;
                    int col = xx / brickW;
                    int localX = xx % brickW;
                    int localY = y % brickH;

                    float tone = 0.72f + Hash(col, row, 11) * 0.26f;
                    float grain = 0.93f + Hash(x, y, 5) * 0.07f;
                    float v = tone * grain;

                    bool mortar = localX < 3 || localY < 3;
                    if (mortar)
                    {
                        v = 0.30f + Hash(x, y, 9) * 0.05f;
                    }
                    else if (localX < 6 || localY < 6)
                    {
                        v *= 0.88f;
                    }

                    pixels[y * size + x] = new Color(v, v, v, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, true);
            return tex;
        }

        private static Texture2D BuildWoodPlanks()
        {
            const int size = 256;
            const int planks = 8;
            Texture2D tex = NewTexture(size, size, TextureWrapMode.Repeat);
            Color[] pixels = new Color[size * size];
            int plankW = size / planks;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int plank = x / plankW;
                    int localX = x % plankW;
                    float tone = 0.62f + Hash(plank, 3, 17) * 0.34f;
                    float grain = Mathf.Sin((y + Hash(plank, 1, 3) * 90f) * 0.11f + localX * 0.35f) * 0.5f + 0.5f;
                    float v = tone * (0.84f + grain * 0.16f) * (0.94f + Hash(x, y, 2) * 0.06f);

                    if (localX < 2)
                    {
                        v = 0.22f;
                    }

                    pixels[y * size + x] = new Color(v, v, v, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, true);
            return tex;
        }

        private static Texture2D BuildRoofTiles()
        {
            const int size = 256;
            const int rows = 10;
            const int cols = 12;
            Texture2D tex = NewTexture(size, size, TextureWrapMode.Repeat);
            Color[] pixels = new Color[size * size];
            float rowH = size / (float)rows;
            float colW = size / (float)cols;

            for (int y = 0; y < size; y++)
            {
                int row = Mathf.FloorToInt(y / rowH);
                float inRow = (y - row * rowH) / rowH;

                for (int x = 0; x < size; x++)
                {
                    int col = Mathf.FloorToInt(x / colW);
                    float inCol = (x - col * colW) / colW;

                    // Each tile is a curved ridge: bright crown, dark seams.
                    float ridge = Mathf.Sin(inCol * Mathf.PI);
                    float tone = 0.55f + Hash(col, row, 23) * 0.25f;
                    float v = tone * (0.55f + ridge * 0.45f) * (0.65f + inRow * 0.35f);

                    if (inCol < 0.06f || inCol > 0.94f || inRow < 0.07f)
                    {
                        v *= 0.45f;
                    }

                    pixels[y * size + x] = new Color(v, v, v, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true, true);
            return tex;
        }
    }
}

using UnityEngine;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    public sealed partial class ImperialDistrictPrototype
    {
        // ------------------------------------------------------------------
        // Sky
        // ------------------------------------------------------------------

        private void BuildSkyAndMoon()
        {
            // Camera-locked layer: the sky never scrolls, only the moon drifts slightly.
            Transform sky = NewLayer("Sky and Moon", 1f, 1f);

            CreateGlow("Sky Horizon Glow", new Vector3(0f, -1f, 55f), new Vector2(120f, 30f), glowRed,
                new Color(0.85f, 0.08f, 0.1f, 0.35f), sky);
            CreateGlow("Sky Zenith Shade", new Vector3(0f, 14f, 56f), new Vector2(120f, 18f), glowDark,
                new Color(0.02f, 0f, 0.01f, 0.65f), sky);

            Vector3 moonPos = new(9f, 8.2f, 50f);
            CreateGlow("Moon Halo Outer", moonPos + Vector3.forward * 1.2f, Vector2.one * 34f, glowRed,
                new Color(1f, 0.06f, 0.08f, 0.30f), sky);
            CreateGlow("Moon Halo Inner", moonPos + Vector3.forward * 0.8f, Vector2.one * 20f, glowRed,
                new Color(1f, 0.15f, 0.12f, 0.45f), sky);
            CreateSphere("Blood Moon", moonPos, Vector3.one * 10f, moonMaterial, false, sky);

            // Maria: darker blotches on the disc.
            for (int i = 0; i < 6; i++)
            {
                Vector2 o = Random.insideUnitCircle * 3.0f;
                CreateGlow("Moon Mare", moonPos + new Vector3(o.x, o.y, -4.9f), Vector2.one * Random.Range(1.4f, 2.8f), glowDark,
                    new Color(0.15f, 0f, 0.01f, 0.5f), sky);
            }

            // Cloud streaks crossing the moon.
            for (int i = 0; i < 7; i++)
            {
                GameObject cloud = CreateGlow("Cloud Streak",
                    new Vector3(Random.Range(-30f, 30f), Random.Range(3f, 13f), 46f),
                    new Vector2(Random.Range(14f, 30f), Random.Range(1.2f, 2.6f)), glowDark,
                    new Color(0.06f, 0.005f, 0.02f, 0.55f), sky, Random.Range(-4f, 4f));
                AddDrift(cloud, Random.Range(1.5f, 3.5f), Random.Range(0.04f, 0.1f), Random.value * 6f);
            }

            // Sparks of embers high in the sky.
            for (int i = 0; i < 24; i++)
            {
                GameObject spark = CreateGlow("Sky Spark",
                    new Vector3(Random.Range(-32f, 32f), Random.Range(0f, 15f), 44f),
                    Vector2.one * Random.Range(0.18f, 0.5f), glowAmber, new Color(1f, 0.3f, 0.08f, Random.Range(0.25f, 0.7f)), sky);
                AddDrift(spark, Random.Range(0.2f, 0.7f), Random.Range(0.3f, 1.1f), Random.value * 6f);
            }
        }

        // ------------------------------------------------------------------
        // Mist, light shafts, foreground
        // ------------------------------------------------------------------

        private void BuildMistAndLightShafts()
        {
            // Low mist banks in the gameplay depth range, slow horizontal drift.
            Transform mist = NewGroup("Mist");
            for (int i = 0; i < 12; i++)
            {
                float x = -HalfLength + i * (HalfLength * 2f / 11f);
                GameObject bank = CreateGlow("Mist Bank", new Vector3(x, Random.Range(-1.4f, -0.2f), Random.Range(0.8f, 3.2f)),
                    new Vector2(Random.Range(9f, 15f), Random.Range(2.2f, 3.6f)), glowRed,
                    new Color(0.7f, 0.12f, 0.14f, 0.11f), mist);
                AddDrift(bank, Random.Range(0.8f, 1.8f), Random.Range(0.08f, 0.2f), Random.value * 6f);
            }

            // Moonlight shafts falling diagonally across the street.
            Transform shafts = NewLayer("Light Shafts", 0.15f, 0.15f);
            for (int i = 0; i < 5; i++)
            {
                float x = Random.Range(-18f, 18f);
                CreateGlow("Light Shaft", new Vector3(x, 5f, 8f), new Vector2(Random.Range(2.5f, 4.5f), 22f), beamMaterial,
                    new Color(1f, 0.18f, 0.16f, Random.Range(0.08f, 0.16f)), shafts, 24f);
            }
        }

        private void BuildForegroundLayer()
        {
            // Foreground moves faster than the camera (negative factor) for depth. Kept at the very top and
            // bottom edge of the screen so it never hides combat.
            Transform fg = NewLayer("Foreground", -0.25f, 1f);
            float span = LayerHalfSpan(-0.25f);

            for (float x = -span; x < span; x += 11f)
            {
                float px = x + Random.Range(-2f, 2f);
                float y = CameraBaseY + OrthoSize - 0.4f;

                // Hanging branch with leaves.
                GameObject branch = CreateCube("Foreground Branch", new Vector3(px, y, -6f), new Vector3(Random.Range(5f, 8f), 0.22f, 0.2f), silhouetteNear, false, fg);
                branch.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-14f, 6f));

                for (int i = 0; i < 7; i++)
                {
                    CreateSphere("Foreground Leaf", new Vector3(px + Random.Range(-3f, 3f), y - Random.Range(0.1f, 0.9f), -6f),
                        new Vector3(Random.Range(0.8f, 1.8f), Random.Range(0.4f, 0.9f), 0.5f), silhouetteNear, false, fg);
                }

                // Paper lantern dangling from the branch.
                if (Random.value < 0.6f)
                {
                    float lx = px + Random.Range(-2f, 2f);
                    CreateCube("Paper Lantern Cord", new Vector3(lx, y - 0.8f, -6f), new Vector3(0.03f, 1.2f, 0.03f), silhouetteNear, false, fg);
                    CreateSphere("Paper Lantern", new Vector3(lx, y - 1.7f, -6f), new Vector3(0.7f, 0.9f, 0.7f), lanternGlow, false, fg);
                    CreateGlow("Paper Lantern Halo", new Vector3(lx, y - 1.7f, -6.3f), Vector2.one * 4f, glowAmber,
                        new Color(1f, 0.3f, 0.06f, 0.35f), fg);
                }
            }

            // Dark vignette posts on the bottom edge.
            for (float x = -span; x < span; x += 17f)
            {
                CreateCube("Foreground Post", new Vector3(x + Random.Range(-3f, 3f), CameraBaseY - OrthoSize + 0.8f, -6f),
                    new Vector3(0.9f, 3.2f, 0.6f), silhouetteNear, false, fg);
            }
        }

        // ------------------------------------------------------------------
        // Weather
        // ------------------------------------------------------------------

        private void BuildWeather()
        {
            if (rain)
            {
                CreateRainSystem("Rain Far", 420, 0.5f, 22f, 0.07f, new Color(1f, 0.35f, 0.38f, 0.28f), 12f);
                CreateRainSystem("Rain Near", 160, 0.8f, 30f, 0.12f, new Color(1f, 0.5f, 0.5f, 0.4f), -3f);
                CreateSplashSystem();
            }

            if (petalsAndEmbers)
            {
                CreateAmbientDrifter("Crimson Petals", 70, new Color(0.95f, 0.05f, 0.1f, 0.85f), 0.12f, 1.2f, 3.5f, glowRed, false);
                CreateAmbientDrifter("Embers", 50, new Color(1f, 0.45f, 0.1f, 0.9f), 0.07f, 1.8f, 0.5f, glowAmber, true);
            }
        }

        private void CreateRainSystem(string name, int maxParticles, float lifetime, float speed, float width, Color color, float depth)
        {
            GameObject go = new(name);
            Transform parent = sceneCamera != null ? sceneCamera.transform : generatedRoot;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, OrthoSize + 2f, depth + 8f);
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            cameraAttached.Add(go);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.maxParticles = maxParticles;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = width;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;
            main.prewarm = true;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = maxParticles / lifetime;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(OrthoSize * 2f * 1.9f + 6f, 1f, 1f);

            // Slight wind tilt.
            ParticleSystem.VelocityOverLifetimeModule vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            // Los tres ejes deben declararse en el mismo modo de curva.
            vel.x = new ParticleSystem.MinMaxCurve(-2.2f);
            vel.y = new ParticleSystem.MinMaxCurve(0f);
            vel.z = new ParticleSystem.MinMaxCurve(0f);

            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 6f;
            renderer.velocityScale = 0.04f;
            renderer.sharedMaterial = glowRed;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            ps.Play();
        }

        private void CreateSplashSystem()
        {
            // Small splashes on the deck, kept under the camera horizontally.
            GameObject go = new("Deck Splashes");
            go.transform.SetParent(generatedRoot, false);
            go.transform.position = new Vector3(0f, DeckTop + 0.05f, LaneZ - 0.5f);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.maxParticles = 140;
            main.startLifetime = 0.3f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startColor = new Color(1f, 0.4f, 0.42f, 0.5f);
            main.gravityModifier = 1.4f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 70f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(HalfView * 2f, 0.05f, 1.2f);

            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = glowRed;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            go.AddComponent<PrototypeFollowCameraX>().Configure(sceneCamera != null ? sceneCamera.transform : null);
            ps.Play();
        }

        private void CreateAmbientDrifter(string name, int count, Color color, float size, float lifetime, float fallSpeed,
            Material material, bool rising)
        {
            GameObject go = new(name);
            go.transform.SetParent(generatedRoot, false);
            go.transform.position = new Vector3(0f, rising ? DeckTop : CameraBaseY + OrthoSize, LaneZ + 1.5f);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.maxParticles = count;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 3f, lifetime * 5f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(size, size * 2.2f);
            main.startColor = color;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.prewarm = true;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = count / (lifetime * 4f);

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(HalfView * 2f, rising ? 1f : 0.5f, 5f);

            ParticleSystem.VelocityOverLifetimeModule vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-1.4f, -0.2f);
            vel.y = rising ? new ParticleSystem.MinMaxCurve(0.4f, 1.2f) : new ParticleSystem.MinMaxCurve(-fallSpeed * 0.35f, -fallSpeed * 0.1f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = rising ? 0.6f : 1.0f;
            noise.frequency = 0.4f;

            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient gradient = new();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
            col.color = gradient;

            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            go.AddComponent<PrototypeFollowCameraX>().Configure(sceneCamera != null ? sceneCamera.transform : null);
            ps.Play();
        }
    }
}

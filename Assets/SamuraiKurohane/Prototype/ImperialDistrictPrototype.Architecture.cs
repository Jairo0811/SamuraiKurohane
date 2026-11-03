using UnityEngine;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    public sealed partial class ImperialDistrictPrototype
    {
        private struct Ledge
        {
            public float X;
            public float Width;
            public float TopY;
            public float Thickness;

            public Ledge(float x, float width, float topY, float thickness)
            {
                X = x;
                Width = width;
                TopY = topY;
                Thickness = thickness;
            }
        }

        // ------------------------------------------------------------------
        // Background layers
        // ------------------------------------------------------------------

        private void BuildSkylineLayers()
        {
            BuildSkylineLayer("Far Skyline", 0.92f, 38f, silhouetteFar, 4.6f, 3.5f, 8.5f, false);
            BuildSkylineLayer("Mid Skyline", 0.80f, 28f, silhouetteMid, 6.4f, 5.5f, 12f, true);
        }

        private void BuildSkylineLayer(string name, float factor, float z, Material material, float spacing, float minHeight, float maxHeight, bool lit)
        {
            Transform layer = NewLayer(name, factor, factor);
            float span = LayerHalfSpan(factor);

            // A continuous dark bank under the skyline so the lower screen never shows bare sky.
            CreateCube("Skyline Base", new Vector3(0f, -6f, z), new Vector3(span * 2.4f, 8f, 2f), material, false, layer);

            for (float x = -span; x <= span; x += spacing)
            {
                float px = x + Random.Range(-1.1f, 1.1f);
                float h = Random.Range(minHeight, maxHeight);
                float w = Random.Range(3.4f, 6.4f);
                float pz = z + Random.Range(-2f, 2f);

                BuildPagoda(layer, px, -2f + h * 0.5f, h, w, pz, material, material, false, lit);

                if (Random.value < 0.5f)
                {
                    // Slender watch tower between the pagodas.
                    float tx = px + spacing * 0.5f;
                    float th = Random.Range(minHeight * 0.9f, maxHeight * 1.2f);
                    CreateCube("Watch Tower", new Vector3(tx, -2f + th * 0.5f, z + 3f), new Vector3(1.3f, th, 2.2f), material, false, layer);
                    CreateRoof(tx, -2f + th + 0.15f, 3.2f, 2.8f, z + 3f, material, layer);

                    if (lit)
                    {
                        CreateCube("Tower Light", new Vector3(tx, -2f + th * 0.72f, z + 1.88f), new Vector3(0.45f, 0.7f, 0.06f), warmWindow, false, layer);
                    }
                }
            }

            // Haze that fades the base of the skyline into the red sky.
            CreateGlow("Skyline Haze", new Vector3(0f, -0.5f, z - 2.5f), new Vector2(span * 2.4f, 9f), glowRed,
                new Color(0.95f, 0.12f, 0.13f, lit ? 0.18f : 0.28f), layer);
        }

        private void BuildImperialPalace()
        {
            const float factor = 0.55f;
            Transform palace = NewLayer("Upper Imperial Palace", factor, factor);

            CreateCube("Palace Terrace", new Vector3(5.5f, 1.3f, 15f), new Vector3(24f, 1.4f, 7f), stone, false, palace);

            for (int level = 0; level < 3; level++)
            {
                float width = 18f - level * 2.2f;
                float y = 3.2f + level * 2.35f;
                CreateCube($"Palace Floor {level + 1}", new Vector3(5.5f, y, 17f), new Vector3(width, 1.8f, 5f), darkWood, false, palace);
                CreateRoof(5.5f, y + 1.05f, width + 2f, 6f, 17f, roof, palace);

                int windows = Mathf.Max(4, Mathf.RoundToInt(width / 2f));
                for (int i = 0; i < windows; i++)
                {
                    float t = windows == 1 ? 0f : i / (float)(windows - 1);
                    float x = Mathf.Lerp(5.5f - width * 0.42f, 5.5f + width * 0.42f, t);
                    CreateShoji(palace, new Vector3(x, y, 14.45f), 0.62f, 0.9f);
                }
            }

            // Spire and finial.
            CreateCylinder("Palace Spire", new Vector3(5.5f, 11.4f, 17f), new Vector3(0.14f, 1.4f, 0.14f), blackMetal, false, palace);
            CreateSphere("Palace Finial", new Vector3(5.5f, 12.9f, 17f), Vector3.one * 0.36f, emissiveRed, false, palace);

            // Flanking wings and a long palace wall give the layer width for scrolling.
            BuildPagoda(palace, -9f, 4.5f, 8f, 5.5f, 18f, darkWood, roof, false, true);
            BuildPagoda(palace, 20.5f, 5f, 9f, 6f, 18.5f, darkWood, roof, false, true);
            CreateCube("Palace Wall West", new Vector3(-24f, 0.4f, 14f), new Vector3(26f, 2.6f, 1f), darkWood, false, palace);
            CreateRoof(-24f, 1.85f, 27f, 2.2f, 14f, roof, palace);
            CreateCube("Palace Wall East", new Vector3(36f, 0.4f, 14f), new Vector3(26f, 2.6f, 1f), darkWood, false, palace);
            CreateRoof(36f, 1.85f, 27f, 2.2f, 14f, roof, palace);

            CreateTorii(palace, 5.5f, 0.6f, 12f, 13.4f, 9.5f);

            // Terrace lanterns with halos.
            for (int i = 0; i < 7; i++)
            {
                float x = -4f + i * 3.6f;
                CreateCube("Terrace Lantern", new Vector3(x, 2.35f, 11.4f), new Vector3(0.4f, 0.55f, 0.4f), lanternGlow, false, palace);
                CreateGlow("Lantern Halo", new Vector3(x, 2.35f, 11f), Vector2.one * 3.2f, glowAmber, new Color(1f, 0.32f, 0.07f, 0.45f), palace);
            }

            CreateGlow("Palace Glow", new Vector3(5.5f, 6f, 13f), new Vector2(34f, 14f), glowRed, new Color(0.9f, 0.1f, 0.1f, 0.12f), palace);
        }

        private void BuildCanalBackdrop()
        {
            const float factor = 0.3f;
            Transform layer = NewLayer("Canal Backdrop", factor, factor);
            float span = LayerHalfSpan(factor);

            // The deck underside is visible in the lower third of the screen, so give it a reflective canal behind.
            CreateCube("Canal Far Wall", new Vector3(0f, -7f, 12f), new Vector3(span * 2.2f, 6f, 0.5f), waterMaterial, false, layer);

            for (int i = 0; i < 14; i++)
            {
                float x = Random.Range(-span, span);
                bool amber = Random.value < 0.45f;
                GameObject streak = CreateGlow("Reflection Streak", new Vector3(x, -5.9f, 11.4f),
                    new Vector2(Random.Range(0.5f, 1.5f), Random.Range(2.2f, 4.6f)),
                    amber ? glowAmber : glowRed,
                    amber ? new Color(1f, 0.38f, 0.1f, 0.32f) : new Color(1f, 0.1f, 0.1f, 0.34f), layer);
                AddDrift(streak, 0.35f, Random.Range(0.4f, 0.9f), Random.value * 6f);
            }

            for (int i = 0; i < 18; i++)
            {
                GameObject ripple = CreateGlow("Ripple", new Vector3(Random.Range(-span, span), Random.Range(-8.6f, -4.3f), 11.2f),
                    new Vector2(Random.Range(5f, 16f), 0.06f), glowRed, new Color(1f, 0.35f, 0.35f, 0.3f), layer);
                AddDrift(ripple, Random.Range(0.6f, 1.8f), Random.Range(0.25f, 0.6f), Random.value * 6f);
            }
        }

        // ------------------------------------------------------------------
        // Gameplay world: deck, ledges, bounds
        // ------------------------------------------------------------------

        private void BuildMainCombatBridge()
        {
            Transform bridge = NewGroup("Main Combat Bridge");
            float length = HalfLength * 2f;

            CreateCube("Main Stone Deck", new Vector3(0f, -2.35f, 0f), new Vector3(length, 1.15f, 5.8f), stone, true, bridge, 4f);

            // Neon lip along the walkable edge + dark recessed band + seams on the visible front face.
            CreateCube("Deck Lip Glow", new Vector3(0f, -1.80f, -2.92f), new Vector3(length, 0.05f, 0.06f), emissiveRed, false, bridge);
            CreateCube("Deck Lower Band", new Vector3(0f, -2.82f, -2.93f), new Vector3(length, 0.14f, 0.08f), darkStone, false, bridge);
            CreateCube("Deck Underglow", new Vector3(0f, -2.96f, -2.55f), new Vector3(length, 0.05f, 0.05f), emissiveRed, false, bridge);

            for (float x = -HalfLength + 2.4f; x < HalfLength; x += 4.8f)
            {
                CreateCube("Deck Seam", new Vector3(x, -2.33f, -2.93f), new Vector3(0.08f, 1.02f, 0.06f), darkStone, false, bridge);
            }

            // Piers and hanging canal lanterns under the deck.
            for (float x = -33f; x <= 33f; x += 11f)
            {
                CreateCube("Bridge Pier", new Vector3(x, -5.35f, 0f), new Vector3(2.4f, 4.8f, 3.6f), stone, false, bridge, 3f);
                CreateCube("Pier Cap", new Vector3(x, -2.98f, 0f), new Vector3(3.0f, 0.32f, 4.0f), darkStone, false, bridge);

                float lx = x + 5.5f;
                CreateCube("Canal Lantern", new Vector3(lx, -3.9f, -2.2f), new Vector3(0.42f, 0.6f, 0.42f), lanternGlow, false, bridge);
                CreateCube("Canal Lantern Cord", new Vector3(lx, -3.35f, -2.2f), new Vector3(0.04f, 0.6f, 0.04f), blackMetal, false, bridge);
                CreateGlow("Canal Lantern Halo", new Vector3(lx, -3.9f, -2.6f), Vector2.one * 3.4f, glowAmber, new Color(1f, 0.3f, 0.06f, 0.4f), bridge);
            }

            // Rear railing (behind the combat lane so it never hides Akio).
            for (float x = -HalfLength + 1.2f; x < HalfLength; x += 2.4f)
            {
                CreateCube("Rail Post", new Vector3(x, -0.85f, 2.35f), new Vector3(0.2f, 1.8f, 0.2f), blackMetal, false, bridge);
                CreateCube("Rail Post Cap", new Vector3(x, 0.08f, 2.35f), new Vector3(0.3f, 0.12f, 0.3f), emissiveRed, false, bridge);
            }

            CreateCube("Rail Top", new Vector3(0f, -0.3f, 2.35f), new Vector3(length, 0.13f, 0.13f), blackMetal, false, bridge);
            CreateCube("Rail Mid", new Vector3(0f, -1.0f, 2.35f), new Vector3(length, 0.1f, 0.1f), blackMetal, false, bridge);
        }

        private void BuildLanePlatforms()
        {
            Transform platforms = NewGroup("Lane Platforms");

            // All collision lives on the combat lane (z = LaneZ) so every platform is reachable.
            // Gaps are kept under ~2.1 world units of height so a held jump (about 3.3 units) clears them.
            Ledge[] ledges =
            {
                new(-26.5f, 5.0f, 0.8f, 0.6f),    // gate ledge
                new(-14.4f, 7.6f, 2.975f, 0.75f), // upper platform
                new(-12.8f, 3.2f, 0.7f, 0.5f),    // mid step
                new(-4.5f, 4.0f, 1.9f, 0.55f),    // roof ledge A
                new(2.0f, 3.0f, 3.6f, 0.55f),     // roof ledge B
                new(9.5f, 3.5f, 2.1f, 0.55f)      // roof ledge C
            };

            foreach (Ledge ledge in ledges)
            {
                BuildLedge(platforms, ledge);
            }

            // Stairs up to the palace terrace.
            const float rise = 0.45f;
            const float stepWidth = 1.3f;
            for (int i = 0; i < 6; i++)
            {
                float height = rise * (i + 1);
                float x = 20.65f + i * stepWidth;
                CreateCube("Palace Step", new Vector3(x, DeckTop + height * 0.5f, LaneZ), new Vector3(stepWidth, height, 3.6f), stone, true, platforms, 2f);
                CreateCube("Step Edge Glow", new Vector3(x, DeckTop + height - 0.02f, LaneZ - 1.82f), new Vector3(stepWidth, 0.04f, 0.04f), emissiveRed, false, platforms);
            }

            float terraceHeight = rise * 6f;
            CreateCube("Palace Terrace Block", new Vector3(32.65f, DeckTop + terraceHeight * 0.5f, LaneZ), new Vector3(10.7f, terraceHeight, 3.6f), stone, true, platforms, 2f);
            CreateCube("Terrace Edge Glow", new Vector3(32.65f, DeckTop + terraceHeight - 0.02f, LaneZ - 1.82f), new Vector3(10.7f, 0.04f, 0.04f), emissiveRed, false, platforms);
        }

        private void BuildLedge(Transform parent, Ledge ledge)
        {
            float centerY = ledge.TopY - ledge.Thickness * 0.5f;
            CreateCube("Ledge", new Vector3(ledge.X, centerY, LaneZ), new Vector3(ledge.Width, ledge.Thickness, 3.6f), stone, true, parent, 2f);
            CreateCube("Ledge Edge Glow", new Vector3(ledge.X, ledge.TopY - 0.03f, LaneZ - 1.82f), new Vector3(ledge.Width, 0.04f, 0.04f), emissiveRed, false, parent);

            // Dark cornice and carved brackets so ledges read as architecture, not floating slabs.
            CreateCube("Ledge Cornice", new Vector3(ledge.X, ledge.TopY - ledge.Thickness - 0.08f, LaneZ - 1.6f), new Vector3(ledge.Width + 0.3f, 0.16f, 0.5f), darkWood, false, parent);

            int brackets = Mathf.Max(2, Mathf.RoundToInt(ledge.Width / 2.2f));
            for (int i = 0; i < brackets; i++)
            {
                float t = brackets == 1 ? 0.5f : i / (float)(brackets - 1);
                float bx = Mathf.Lerp(ledge.X - ledge.Width * 0.42f, ledge.X + ledge.Width * 0.42f, t);
                GameObject bracket = CreateCube("Ledge Bracket", new Vector3(bx, ledge.TopY - ledge.Thickness - 0.55f, LaneZ - 1.6f), new Vector3(0.18f, 0.9f, 0.18f), darkWood, false, parent);
                bracket.transform.rotation = Quaternion.Euler(0f, 0f, (i % 2 == 0 ? 1f : -1f) * 12f);
            }

            // Hanging lantern under larger ledges.
            if (ledge.Width >= 3f)
            {
                Vector3 lanternPos = new(ledge.X + ledge.Width * 0.38f, ledge.TopY - ledge.Thickness - 0.9f, LaneZ - 1.5f);
                CreateCube("Ledge Lantern", lanternPos, new Vector3(0.34f, 0.5f, 0.34f), lanternGlow, false, parent);
                CreateGlow("Ledge Lantern Halo", lanternPos + Vector3.back * 0.4f, Vector2.one * 2.8f, glowAmber, new Color(1f, 0.3f, 0.06f, 0.4f), parent);
            }
        }

        private void BuildLevelBounds()
        {
            Transform bounds = NewGroup("Level Bounds");

            foreach (float sign in new[] { -1f, 1f })
            {
                GameObject wall = new(sign < 0f ? "Bound West" : "Bound East");
                wall.transform.SetParent(bounds, false);
                wall.transform.position = new Vector3(sign * (HalfLength + 0.5f), 6f, LaneZ);
                BoxCollider box = wall.AddComponent<BoxCollider>();
                box.size = new Vector3(1f, 40f, 8f);
            }
        }

        // ------------------------------------------------------------------
        // Zones
        // ------------------------------------------------------------------

        private void BuildSideArchitecture()
        {
            Transform architecture = NewGroup("Side Architecture");

            BuildPagoda(architecture, -18f, 2f, 10f, 7f, 6f, darkWood, roof, false, true);
            BuildPagoda(architecture, 18.3f, 1.5f, 9f, 6.5f, 7f, darkWood, roof, false, true);

            // The fortress sits behind the combat lane so it can never swallow Akio.
            CreateCube("Right Fortress Wall", new Vector3(11.4f, 0.5f, 4.9f), new Vector3(4.7f, 8f, 6.5f), darkWood, false, architecture, 3f);
            CreateRoof(11.4f, 4.65f, 6.1f, 7f, 4.9f, roof, architecture);

            CreateCube("Fortress Door", new Vector3(11.4f, -0.2f, 1.6f), new Vector3(2.5f, 3.6f, 0.18f), blackMetal, false, architecture);
            CreateCube("Door Neon Seam", new Vector3(11.4f, -0.2f, 1.49f), new Vector3(0.11f, 3.4f, 0.06f), emissiveRed, false, architecture);
            CreateGlow("Door Glow", new Vector3(11.4f, -0.2f, 1.3f), new Vector2(5f, 5.5f), glowRed, new Color(1f, 0.1f, 0.08f, 0.22f), architecture);

            for (int i = 0; i < 5; i++)
            {
                CreateCube("Left Tower Slat", new Vector3(-17.2f, -1.25f + i * 1.25f, 2.35f), new Vector3(3.8f, 0.13f, 0.18f), crimson, false, architecture);
            }
        }

        private void BuildOuterGateZone()
        {
            Transform zone = NewGroup("Outer Gate Zone");

            // The great gate is the first landmark the player sees.
            CreateTorii(zone, -30f, DeckTop, 2.4f, 10f, 9.8f);

            BuildStoneLantern(zone, -35.2f, DeckTop, 1.0f);
            BuildStoneLantern(zone, -24.8f, DeckTop, 1.0f);

            CreateCube("Gate Wall", new Vector3(-30f, DeckTop + 2.0f, 5.6f), new Vector3(18f, 4.2f, 1.2f), darkWood, false, zone, 3f);
            CreateRoof(-30f, DeckTop + 4.3f, 19f, 2.6f, 5.6f, roof, zone);

            BuildPagoda(zone, -36.6f, 2.8f, 11.2f, 6.4f, 6.5f, darkWood, roof, false, true);

            for (int i = 0; i < 4; i++)
            {
                BuildBanner(zone, -33f + i * 2.7f, 4.6f, 1.6f, 2.2f);
            }
        }

        private void BuildPalaceApproach()
        {
            Transform zone = NewGroup("Palace Approach");
            const float terraceTop = DeckTop + 0.45f * 6f;

            // Gate hall crowning the terrace.
            CreateCube("Gate Hall Body", new Vector3(33f, terraceTop + 3.3f, 3.4f), new Vector3(9f, 6.6f, 3.4f), darkWood, false, zone, 3f);
            CreateRoof(33f, terraceTop + 6.8f, 12f, 4.8f, 3.4f, roof, zone);
            CreateRoof(33f, terraceTop + 8.4f, 8.2f, 4.0f, 3.4f, roof, zone);
            CreateCube("Gate Hall Door", new Vector3(33f, terraceTop + 1.9f, 1.62f), new Vector3(2.6f, 3.8f, 0.12f), warmWindow, false, zone);
            CreateCube("Gate Hall Door Frame", new Vector3(33f, terraceTop + 1.9f, 1.55f), new Vector3(2.9f, 4.1f, 0.1f), blackMetal, false, zone);
            CreateCube("Gate Hall Door Seam", new Vector3(33f, terraceTop + 1.9f, 1.5f), new Vector3(0.06f, 3.8f, 0.06f), blackMetal, false, zone);
            CreateGlow("Gate Hall Glow", new Vector3(33f, terraceTop + 2.2f, 1.2f), new Vector2(9f, 8f), glowAmber, new Color(1f, 0.35f, 0.08f, 0.25f), zone);

            BuildStoneLantern(zone, 28.4f, terraceTop, 1.0f);
            BuildStoneLantern(zone, 37.0f, terraceTop, 1.0f);

            // Banner pair framing the stairs.
            BuildBanner(zone, 21.5f, 5.2f, 1.8f, 1.4f);
            BuildBanner(zone, 26.5f, 5.2f, 1.8f, 1.4f);
        }

        private void BuildLanternNetwork()
        {
            Transform lanterns = NewGroup("Lanterns");

            int index = 0;
            for (float x = -34f; x <= 17f; x += 4.8f)
            {
                // Only every other lantern carries a real light; the rest use halos.
                BuildLantern(lanterns, new Vector3(x, -0.5f, 1.6f), index % 2 == 0);
                index++;
            }

            BuildLantern(lanterns, new Vector3(-15.5f, 4.2f, 1.2f), true);
            BuildLantern(lanterns, new Vector3(13.2f, 3.2f, 3.5f), true);
        }

        private void BuildGameplayReadabilityLights()
        {
            Transform lighting = NewGroup("Gameplay Readability Lights");

            for (float x = -32f; x <= 34f; x += 8.25f)
            {
                GameObject lightObject = new("Gameplay Fill Light");
                lightObject.transform.SetParent(lighting, false);
                lightObject.transform.position = new Vector3(x, 1.35f, -1.2f);

                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 9f;
                light.intensity = 3.0f;
                light.color = new Color(1f, 0.34f, 0.12f);
                light.shadows = LightShadows.None;
            }
        }

        private void BuildToriiAndBanners()
        {
            Transform dressing = NewGroup("Torii and Clan Banners");

            CreateTorii(dressing, -9.5f, DeckTop, 2.0f, 7.5f, 7.2f);
            CreateTorii(dressing, 13.5f, DeckTop, 3.8f, 7.0f, 7.0f);

            float[] bannerX = { -21f, -8.2f, 4.5f, 16.5f };
            foreach (float x in bannerX)
            {
                BuildBanner(dressing, x, 5.4f, 1.5f, 3.4f);
            }
        }

        private void BuildCrimsonGardens()
        {
            Transform gardens = NewGroup("Crimson Gardens");

            float[] treeX = { -33f, -16.5f, -10.8f, 7.4f, 11.4f, 16.2f, 29f };
            foreach (float x in treeX)
            {
                float z = x < 0f ? 5.2f : 6.5f;
                CreateCube("Crimson Tree Trunk", new Vector3(x, 0.3f, z), new Vector3(0.45f, 5.5f, 0.45f), darkWood, false, gardens);

                GameObject branch = CreateCube("Crimson Tree Branch", new Vector3(x + 0.9f, 2.9f, z), new Vector3(2.0f, 0.2f, 0.2f), darkWood, false, gardens);
                branch.transform.rotation = Quaternion.Euler(0f, 0f, 22f);

                for (int i = 0; i < 10; i++)
                {
                    Vector3 crown = new(
                        x + Random.Range(-2.4f, 2.4f),
                        3.2f + Random.Range(-1.1f, 1.7f),
                        z + Random.Range(-0.8f, 0.8f));

                    CreateSphere("Crimson Foliage", crown, new Vector3(
                        Random.Range(1.3f, 2.6f),
                        Random.Range(0.8f, 1.7f),
                        Random.Range(0.7f, 1.3f)), crimson, false, gardens);
                }

                CreateGlow("Foliage Glow", new Vector3(x, 3.3f, z - 1.2f), new Vector2(7f, 4.5f), glowRed, new Color(1f, 0.08f, 0.1f, 0.16f), gardens);
            }
        }

        // ------------------------------------------------------------------
        // Building blocks
        // ------------------------------------------------------------------

        private void BuildPagoda(Transform parent, float x, float centerY, float height, float width, float z,
            Material bodyMaterial, Material roofMaterial, bool collision, bool lit)
        {
            int floors = Mathf.Clamp(Mathf.RoundToInt(height / 2.4f), 2, 4);
            float floorHeight = height / floors;

            for (int i = 0; i < floors; i++)
            {
                float taper = 1f - i * 0.12f;
                float y = centerY - height * 0.5f + floorHeight * (i + 0.5f);
                CreateCube("Pagoda Floor", new Vector3(x, y, z), new Vector3(width * taper, floorHeight * 0.82f, 3.4f), bodyMaterial, collision, parent);
                CreateRoof(x, y + floorHeight * 0.46f, width * taper + 1.6f, 4f, z, roofMaterial, parent);

                if (lit)
                {
                    float frontZ = z - 1.72f;
                    CreateShoji(parent, new Vector3(x - width * taper * 0.22f, y, frontZ), 0.7f, floorHeight * 0.46f);
                    CreateShoji(parent, new Vector3(x + width * taper * 0.22f, y, frontZ), 0.7f, floorHeight * 0.46f);
                }
            }

            // Finial spire.
            float topY = centerY + height * 0.5f + floorHeight * 0.46f;
            CreateCylinder("Pagoda Spire", new Vector3(x, topY + 0.7f, z), new Vector3(0.1f, 0.9f, 0.1f), roofMaterial == roof ? blackMetal : roofMaterial, false, parent);
            if (lit)
            {
                CreateSphere("Pagoda Beacon", new Vector3(x, topY + 1.65f, z), Vector3.one * 0.22f, emissiveRed, false, parent);
            }
        }

        private void CreateRoof(float x, float y, float width, float depth, float z, Material material, Transform parent)
        {
            Transform targetParent = parent != null ? parent : generatedRoot;

            CreateCube("Roof Main", new Vector3(x, y, z), new Vector3(width, 0.34f, depth), material, false, targetParent);

            // Upturned eaves.
            GameObject left = CreateCube("Roof Eave L", new Vector3(x - width * 0.49f, y + 0.18f, z), new Vector3(1.15f, 0.25f, depth), material, false, targetParent);
            left.transform.rotation = Quaternion.Euler(0f, 0f, 12f);

            GameObject right = CreateCube("Roof Eave R", new Vector3(x + width * 0.49f, y + 0.18f, z), new Vector3(1.15f, 0.25f, depth), material, false, targetParent);
            right.transform.rotation = Quaternion.Euler(0f, 0f, -12f);

            // Ridge cap.
            CreateCube("Roof Ridge", new Vector3(x, y + 0.24f, z), new Vector3(width * 0.94f, 0.14f, 0.3f), blackMetal, false, targetParent);
        }

        private void CreateTorii(Transform parent, float x, float baseY, float z, float width, float height)
        {
            float postX = width * 0.38f;
            float postWidth = 0.4f + width * 0.02f;

            foreach (float sign in new[] { -1f, 1f })
            {
                float px = x + sign * postX;
                CreateCube("Torii Post", new Vector3(px, baseY + height * 0.5f, z), new Vector3(postWidth, height, postWidth * 1.1f), darkWood, false, parent);
                CreateCylinder("Torii Base", new Vector3(px, baseY + 0.28f, z), new Vector3(postWidth * 1.7f, 0.28f, postWidth * 1.7f), darkStone, false, parent);
                CreateCube("Torii Neon", new Vector3(px, baseY + height * 0.5f, z - postWidth * 0.58f), new Vector3(0.06f, height * 0.9f, 0.05f), emissiveRed, false, parent);
            }

            CreateCube("Torii Nuki", new Vector3(x, baseY + height * 0.72f, z), new Vector3(width * 0.92f, 0.34f, 0.5f), darkWood, false, parent);

            float topY = baseY + height + 0.15f;
            CreateCube("Torii Kasagi", new Vector3(x, topY, z), new Vector3(width * 1.02f, 0.42f, 0.8f), crimson, false, parent);

            foreach (float sign in new[] { -1f, 1f })
            {
                GameObject end = CreateCube("Torii Kasagi End", new Vector3(x + sign * width * 0.56f, topY + 0.12f, z), new Vector3(width * 0.14f, 0.40f, 0.8f), crimson, false, parent);
                end.transform.rotation = Quaternion.Euler(0f, 0f, sign * 10f);
            }

            CreateCube("Torii Shimaki", new Vector3(x, topY + 0.32f, z), new Vector3(width * 1.04f, 0.16f, 0.9f), blackMetal, false, parent);
            CreateCube("Torii Tablet", new Vector3(x, baseY + height * 0.87f, z), new Vector3(0.6f, height * 0.13f, 0.3f), blackMetal, false, parent);
            CreateCube("Torii Tablet Glow", new Vector3(x, baseY + height * 0.87f, z - 0.16f), new Vector3(0.3f, height * 0.09f, 0.03f), emissiveRed, false, parent);
        }

        private void BuildLantern(Transform parent, Vector3 position, bool withLight)
        {
            CreateCube("Lantern Post", position + new Vector3(0f, -1.2f, 0f), new Vector3(0.13f, 2.5f, 0.13f), blackMetal, false, parent);
            CreateCube("Lantern Housing", position, new Vector3(0.65f, 0.95f, 0.65f), blackMetal, false, parent);
            CreateCube("Lantern Core", position, new Vector3(0.42f, 0.68f, 0.42f), lanternGlow, false, parent);
            CreateRoof(position.x, position.y + 0.58f, 0.95f, 0.95f, position.z, roof, parent);
            CreateGlow("Lantern Halo", position + new Vector3(0f, 0f, -0.7f), Vector2.one * 3.8f, glowAmber, new Color(1f, 0.28f, 0.05f, 0.5f), parent);

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

        private void BuildStoneLantern(Transform parent, float x, float baseY, float z)
        {
            CreateCube("Toro Base", new Vector3(x, baseY + 0.15f, z), new Vector3(0.95f, 0.3f, 0.95f), darkStone, false, parent);
            CreateCube("Toro Post", new Vector3(x, baseY + 0.85f, z), new Vector3(0.34f, 1.1f, 0.34f), stone, false, parent);
            CreateCube("Toro Platform", new Vector3(x, baseY + 1.5f, z), new Vector3(0.9f, 0.18f, 0.9f), darkStone, false, parent);
            CreateCube("Toro Firebox", new Vector3(x, baseY + 1.95f, z), new Vector3(0.6f, 0.62f, 0.6f), darkStone, false, parent);
            CreateCube("Toro Flame", new Vector3(x, baseY + 1.95f, z - 0.02f), new Vector3(0.36f, 0.42f, 0.64f), lanternGlow, false, parent);
            CreateCube("Toro Cap", new Vector3(x, baseY + 2.42f, z), new Vector3(1.15f, 0.22f, 1.15f), darkStone, false, parent);
            CreateSphere("Toro Finial", new Vector3(x, baseY + 2.68f, z), Vector3.one * 0.26f, stone, false, parent);
            CreateGlow("Toro Halo", new Vector3(x, baseY + 1.95f, z - 0.7f), Vector2.one * 3.4f, glowAmber, new Color(1f, 0.3f, 0.06f, 0.42f), parent);
        }

        private void BuildBanner(Transform parent, float x, float topY, float width, float height)
        {
            float centerY = topY - height * 0.5f;
            const float bannerZ = 1.4f;

            CreateCube("Banner Spine", new Vector3(x, topY + 0.05f, bannerZ), new Vector3(width + 0.4f, 0.12f, 0.15f), blackMetal, false, parent);
            CreateCube("Kurohane Banner", new Vector3(x, centerY, bannerZ), new Vector3(width, height, 0.08f), crimson, false, parent);
            CreateCube("Banner Hem", new Vector3(x, topY - height + 0.04f, bannerZ - 0.02f), new Vector3(width, 0.08f, 0.1f), blackMetal, false, parent);

            // Feather emblem: spine + two barbs.
            float emblemY = centerY + height * 0.1f;
            CreateCube("Banner Emblem Spine", new Vector3(x, emblemY, bannerZ - 0.06f), new Vector3(width * 0.55f, 0.07f, 0.02f), emissiveRed, false, parent).transform.rotation = Quaternion.Euler(0f, 0f, 58f);
            CreateCube("Banner Emblem Barb A", new Vector3(x - width * 0.08f, emblemY + height * 0.1f, bannerZ - 0.06f), new Vector3(width * 0.3f, 0.05f, 0.02f), emissiveRed, false, parent).transform.rotation = Quaternion.Euler(0f, 0f, 22f);
            CreateCube("Banner Emblem Barb B", new Vector3(x + width * 0.1f, emblemY - height * 0.08f, bannerZ - 0.06f), new Vector3(width * 0.3f, 0.05f, 0.02f), emissiveRed, false, parent).transform.rotation = Quaternion.Euler(0f, 0f, 22f);
        }

        /// <summary>Glowing shoji window: warm pane, dark lattice and a soft halo.</summary>
        private void CreateShoji(Transform parent, Vector3 center, float width, float height)
        {
            CreateCube("Shoji Pane", center, new Vector3(width, height, 0.06f), warmWindow, false, parent);
            CreateCube("Shoji Lattice V", center + Vector3.back * 0.04f, new Vector3(0.04f, height, 0.04f), blackMetal, false, parent);
            CreateCube("Shoji Lattice H", center + Vector3.back * 0.04f, new Vector3(width, 0.04f, 0.04f), blackMetal, false, parent);
            CreateGlow("Shoji Halo", center + Vector3.back * 0.3f, new Vector2(width * 2.8f, height * 2.4f), glowAmber, new Color(1f, 0.3f, 0.06f, 0.2f), parent);
        }
    }
}

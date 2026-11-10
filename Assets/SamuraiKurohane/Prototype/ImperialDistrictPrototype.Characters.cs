using UnityEngine;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    public sealed partial class ImperialDistrictPrototype
    {
        private AkioPrototypeController BuildPrototypeSpawn()
        {
            GameObject player = new("Akio Kurohane");
            player.transform.SetParent(generatedRoot, false);
            player.transform.position = new Vector3(-7.2f, -1.59f, LaneZ);

            Rigidbody body = player.AddComponent<Rigidbody>();
            body.mass = 72f;

            CapsuleCollider capsule = player.AddComponent<CapsuleCollider>();
            capsule.direction = 1;
            capsule.radius = 0.42f;
            capsule.height = 2.35f;
            capsule.center = new Vector3(0f, 1.175f, 0f);

            AkioPrototypeController controller = player.AddComponent<AkioPrototypeController>();

            SamuraiVisualRig rig = SamuraiVisualRig.Build(player.transform, SamuraiVisualRig.Archetype.Akio);
            controller.Initialize(rig.transform, player.transform.position);
            return controller;
        }

        private void BuildEnemies(AkioPrototypeController player)
        {
            Transform enemies = NewGroup("Imperial Guards");

            (float x, string label)[] spawns =
            {
                (4.2f, "TRAINING SAMURAI"),
                (-22f, "GATE SENTINEL"),
                (14f, "STAIR GUARD")
            };

            foreach ((float x, string label) in spawns)
            {
                GameObject enemy = new(label);
                enemy.transform.SetParent(enemies, false);
                enemy.transform.position = new Vector3(x, DeckTop + 0.02f, LaneZ);

                CapsuleCollider capsule = enemy.AddComponent<CapsuleCollider>();
                capsule.direction = 1;
                capsule.radius = 0.45f;
                capsule.height = 2.3f;
                capsule.center = new Vector3(0f, 1.15f, 0f);

                PrototypeCombatTarget target = enemy.AddComponent<PrototypeCombatTarget>();
                target.SetDisplayName(label);

                SamuraiVisualRig rig = SamuraiVisualRig.Build(enemy.transform, SamuraiVisualRig.Archetype.ImperialGuard);
                target.Initialize(player, rig, -HalfLength + 1.5f, HalfLength - 1.5f);

                AttachReadabilityLights(enemy.transform);
            }
        }

        /// <summary>
        /// Clave calida delante y recorte frio detras. Es lo unico que garantiza que un
        /// personaje se lea sobre un fondo oscuro, pase lo que pase con la luz del entorno.
        /// </summary>
        private static void AttachReadabilityLights(Transform character)
        {
            GameObject keyObject = new("Readability Key");
            keyObject.transform.SetParent(character, false);
            keyObject.transform.localPosition = new Vector3(0f, 2.0f, -2.2f);

            Light key = keyObject.AddComponent<Light>();
            key.type = LightType.Point;
            key.range = 5.5f;
            key.intensity = 3.6f;
            key.color = new Color(1f, 0.6f, 0.42f);
            key.shadows = LightShadows.None;

            GameObject rimObject = new("Readability Rim");
            rimObject.transform.SetParent(character, false);
            rimObject.transform.localPosition = new Vector3(0f, 1.9f, 1.8f);

            Light rim = rimObject.AddComponent<Light>();
            rim.type = LightType.Point;
            rim.range = 5f;
            rim.intensity = 5.0f;
            rim.color = new Color(0.66f, 0.8f, 1f);
            rim.shadows = LightShadows.None;
        }

        private void BuildHeroFocusLight(Transform player)
        {
            // Luz clave calida, delante y arriba: define el volumen de la armadura.
            GameObject keyObject = new("Hero Key Light");
            keyObject.transform.SetParent(player, false);
            keyObject.transform.localPosition = new Vector3(0.4f, 2.2f, -2.4f);

            Light key = keyObject.AddComponent<Light>();
            key.type = LightType.Point;
            key.range = 7f;
            key.intensity = 4.2f;
            key.color = new Color(1f, 0.62f, 0.45f);
            key.shadows = LightShadows.None;

            // Luz de recorte fria, detras: es lo que separa la silueta de Akio del
            // fondo calido y rojo. Sin ella el personaje se funde con el entorno.
            GameObject rimObject = new("Hero Rim Light");
            rimObject.transform.SetParent(player, false);
            rimObject.transform.localPosition = new Vector3(-0.2f, 1.9f, 1.9f);

            Light rim = rimObject.AddComponent<Light>();
            rim.type = LightType.Point;
            rim.range = 6.5f;
            rim.intensity = 6.0f;
            rim.color = new Color(0.62f, 0.78f, 1f);
            rim.shadows = LightShadows.None;
        }

        private void BuildCameraRig(AkioPrototypeController player)
        {
            if (sceneCamera == null)
            {
                return;
            }

            PrototypeCameraRig rig = sceneCamera.GetComponent<PrototypeCameraRig>();
            if (rig == null)
            {
                rig = sceneCamera.gameObject.AddComponent<PrototypeCameraRig>();
            }

            rig.Configure(player, -CameraRange, CameraRange, OrthoSize, CameraBaseY, DeckTop + 0.18f);
        }

        private void BuildHud(AkioPrototypeController player)
        {
            PrototypeHUD hud = FindFirstObjectByType<PrototypeHUD>();
            if (hud == null)
            {
                hud = new GameObject("Prototype HUD").AddComponent<PrototypeHUD>();
            }

            hud.Initialize(player);
        }
    }
}

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
            }
        }

        private void BuildHeroFocusLight(Transform player)
        {
            GameObject lightObject = new("Hero Focus Light");
            lightObject.transform.SetParent(player, false);
            lightObject.transform.localPosition = new Vector3(0.4f, 2.2f, -2.4f);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 8f;
            light.intensity = 2.4f;
            light.color = new Color(1f, 0.55f, 0.4f);
            light.shadows = LightShadows.None;
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

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    /// <summary>
    /// Self-installing URP Volume (bloom, vignette, grade, ACES) used only when the Imperial District
    /// prototype exists in the loaded scene. No assets or scene edits required.
    /// </summary>
    public static class PrototypePostProcessing
    {
        private const string VolumeName = "__PrototypePostProcessing";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Object.FindFirstObjectByType<ImperialDistrictPrototype>() == null)
            {
                return;
            }

            if (GameObject.Find(VolumeName) != null)
            {
                return;
            }

            GameObject go = new(VolumeName);
            Volume volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 50f;

            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "Prototype Runtime Profile";

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(0.95f);
            bloom.intensity.Override(0.85f);
            bloom.scatter.Override(0.7f);

            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.34f);
            vignette.smoothness.Override(0.5f);

            ColorAdjustments adjustments = profile.Add<ColorAdjustments>(true);
            adjustments.contrast.Override(14f);

            Tonemapping tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.ACES);

            volume.sharedProfile = profile;

            Camera cam = Camera.main;
            if (cam != null && cam.TryGetComponent(out UniversalAdditionalCameraData data))
            {
                data.renderPostProcessing = true;
            }
        }
    }
}

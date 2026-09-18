using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace JMGameStudios.SamuraiKurohane.Editor
{
    public static class SamuraiKurohaneWindowsBuilder
    {
        private const string ScenePath = "Assets/Scenes/ImperialDistrict_Prototype.unity";
        private const string OutputRelativePath =
            "Builds/Windows/SamuraiKurohanePrototype/SamuraiKurohanePrototype.exe";

        [MenuItem("JM Game Studios/Build/Windows Prototype (.exe)")]
        public static void BuildWindowsPrototype()
        {
            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (scene == null)
            {
                throw new InvalidOperationException(
                    $"Required prototype scene was not found: {ScenePath}");
            }

            string outputPath = Path.GetFullPath(OutputRelativePath);
            string outputDirectory = Path.GetDirectoryName(outputPath);

            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new InvalidOperationException("Could not resolve the Windows build output directory.");
            }

            Directory.CreateDirectory(outputDirectory);

            string previousCompanyName = PlayerSettings.companyName;
            string previousProductName = PlayerSettings.productName;
            string previousVersion = PlayerSettings.bundleVersion;

            try
            {
                PlayerSettings.companyName = "JM Game Studios";
                PlayerSettings.productName = "Samurai Kurohane Prototype";
                PlayerSettings.bundleVersion = "0.1.0";

                BuildPlayerOptions buildOptions = new()
                {
                    scenes = new[] { ScenePath },
                    locationPathName = outputPath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development
                };

                Debug.Log($"[Samurai Kurohane] Building Windows prototype to: {outputPath}");

                BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
                BuildSummary summary = report.summary;

                if (summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Windows build failed. Result: {summary.result}. " +
                        $"Errors: {summary.totalErrors}. Warnings: {summary.totalWarnings}.");
                }

                Debug.Log(
                    $"[Samurai Kurohane] Windows prototype build succeeded. " +
                    $"Size: {summary.totalSize / (1024f * 1024f):0.0} MB. " +
                    $"Output: {outputPath}");
            }
            finally
            {
                PlayerSettings.companyName = previousCompanyName;
                PlayerSettings.productName = previousProductName;
                PlayerSettings.bundleVersion = previousVersion;
            }
        }
    }
}

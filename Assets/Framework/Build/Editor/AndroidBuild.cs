using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Framework.Build
{
    /// <summary>
    /// Builds an Android APK of the scenes enabled in the build settings, with the player settings as they are.
    /// The APK goes to <c>Builds/Android</c>, named after the product. From the menu, or from the command line:
    /// <code>Unity -batchmode -quit -buildTarget Android -projectPath . -executeMethod Framework.Build.AndroidBuild.BuildFromCommandLine</code>
    /// </summary>
    public static class AndroidBuild
    {
        private const string OutputFolder = "Builds/Android";

        [MenuItem("Tools/Build/Android APK")]
        public static void BuildFromMenu()
        {
            var report = Build();
            if (report.summary.result == BuildResult.Succeeded)
            {
                EditorUtility.RevealInFinder(report.summary.outputPath);
            }
        }

        /// <summary>For <c>-executeMethod</c>. Exits with 1 when the build fails, so a script can tell.</summary>
        public static void BuildFromCommandLine()
        {
            var report = Build();
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        private static BuildReport Build()
        {
            var path = Path.Combine(OutputFolder, PlayerSettings.productName.Replace(" ", string.Empty) + ".apk");
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = path,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            // An APK rather than an app bundle, so it installs straight onto a device; the setting is put back after.
            var wasAppBundle = EditorUserBuildSettings.buildAppBundle;
            EditorUserBuildSettings.buildAppBundle = false;
            try
            {
                var report = BuildPipeline.BuildPlayer(options);
                var summary = report.summary;

                // The report's total size counts the build's intermediate files too, so the APK itself is measured.
                var megabytes = summary.result == BuildResult.Succeeded ? new FileInfo(path).Length / (1024f * 1024f) : 0f;
                Debug.Log($"Android build {summary.result}: {path}, {megabytes:0.0} MB, {summary.totalErrors} errors, " +
                          $"{summary.totalTime.TotalSeconds:0} s.");
                return report;
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = wasAppBundle;
            }
        }
    }
}

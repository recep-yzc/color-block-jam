using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ColorBlockJam.Build
{
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

            var wasAppBundle = EditorUserBuildSettings.buildAppBundle;
            EditorUserBuildSettings.buildAppBundle = false;
            try
            {
                var report = BuildPipeline.BuildPlayer(options);
                var summary = report.summary;

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

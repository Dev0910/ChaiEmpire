using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ChaiEmpire.Editor
{
    public static class ChaiEmpireBuild
    {
        public static void BuildAndroid()
        {
            ChaiEmpireSceneBuilder.Build();

            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../ChaiEmpire.apk"));
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.taprilabs.chaiempire");

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/ChaiEmpire/Scenes/ChaiEmpire.unity" },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new System.Exception("Android build failed: " + report.summary.result);
            }

            Debug.Log("Android build written to " + outputPath);
        }
    }
}

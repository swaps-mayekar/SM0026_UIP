#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UIP.EditorTools
{
    public static class MvpIosBuild
    {
        public static void BuildDevelopment()
        {
            try
            {
                var outDir = Path.GetFullPath("Builds/iOS");
                Directory.CreateDirectory(outDir);
                PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.goldbox.uip");
                var options = new BuildPlayerOptions
                {
                    scenes = new[]
                    {
                        "Assets/Scenes/0_SplashScene.unity",
                        "Assets/Scenes/1_AppScene.unity"
                    },
                    locationPathName = outDir,
                    target = BuildTarget.iOS,
                    options = BuildOptions.Development
                };
                var report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError($"iOS build failed: {report.summary.result}");
                    EditorApplication.Exit(1);
                    return;
                }

                Debug.Log($"iOS build succeeded: {outDir}");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                EditorApplication.Exit(1);
            }
        }
    }
}
#endif

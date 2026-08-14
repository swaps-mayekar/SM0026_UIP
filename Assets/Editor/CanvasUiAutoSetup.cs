#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UIP.EditorTools
{
    [InitializeOnLoad]
    public static class CanvasUiAutoSetup
    {
        const string FlagKey = "UIP.CanvasUi.DisclaimerSplash.v1";

        static CanvasUiAutoSetup()
        {
            EditorApplication.delayCall += TrySetupOnce;
        }

        static void TrySetupOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (SessionState.GetBool(FlagKey, false))
            {
                return;
            }

            SessionState.SetBool(FlagKey, true);

            var splashText = File.Exists(CanvasUiBuilder.SplashScenePath)
                ? File.ReadAllText(CanvasUiBuilder.SplashScenePath)
                : string.Empty;
            var appText = File.Exists(CanvasUiBuilder.AppScenePath)
                ? File.ReadAllText(CanvasUiBuilder.AppScenePath)
                : string.Empty;

            var splashOk = splashText.Contains("SplashBootstrap")
                && !splashText.Contains("SettingsPanel")
                && !splashText.Contains("HomePanel");
            var appOk = appText.Contains("AppBootstrap")
                && appText.Contains("HomePanel")
                && !appText.Contains("SettingsPanel")
                && !appText.Contains("SplashPanel");

            if (splashOk && appOk)
            {
                return;
            }

            Debug.Log("UIP: Rebuilding splash disclaimer and app scenes.");
            CanvasUiBuilder.BuildAll();
        }
    }
}
#endif

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UIP.UI;

namespace UIP.EditorTools
{
    [InitializeOnLoad]
    public static class CanvasUiAutoSetup
    {
        const string ScenePath = "Assets/Scenes/0_SplashScene.unity";
        const string FlagKey = "UIP.CanvasUi.AutoSetupAttempted";

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

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.path.EndsWith("0_SplashScene.unity"))
            {
                return;
            }

            var screens = Object.FindObjectsByType<UiScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var expected = System.Enum.GetValues(typeof(UIP.Core.AppScreen)).Length;
            if (screens.Length >= expected)
            {
                return;
            }

            Debug.Log("UIP: Canvas UI screens missing — running Setup Canvas UI automatically.");
            CanvasUiBuilder.BuildScene(ScenePath);
        }
    }
}
#endif

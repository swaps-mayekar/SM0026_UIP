#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UIP.App;
using UIP.Content;
using UIP.Core;
using UIP.UI;

namespace UIP.EditorTools
{
    public static class MvpBuildValidation
    {
        public static void ValidateAndExit()
        {
            try
            {
                Validate();
                Debug.Log("UIP MVP validation succeeded.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("UIP/Validate MVP")]
        public static void ValidateMenu()
        {
            Validate();
            Debug.Log("UIP MVP validation succeeded.");
        }

        public static void Validate()
        {
            var repo = ContentRepository.LoadFromResources();
            if (repo.Questions.Count < 75)
            {
                throw new InvalidOperationException("Expected at least 75 questions.");
            }

            var splash = EditorSceneManager.OpenScene(CanvasUiBuilder.SplashScenePath, OpenSceneMode.Single);
            if (UnityEngine.Object.FindFirstObjectByType<SplashBootstrap>() == null)
            {
                throw new InvalidOperationException("SplashBootstrap missing from splash scene.");
            }

            var splashCanvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (splashCanvas == null)
            {
                throw new InvalidOperationException("Canvas missing from splash scene. Run UIP/Setup Canvas UI.");
            }

            if (UnityEngine.Object.FindFirstObjectByType<Button>() == null)
            {
                throw new InvalidOperationException("Continue button missing from splash disclaimer.");
            }

            if (UnityEngine.Object.FindFirstObjectByType<NavBarView>() != null)
            {
                throw new InvalidOperationException("Splash scene should not contain a nav bar.");
            }

            if (UnityEngine.Object.FindFirstObjectByType<UiScreen>(FindObjectsInactive.Include) != null)
            {
                throw new InvalidOperationException("Splash scene should only contain the full-screen disclaimer.");
            }

            var app = EditorSceneManager.OpenScene(CanvasUiBuilder.AppScenePath, OpenSceneMode.Single);
            var bootstrap = UnityEngine.Object.FindFirstObjectByType<AppBootstrap>();
            if (bootstrap == null)
            {
                throw new InvalidOperationException("AppBootstrap missing from app scene.");
            }

            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                throw new InvalidOperationException("Canvas missing from app scene. Run UIP/Setup Canvas UI.");
            }

            var screens = Resources.FindObjectsOfTypeAll<UiScreen>()
                .Where(s => s != null && s.gameObject.scene.IsValid() && s.gameObject.scene.path.Contains("1_AppScene"))
                .ToList();
            var expected = Enum.GetValues(typeof(AppScreen)).Length;
            if (screens.Count < expected)
            {
                var foundIds = string.Join(", ", screens.Select(s => s.ScreenId).Distinct().OrderBy(x => x));
                throw new InvalidOperationException(
                    $"Expected {expected} UiScreen panels, found {screens.Count}. Present: {foundIds}");
            }

            var ids = screens.Select(s => s.ScreenId).Distinct().Count();
            if (ids < expected)
            {
                throw new InvalidOperationException("Not all AppScreen values have a UiScreen panel.");
            }

            if (UnityEngine.Object.FindFirstObjectByType<NavBarView>() == null)
            {
                throw new InvalidOperationException("NavBarView missing from app scene.");
            }

            var buildScenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
            if (!buildScenes.Contains(CanvasUiBuilder.SplashScenePath) ||
                !buildScenes.Contains(CanvasUiBuilder.AppScenePath))
            {
                throw new InvalidOperationException("Build settings must include splash then app scene.");
            }

            if (PlayerSettings.productName != "Unity Interview Prep")
            {
                throw new InvalidOperationException("Unexpected product name.");
            }

            if (PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS) != "com.goldbox.uip")
            {
                throw new InvalidOperationException("Unexpected iOS bundle id.");
            }

            if (!File.Exists("Docs/AppStoreReviewNotes.md"))
            {
                throw new InvalidOperationException("Missing App Store review notes.");
            }

            Debug.Log($"Validated content v{repo.ContentVersion} with {repo.Questions.Count} questions, splash '{splash.path}', app '{app.path}', {screens.Count} screens.");
        }
    }
}
#endif

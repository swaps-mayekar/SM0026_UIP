#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UIP.Content;

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

            if (Resources.Load<PanelSettings>("Panel/AppPanelSettings") == null)
            {
                throw new InvalidOperationException("Missing PanelSettings resource.");
            }

            if (Resources.Load<UnityEngine.UIElements.VisualTreeAsset>("UI/AppShell") == null)
            {
                throw new InvalidOperationException("Missing AppShell UXML resource.");
            }

            if (Resources.Load<UnityEngine.UIElements.StyleSheet>("UI/AppTheme") == null)
            {
                throw new InvalidOperationException("Missing AppTheme USS resource.");
            }

            if (Resources.Load<UnityEngine.TextCore.Text.FontAsset>("Fonts & Materials/LiberationSans SDF") == null
                && Resources.Load<Font>("UI/Fonts/LiberationSans") == null)
            {
                throw new InvalidOperationException("Missing UI font assets.");
            }

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/0_SplashScene.unity", OpenSceneMode.Single);
            var bootstrap = UnityEngine.Object.FindFirstObjectByType<UIP.App.AppBootstrap>();
            if (bootstrap == null)
            {
                throw new InvalidOperationException("AppBootstrap missing from splash scene.");
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

            Debug.Log($"Validated content v{repo.ContentVersion} with {repo.Questions.Count} questions, scene '{scene.path}'.");
        }
    }
}
#endif

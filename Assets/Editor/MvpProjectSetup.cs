#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UIP.App;

namespace UIP.EditorTools
{
    public static class MvpProjectSetup
    {
        const string ScenePath = "Assets/Scenes/0_SplashScene.unity";
        const string PanelPath = "Assets/Resources/Panel/AppPanelSettings.asset";
        const string ShellPath = "Assets/UI/Views/AppShell.uxml";
        const string ThemePath = "Assets/UI/Styles/AppTheme.uss";
        const string RuntimeThemePath = "Assets/Resources/UI/AppRuntimeTheme.tss";

        [MenuItem("UIP/Setup MVP Scene")]
        public static void SetupFromMenu()
        {
            Setup();
        }

        public static void Setup()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/Panel");
            EnsureFolder("Assets/Resources/Content");
            EnsureFolder("Assets/Resources/UI");

            if (!File.Exists(RuntimeThemePath))
            {
                File.WriteAllText(RuntimeThemePath,
                    "@import url(\"unity-theme://default\");\n@import url(\"AppTheme.uss\");\n");
                AssetDatabase.ImportAsset(RuntimeThemePath);
            }

            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, PanelPath);
            }

            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(390, 844);
            panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panel.match = 0.5f;

            var runtimeTheme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(RuntimeThemePath);
            if (runtimeTheme != null)
            {
                var panelSo = new SerializedObject(panel);
                var themeProp = panelSo.FindProperty("themeUss") ?? panelSo.FindProperty("m_ThemeStyleSheet");
                if (themeProp != null)
                {
                    themeProp.objectReferenceValue = runtimeTheme;
                    panelSo.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorUtility.SetDirty(panel);

            var shell = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ShellPath);
            var theme = AssetDatabase.LoadAssetAtPath<StyleSheet>(ThemePath);
            if (shell == null)
            {
                Debug.LogError($"Missing shell UXML at {ShellPath}");
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name != "Main Camera")
                {
                    Object.DestroyImmediate(root);
                }
            }

            var camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.043f, 0.110f, 0.200f, 1f);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.orthographic = true;
            }

            var appGo = new GameObject("App");
            var doc = appGo.AddComponent<UIDocument>();
            doc.panelSettings = panel;
            doc.visualTreeAsset = shell;
            var bootstrap = appGo.AddComponent<AppBootstrap>();
            var so = new SerializedObject(bootstrap);
            so.FindProperty("uiDocument").objectReferenceValue = doc;
            so.FindProperty("panelSettings").objectReferenceValue = panel;
            so.FindProperty("shellAsset").objectReferenceValue = shell;
            so.FindProperty("theme").objectReferenceValue = theme;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("UIP MVP scene setup complete.");
        }

        [MenuItem("UIP/Batch/Setup MVP")]
        public static void BatchSetup()
        {
            Setup();
            EditorApplication.Exit(0);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            var name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif

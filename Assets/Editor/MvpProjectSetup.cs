#if UNITY_EDITOR
using UnityEditor;
using UIP.Core;

namespace UIP.EditorTools
{
    public static class MvpProjectSetup
    {
        [MenuItem("UIP/Setup Canvas UI")]
        public static void SetupFromMenu()
        {
            Setup();
        }

        [MenuItem("UIP/Setup MVP Scene")]
        public static void SetupMvpAlias()
        {
            Setup();
        }

        public static void Setup()
        {
            CanvasUiBuilder.BuildAll();
        }

        [MenuItem("UIP/Batch/Setup MVP")]
        public static void BatchSetup()
        {
            Setup();
            EditorApplication.Exit(0);
        }
    }
}
#endif

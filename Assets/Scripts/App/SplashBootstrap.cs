using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UIP.UI;

namespace UIP.App
{
    [DefaultExecutionOrder(-100)]
    public sealed class SplashBootstrap : MonoBehaviour
    {
        public const string AppSceneName = "1_AppScene";

        [SerializeField] Button continueButton;
        [SerializeField] Canvas rootCanvas;

        public void Wire(Button continueBtn, Canvas canvas)
        {
            continueButton = continueBtn;
            rootCanvas = canvas;
        }

        void Awake()
        {
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = true;

            if (continueButton == null)
            {
                continueButton = FindFirstObjectByType<Button>();
            }

            if (rootCanvas == null)
            {
                rootCanvas = FindFirstObjectByType<Canvas>();
            }

            if (rootCanvas != null)
            {
                TmpUiFixer.Fix(rootCanvas.transform);
            }

            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(ContinueToApp);
            }
        }

        void ContinueToApp()
        {
            SceneManager.LoadScene(AppSceneName);
        }
    }
}

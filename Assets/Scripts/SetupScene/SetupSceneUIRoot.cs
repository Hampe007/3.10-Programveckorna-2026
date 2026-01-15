using System;
using UnityEngine;
using LocalGame.Session;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LocalGame.SetupScene
{
    /// <summary>
    /// Owns the 3 menu GameObjects and toggles which one is active.
    /// Scene 2 spec: single Canvas, toggling 3 menu GameObjects. :contentReference[oaicite:5]{index=5}
    /// </summary>
    public sealed class SetupSceneUIRoot : MonoBehaviour
    {
        private const string LogPrefix = "[SetupSceneUIRoot]";

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Menu GameObjects (children of the Canvas)")]
        [SerializeField] private GameObject controllerClaimMenu;
        [SerializeField] private GameObject controlsViewMenu;
        [SerializeField] private GameObject characterSelectMenu;

        [Header("Shared UI")]
        [SerializeField] private CenterToastPresenter toast;

        private void Awake()
        {
            // Ensure menu timers work if we came from a paused game.
            Time.timeScale = 1f;

            // Safety: ensure Session exists even if scene is loaded directly in editor.
            try
            {
                var session = GameSession.EnsureExists();
                session.ResetToDefaults();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SetupScene] Reset session failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
            
            ActivateControllerClaim();
        }

        public void ActivateControllerClaim() => SetActiveMenu(controllerClaimMenu);
        public void ActivateControlsView() => SetActiveMenu(controlsViewMenu);
        public void ActivateCharacterSelect() => SetActiveMenu(characterSelectMenu);

        public void ShowToast(string message)
        {
            if (toast != null)
                toast.ShowToast(message);
        }

        public void ReturnToMainMenu()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(mainMenuSceneName))
                {
                    Debug.LogError($"{LogPrefix} mainMenuSceneName is not set.", this);
                    return;
                }

                SceneManager.LoadScene(mainMenuSceneName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} ReturnToMainMenu failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        #if UNITY_EDITOR
        // Dev-only shortcuts.
        private void Update()
        {
            try
            {
                var kb = Keyboard.current;
                if (kb == null)
                    return;

                if (kb.f1Key.wasPressedThisFrame) ActivateControllerClaim();
                if (kb.f2Key.wasPressedThisFrame) ActivateControlsView();
                if (kb.f3Key.wasPressedThisFrame) ActivateCharacterSelect();
                if (kb.tKey.wasPressedThisFrame) ShowToast("Test toast (2 seconds)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Dev shortcut Update failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }
        #endif

        private void SetActiveMenu(GameObject menuToEnable)
        {
            try
            {
                if (controllerClaimMenu == null || controlsViewMenu == null || characterSelectMenu == null)
                {
                    Debug.LogError($"{LogPrefix} One or more menu references are not assigned.", this);
                    return;
                }

                controllerClaimMenu.SetActive(menuToEnable == controllerClaimMenu);
                controlsViewMenu.SetActive(menuToEnable == controlsViewMenu);
                characterSelectMenu.SetActive(menuToEnable == characterSelectMenu);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} SetActiveMenu failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }
    }
}

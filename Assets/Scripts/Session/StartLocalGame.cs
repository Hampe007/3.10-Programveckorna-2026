using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;

namespace LocalGame.Session
{
    /// <summary>
    /// Scene 1 helper:
    /// - Ensure Session exists (DontDestroyOnLoad)
    /// - Reset to defaults
    /// - Load Scene 2 (Setup)
    /// </summary>
    public sealed class StartLocalGame : MonoBehaviour
    {
        private const string LogPrefix = "[StartLocalGame]";

        [Header("Scene Names (match your Build Settings)")]
        [SerializeField] private string setupSceneName = "SetupScene";

        public void StartLocalGameFlow()
        {
            try
            {
                var session = GameSession.EnsureExists();
                session.ResetToDefaults();

                if (string.IsNullOrWhiteSpace(setupSceneName))
                {
                    Debug.LogError($"{LogPrefix} setupSceneName is empty.", this);
                    return;
                }

                SceneManager.LoadScene(setupSceneName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Failed to start game: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }
    }
}
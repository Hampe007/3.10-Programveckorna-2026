using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace LocalGame.SetupScene
{
    /// <summary>
    /// Center-screen toast system.
    /// Spec: centered, 2 seconds, no audio for now. :contentReference[oaicite:4]{index=4}
    /// </summary>
    public sealed class CenterToastPresenter : MonoBehaviour
    {
        private const string LogPrefix = "[CenterToastPresenter]";

        [Header("UI Refs")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text toastText;

        [Header("Behavior")]
        [SerializeField] private bool enableToast = true;
        [SerializeField, Min(0.1f)] private float durationSeconds = 2f;

        private Coroutine _running;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            toastText = GetComponentInChildren<TMP_Text>(true);
        }

        private void Awake()
        {
            HideImmediate();
        }

        public bool IsEnabled => enableToast;

        public void SetEnabled(bool enabled)
        {
            enableToast = enabled;

            if (!enableToast)
            {
                if (_running != null)
                {
                    StopCoroutine(_running);
                    _running = null;
                }

                HideImmediate();
            }
        }

        public void ShowToast(string message)
        {
            if (!enableToast || string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                if (canvasGroup == null || toastText == null)
                {
                    Debug.LogError($"{LogPrefix} Missing references (CanvasGroup and/or TMP_Text).", this);
                    return;
                }

                toastText.text = message;

                if (_running != null)
                {
                    StopCoroutine(_running);
                    _running = null;
                }

                _running = StartCoroutine(ShowForSeconds(durationSeconds));
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} ShowToast failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        public void HideImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private IEnumerator ShowForSeconds(float seconds)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            yield return new WaitForSeconds(seconds);

            HideImmediate();
            _running = null;
        }
    }
}

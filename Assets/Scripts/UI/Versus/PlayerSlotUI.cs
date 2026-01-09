using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlotUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] TMP_Text titleLabel;
    [SerializeField] TMP_Text statusLabel;
    [SerializeField] TMP_Text deviceLabel;
    [SerializeField] TMP_Text helperLabel;

    [Header("Visuals")]
    [SerializeField] Image backdrop;
    [SerializeField] GameObject keyboardBadge;
    [SerializeField] GameObject controllerBadge;
    [SerializeField] GameObject readyBadge;
    [SerializeField] GameObject lockProgressRoot;
    [SerializeField] Image lockProgressFill;

    [Header("Colors")]
    [SerializeField] Color idleColor = Color.gray;
    [SerializeField] Color claimedColor = Color.white;
    [SerializeField] Color readyColor = Color.green;

    string defaultHelperText;

    void Awake()
    {
        if (helperLabel != null)
        {
            defaultHelperText = helperLabel.text;
        }
    }

    public void SetTitle(string text)
    {
        if (titleLabel != null)
        {
            titleLabel.text = text;
        }
    }

    public void ShowIdlePrompt(string prompt, string helperText = "")
    {
        UpdateBackdrop(idleColor);
        SetStatus(prompt);
        SetDeviceText(string.Empty);
        ToggleBadge(keyboardBadge, false);
        ToggleBadge(controllerBadge, false);
        ToggleBadge(readyBadge, false);
        ToggleLockProgress(false, 0);
        SetHelper(helperText);
    }

    public void ShowClaimed(string deviceDescription, bool isController, Color colorAccent, string helperText)
    {
        UpdateBackdrop(claimedColor);
        SetDeviceText(deviceDescription);
        ToggleBadge(keyboardBadge, !isController);
        ToggleBadge(controllerBadge, isController);
        ToggleBadge(readyBadge, false);
        ToggleLockProgress(false, 0);
        SetHelper(helperText);
        if (statusLabel != null)
        {
            statusLabel.text = "Connected";
            statusLabel.color = colorAccent;
        }
    }

    public void ShowStatus(string message)
    {
        SetStatus(message);
    }

    public void ShowBlockingMessage(string message)
    {
        SetStatus(message);
    }

    public void ShowReady(Color accent)
    {
        UpdateBackdrop(readyColor);
        ToggleBadge(readyBadge, true);
        ToggleLockProgress(false, 1f);
        if (statusLabel != null)
        {
            statusLabel.text = "READY";
            statusLabel.color = accent;
        }
    }

    public void ClearReady()
    {
        ToggleBadge(readyBadge, false);
        ToggleLockProgress(false, 0);
    }

    public void ToggleLockProgress(bool visible, float normalized)
    {
        if (lockProgressRoot != null)
        {
            lockProgressRoot.SetActive(visible);
        }
        if (lockProgressFill != null)
        {
            lockProgressFill.fillAmount = Mathf.Clamp01(normalized);
        }
    }

    void SetStatus(string message)
    {
        if (statusLabel != null)
        {
            statusLabel.text = message;
        }
    }

    void SetDeviceText(string message)
    {
        if (deviceLabel != null)
        {
            deviceLabel.text = message;
            deviceLabel.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }
    }

    void UpdateBackdrop(Color color)
    {
        if (backdrop != null)
        {
            backdrop.color = color;
        }
    }

    void ToggleBadge(GameObject target, bool enabled)
    {
        if (target != null)
        {
            target.SetActive(enabled);
        }
    }

    void SetHelper(string helperText)
    {
        if (helperLabel == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(helperText))
        {
            helperLabel.text = defaultHelperText;
            helperLabel.gameObject.SetActive(!string.IsNullOrEmpty(defaultHelperText));
            return;
        }

        helperLabel.text = helperText;
        helperLabel.gameObject.SetActive(true);
    }
}

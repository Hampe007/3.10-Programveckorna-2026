using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCardView : MonoBehaviour
{
    [SerializeField] Image portraitImage;
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] GameObject[] hoverBadges = new GameObject[2];
    [SerializeField] GameObject[] selectionBadges = new GameObject[2];
    [SerializeField] Image[] selectionSwatches = new Image[2];
    [SerializeField] GameObject takenOverlay;
    [SerializeField] Image takenColorOverlay;

    public CharacterDefinition Definition { get; private set; }

    public void Initialize(CharacterDefinition definition)
    {
        Definition = definition;
        if (portraitImage != null)
        {
            portraitImage.sprite = definition != null ? definition.Portrait : null;
            portraitImage.enabled = portraitImage.sprite != null;
        }

        if (nameLabel != null)
        {
            nameLabel.text = definition != null ? definition.DisplayName : "???";
        }

        SetAvailability(true);
        for (int i = 0; i < hoverBadges.Length; i++)
        {
            SetHover(i, false, Color.white);
            SetSelection(i, false, Color.white);
        }
    }

    public void SetHover(int slotIndex, bool enabled, Color accent)
    {
        if (slotIndex < 0 || slotIndex >= hoverBadges.Length)
        {
            return;
        }
        GameObject element = hoverBadges[slotIndex];
        if (element != null)
        {
            element.SetActive(enabled);
            if (enabled && element.TryGetComponent(out Image img))
            {
                img.color = accent;
            }
        }
    }

    public void SetSelection(int slotIndex, bool enabled, Color accent)
    {
        if (slotIndex < 0 || slotIndex >= selectionBadges.Length)
        {
            return;
        }

        GameObject badge = selectionBadges[slotIndex];
        if (badge != null)
        {
            badge.SetActive(enabled);
        }

        if (slotIndex < selectionSwatches.Length && selectionSwatches[slotIndex] != null)
        {
            selectionSwatches[slotIndex].color = accent;
        }
    }

    public void SetAvailability(bool available)
    {
        if (takenOverlay != null)
        {
            takenOverlay.SetActive(!available);
        }
        if (!available && takenColorOverlay != null)
        {
            takenColorOverlay.color = new Color(takenColorOverlay.color.r, takenColorOverlay.color.g, takenColorOverlay.color.b, 0.6f);
        }
    }

    public void SetTakenBy(int slotIndex, Color accent)
    {
        SetAvailability(false);
        if (takenColorOverlay != null)
        {
            takenColorOverlay.color = accent;
        }
    }
}

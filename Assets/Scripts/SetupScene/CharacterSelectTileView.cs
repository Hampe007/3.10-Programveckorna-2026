using System;
using TMPro;
using UnityEngine;

namespace LocalGame.SetupScene
{
    /// <summary>
    /// View component for one character tile in the grid.
    /// Pure UI toggles; no input logic here.
    /// </summary>
    public sealed class CharacterSelectTileView : MonoBehaviour
    {
        private const string LogPrefix = "[CharacterSelectTileView]";

        [Header("UI")]
        [SerializeField] private TMP_Text nameText;

        [Header("Markers (enable/disable)")]
        [SerializeField] private GameObject p1Cursor;
        [SerializeField] private GameObject p2Cursor;
        [SerializeField] private GameObject p1Selected;
        [SerializeField] private GameObject p2Selected;
        [SerializeField] private GameObject lockedOverlay;

        public void SetName(string value)
        {
            if (nameText != null)
                nameText.text = value ?? string.Empty;
        }

        public void SetMarkers(
            bool showP1Cursor,
            bool showP2Cursor,
            bool showP1Selected,
            bool showP2Selected,
            bool showLocked)
        {
            try
            {
                if (p1Cursor != null) p1Cursor.SetActive(showP1Cursor);
                if (p2Cursor != null) p2Cursor.SetActive(showP2Cursor);
                if (p1Selected != null) p1Selected.SetActive(showP1Selected);
                if (p2Selected != null) p2Selected.SetActive(showP2Selected);
                if (lockedOverlay != null) lockedOverlay.SetActive(showLocked);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} SetMarkers failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void Reset()
        {
            nameText = GetComponentInChildren<TMP_Text>(true);
        }
    }
}
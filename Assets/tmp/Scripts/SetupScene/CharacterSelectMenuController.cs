using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LocalGame.Roster;
using LocalGame.Session;

namespace LocalGame.SetupScene
{
    public sealed class CharacterSelectMenuController : MonoBehaviour
    {
        private const string LogPrefix = "[CharacterSelectMenu]";

        private enum PickState
        {
            Hovering,
            Selected,
            Locked
        }

        [Serializable]
        private struct TileModel
        {
            public bool isRandom;
            public byte characterId; // ignored if isRandom=true
            public string displayName;
        }

        [Header("Scene refs")]
        [SerializeField] private SetupSceneUIRoot uiRoot;

        [Header("Roster + Grid")]
        [SerializeField] private CharacterRosterDatabase rosterDatabase;
        [SerializeField] private RectTransform gridRoot;
        [SerializeField] private CharacterSelectTileView tilePrefab;
        [SerializeField, Min(1)] private int gridColumns = 4;

        [Header("Rules")]
        [SerializeField] private bool allowMirrorMatches = false;

        [Tooltip("How long Confirm must be held AFTER arming to lock.")]
        [SerializeField, Min(0.25f)] private float holdToLockSeconds = 3f;

        [Tooltip("Delay after confirming a selection before hold-to-lock starts filling.")]
        [SerializeField, Min(0f)] private float lockArmDelaySeconds = 1f;

        [Header("Scene Names")]
        [SerializeField] private string gameSceneName = "GameScene";

        [Header("P1 Info UI")]
        [SerializeField] private TMP_Text p1HoverText;
        [SerializeField] private TMP_Text p1SelectedText;
        [SerializeField] private TMP_Text p1StatusText;
        [SerializeField] private Image p1HoldFill;

        [Header("P2 Info UI")]
        [SerializeField] private TMP_Text p2HoverText;
        [SerializeField] private TMP_Text p2SelectedText;
        [SerializeField] private TMP_Text p2StatusText;
        [SerializeField] private Image p2HoldFill;

        [Header("Navigation")]
        [SerializeField] private float stickDeadzone = 0.55f;
        [SerializeField] private float repeatDelaySeconds = 0.18f;

        private GameSession _session;

        private Gamepad _p1Pad;
        private Gamepad _p2Pad;

        private readonly List<TileModel> _tiles = new();
        private readonly List<CharacterSelectTileView> _tileViews = new();

        // Per-player state
        private int _p1HoverIndex;
        private int _p2HoverIndex;

        private PickState _p1State;
        private PickState _p2State;

        private bool _p1HasSelection;
        private bool _p2HasSelection;

        private bool _p1SelectionIsRandom;
        private bool _p2SelectionIsRandom;

        private byte _p1SelectedId;
        private byte _p2SelectedId;

        private byte? _p1LockedId;
        private byte? _p2LockedId;

        // Hold-to-lock progress (only runs AFTER arming delay)
        private float _p1Hold;
        private float _p2Hold;

        // Arming delay timers (prevents fill immediately after tap confirm)
        private float _p1ArmDelayRemaining;
        private float _p2ArmDelayRemaining;

        private float _p1NavCooldown;
        private float _p2NavCooldown;

        // Prevent "held confirm" from previous menu auto-selecting on first frame.
        private bool _p1ConfirmHeldPrev;
        private bool _p2ConfirmHeldPrev;

        private void Awake()
        {
            try
            {
                _session = GameSession.EnsureExists();
                ResolvePads();

                // Seed prev-held so a held button from the previous menu does NOT count as a new hold here.
                _p1ConfirmHeldPrev = _p1Pad != null && _p1Pad.buttonSouth.isPressed;
                _p2ConfirmHeldPrev = _p2Pad != null && _p2Pad.buttonSouth.isPressed;

                BuildTileModels();
                BuildTileViews();

                ResetState();
                RefreshAllUI();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Awake failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void OnEnable()
        {
            try
            {
                _session = GameSession.EnsureExists();
                ResolvePads();

                // Seed prev-held so a held button from the previous menu does NOT count as a new hold here.
                _p1ConfirmHeldPrev = _p1Pad != null && _p1Pad.buttonSouth.isPressed;
                _p2ConfirmHeldPrev = _p2Pad != null && _p2Pad.buttonSouth.isPressed;

                ResetState();
                RefreshAllUI();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} OnEnable failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void Update()
        {
            try
            {
                if (_p1Pad == null || _p2Pad == null)
                    ResolvePads();

                float dt = Time.deltaTime;
                
                // BACK out of Character Select:
                // Only when BOTH players are hovering (no selection / no lock).
                bool p1Back = _p1Pad != null && _p1Pad.buttonEast.wasPressedThisFrame;
                bool p2Back = _p2Pad != null && _p2Pad.buttonEast.wasPressedThisFrame;

                if ((p1Back || p2Back) && CanBackOutToControlsView())
                {
                    BackToControlsView();
                    return;
                }
                
                UpdatePlayer(dt,
                    pad: _p1Pad,
                    playerLabel: "P1",
                    ref _p1HoverIndex,
                    ref _p1State,
                    ref _p1HasSelection,
                    ref _p1SelectionIsRandom,
                    ref _p1SelectedId,
                    ref _p1LockedId,
                    ref _p1Hold,
                    ref _p1ArmDelayRemaining,
                    ref _p1NavCooldown,
                    ref _p1ConfirmHeldPrev);

                UpdatePlayer(dt,
                    pad: _p2Pad,
                    playerLabel: "P2",
                    ref _p2HoverIndex,
                    ref _p2State,
                    ref _p2HasSelection,
                    ref _p2SelectionIsRandom,
                    ref _p2SelectedId,
                    ref _p2LockedId,
                    ref _p2Hold,
                    ref _p2ArmDelayRemaining,
                    ref _p2NavCooldown,
                    ref _p2ConfirmHeldPrev);

                RefreshAllUI();

                if (_p1State == PickState.Locked && _p2State == PickState.Locked)
                {
                    if (_p1LockedId.HasValue && _p2LockedId.HasValue)
                    {
                        _session.SetP1CharacterId(_p1LockedId.Value);
                        _session.SetP2CharacterId(_p2LockedId.Value);

                        if (string.IsNullOrWhiteSpace(gameSceneName))
                        {
                            uiRoot?.ShowToast("Game scene name not set");
                            return;
                        }

                        SceneManager.LoadScene(gameSceneName);
                    }
                    else
                    {
                        Debug.LogError($"{LogPrefix} Both locked but missing locked IDs.", this);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Update failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private void UpdatePlayer(
            float dt,
            Gamepad pad,
            string playerLabel,
            ref int hoverIndex,
            ref PickState state,
            ref bool hasSelection,
            ref bool selectionIsRandom,
            ref byte selectedId,
            ref byte? lockedId,
            ref float holdTimer,
            ref float armDelayRemaining,
            ref float navCooldown,
            ref bool confirmHeldPrev)
        {
            if (pad == null)
                return;

            if (state == PickState.Locked)
            {
                // Locked is final.
                holdTimer = 0f;
                armDelayRemaining = 0f;
                confirmHeldPrev = pad.buttonSouth.isPressed; // keep in sync even while locked
                return;
            }

            // --- Inputs ---
            bool confirmPressed = pad.buttonSouth.wasPressedThisFrame; // A / Cross
            bool confirmHeld = pad.buttonSouth.isPressed;
            bool cancelPressed = pad.buttonEast.wasPressedThisFrame;   // B / Circle

            // This is the key: only treat "hold" as meaningful if it just became held THIS menu.
            bool confirmJustBecameHeld = confirmHeld && !confirmHeldPrev;

            // Cancel clears selection ONLY if selected (not locked)
            if (cancelPressed)
            {
                if (state == PickState.Selected)
                {
                    hasSelection = false;
                    selectionIsRandom = false;
                    selectedId = 0;

                    holdTimer = 0f;
                    armDelayRemaining = 0f;

                    state = PickState.Hovering;
                    uiRoot?.ShowToast($"{playerLabel} selection cleared");
                }

                confirmHeldPrev = confirmHeld;
                return;
            }

            // --- Navigation ---
            // Movement only allowed while Hovering.
            if (state == PickState.Hovering)
            {
                navCooldown -= dt;
                var move = ReadNavIntent(pad, navCooldown <= 0f);
                if (move != Vector2Int.zero)
                {
                    hoverIndex = MoveIndex(hoverIndex, move, _tiles.Count, gridColumns);
                    navCooldown = repeatDelaySeconds;
                }
            }
            else
            {
                // Selected freezes cursor, so no nav updates.
                navCooldown = 0f;
            }

            // --- Confirm tap sets selection and freezes cursor ---
            if (confirmPressed)
            {
                if (state == PickState.Hovering)
                {
                    ApplySelectionFromHover(playerLabel, hoverIndex, ref hasSelection, ref selectionIsRandom, ref selectedId);
                    state = PickState.Selected;

                    // Freeze cursor on the selected tile explicitly (so it can't drift).
                    hoverIndex = FindTileIndexForSelection(selectionIsRandom, selectedId, hoverIndex);

                    // Arm delay: prevents lock fill from starting immediately.
                    holdTimer = 0f;
                    armDelayRemaining = lockArmDelaySeconds;
                }
                else if (state == PickState.Selected)
                {
                    // Already selected: do NOT change selection (since cursor is frozen).
                    // Just re-arm the lock (useful if player tapped confirm again).
                    holdTimer = 0f;
                    armDelayRemaining = lockArmDelaySeconds;
                }
            }

            // Holding confirm while hovering with no selection: auto-select hovered, then freeze.
            // IMPORTANT: requires a fresh hold (release+press) so previous menu's held confirm doesn't auto-select.
            if (confirmJustBecameHeld && state == PickState.Hovering && !hasSelection)
            {
                ApplySelectionFromHover(playerLabel, hoverIndex, ref hasSelection, ref selectionIsRandom, ref selectedId);
                state = PickState.Selected;

                hoverIndex = FindTileIndexForSelection(selectionIsRandom, selectedId, hoverIndex);

                holdTimer = 0f;
                armDelayRemaining = lockArmDelaySeconds;
            }

            // Hold-to-lock only applies in Selected state.
            if (state == PickState.Selected && hasSelection)
            {
                // Tick down arming delay first
                if (armDelayRemaining > 0f)
                {
                    armDelayRemaining -= dt;
                    if (armDelayRemaining < 0f) armDelayRemaining = 0f;

                    // While arming, circle should NOT fill.
                    holdTimer = 0f;

                    confirmHeldPrev = confirmHeld;
                    return;
                }

                // Armed; now fill only while holding confirm.
                if (confirmHeld)
                {
                    holdTimer += dt;

                    if (holdTimer >= holdToLockSeconds)
                    {
                        if (TryLock(playerLabel, ref selectionIsRandom, ref selectedId, ref lockedId))
                        {
                            state = PickState.Locked;
                            holdTimer = holdToLockSeconds;
                            uiRoot?.ShowToast($"{playerLabel} LOCKED");
                        }
                        else
                        {
                            // Blocked lock -> make them hold again from zero.
                            holdTimer = 0f;
                        }
                    }
                }
                else
                {
                    holdTimer = 0f;
                }
            }
            else
            {
                // Hovering, no selection
                holdTimer = 0f;
                armDelayRemaining = 0f;
                state = PickState.Hovering;
            }

            // Update the "prev held" state at the end of the frame.
            confirmHeldPrev = confirmHeld;
        }

        private bool TryLock(string playerLabel, ref bool selectionIsRandom, ref byte selectedId, ref byte? lockedId)
        {
            try
            {
                byte targetId;

                if (selectionIsRandom)
                {
                    byte? otherLocked = GetOtherLockedId(playerLabel);
                    if (!TryResolveRandom(otherLocked, out targetId))
                    {
                        uiRoot?.ShowToast("Random failed (no valid options)");
                        return false;
                    }
                }
                else
                {
                    targetId = selectedId;
                }

                if (!allowMirrorMatches)
                {
                    byte? otherLocked = GetOtherLockedId(playerLabel);
                    if (otherLocked.HasValue && otherLocked.Value == targetId)
                    {
                        uiRoot?.ShowToast("Character already taken");
                        return false;
                    }
                }

                lockedId = targetId;

                // If random, convert selection to the resolved ID for clarity.
                selectionIsRandom = false;
                selectedId = targetId;

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} TryLock failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }

        private byte? GetOtherLockedId(string playerLabel) => playerLabel == "P1" ? _p2LockedId : _p1LockedId;

        private void ApplySelectionFromHover(string playerLabel, int hoverIndex, ref bool hasSelection, ref bool selectionIsRandom, ref byte selectedId)
        {
            if (hoverIndex < 0 || hoverIndex >= _tiles.Count)
                return;

            var tile = _tiles[hoverIndex];

            hasSelection = true;
            selectionIsRandom = tile.isRandom;
            selectedId = tile.isRandom ? (byte)0 : tile.characterId;

            uiRoot?.ShowToast(tile.isRandom ? $"{playerLabel} selected Random" : $"{playerLabel} selected {tile.displayName}");
        }

        private void ResetState()
        {
            _p1HoverIndex = 0;
            _p2HoverIndex = Mathf.Min(1, _tiles.Count - 1);

            _p1State = PickState.Hovering;
            _p2State = PickState.Hovering;

            _p1HasSelection = false;
            _p2HasSelection = false;

            _p1SelectionIsRandom = false;
            _p2SelectionIsRandom = false;

            _p1SelectedId = 0;
            _p2SelectedId = 0;

            _p1LockedId = null;
            _p2LockedId = null;

            _p1Hold = 0f;
            _p2Hold = 0f;

            _p1ArmDelayRemaining = 0f;
            _p2ArmDelayRemaining = 0f;

            _p1NavCooldown = 0f;
            _p2NavCooldown = 0f;
        }

        private void ResolvePads()
        {
            try
            {
                _session ??= GameSession.EnsureExists();

                _p1Pad = _session.ResolveDevice(_session.P1Device) as Gamepad;
                _p2Pad = _session.ResolveDevice(_session.P2Device) as Gamepad;

                if (_p1Pad == null) Debug.LogWarning($"{LogPrefix} P1 gamepad missing/unresolved.", this);
                if (_p2Pad == null) Debug.LogWarning($"{LogPrefix} P2 gamepad missing/unresolved.", this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} ResolvePads failed: {ex.GetType().Name}: {ex.Message}", this);
            }
        }

        private void BuildTileModels()
        {
            if (rosterDatabase == null)
                throw new InvalidOperationException($"{LogPrefix} rosterDatabase is not assigned.");

            _tiles.Clear();

            var sorted = new List<CharacterRosterDatabase.Entry>(rosterDatabase.Entries);
            sorted.Sort((a, b) => a.characterId.CompareTo(b.characterId));

            foreach (var e in sorted)
            {
                if (e == null) continue;

                _tiles.Add(new TileModel
                {
                    isRandom = false,
                    characterId = e.characterId,
                    displayName = string.IsNullOrWhiteSpace(e.displayName) ? $"Character {e.characterId}" : e.displayName
                });
            }

            _tiles.Add(new TileModel
            {
                isRandom = true,
                characterId = 0,
                displayName = "Random"
            });
        }

        private void BuildTileViews()
        {
            if (gridRoot == null)
                throw new InvalidOperationException($"{LogPrefix} gridRoot is not assigned.");
            if (tilePrefab == null)
                throw new InvalidOperationException($"{LogPrefix} tilePrefab is not assigned.");

            for (int i = gridRoot.childCount - 1; i >= 0; i--)
                Destroy(gridRoot.GetChild(i).gameObject);

            _tileViews.Clear();

            for (int i = 0; i < _tiles.Count; i++)
            {
                var view = Instantiate(tilePrefab, gridRoot);
                view.name = $"Tile_{i:00}_{_tiles[i].displayName}";
                view.SetName(_tiles[i].displayName);
                _tileViews.Add(view);
            }
        }

        private void RefreshAllUI()
        {
            RefreshTileMarkers();
            RefreshInfoPanels();
        }

        private void RefreshTileMarkers()
        {
            for (int i = 0; i < _tileViews.Count; i++)
            {
                bool p1Cursor = (_p1State != PickState.Locked) && (i == _p1HoverIndex);
                bool p2Cursor = (_p2State != PickState.Locked) && (i == _p2HoverIndex);

                bool p1Selected = (_p1HasSelection && _p1State != PickState.Locked) && IsTileSelectedByPlayer(1, i);
                bool p2Selected = (_p2HasSelection && _p2State != PickState.Locked) && IsTileSelectedByPlayer(2, i);

                bool locked =
                    (_p1State == PickState.Locked && IsLockedTileByPlayer(1, i)) ||
                    (_p2State == PickState.Locked && IsLockedTileByPlayer(2, i));

                _tileViews[i].SetMarkers(p1Cursor, p2Cursor, p1Selected, p2Selected, locked);
            }
        }

        private int FindTileIndexForSelection(bool selectionIsRandom, byte selectedId, int fallbackIndex)
        {
            try
            {
                for (int i = 0; i < _tiles.Count; i++)
                {
                    var t = _tiles[i];
                    if (selectionIsRandom)
                    {
                        if (t.isRandom)
                            return i;
                    }
                    else
                    {
                        if (!t.isRandom && t.characterId == selectedId)
                            return i;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} FindTileIndexForSelection failed: {ex.GetType().Name}: {ex.Message}", this);
            }

            return Mathf.Clamp(fallbackIndex, 0, Mathf.Max(0, _tiles.Count - 1));
        }

        private bool IsTileSelectedByPlayer(int player, int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= _tiles.Count) return false;
            var tile = _tiles[tileIndex];

            if (player == 1)
            {
                if (!_p1HasSelection) return false;
                if (_p1SelectionIsRandom) return tile.isRandom;
                return !tile.isRandom && tile.characterId == _p1SelectedId;
            }

            if (!_p2HasSelection) return false;
            if (_p2SelectionIsRandom) return tile.isRandom;
            return !tile.isRandom && tile.characterId == _p2SelectedId;
        }

        private bool IsLockedTileByPlayer(int player, int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= _tiles.Count) return false;
            var tile = _tiles[tileIndex];
            if (tile.isRandom) return false;

            if (player == 1)
                return _p1LockedId.HasValue && tile.characterId == _p1LockedId.Value;
            return _p2LockedId.HasValue && tile.characterId == _p2LockedId.Value;
        }

        private void RefreshInfoPanels()
        {
            if (p1HoverText != null) p1HoverText.text = $"Hover: {SafeTileName(_p1HoverIndex)}";
            if (p2HoverText != null) p2HoverText.text = $"Hover: {SafeTileName(_p2HoverIndex)}";

            if (p1SelectedText != null) p1SelectedText.text = _p1HasSelection ? $"Selected: {PlayerSelectionName(1)}" : "Selected: (none)";
            if (p2SelectedText != null) p2SelectedText.text = _p2HasSelection ? $"Selected: {PlayerSelectionName(2)}" : "Selected: (none)";

            if (p1StatusText != null) p1StatusText.text = $"Status: {_p1State}";
            if (p2StatusText != null) p2StatusText.text = $"Status: {_p2State}";

            if (p1HoldFill != null)
                p1HoldFill.fillAmount = (_p1ArmDelayRemaining > 0f) ? 0f : Mathf.Clamp01(_p1Hold / holdToLockSeconds);

            if (p2HoldFill != null)
                p2HoldFill.fillAmount = (_p2ArmDelayRemaining > 0f) ? 0f : Mathf.Clamp01(_p2Hold / holdToLockSeconds);
        }

        private string SafeTileName(int index)
        {
            if (index < 0 || index >= _tiles.Count) return "(invalid)";
            return _tiles[index].displayName;
        }

        private string PlayerSelectionName(int player)
        {
            if (player == 1)
            {
                if (_p1SelectionIsRandom) return "Random";
                return FindNameById(_p1SelectedId);
            }

            if (_p2SelectionIsRandom) return "Random";
            return FindNameById(_p2SelectedId);
        }

        private string FindNameById(byte id)
        {
            for (int i = 0; i < _tiles.Count; i++)
            {
                if (!_tiles[i].isRandom && _tiles[i].characterId == id)
                    return _tiles[i].displayName;
            }
            return $"Character {id}";
        }

        private bool TryResolveRandom(byte? excludeLockedId, out byte resolvedId)
        {
            var candidates = new List<byte>(16);
            for (int i = 0; i < _tiles.Count; i++)
            {
                var t = _tiles[i];
                if (t.isRandom) continue;

                if (!allowMirrorMatches && excludeLockedId.HasValue && t.characterId == excludeLockedId.Value)
                    continue;

                candidates.Add(t.characterId);
            }

            if (candidates.Count == 0)
            {
                resolvedId = 0;
                return false;
            }

            resolvedId = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            return true;
        }

        private Vector2Int ReadNavIntent(Gamepad pad, bool allowMoveThisFrame)
        {
            if (!allowMoveThisFrame)
                return Vector2Int.zero;

            if (pad.dpad.up.wasPressedThisFrame) return Vector2Int.up;
            if (pad.dpad.down.wasPressedThisFrame) return Vector2Int.down;
            if (pad.dpad.left.wasPressedThisFrame) return Vector2Int.left;
            if (pad.dpad.right.wasPressedThisFrame) return Vector2Int.right;

            Vector2 v = pad.leftStick.ReadValue();
            if (v.y >= stickDeadzone) return Vector2Int.up;
            if (v.y <= -stickDeadzone) return Vector2Int.down;
            if (v.x <= -stickDeadzone) return Vector2Int.left;
            if (v.x >= stickDeadzone) return Vector2Int.right;

            return Vector2Int.zero;
        }

        private static int MoveIndex(int current, Vector2Int dir, int count, int columns)
        {
            if (count <= 0) return 0;
            columns = Mathf.Max(1, columns);

            int rows = Mathf.CeilToInt(count / (float)columns);
            int row = current / columns;
            int col = current % columns;

            row -= dir.y;
            col += dir.x;

            row = Mathf.Clamp(row, 0, rows - 1);
            col = Mathf.Clamp(col, 0, columns - 1);

            int next = row * columns + col;
            return Mathf.Clamp(next, 0, count - 1);
        }
        
        private bool CanBackOutToControlsView()
        {
            // Only allow backing out when BOTH players are just hovering.
            // This preserves: B/Circle = cancel selection while Selected.
            return _p1State == PickState.Hovering &&
                _p2State == PickState.Hovering &&
                !_p1HasSelection &&
                !_p2HasSelection;
        }

        private void BackToControlsView()
        {
            try
            {
                // Clear selections + locks as requested.
                ResetState();
                RefreshAllUI();

                uiRoot?.ActivateControlsView();
                gameObject.SetActive(false);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} BackToControlsView failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", this);
            }
        }
    }
}
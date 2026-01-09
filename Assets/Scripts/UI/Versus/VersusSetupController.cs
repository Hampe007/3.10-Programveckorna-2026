using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class VersusSetupController : MonoBehaviour
{
    enum SetupPhase
    {
        InputClaim,
        CharacterSelect,
        MatchIntro,
        Match
    }

    [Serializable]
    class KeyboardLayoutBinding
    {
        public string layoutId = "Keyboard";
        public string displayLabel = "Keyboard";
        public string layoutDescription = "WASD";
        public Key joinKey = Key.W;
        public Key confirmKey = Key.Space;
        public Key cancelKey = Key.LeftShift;
        public Key upKey = Key.W;
        public Key downKey = Key.S;
        public Key leftKey = Key.A;
        public Key rightKey = Key.D;

        public string GetJoinPrompt() => $"{displayLabel} ({layoutDescription})";
    }

    class PlayerLobbyInput : IDisposable
    {
        readonly InputAction navigateAction;
        readonly InputAction confirmAction;
        readonly InputAction cancelAction;
        readonly LobbyInputType inputSourceType;
        readonly Gamepad boundGamepad;

        public event Action<Vector2> OnNavigate;
        public event Action OnConfirm;
        public event Action<bool> OnConfirmHoldChanged;
        public event Action OnCancel;

        public PlayerLobbyInput(LobbyInputType sourceType, KeyboardLayoutBinding keyboardLayout, Gamepad gamepad)
        {
            navigateAction = new InputAction("Navigate", InputActionType.Value);
            confirmAction = new InputAction("Confirm", InputActionType.Button);
            cancelAction = new InputAction("Cancel", InputActionType.Button);
            inputSourceType = sourceType;
            boundGamepad = gamepad;

            if (sourceType == LobbyInputType.Gamepad)
            {
                if (gamepad == null)
                {
                    Debug.LogError("Gamepad source missing device.");
                }
                navigateAction.AddBinding("<Gamepad>/dpad");
                navigateAction.AddBinding("<Gamepad>/leftStick");
                confirmAction.AddBinding("<Gamepad>/buttonSouth");
                cancelAction.AddBinding("<Gamepad>/buttonEast");
            }
            else
            {
                if (keyboardLayout == null)
                {
                    Debug.LogError("Keyboard layout missing.");
                }
                var composite = navigateAction.AddCompositeBinding("2DVector");
                composite.With("Up", FormatKeyPath(keyboardLayout.upKey));
                composite.With("Down", FormatKeyPath(keyboardLayout.downKey));
                composite.With("Left", FormatKeyPath(keyboardLayout.leftKey));
                composite.With("Right", FormatKeyPath(keyboardLayout.rightKey));
                confirmAction.AddBinding(FormatKeyPath(keyboardLayout.confirmKey));
                cancelAction.AddBinding(FormatKeyPath(keyboardLayout.cancelKey));
            }

            navigateAction.performed += ctx =>
            {
                if (!IsExpectedDevice(ctx))
                {
                    return;
                }
                OnNavigate?.Invoke(ctx.ReadValue<Vector2>());
            };
            confirmAction.performed += ctx =>
            {
                if (!IsExpectedDevice(ctx))
                {
                    return;
                }
                OnConfirm?.Invoke();
            };
            confirmAction.started += ctx =>
            {
                if (!IsExpectedDevice(ctx))
                {
                    return;
                }
                OnConfirmHoldChanged?.Invoke(true);
            };
            confirmAction.canceled += ctx =>
            {
                if (!IsExpectedDevice(ctx))
                {
                    return;
                }
                OnConfirmHoldChanged?.Invoke(false);
            };
            cancelAction.performed += ctx =>
            {
                if (!IsExpectedDevice(ctx))
                {
                    return;
                }
                OnCancel?.Invoke();
            };

            navigateAction.Enable();
            confirmAction.Enable();
            cancelAction.Enable();
        }

        bool IsExpectedDevice(InputAction.CallbackContext ctx)
        {
            if (inputSourceType != LobbyInputType.Gamepad)
            {
                return true;
            }

            if (boundGamepad == null)
            {
                return true;
            }

            return ctx.control != null && ctx.control.device == boundGamepad;
        }

        static string FormatKeyPath(Key key)
        {
            string keyName = key.ToString();
            if (string.IsNullOrEmpty(keyName))
            {
                return string.Empty;
            }
            return $"<Keyboard>/{char.ToLowerInvariant(keyName[0])}{keyName.Substring(1)}";
        }

        public void Dispose()
        {
            navigateAction.Dispose();
            confirmAction.Dispose();
            cancelAction.Dispose();
        }
    }

    class PlayerSlotState
    {
        public int slotIndex;
        public PlayerSlotUI view;
        public LobbyInputType inputType;
        public Gamepad gamepad;
        public KeyboardLayoutBinding keyboardLayout;
        public PlayerLobbyInput lobbyInput;
        public int hoverIndex;
        public int softSelectionIndex = -1;
        public int lockedIndex = -1;
        public bool lockHoldActive;
        public float lockHoldElapsed;
        public bool replacementActive;
        public float replacementElapsed;
        public JoinSource pendingReplacementSource;

        public bool Claimed => inputType != LobbyInputType.None;
        public bool IsReady => lockedIndex >= 0;
    }

    struct JoinSource
    {
        public LobbyInputType type;
        public Gamepad gamepad;

        public bool Equals(JoinSource other)
        {
            return type == other.type && gamepad == other.gamepad;
        }
    }

    [Serializable]
    class JoinInstruction
    {
        [TextArea]
        public string message;
    }

    class JoinBinding
    {
        public InputAction action;
        public int targetSlotIndex;
        public LobbyInputType sourceType;
        public bool allowFallbackSlot;
    }

    [Header("Slots")]
    [SerializeField] PlayerSlotUI[] slotViews = new PlayerSlotUI[2];
    [SerializeField] Color[] slotColors = new Color[2] { Color.cyan, Color.magenta };
    [SerializeField] JoinInstruction[] joinPrompts = new JoinInstruction[2];

    [Header("Layout")]
    [SerializeField] GameObject inputClaimRoot;
    [SerializeField] GameObject characterSelectRoot;
    [SerializeField] TMP_Text centerPrompt;
    [SerializeField] TMP_Text continuePrompt;

    [Header("Roster")]
    [SerializeField] Transform rosterParent;
    [SerializeField] CharacterCardView characterCardPrefab;
    [SerializeField] CharacterDefinition[] roster;
    [SerializeField] int rosterColumns = 4;

    [Header("Input Bindings")]
    [SerializeField] KeyboardLayoutBinding leftKeyboard = new KeyboardLayoutBinding
    {
        layoutId = "Left Keyboard",
        displayLabel = "Keyboard",
        layoutDescription = "WASD",
        joinKey = Key.W,
        confirmKey = Key.Space,
        cancelKey = Key.LeftShift,
        upKey = Key.W,
        downKey = Key.S,
        leftKey = Key.A,
        rightKey = Key.D
    };

    [SerializeField] KeyboardLayoutBinding rightKeyboard = new KeyboardLayoutBinding
    {
        layoutId = "Right Keyboard",
        displayLabel = "Keyboard",
        layoutDescription = "Arrow Keys",
        joinKey = Key.UpArrow,
        confirmKey = Key.Enter,
        cancelKey = Key.RightShift,
        upKey = Key.UpArrow,
        downKey = Key.DownArrow,
        leftKey = Key.LeftArrow,
        rightKey = Key.RightArrow
    };
    [SerializeField] float lockHoldSeconds = 3f;
    [SerializeField] float replaceHoldSeconds = 1.5f;

    [Header("Scenes")]
    [SerializeField] string matchSceneName = "The Battleground";

    readonly List<CharacterCardView> cardViews = new();
    readonly PlayerSlotState[] slots = new PlayerSlotState[2];
    readonly List<JoinBinding> joinBindings = new();
    SetupPhase phase = SetupPhase.InputClaim;
    bool isMatchStarting;

    void Awake()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new PlayerSlotState
            {
                slotIndex = i,
                view = slotViews != null && slotViews.Length > i ? slotViews[i] : null,
                inputType = LobbyInputType.None
            };
        }

        BuildJoinActions();
    }

    void Start()
    {
        SetPhase(SetupPhase.InputClaim);
        BuildRoster();
        ApplyJoinPrompts();
    }

    void OnEnable()
    {
        foreach (JoinBinding binding in joinBindings)
        {
            binding.action.Enable();
        }
    }

    void OnDisable()
    {
        foreach (JoinBinding binding in joinBindings)
        {
            binding.action.Disable();
        }
    }

    void Update()
    {
        UpdateReplacementTimers();
        UpdateLockTimers();
        if (phase == SetupPhase.CharacterSelect)
        {
            if (AllPlayersReady() && !isMatchStarting)
            {
                BeginMatchStart();
            }
        }
    }

    void BuildRoster()
    {
        cardViews.Clear();
        if (rosterParent == null || characterCardPrefab == null)
        {
            return;
        }

        foreach (Transform child in rosterParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < roster.Length; i++)
        {
            CharacterDefinition definition = roster[i];
            CharacterCardView instance = Instantiate(characterCardPrefab, rosterParent);
            instance.Initialize(definition);
            cardViews.Add(instance);
        }
    }

    void ApplyJoinPrompts()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].view?.SetTitle($"Player {i + 1}");
            string fallback = i == 0
                ? "Press W or D-Pad Left / Start to join."
                : "Press Up Arrow or D-Pad Right / Start to join.";
            string msg = joinPrompts != null && joinPrompts.Length > i && joinPrompts[i] != null
                ? joinPrompts[i].message
                : fallback;
            slots[i].view?.ShowIdlePrompt(msg);
        }
        UpdateContinuePrompt(false);
    }

    void BuildJoinActions()
    {
        joinBindings.Clear();
        RegisterJoinAction("JoinLeftKeyboard", FormatKeyPath(leftKeyboard.joinKey), 0, LobbyInputType.LeftKeyboard, false);
        RegisterJoinAction("JoinRightKeyboard", FormatKeyPath(rightKeyboard.joinKey), 1, LobbyInputType.RightKeyboard, false);
        RegisterJoinAction("JoinControllerLeft", "<Gamepad>/dpad/left", 0, LobbyInputType.Gamepad, false);
        RegisterJoinAction("JoinControllerRight", "<Gamepad>/dpad/right", 1, LobbyInputType.Gamepad, false);
        RegisterJoinAction("JoinControllerStart", "<Gamepad>/start", -1, LobbyInputType.Gamepad, true);
    }

    void RegisterJoinAction(string name, string binding, int slotIndex, LobbyInputType sourceType, bool allowFallbackSlot)
    {
        InputAction action = new InputAction(name, InputActionType.Button, binding);
        JoinBinding bindingInfo = new JoinBinding
        {
            action = action,
            targetSlotIndex = slotIndex,
            sourceType = sourceType,
            allowFallbackSlot = allowFallbackSlot
        };
        action.started += ctx => OnJoinStarted(bindingInfo, ctx);
        action.performed += ctx => OnJoinPerformed(bindingInfo, ctx);
        action.canceled += ctx => OnJoinCanceled(bindingInfo, ctx);
        joinBindings.Add(bindingInfo);
    }

    void OnJoinStarted(JoinBinding binding, InputAction.CallbackContext ctx)
    {
        if (phase != SetupPhase.InputClaim)
        {
            return;
        }
        JoinSource source = CreateJoinSource(binding.sourceType, ctx.control.device as Gamepad);
        int slotIndex = ResolveTargetSlot(binding, source);
        if (slotIndex < 0)
        {
            return;
        }
        PlayerSlotState slot = slots[slotIndex];
        if (!slot.Claimed)
        {
            return;
        }
        if (slot.inputType == source.type && slot.gamepad == source.gamepad)
        {
            return;
        }
        if (source.type == LobbyInputType.Gamepad && ControllerInUseByOtherSlot(slotIndex, source.gamepad))
        {
            return;
        }
        slot.pendingReplacementSource = source;
        slot.replacementActive = true;
        slot.replacementElapsed = 0f;
        slot.view?.ShowBlockingMessage("Hold join input to replace this slot.");
    }

    void OnJoinCanceled(JoinBinding binding, InputAction.CallbackContext ctx)
    {
        JoinSource source = CreateJoinSource(binding.sourceType, ctx.control.device as Gamepad);
        foreach (PlayerSlotState slot in slots)
        {
            if (slot.replacementActive && slot.pendingReplacementSource.Equals(source))
            {
                slot.replacementActive = false;
                slot.view?.ShowStatus("Connected");
            }
        }
    }

    void OnJoinPerformed(JoinBinding binding, InputAction.CallbackContext ctx)
    {
        if (phase != SetupPhase.InputClaim)
        {
            return;
        }
        JoinSource source = CreateJoinSource(binding.sourceType, ctx.control.device as Gamepad);
        int slotIndex = ResolveTargetSlot(binding, source);
        if (slotIndex < 0)
        {
            centerPrompt?.SetText("Both players already joined.");
            return;
        }
        PlayerSlotState slot = slots[slotIndex];
        if (!slot.Claimed)
        {
            AssignSlot(slotIndex, source);
            TryEnableContinuePrompt();
        }
        else if (!slot.pendingReplacementSource.Equals(source))
        {
            slot.view?.ShowBlockingMessage("Player already joined. Hold join to replace or press Back to unjoin.");
        }
    }

    void AssignSlot(int slotIndex, JoinSource source)
    {
        PlayerSlotState slot = slots[slotIndex];
        ReleaseSlotInput(slot);
        if (source.type == LobbyInputType.Gamepad && ControllerInUseByOtherSlot(slotIndex, source.gamepad))
        {
            slot.view?.ShowBlockingMessage("That controller is already assigned to the other player.");
            return;
        }

        slot.inputType = source.type;
        slot.gamepad = source.gamepad;
        slot.keyboardLayout = source.type == LobbyInputType.LeftKeyboard ? leftKeyboard :
            source.type == LobbyInputType.RightKeyboard ? rightKeyboard : null;
        slot.hoverIndex = cardViews.Count > 0 ? Mathf.Clamp(slotIndex, 0, cardViews.Count - 1) : 0;
        slot.softSelectionIndex = -1;
        slot.lockedIndex = -1;
        slot.lobbyInput = new PlayerLobbyInput(source.type, slot.keyboardLayout, slot.gamepad);
        slot.lobbyInput.OnNavigate += value => OnNavigate(slotIndex, value);
        slot.lobbyInput.OnConfirm += () => OnConfirm(slotIndex);
        slot.lobbyInput.OnConfirmHoldChanged += held => OnConfirmHold(slotIndex, held);
        slot.lobbyInput.OnCancel += () => OnCancel(slotIndex);
        slot.view?.ShowClaimed(GetDeviceLabel(slot), slot.inputType == LobbyInputType.Gamepad, slotColors[slotIndex], "Press Confirm to continue");
        slot.view?.ShowStatus("Joined");
        if (source.type == LobbyInputType.Gamepad && InputManager.instance != null)
        {
            InputManager.instance.AssignDeviceToSlot(source.gamepad, slotIndex);
        }
    }

    void ReleaseSlotInput(PlayerSlotState slot)
    {
        if (slot.inputType == LobbyInputType.Gamepad && slot.gamepad != null && InputManager.instance != null)
        {
            InputManager.instance.ClearDeviceOverride(slot.gamepad);
        }
        slot.lobbyInput?.Dispose();
        slot.lobbyInput = null;
        if (slot.lockedIndex >= 0)
        {
            UpdateCardLock(slot.lockedIndex, false, slot.slotIndex);
        }
        slot.inputType = LobbyInputType.None;
        slot.gamepad = null;
        slot.keyboardLayout = null;
        slot.softSelectionIndex = -1;
        slot.lockedIndex = -1;
        slot.lockHoldActive = false;
        slot.lockHoldElapsed = 0;
        slot.replacementActive = false;
    }

    void TryEnableContinuePrompt()
    {
        bool ready = AllSlotsClaimed();
        UpdateContinuePrompt(ready);
    }

    void UpdateContinuePrompt(bool visible)
    {
        if (continuePrompt != null)
        {
            continuePrompt.gameObject.SetActive(visible);
            continuePrompt.text = visible ? "Both players joined! Press Confirm to continue." : "";
        }
    }

    void OnNavigate(int slotIndex, Vector2 value)
    {
        if (phase != SetupPhase.CharacterSelect)
        {
            return;
        }
        PlayerSlotState slot = slots[slotIndex];
        if (slot.IsReady)
        {
            return;
        }
        Vector2Int dir = Vector2Int.zero;
        if (Mathf.Abs(value.x) > Mathf.Abs(value.y))
        {
            if (value.x > 0.5f) dir = Vector2Int.right;
            else if (value.x < -0.5f) dir = Vector2Int.left;
        }
        else
        {
            if (value.y > 0.5f) dir = Vector2Int.up;
            else if (value.y < -0.5f) dir = Vector2Int.down;
        }
        if (dir == Vector2Int.zero)
        {
            return;
        }
        int nextIndex = CalculateNextIndex(slot.hoverIndex, dir);
        if (nextIndex == slot.hoverIndex)
        {
            return;
        }
        slot.hoverIndex = nextIndex;
        UpdateHoverVisuals(slotIndex);
    }

    int CalculateNextIndex(int currentIndex, Vector2Int dir)
    {
        if (cardViews.Count == 0)
        {
            return 0;
        }
        int rows = Mathf.CeilToInt((float)cardViews.Count / Mathf.Max(1, rosterColumns));
        int row = Mathf.Clamp(currentIndex / Mathf.Max(1, rosterColumns), 0, rows - 1);
        int column = currentIndex % Mathf.Max(1, rosterColumns);

        row = Mathf.Clamp(row - dir.y, 0, rows - 1);
        column = Mathf.Clamp(column + dir.x, 0, Mathf.Max(1, rosterColumns) - 1);
        int index = Mathf.Clamp(row * Mathf.Max(1, rosterColumns) + column, 0, cardViews.Count - 1);
        return index;
    }

    void UpdateHoverVisuals(int slotIndex)
    {
        if (cardViews.Count == 0)
        {
            return;
        }
        for (int i = 0; i < cardViews.Count; i++)
        {
            bool isHover = slots[slotIndex].hoverIndex == i;
            cardViews[i].SetHover(slotIndex, isHover, slotColors[slotIndex]);
        }
        CharacterDefinition def = cardViews[slots[slotIndex].hoverIndex].Definition;
        slots[slotIndex].view?.ShowStatus(def != null ? $"Hovering {def.DisplayName}" : "Hovering");
    }

    void OnConfirm(int slotIndex)
    {
        if (phase == SetupPhase.InputClaim)
        {
            if (AllSlotsClaimed())
            {
                BeginCharacterSelect();
            }
            return;
        }

        if (phase != SetupPhase.CharacterSelect)
        {
            return;
        }

        PlayerSlotState slot = slots[slotIndex];
        if (slot.IsReady)
        {
            return;
        }

        int hoveredIndex = slot.hoverIndex;
        if (IsCardTakenByOther(slotIndex, hoveredIndex))
        {
            slot.view?.ShowBlockingMessage("That fighter is already locked in.");
            return;
        }

        slot.softSelectionIndex = hoveredIndex;
        CharacterCardView card = cardViews[hoveredIndex];
        CharacterDefinition definition = card.Definition;
        if (definition != null)
        {
            slot.view?.ShowStatus($"Selected {definition.DisplayName}");
        }
        card.SetSelection(slotIndex, true, slotColors[slotIndex]);
    }

    void OnConfirmHold(int slotIndex, bool held)
    {
        if (phase != SetupPhase.CharacterSelect)
        {
            return;
        }
        PlayerSlotState slot = slots[slotIndex];
        if (slot.softSelectionIndex < 0)
        {
            return;
        }
        if (IsCardTakenByOther(slotIndex, slot.softSelectionIndex))
        {
            return;
        }
        slot.lockHoldActive = held;
        if (!held)
        {
            slot.lockHoldElapsed = 0;
            slot.view?.ToggleLockProgress(false, 0);
        }
        else
        {
            slot.view?.ShowStatus("Locking in...");
        }
    }

    void OnCancel(int slotIndex)
    {
        PlayerSlotState slot = slots[slotIndex];
        if (phase == SetupPhase.InputClaim)
        {
            ReleaseSlotInput(slot);
            string fallback = slot.slotIndex == 0
                ? "Press W or D-Pad Left / Start to join."
                : "Press Up Arrow or D-Pad Right / Start to join.";
            string prompt = joinPrompts != null && joinPrompts.Length > slot.slotIndex && joinPrompts[slot.slotIndex] != null
                ? joinPrompts[slot.slotIndex].message
                : fallback;
            slot.view?.ShowIdlePrompt(prompt);
            UpdateContinuePrompt(AllSlotsClaimed());
            return;
        }

        if (phase != SetupPhase.CharacterSelect)
        {
            return;
        }

        if (slot.IsReady)
        {
            slot.lockedIndex = -1;
            slot.view?.ClearReady();
            slot.view?.ShowStatus("Selection reopened");
            slot.lockHoldActive = false;
            slot.lockHoldElapsed = 0;
            UpdateCardLock(slot.softSelectionIndex, false, slotIndex);
            return;
        }

        if (slot.softSelectionIndex >= 0)
        {
            CharacterCardView card = cardViews[slot.softSelectionIndex];
            card.SetSelection(slotIndex, false, slotColors[slotIndex]);
            slot.softSelectionIndex = -1;
            slot.view?.ShowStatus("Selection cleared");
        }
    }

    void BeginCharacterSelect()
    {
        SetPhase(SetupPhase.CharacterSelect);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].view?.ShowStatus("Select your character");
            UpdateHoverVisuals(i);
        }
        UpdateContinuePrompt(false);
    }

    void SetPhase(SetupPhase newPhase)
    {
        phase = newPhase;
        if (inputClaimRoot != null)
        {
            inputClaimRoot.SetActive(newPhase == SetupPhase.InputClaim);
        }
        if (characterSelectRoot != null)
        {
            characterSelectRoot.SetActive(newPhase == SetupPhase.CharacterSelect);
        }
        if (centerPrompt != null)
        {
            centerPrompt.text = newPhase == SetupPhase.InputClaim ? "Press to Join" :
                newPhase == SetupPhase.CharacterSelect ? "Select Fighters" : "";
        }
    }

    bool AllSlotsClaimed()
    {
        foreach (PlayerSlotState slot in slots)
        {
            if (!slot.Claimed)
            {
                return false;
            }
        }
        return true;
    }

    bool AllPlayersReady()
    {
        foreach (PlayerSlotState slot in slots)
        {
            if (!slot.IsReady)
            {
                return false;
            }
        }
        return true;
    }

    void UpdateReplacementTimers()
    {
        if (phase != SetupPhase.InputClaim)
        {
            return;
        }
        foreach (PlayerSlotState slot in slots)
        {
            if (!slot.replacementActive)
            {
                continue;
            }
            slot.replacementElapsed += Time.unscaledDeltaTime;
            if (slot.replacementElapsed >= replaceHoldSeconds)
            {
                slot.replacementActive = false;
                AssignSlot(slot.slotIndex, slot.pendingReplacementSource);
                TryEnableContinuePrompt();
            }
        }
    }

    void UpdateLockTimers()
    {
        if (phase != SetupPhase.CharacterSelect)
        {
            return;
        }
        foreach (PlayerSlotState slot in slots)
        {
            if (!slot.lockHoldActive)
            {
                continue;
            }
            slot.lockHoldElapsed += Time.unscaledDeltaTime;
            slot.view?.ToggleLockProgress(true, slot.lockHoldElapsed / lockHoldSeconds);
            if (slot.lockHoldElapsed >= lockHoldSeconds)
            {
                slot.lockHoldElapsed = 0;
                slot.lockHoldActive = false;
                slot.lockedIndex = slot.softSelectionIndex;
                slot.view?.ShowReady(slotColors[slot.slotIndex]);
                UpdateCardLock(slot.lockedIndex, true, slot.slotIndex);
            }
        }
    }

    void UpdateCardLock(int cardIndex, bool locked, int ownerSlot)
    {
        if (cardIndex < 0 || cardIndex >= cardViews.Count)
        {
            return;
        }
        CharacterCardView card = cardViews[cardIndex];
        if (locked)
        {
            card.SetTakenBy(ownerSlot, slotColors[ownerSlot]);
            for (int i = 0; i < slots.Length; i++)
            {
                if (i == ownerSlot)
                {
                    continue;
                }
                if (slots[i].softSelectionIndex == cardIndex)
                {
                    slots[i].softSelectionIndex = -1;
                    card.SetSelection(i, false, slotColors[i]);
                    slots[i].view?.ShowStatus("Selection taken - choose another fighter");
                }
            }
        }
        else
        {
            card.SetAvailability(true);
        }
    }

    bool IsCardTakenByOther(int slotIndex, int cardIndex)
    {
        foreach (PlayerSlotState slot in slots)
        {
            if (slot.slotIndex == slotIndex)
            {
                continue;
            }
            if (slot.lockedIndex == cardIndex)
            {
                return true;
            }
        }
        return false;
    }

    void BeginMatchStart()
    {
        isMatchStarting = true;
        if (centerPrompt != null)
        {
            centerPrompt.text = "FIGHT!";
        }
        StartCoroutine(LoadMatchRoutine());
    }

    IEnumerator LoadMatchRoutine()
    {
        yield return new WaitForSecondsRealtime(1.2f);
        for (int i = 0; i < slots.Length; i++)
        {
            CharacterDefinition character = slots[i].lockedIndex >= 0 && slots[i].lockedIndex < cardViews.Count
                ? cardViews[slots[i].lockedIndex].Definition
                : null;
            MatchSetupRuntime.StoreSelection(i, character, slots[i].inputType, slots[i].gamepad);
        }
        if (!string.IsNullOrEmpty(matchSceneName))
        {
            SceneManager.LoadScene(matchSceneName);
        }
    }

    int ResolveTargetSlot(JoinBinding binding, JoinSource source)
    {
        if (binding.targetSlotIndex >= 0)
        {
            return binding.targetSlotIndex;
        }

        if (binding.allowFallbackSlot)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].Claimed)
                {
                    return i;
                }
            }
            return 0;
        }

        return -1;
    }

    JoinSource CreateJoinSource(LobbyInputType type, Gamepad gamepad)
    {
        return new JoinSource
        {
            type = type,
            gamepad = type == LobbyInputType.Gamepad ? gamepad : null
        };
    }

    bool ControllerInUseByOtherSlot(int requestingSlot, Gamepad pad)
    {
        if (pad == null)
        {
            return false;
        }
        foreach (PlayerSlotState slot in slots)
        {
            if (slot.slotIndex == requestingSlot)
            {
                continue;
            }
            if (slot.gamepad == pad)
            {
                return true;
            }
        }
        return false;
    }

    string GetDeviceLabel(PlayerSlotState slot)
    {
        if (slot.inputType == LobbyInputType.Gamepad)
        {
            return slot.gamepad != null ? slot.gamepad.displayName : "Controller";
        }
        if (slot.keyboardLayout != null)
        {
            return slot.keyboardLayout.GetJoinPrompt();
        }
        return "Input";
    }

    static string FormatKeyPath(Key key)
    {
        string keyName = key.ToString();
        if (string.IsNullOrEmpty(keyName))
        {
            return string.Empty;
        }
        return $"<Keyboard>/{char.ToLowerInvariant(keyName[0])}{keyName.Substring(1)}";
    }
}

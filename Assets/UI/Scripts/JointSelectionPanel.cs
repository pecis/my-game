using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Core interaction panel: 17 joint buttons + 4 state buttons + Confirm.
/// Works in both desktop (click) and iOS (scroll list) modes.
/// Talks to TurnManager.fighter0/fighter1 through TurnManager.
/// </summary>
public class JointSelectionPanel : MonoBehaviour
{
    [Header("References")]
    public TurnManager turnManager;

    [Header("UI Containers")]
    public Transform jointButtonContainer;  // parent for joint buttons
    public Button    btnHold, btnContract, btnExtend, btnRelax;
    public Button    btnConfirm;
    public TextMeshProUGUI lblSelectedJoint;
    public TextMeshProUGUI lblPhase;

    [Header("Prefab")]
    public Button jointButtonPrefab;

    [Header("Player")]
    public int playerIndex; // 0 or 1

    private BodyPartType          _selectedPart = BodyPartType.Hips;
    private JointState            _selectedState = JointState.Hold;
    private Dictionary<BodyPartType, Button> _jointButtons = new();

    // Color coding
    private static readonly Color ColorHold     = new(0.3f, 0.5f, 1.0f);
    private static readonly Color ColorContract = new(0.2f, 0.8f, 0.3f);
    private static readonly Color ColorExtend   = new(1.0f, 0.6f, 0.1f);
    private static readonly Color ColorRelax    = new(0.6f, 0.6f, 0.6f);
    private static readonly Color ColorSelected = Color.yellow;

    // ──────────────────────────────────────────────────────────

    void Start()
    {
        BuildJointButtons();

        btnHold.onClick.AddListener(    () => SelectState(JointState.Hold));
        btnContract.onClick.AddListener(() => SelectState(JointState.Contract));
        btnExtend.onClick.AddListener(  () => SelectState(JointState.Extend));
        btnRelax.onClick.AddListener(   () => SelectState(JointState.Relax));
        btnConfirm.onClick.AddListener(OnConfirm);

        turnManager.OnPhaseChanged += OnPhaseChanged;
        SetInteractable(false);

        RefreshUI();
    }

    void BuildJointButtons()
    {
        foreach (BodyPartType t in System.Enum.GetValues(typeof(BodyPartType)))
        {
            var btn = Instantiate(jointButtonPrefab, jointButtonContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = t.ToString().Replace("_", " ");
            var captured = t;
            btn.onClick.AddListener(() => SelectJoint(captured));
            _jointButtons[t] = btn;
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Interaction
    // ──────────────────────────────────────────────────────────

    void SelectJoint(BodyPartType part)
    {
        _selectedPart = part;
        lblSelectedJoint.text = part.ToString().Replace("_", " ");
        RefreshJointColors();
    }

    void SelectState(JointState state)
    {
        _selectedState = state;
        ApplyStateToSelected();
    }

    void ApplyStateToSelected()
    {
        var fighter = playerIndex == 0 ? turnManager.fighter0 : turnManager.fighter1;
        fighter.SetPendingState(_selectedPart, _selectedState);
        RefreshJointColors();
    }

    void OnConfirm()
    {
        turnManager.ConfirmPlayer(playerIndex);
        SetInteractable(false);
    }

    // ──────────────────────────────────────────────────────────
    //  Phase handling
    // ──────────────────────────────────────────────────────────

    void OnPhaseChanged(TurnPhase phase)
    {
        bool isSelectPhase = phase == TurnPhase.PlayerSelectPhase;
        SetInteractable(isSelectPhase);
        lblPhase.text = phase.ToString();
        if (isSelectPhase) RefreshJointColors();
    }

    void SetInteractable(bool value)
    {
        foreach (var kv in _jointButtons) kv.Value.interactable = value;
        btnHold.interactable     = value;
        btnContract.interactable = value;
        btnExtend.interactable   = value;
        btnRelax.interactable    = value;
        btnConfirm.interactable  = value;
    }

    // ──────────────────────────────────────────────────────────
    //  Visual feedback
    // ──────────────────────────────────────────────────────────

    void RefreshJointColors()
    {
        var fighter = playerIndex == 0 ? turnManager.fighter0 : turnManager.fighter1;

        foreach (var kv in _jointButtons)
        {
            var state   = fighter.GetPendingState(kv.Key);
            var color   = StateColor(state);
            if (kv.Key == _selectedPart) color = ColorSelected;

            var colors           = kv.Value.colors;
            colors.normalColor   = color;
            colors.selectedColor = color;
            kv.Value.colors      = colors;
        }
    }

    void RefreshUI()
    {
        lblSelectedJoint.text = _selectedPart.ToString().Replace("_", " ");
    }

    static Color StateColor(JointState s) => s switch
    {
        JointState.Hold     => ColorHold,
        JointState.Contract => ColorContract,
        JointState.Extend   => ColorExtend,
        JointState.Relax    => ColorRelax,
        _                   => Color.white
    };

    void OnDestroy()
    {
        if (turnManager != null) turnManager.OnPhaseChanged -= OnPhaseChanged;
    }
}

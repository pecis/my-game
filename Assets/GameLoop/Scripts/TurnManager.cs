using System;
using UnityEngine;

public enum TurnPhase { Idle, PlayerSelectPhase, SimulatePhase, EvaluatePhase, MatchOver }

/// <summary>
/// Central state machine: PlayerSelectPhase → SimulatePhase → EvaluatePhase → repeat.
/// </summary>
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("References")]
    public RagdollController fighter0;
    public RagdollController fighter1;
    public SimulationRunner  simulationRunner;
    public EvaluationSystem  evaluationSystem;
    public ScoreManager      scoreManager;
    public MatchRules        rules;

    [Header("AI")]
    public AIBrain aiBrain; // null = hotseat mode

    // Events for UI
    public event Action<TurnPhase> OnPhaseChanged;
    public event Action<float>     OnSelectTimerTick;  // remaining seconds
    public event Action<MatchResult> OnMatchOver;

    public TurnPhase CurrentPhase { get; private set; } = TurnPhase.Idle;
    public int CurrentTurn        { get; private set; }

    private float _selectTimer;
    private bool  _p0Confirmed;
    private bool  _p1Confirmed;

    // ──────────────────────────────────────────────────────────
    //  Lifecycle
    // ──────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        evaluationSystem.Initialize(fighter0, fighter1);
        scoreManager.ResetScores();

        fighter0.Initialize();
        fighter1.Initialize();

        evaluationSystem.OnMatchEnd += result =>
        {
            ChangePhase(TurnPhase.MatchOver);
            OnMatchOver?.Invoke(result);
        };

        simulationRunner.OnComplete += OnSimulationComplete;

        StartSelectPhase();
    }

    void Update()
    {
        if (CurrentPhase == TurnPhase.PlayerSelectPhase)
        {
            _selectTimer -= Time.deltaTime;
            OnSelectTimerTick?.Invoke(Mathf.Max(0f, _selectTimer));
            if (_selectTimer <= 0f)
                AutoConfirmRemaining();
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Phase transitions
    // ──────────────────────────────────────────────────────────

    void StartSelectPhase()
    {
        if (evaluationSystem.IsMatchOver) return;

        CurrentTurn++;
        _p0Confirmed = false;
        _p1Confirmed = false;
        _selectTimer = rules != null ? rules.playerSelectTimeLimit : 30f;

        ChangePhase(TurnPhase.PlayerSelectPhase);
        Debug.Log($"[TurnManager] Turn {CurrentTurn} — PlayerSelectPhase");

        // AI confirms immediately
        if (aiBrain != null)
        {
            aiBrain.ChooseMoves(fighter1, fighter0);
            ConfirmPlayer(1);
        }
    }

    void StartSimulatePhase()
    {
        ChangePhase(TurnPhase.SimulatePhase);
        Debug.Log("[TurnManager] SimulatePhase — committing states and running physics.");

        fighter0.CommitAndUnfreeze();
        fighter1.CommitAndUnfreeze();
        simulationRunner.StartSimulation();
    }

    void OnSimulationComplete()
    {
        fighter0.FreezeAll();
        fighter1.FreezeAll();
        ChangePhase(TurnPhase.EvaluatePhase);
        Debug.Log("[TurnManager] EvaluatePhase");

        int turnsLeft = rules != null ? rules.maxTurns - CurrentTurn : 999;
        var result = evaluationSystem.EvaluateEndOfTurn(
            (int)scoreManager.GetScore(0),
            (int)scoreManager.GetScore(1),
            turnsLeft);

        if (result != MatchResult.None) return; // OnMatchEnd already fired

        StartSelectPhase();
    }

    // ──────────────────────────────────────────────────────────
    //  Player confirmation
    // ──────────────────────────────────────────────────────────

    public void ConfirmPlayer(int playerIndex)
    {
        if (CurrentPhase != TurnPhase.PlayerSelectPhase) return;

        if (playerIndex == 0) _p0Confirmed = true;
        else                  _p1Confirmed = true;

        Debug.Log($"[TurnManager] P{playerIndex} confirmed. P0={_p0Confirmed} P1={_p1Confirmed}");

        if (_p0Confirmed && _p1Confirmed)
            StartSimulatePhase();
    }

    void AutoConfirmRemaining()
    {
        if (!_p0Confirmed) ConfirmPlayer(0);
        if (!_p1Confirmed) ConfirmPlayer(1);
    }

    // ──────────────────────────────────────────────────────────

    void ChangePhase(TurnPhase phase)
    {
        CurrentPhase = phase;
        OnPhaseChanged?.Invoke(phase);
    }
}

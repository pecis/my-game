using System;
using UnityEngine;

public enum MatchResult { None, Player0Wins, Player1Wins, Draw }

/// <summary>
/// Singleton that receives collision events from DamageReceiver and
/// checks win conditions after each simulation phase.
/// </summary>
public class EvaluationSystem : MonoBehaviour
{
    public static EvaluationSystem Instance { get; private set; }

    public MatchRules rules;

    // Fired when a fighter is eliminated mid-simulation
    public event Action<MatchResult> OnMatchEnd;

    private RagdollController _fighter0;
    private RagdollController _fighter1;
    private bool _matchOver;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Initialize(RagdollController f0, RagdollController f1)
    {
        _fighter0  = f0;
        _fighter1  = f1;
        _matchOver = false;
    }

    // ──────────────────────────────────────────────────────────
    //  Called by DamageReceiver
    // ──────────────────────────────────────────────────────────

    public void RegisterImpact(BodyPart hit, BodyPart striker, float force)
    {
        // Could trigger visual/audio feedback here
        Debug.Log($"[EvaluationSystem] Impact: {striker.partType}(P{striker.ownerPlayerIndex}) → {hit.partType}(P{hit.ownerPlayerIndex}) force={force:F1}");
    }

    public void RegisterGroundContact(BodyPart part)
    {
        if (_matchOver || rules == null || !rules.instantLossOnTorsoGround) return;

        bool isVital = part.partType == BodyPartType.Chest   ||
                       part.partType == BodyPartType.Abdomen  ||
                       part.partType == BodyPartType.Hips     ||
                       part.partType == BodyPartType.Head;

        if (!isVital) return;

        var result = part.ownerPlayerIndex == 0 ? MatchResult.Player1Wins : MatchResult.Player0Wins;
        EndMatch(result, $"P{part.ownerPlayerIndex}'s {part.partType} touched the ground.");
    }

    // ──────────────────────────────────────────────────────────
    //  Called by TurnManager after simulation
    // ──────────────────────────────────────────────────────────

    public MatchResult EvaluateEndOfTurn(int p0Score, int p1Score, int turnsRemaining)
    {
        if (_matchOver) return MatchResult.None;

        bool p0Down = _fighter0 != null && _fighter0.HasVitalGroundContact();
        bool p1Down = _fighter1 != null && _fighter1.HasVitalGroundContact();

        if (p0Down && p1Down) return EndMatch(MatchResult.Draw, "Both fighters down.");
        if (p0Down)           return EndMatch(MatchResult.Player1Wins, "P0 vital hit ground.");
        if (p1Down)           return EndMatch(MatchResult.Player0Wins, "P1 vital hit ground.");

        if (turnsRemaining <= 0)
        {
            if (p0Score > p1Score) return EndMatch(MatchResult.Player0Wins, "Time — P0 wins by score.");
            if (p1Score > p0Score) return EndMatch(MatchResult.Player1Wins, "Time — P1 wins by score.");
            return EndMatch(MatchResult.Draw, "Time — Draw.");
        }

        return MatchResult.None;
    }

    // ──────────────────────────────────────────────────────────

    private MatchResult EndMatch(MatchResult result, string reason)
    {
        _matchOver = true;
        Debug.Log($"[EvaluationSystem] Match over: {result} — {reason}");
        OnMatchEnd?.Invoke(result);
        return result;
    }

    public bool IsMatchOver => _matchOver;
}

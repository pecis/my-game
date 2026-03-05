using UnityEngine;

/// <summary>
/// Accumulates impact scores. Scoring player X means player X dealt damage
/// to the opponent. ScoreManager is told which body part of the opponent was hit.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public MatchRules rules;

    private float[] _scores = new float[2];

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetScores()
    {
        _scores[0] = 0f;
        _scores[1] = 0f;
    }

    /// <param name="attackerIndex">Player who struck</param>
    /// <param name="hitPart">Body part that was hit (on the defender)</param>
    /// <param name="force">Collision impulse magnitude</param>
    public void AddImpactScore(int attackerIndex, BodyPartType hitPart, float force)
    {
        if (attackerIndex < 0 || attackerIndex > 1) return;
        if (rules == null) return;

        float weight = rules.GetScoreWeight(hitPart);
        float points = weight * Mathf.Clamp(force * 0.01f, 0f, 10f); // normalize force
        _scores[attackerIndex] += points;

        Debug.Log($"[ScoreManager] P{attackerIndex} +{points:F2} (hit {hitPart}, force {force:F1}). Totals: P0={_scores[0]:F1} P1={_scores[1]:F1}");
    }

    public float GetScore(int playerIndex) => playerIndex >= 0 && playerIndex < 2 ? _scores[playerIndex] : 0f;

    public int LeadingPlayer()
    {
        if (_scores[0] > _scores[1]) return 0;
        if (_scores[1] > _scores[0]) return 1;
        return -1; // tie
    }
}

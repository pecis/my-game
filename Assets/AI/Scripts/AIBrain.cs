using System.Collections.Generic;
using UnityEngine;

public enum AIDifficulty { Easy, Medium, Hard }

/// <summary>
/// Heuristic AI that selects joint states for the AI fighter on each turn.
/// Easy = random; Medium = blended; Hard = strategy-based.
/// </summary>
public class AIBrain : MonoBehaviour
{
    public AIDifficulty difficulty = AIDifficulty.Medium;

    public void ChooseMoves(RagdollController aiFighter, RagdollController opponent)
    {
        switch (difficulty)
        {
            case AIDifficulty.Easy:   ChooseEasy(aiFighter);                   break;
            case AIDifficulty.Medium: ChooseMedium(aiFighter, opponent);       break;
            case AIDifficulty.Hard:   ChooseHard(aiFighter, opponent);         break;
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Easy — pure random
    // ──────────────────────────────────────────────────────────

    void ChooseEasy(RagdollController ai)
    {
        foreach (BodyPartType t in System.Enum.GetValues(typeof(BodyPartType)))
            ai.SetPendingState(t, AIHeuristics.RandomState());
    }

    // ──────────────────────────────────────────────────────────
    //  Medium — 60% strategy / 40% random
    // ──────────────────────────────────────────────────────────

    void ChooseMedium(RagdollController ai, RagdollController opponent)
    {
        ChooseHard(ai, opponent); // start with strategy

        // Randomly override 40% of joints
        foreach (BodyPartType t in System.Enum.GetValues(typeof(BodyPartType)))
        {
            if (Random.value < 0.4f)
                ai.SetPendingState(t, AIHeuristics.RandomState());
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Hard — strategy-based
    // ──────────────────────────────────────────────────────────

    void ChooseHard(RagdollController ai, RagdollController opponent)
    {
        bool selfOffBalance  = AIHeuristics.IsOffBalance(ai);
        bool inRange         = AIHeuristics.IsInStrikingRange(ai, opponent);
        bool oppLow          = AIHeuristics.OpponentIsLow(opponent);
        BodyPartType domArm  = AIHeuristics.GetDominantArm(ai, opponent);
        BodyPartType offArm  = domArm == BodyPartType.R_Shoulder ? BodyPartType.L_Shoulder : BodyPartType.R_Shoulder;
        BodyPartType domElbow = domArm == BodyPartType.R_Shoulder ? BodyPartType.R_Elbow : BodyPartType.L_Elbow;
        BodyPartType domWrist = domArm == BodyPartType.R_Shoulder ? BodyPartType.R_Wrist : BodyPartType.L_Wrist;

        // Default: hold everything
        foreach (BodyPartType t in System.Enum.GetValues(typeof(BodyPartType)))
            ai.SetPendingState(t, JointState.Hold);

        if (selfOffBalance)
        {
            // Recover: tighten core, relax limbs
            ai.SetPendingState(BodyPartType.Abdomen,  JointState.Contract);
            ai.SetPendingState(BodyPartType.L_Glute,  JointState.Contract);
            ai.SetPendingState(BodyPartType.R_Glute,  JointState.Contract);
            ai.SetPendingState(BodyPartType.L_Knee,   JointState.Extend);
            ai.SetPendingState(BodyPartType.R_Knee,   JointState.Extend);
        }
        else if (inRange)
        {
            // Strike with dominant arm
            ai.SetPendingState(domArm,   JointState.Extend);
            ai.SetPendingState(domElbow, JointState.Extend);
            ai.SetPendingState(domWrist, JointState.Extend);

            // Guard with off arm
            ai.SetPendingState(offArm,   JointState.Contract);
        }
        else
        {
            // Advance: lean forward, step
            ai.SetPendingState(BodyPartType.Abdomen, JointState.Contract);
            ai.SetPendingState(BodyPartType.L_Glute, oppLow ? JointState.Extend : JointState.Contract);
            ai.SetPendingState(BodyPartType.R_Glute, JointState.Hold);
        }
    }
}

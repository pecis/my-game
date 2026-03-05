using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the pending/committed joint-state cycle for one fighter.
/// TurnManager drives Freeze/Unfreeze; UI/AI write pending states.
/// </summary>
[RequireComponent(typeof(RagdollBuilder))]
public class RagdollController : MonoBehaviour
{
    // Pending states set during PlayerSelectPhase (not yet applied to physics)
    private Dictionary<BodyPartType, JointState> _pending = new();

    private RagdollBuilder _builder;

    void Awake()
    {
        _builder = GetComponent<RagdollBuilder>();
    }

    // Called once scene is ready (after RagdollBuilder.Build())
    public void Initialize()
    {
        ResetPending();
    }

    // ──────────────────────────────────────────────────────────
    //  UI / AI interface
    // ──────────────────────────────────────────────────────────

    public void SetPendingState(BodyPartType part, JointState state)
    {
        _pending[part] = state;
    }

    public JointState GetPendingState(BodyPartType part)
    {
        return _pending.TryGetValue(part, out var s) ? s : JointState.Hold;
    }

    public void ResetPending()
    {
        foreach (BodyPartType t in System.Enum.GetValues(typeof(BodyPartType)))
            _pending[t] = JointState.Hold;
    }

    // ──────────────────────────────────────────────────────────
    //  TurnManager interface
    // ──────────────────────────────────────────────────────────

    /// <summary>Applies pending states and releases frozen joints.</summary>
    public void CommitAndUnfreeze()
    {
        foreach (var kv in _pending)
        {
            if (_builder.parts.TryGetValue(kv.Key, out var bp))
                bp.SetState(kv.Value);
        }

        foreach (var kv in _builder.parts)
            SetKinematic(kv.Value, false);
    }

    /// <summary>Freezes all joints in place after simulation ends.</summary>
    public void FreezeAll()
    {
        foreach (var kv in _builder.parts)
        {
            SetKinematic(kv.Value, true);
            // Capture current pose as Hold baseline for next turn
            kv.Value.holdRotation = kv.Value.transform.localRotation;
        }
        ResetPending();
    }

    private void SetKinematic(BodyPart bp, bool kinematic)
    {
        if (bp.Rb != null)
            bp.Rb.isKinematic = kinematic;
    }

    // ──────────────────────────────────────────────────────────
    //  Queries
    // ──────────────────────────────────────────────────────────

    public bool HasVitalGroundContact()
    {
        BodyPartType[] vitals = { BodyPartType.Chest, BodyPartType.Abdomen, BodyPartType.Hips, BodyPartType.Head };
        foreach (var v in vitals)
            if (_builder.parts.TryGetValue(v, out var bp) && bp.isGrounded)
                return true;
        return false;
    }

    public Vector3 GetCenterOfMass()
    {
        Vector3 sum = Vector3.zero;
        float totalMass = 0f;
        foreach (var kv in _builder.parts)
        {
            float m = kv.Value.Rb != null ? kv.Value.Rb.mass : 1f;
            sum += kv.Value.transform.position * m;
            totalMass += m;
        }
        return totalMass > 0 ? sum / totalMass : transform.position;
    }

    public Dictionary<BodyPartType, BodyPart> Parts => _builder.parts;
}

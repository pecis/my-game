using System;
using UnityEngine;

/// <summary>
/// Counts FixedUpdate frames during SimulatePhase; fires OnComplete when done.
/// </summary>
public class SimulationRunner : MonoBehaviour
{
    public MatchRules rules;

    public event Action OnComplete;

    private bool _running;
    private int  _framesRemaining;

    public void StartSimulation()
    {
        _framesRemaining = rules != null ? rules.simulationFramesPerTurn : 40;
        _running         = true;
        Debug.Log($"[SimulationRunner] Starting simulation: {_framesRemaining} frames.");
    }

    void FixedUpdate()
    {
        if (!_running) return;

        _framesRemaining--;
        if (_framesRemaining <= 0)
        {
            _running = false;
            Debug.Log("[SimulationRunner] Simulation complete.");
            OnComplete?.Invoke();
        }
    }

    public void StopSimulation() => _running = false;

    public float Progress => rules != null
        ? 1f - (_framesRemaining / (float)rules.simulationFramesPerTurn)
        : 0f;
}

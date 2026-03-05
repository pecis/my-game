using UnityEngine;

/// <summary>
/// Entry point for the fight scene.
/// Place on a single "Bootstrap" GameObject in the scene.
/// Drag all ScriptableObject configs and prefabs in the Inspector.
///
/// SCENE SETUP CHECKLIST
/// ─────────────────────
/// 1. Create GameObject "Fighter0" — add RagdollBuilder, RagdollController
///    • Set playerIndex = 0, assign RagdollConfig + JointStateConfig assets
///    • Position at (-1.5, 1, 0)
/// 2. Create GameObject "Fighter1" — same but playerIndex = 1
///    • Position at  (1.5, 1, 0)
/// 3. Create GameObject "Arena" — a Plane with tag "Ground", scale (5,1,5)
/// 4. Create Canvas (Screen Space - Overlay)
///    • Add JointSelectionPanel (×2, one per player), HUDController, InputRouter
/// 5. Create GameObject "CameraRig" — add CameraRig; child Camera with OrbitCamera
/// 6. Create "Managers" GameObject — add TurnManager, SimulationRunner,
///    EvaluationSystem, ScoreManager. Assign all references + ScriptableObjects.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Fighters")]
    public RagdollBuilder fighter0Builder;
    public RagdollBuilder fighter1Builder;

    [Header("Managers")]
    public TurnManager    turnManager;

    void Awake()
    {
        // Build both ragdolls
        if (fighter0Builder != null) fighter0Builder.Build();
        if (fighter1Builder != null) fighter1Builder.Build();

        Debug.Log("[GameBootstrap] Ragdolls built. TurnManager will start the match.");
    }
}

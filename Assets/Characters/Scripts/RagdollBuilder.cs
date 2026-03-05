using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedurally creates the 17-joint ragdoll hierarchy under this GameObject (Hips = root).
/// Call Build() once at startup, or use the Inspector button in Editor mode.
/// </summary>
public class RagdollBuilder : MonoBehaviour
{
    [Header("Config")]
    public RagdollConfig    config;
    public JointStateConfig jointStateConfig;
    public int              playerIndex;

    // Populated after Build()
    [HideInInspector] public Dictionary<BodyPartType, BodyPart> parts = new();

    // ──────────────────────────────────────────────────────────
    //  Public API
    // ──────────────────────────────────────────────────────────

    public void Build()
    {
        parts.Clear();

        // Root – Hips (no joint)
        var hips = SetupRoot(BodyPartType.Hips, config.massHips, new Vector3(config.hipWidth, 0.15f, 0.20f));

        // Spine
        var abdomen = AddPart(BodyPartType.Abdomen, hips, new Vector3(0, 0.20f, 0), new Vector3(0.25f, 0.18f, 0.18f), config.massAbdomen, config.spineFlex);
        var chest   = AddPart(BodyPartType.Chest,   abdomen, new Vector3(0, 0.24f, 0), new Vector3(config.torsoWidth, 0.22f, 0.20f), config.massChest, config.spineFlex);
        var neck    = AddPart(BodyPartType.Neck,    chest,   new Vector3(0, 0.24f, 0), new Vector3(0.10f, 0.12f, 0.10f), config.massNeck, config.neckFlex);
        var head    = AddPart(BodyPartType.Head,    neck,    new Vector3(0, 0.18f, 0), new Vector3(config.headRadius * 2, config.headRadius * 2, config.headRadius * 2), config.massHead, config.neckFlex);

        // Arms
        float armX = config.torsoWidth / 2 + 0.08f;
        var lShoulder = AddPart(BodyPartType.L_Shoulder, chest, new Vector3(-armX, 0.15f, 0), new Vector3(0.12f, config.upperArmLength, 0.12f), config.massShoulder, config.shoulderSwing);
        var lElbow    = AddPart(BodyPartType.L_Elbow,    lShoulder, new Vector3(0, -config.upperArmLength, 0), new Vector3(0.10f, config.lowerArmLength, 0.10f), config.massElbow, config.elbowFlex);
        var lWrist    = AddPart(BodyPartType.L_Wrist,    lElbow,    new Vector3(0, -config.lowerArmLength, 0), new Vector3(0.08f, config.handLength, 0.05f), config.massWrist, config.wristFlex);

        var rShoulder = AddPart(BodyPartType.R_Shoulder, chest, new Vector3(armX, 0.15f, 0), new Vector3(0.12f, config.upperArmLength, 0.12f), config.massShoulder, config.shoulderSwing);
        var rElbow    = AddPart(BodyPartType.R_Elbow,    rShoulder, new Vector3(0, -config.upperArmLength, 0), new Vector3(0.10f, config.lowerArmLength, 0.10f), config.massElbow, config.elbowFlex);
        var rWrist    = AddPart(BodyPartType.R_Wrist,    rElbow,    new Vector3(0, -config.lowerArmLength, 0), new Vector3(0.08f, config.handLength, 0.05f), config.massWrist, config.wristFlex);

        // Legs
        float legX = config.hipWidth / 2 - 0.05f;
        var lGlute = AddPart(BodyPartType.L_Glute, hips, new Vector3(-legX, -0.12f, 0), new Vector3(0.14f, config.upperLegLength, 0.14f), config.massGlute, config.hipFlex);
        var lKnee  = AddPart(BodyPartType.L_Knee,  lGlute, new Vector3(0, -config.upperLegLength, 0), new Vector3(0.12f, config.lowerLegLength, 0.12f), config.massKnee, config.kneeFlex);
        var lAnkle = AddPart(BodyPartType.L_Ankle, lKnee,  new Vector3(0, -config.lowerLegLength, 0), new Vector3(0.10f, config.footLength, 0.08f), config.massAnkle, config.ankleFlex);

        var rGlute = AddPart(BodyPartType.R_Glute, hips, new Vector3(legX, -0.12f, 0), new Vector3(0.14f, config.upperLegLength, 0.14f), config.massGlute, config.hipFlex);
        var rKnee  = AddPart(BodyPartType.R_Knee,  rGlute, new Vector3(0, -config.upperLegLength, 0), new Vector3(0.12f, config.lowerLegLength, 0.12f), config.massKnee, config.kneeFlex);
        var rAnkle = AddPart(BodyPartType.R_Ankle, rKnee,  new Vector3(0, -config.lowerLegLength, 0), new Vector3(0.10f, config.footLength, 0.08f), config.massAnkle, config.ankleFlex);

        // Initial pose – everything Hold
        foreach (var kv in parts)
            kv.Value.SetState(JointState.Hold);

        Debug.Log($"[RagdollBuilder] Built fighter P{playerIndex} with {parts.Count} body parts.");
    }

    // ──────────────────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────────────────

    BodyPart SetupRoot(BodyPartType type, float mass, Vector3 boxSize)
    {
        var rb  = gameObject.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.solverIterations         = 20;
        rb.solverVelocityIterations = 10;

        var col = gameObject.AddComponent<BoxCollider>();
        col.size = boxSize;

        var bp = gameObject.AddComponent<BodyPart>();
        bp.partType        = type;
        bp.ownerPlayerIndex = playerIndex;
        bp.jointStateConfig = jointStateConfig;
        gameObject.AddComponent<DamageReceiver>();
        parts[type] = bp;
        return bp;
    }

    BodyPart AddPart(BodyPartType type, BodyPart parent, Vector3 localOffset, Vector3 boxSize, float mass, float angularLimit)
    {
        var go = new GameObject(type.ToString());
        go.transform.SetParent(transform);
        go.transform.localPosition = parent.transform.localPosition + localOffset;

        // Rigidbody
        var rb  = go.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.solverIterations         = 20;
        rb.solverVelocityIterations = 10;

        // Collider
        var col  = go.AddComponent<BoxCollider>();
        col.size = boxSize;

        // ConfigurableJoint
        var joint                   = go.AddComponent<ConfigurableJoint>();
        joint.connectedBody         = parent.Rb;
        joint.rotationDriveMode     = RotationDriveMode.Slerp;
        joint.xMotion               = ConfigurableJointMotion.Locked;
        joint.yMotion               = ConfigurableJointMotion.Locked;
        joint.zMotion               = ConfigurableJointMotion.Locked;
        joint.angularXMotion        = ConfigurableJointMotion.Limited;
        joint.angularYMotion        = ConfigurableJointMotion.Limited;
        joint.angularZMotion        = ConfigurableJointMotion.Limited;

        var limit = new SoftJointLimit { limit = angularLimit };
        joint.highAngularXLimit     = limit;
        joint.lowAngularXLimit      = new SoftJointLimit { limit = -angularLimit };
        joint.angularYLimit         = limit;
        joint.angularZLimit         = limit;

        // BodyPart
        var bp = go.AddComponent<BodyPart>();
        bp.partType          = type;
        bp.ownerPlayerIndex  = playerIndex;
        bp.jointStateConfig  = jointStateConfig;
        bp.contractRotation  = Quaternion.Euler(-angularLimit, 0, 0);
        bp.extendRotation    = Quaternion.Euler( angularLimit, 0, 0);
        go.AddComponent<DamageReceiver>();

        parts[type] = bp;
        return bp;
    }
}

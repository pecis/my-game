using UnityEngine;

public enum JointState { Hold, Contract, Extend, Relax }

public enum BodyPartType
{
    Hips, Abdomen, Chest, Neck, Head,
    L_Shoulder, L_Elbow, L_Wrist,
    R_Shoulder, R_Elbow, R_Wrist,
    L_Glute, L_Knee, L_Ankle,
    R_Glute, R_Knee, R_Ankle
}

[RequireComponent(typeof(Rigidbody))]
public class BodyPart : MonoBehaviour
{
    [Header("Identity")]
    public BodyPartType partType;
    public int ownerPlayerIndex; // 0 or 1

    [Header("State")]
    public JointState currentState = JointState.Hold;
    public bool isGrounded;

    [Header("Config")]
    public JointStateConfig jointStateConfig;

    // Target rotations for Contract/Extend (set by RagdollBuilder)
    [HideInInspector] public Quaternion contractRotation;
    [HideInInspector] public Quaternion extendRotation;
    [HideInInspector] public Quaternion holdRotation; // pose when Hold is set

    private ConfigurableJoint _joint;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _joint = GetComponent<ConfigurableJoint>();
    }

    public Rigidbody Rb => _rb;
    public ConfigurableJoint Joint => _joint;

    public void SetState(JointState state)
    {
        currentState = state;

        if (_joint == null) return;

        if (state == JointState.Hold)
            holdRotation = transform.localRotation;

        ApplyDrive(state);
    }

    public void ApplyDrive(JointState state)
    {
        if (_joint == null || jointStateConfig == null) return;

        JointStateConfig.JointDriveSettings settings = state switch
        {
            JointState.Hold     => jointStateConfig.hold,
            JointState.Contract => jointStateConfig.contract,
            JointState.Extend   => jointStateConfig.extend,
            JointState.Relax    => jointStateConfig.relax,
            _                   => jointStateConfig.relax
        };

        var drive = new JointDrive
        {
            positionSpring = settings.positionSpring,
            positionDamper = settings.positionDamper,
            maximumForce   = settings.maxForce
        };

        _joint.slerpDrive = drive;

        // Set target rotation
        Quaternion target = state switch
        {
            JointState.Hold     => Quaternion.Inverse(_joint.transform.rotation) * holdRotation,
            JointState.Contract => contractRotation,
            JointState.Extend   => extendRotation,
            _                   => Quaternion.identity
        };

        if (state != JointState.Relax)
            _joint.targetRotation = target;
    }

    // Called by ground detection collider or trigger
    public void SetGrounded(bool grounded) => isGrounded = grounded;
}

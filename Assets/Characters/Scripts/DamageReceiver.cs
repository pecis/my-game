using UnityEngine;

/// <summary>
/// Fires collision events to EvaluationSystem and ScoreManager.
/// Attach to every BodyPart GameObject.
/// </summary>
[RequireComponent(typeof(BodyPart))]
public class DamageReceiver : MonoBehaviour
{
    private BodyPart _bodyPart;

    void Awake() => _bodyPart = GetComponent<BodyPart>();

    void OnCollisionEnter(Collision col)
    {
        // Check if we collided with an opponent body part
        var otherPart = col.collider.GetComponent<BodyPart>();
        if (otherPart != null && otherPart.ownerPlayerIndex != _bodyPart.ownerPlayerIndex)
        {
            float impactForce = col.impulse.magnitude;
            EvaluationSystem.Instance?.RegisterImpact(_bodyPart, otherPart, impactForce);
            ScoreManager.Instance?.AddImpactScore(otherPart.ownerPlayerIndex, _bodyPart.partType, impactForce);
        }

        // Ground contact detection
        if (col.collider.CompareTag("Ground"))
        {
            _bodyPart.SetGrounded(true);
            EvaluationSystem.Instance?.RegisterGroundContact(_bodyPart);
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.collider.CompareTag("Ground"))
            _bodyPart.SetGrounded(false);
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "RagdollConfig", menuName = "Game/Ragdoll Config")]
public class RagdollConfig : ScriptableObject
{
    [Header("Body Dimensions (meters)")]
    public float hipWidth       = 0.35f;
    public float torsoHeight    = 0.50f;
    public float torsoWidth     = 0.30f;
    public float headRadius     = 0.12f;
    public float upperArmLength = 0.28f;
    public float lowerArmLength = 0.25f;
    public float handLength     = 0.10f;
    public float upperLegLength = 0.42f;
    public float lowerLegLength = 0.38f;
    public float footLength     = 0.15f;

    [Header("Masses (kg)")]
    public float massHips       = 10f;
    public float massAbdomen    = 6f;
    public float massChest      = 8f;
    public float massNeck       = 2f;
    public float massHead       = 4f;
    public float massShoulder   = 2f;
    public float massElbow      = 1.5f;
    public float massWrist      = 1f;
    public float massGlute      = 3f;
    public float massKnee       = 2f;
    public float massAnkle      = 1.5f;

    [Header("Joint Angle Limits (degrees)")]
    public float shoulderSwing  = 90f;
    public float elbowFlex      = 140f;
    public float wristFlex      = 60f;
    public float hipFlex        = 80f;
    public float kneeFlex       = 130f;
    public float ankleFlex      = 45f;
    public float spineFlex      = 30f;
    public float neckFlex       = 40f;
}

using UnityEngine;

[CreateAssetMenu(fileName = "MatchRules", menuName = "Game/Match Rules")]
public class MatchRules : ScriptableObject
{
    [Header("Turn Settings")]
    public int   simulationFramesPerTurn = 40;   // FixedUpdate frames to simulate
    public float playerSelectTimeLimit   = 30f;  // seconds per turn

    [Header("Match Settings")]
    public int   maxTurns                = 20;
    public float matchTimeLimit          = 600f; // seconds total match time

    [Header("Win Conditions")]
    public bool  instantLossOnTorsoGround = true;
    public bool  winByScoreOnTimeout      = true;

    [Header("Score Weights by Body Part")]
    public float scoreHead      = 5f;
    public float scoreChest     = 4f;
    public float scoreAbdomen   = 3f;
    public float scoreHips      = 3f;
    public float scoreLimb      = 1f;

    public float GetScoreWeight(BodyPartType part) => part switch
    {
        BodyPartType.Head    => scoreHead,
        BodyPartType.Chest   => scoreChest,
        BodyPartType.Abdomen => scoreAbdomen,
        BodyPartType.Hips    => scoreHips,
        _                    => scoreLimb
    };
}

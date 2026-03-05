using UnityEngine;

/// <summary>
/// Stateless helper methods used by AIBrain strategies.
/// </summary>
public static class AIHeuristics
{
    /// <summary>Returns true if the fighter's center of mass is outside its support polygon.</summary>
    public static bool IsOffBalance(RagdollController fighter)
    {
        Vector3 com = fighter.GetCenterOfMass();
        // Simplified: if COM horizontal distance from hips is large, fighter is off-balance
        if (!fighter.Parts.TryGetValue(BodyPartType.Hips, out var hips)) return false;
        Vector2 comXZ   = new(com.x, com.z);
        Vector2 hipsXZ  = new(hips.transform.position.x, hips.transform.position.z);
        return Vector2.Distance(comXZ, hipsXZ) > 0.25f;
    }

    /// <summary>Returns normalised direction from self to opponent (XZ plane).</summary>
    public static Vector3 DirectionToOpponent(RagdollController self, RagdollController opponent)
    {
        Vector3 toOpp = opponent.GetCenterOfMass() - self.GetCenterOfMass();
        toOpp.y = 0f;
        return toOpp.normalized;
    }

    /// <summary>Returns L or R shoulder type based on which side faces the opponent.</summary>
    public static BodyPartType GetDominantArm(RagdollController self, RagdollController opponent)
    {
        Vector3 dir = DirectionToOpponent(self, opponent);
        Vector3 right = self.Parts.TryGetValue(BodyPartType.Hips, out var h)
            ? h.transform.right
            : Vector3.right;

        return Vector3.Dot(dir, right) >= 0 ? BodyPartType.R_Shoulder : BodyPartType.L_Shoulder;
    }

    /// <summary>True if the opponent's COM y position is lower than half their own height — they're crouching/falling.</summary>
    public static bool OpponentIsLow(RagdollController opponent)
    {
        Vector3 com = opponent.GetCenterOfMass();
        if (!opponent.Parts.TryGetValue(BodyPartType.Hips, out var hips)) return false;
        return com.y < hips.transform.position.y - 0.1f;
    }

    /// <summary>True if self has a limb close enough to the opponent to land a hit.</summary>
    public static bool IsInStrikingRange(RagdollController self, RagdollController opponent, float range = 1.2f)
    {
        Vector3 oppCom = opponent.GetCenterOfMass();

        BodyPartType[] strikers = { BodyPartType.L_Wrist, BodyPartType.R_Wrist, BodyPartType.L_Ankle, BodyPartType.R_Ankle };
        foreach (var s in strikers)
        {
            if (self.Parts.TryGetValue(s, out var bp))
            {
                if (Vector3.Distance(bp.transform.position, oppCom) <= range)
                    return true;
            }
        }
        return false;
    }

    public static JointState RandomState()
    {
        var values = System.Enum.GetValues(typeof(JointState));
        return (JointState)values.GetValue(Random.Range(0, values.Length));
    }
}

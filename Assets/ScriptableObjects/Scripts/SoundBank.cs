using UnityEngine;

public enum SoundType
{
    Impact_Light,
    Impact_Heavy,
    Impact_Head,
    JointSelect,
    TurnConfirm,
    SimulationStart,
    SimulationEnd,
    MatchWin,
    MatchLose,
    UIClick,
    UIHover,
    Music_Menu,
    Music_Fight,
}

[CreateAssetMenu(fileName = "SoundBank", menuName = "Game/Sound Bank")]
public class SoundBank : ScriptableObject
{
    [System.Serializable]
    public struct SoundEntry
    {
        public SoundType type;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
        [Range(0.8f, 1.2f)] public float pitchVariance;
    }

    public SoundEntry[] sounds;

    public SoundEntry? Get(SoundType type)
    {
        foreach (var s in sounds)
            if (s.type == type) return s;
        return null;
    }
}

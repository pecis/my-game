using UnityEngine;

[CreateAssetMenu(fileName = "JointStateConfig", menuName = "Game/Joint State Config")]
public class JointStateConfig : ScriptableObject
{
    [System.Serializable]
    public struct JointDriveSettings
    {
        public float positionSpring;
        public float positionDamper;
        public float maxForce;
    }

    public JointDriveSettings hold     = new() { positionSpring = 1000, positionDamper = 100, maxForce = 10000 };
    public JointDriveSettings contract = new() { positionSpring = 800,  positionDamper = 20,  maxForce = 8000  };
    public JointDriveSettings extend   = new() { positionSpring = 800,  positionDamper = 20,  maxForce = 8000  };
    public JointDriveSettings relax    = new() { positionSpring = 0,    positionDamper = 5,   maxForce = 0     };
}

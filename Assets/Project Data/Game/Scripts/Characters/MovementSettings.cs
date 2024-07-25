using UnityEngine;
using UnityEngine.Serialization;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class MovementSettings
    {
        [FormerlySerializedAs("RotationSpeed")] public float rotationSpeed;

        [FormerlySerializedAs("MoveSpeed")] [Space]
        public float moveSpeed;
        [FormerlySerializedAs("Acceleration")] public float acceleration;

        [FormerlySerializedAs("AnimationMultiplier")] [Space]
        public DuoFloat animationMultiplier;
    }
}
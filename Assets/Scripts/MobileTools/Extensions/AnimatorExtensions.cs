using System.Linq;
using UnityEngine;
using static UnityEngine.Animator;
using static UnityEngine.AnimatorControllerParameterType;

namespace MobileTools.Extensions
{
    public static class AnimatorExtensions
    {
        const string WALKING = "Walking";
        const string IDLE = "Idle";
        const string HARVESTING = "Harvesting";
        static readonly int Walking = StringToHash(WALKING);
        static readonly int Idle = StringToHash(IDLE);
        static readonly int Harvest = StringToHash(HARVESTING);

        public static bool HasParameter(this Animator anim, string parameter, AnimatorControllerParameterType type)
            => !string.IsNullOrEmpty(parameter)
               && anim.parameters.Any(p => p.type == type && p.name == parameter);

        public static void PlayIdle(Animator animator)
        {
            if (animator.HasParameter(IDLE, Bool))
                animator.SetBool(Idle, true);
        }

        public static void StopIdle(Animator animator)
        {
            if (animator.HasParameter(IDLE, Bool))
                animator.SetBool(Idle, false);
        }

        public static void PlayWalk(Animator animator)
        {
            if (animator.HasParameter(WALKING, Bool))
                animator.SetBool(Walking, true);
        }

        public static void StopWalk(Animator animator)
        {
            if (animator.HasParameter(WALKING, Bool))
                animator.SetBool(Walking, false);
        }
        public static void PlayHarvest(Animator animator)
        {
            if (animator.HasParameter(HARVESTING, Bool))
                animator.SetBool(Harvest,true);
        }
        public static void StopHarvest(Animator animator)
        {
            if (animator.HasParameter(HARVESTING, Bool))
                animator.SetBool(Harvest,false);
        }
    }
}
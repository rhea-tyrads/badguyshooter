using UnityEngine;

namespace Watermelon
{
    public class AnimationCurveEasingFunction : Ease.IEasingFunction
    {
        AnimationCurve easingCurve;
        float totalEasingTime;

        public AnimationCurveEasingFunction(AnimationCurve easingCurve)
        {
            this.easingCurve = easingCurve;

            totalEasingTime = easingCurve.keys[easingCurve.keys.Length - 1].time;
        }

        public float Interpolate(float p)
        {
            return easingCurve.Evaluate(p * totalEasingTime);
        }
    }
}
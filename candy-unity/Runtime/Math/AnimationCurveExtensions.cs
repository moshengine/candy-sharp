using System.Linq;
using UnityEngine;

namespace General.Utility
{
    public static class AnimationCurveExtensions
    {
        public static float InvertEvaluateHigherThan(this AnimationCurve curve, float y, float xStepLength)
        {
            for (float x = curve.keys.First().time; x <= curve.keys.Last().time; x += xStepLength)
            {
                if (curve.Evaluate(x) >= y)
                {
                    return x;
                }
            }
            return 0;
        }

        public static float InvertEvaluateLowerThan(this AnimationCurve curve, float y, float xStepLength)
        {
            for (float x = curve.keys.First().time; x <= curve.keys.Last().time; x += xStepLength)
            {
                if (curve.Evaluate(x) <= y)
                {
                    return x;
                }
            }
            return 0;
        }
    }
}

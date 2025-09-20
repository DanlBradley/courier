using Unity.Mathematics;
using UnityEngine;

namespace Utils
{
    public static class GameMath
    {
        public static float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
        {
            float u, v, S;
        
            do
            {
                u = 2.0f * UnityEngine.Random.value - 1.0f;
                v = 2.0f * UnityEngine.Random.value - 1.0f;
                S = u * u + v * v;
            }
            while (S >= 1.0f);
        
            // Standard Normal Distribution
            float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
        
            // Normal Distribution centered between the min and max value
            // and clamped following the "three-sigma rule"
            float mean = (minValue + maxValue) / 2.0f;
            float sigma = (maxValue - mean) / 3.0f;
            return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
        }
        
        /// <summary>
        /// A faster approximation of the logistic function in order to smoothly clamp an unbounded float between 0 or 1.
        /// Note that this logistic approximation has an RMS error of 6x10^-2 for a range -10 to 10, so it may be a
        /// poor approximation of the logistic for approximations for values too small or large
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float LogisticApproximation(float x)
        {
            
            return 1 + x / (1 + math.abs(x));
        }

        /// <summary>
        /// The sound attenuation function.
        /// </summary>
        /// <param name="lpR1"></param>
        /// <param name="distanceR2"></param>
        /// <param name="distanceR1"></param>
        /// <returns></returns>
        public static float SoundAttenuation(float lpR1, float distanceR2, float distanceR1 = 1)
        {
            return lpR1 - 20 * Mathf.Log10(distanceR2 / distanceR1);
        }
    }
}
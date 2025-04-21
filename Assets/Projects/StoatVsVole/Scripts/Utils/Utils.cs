using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StoatVsVole
{
    /// <summary>
    /// Provides utility functions for random sampling and statistical evaluation, 
    /// particularly focused on normal distributions.
    /// </summary>
    public static class Utils
    {
        #region Public Methods

        /// <summary>
        /// Returns a random float sampled from a normal (Gaussian) distribution.
        /// </summary>
        /// <param name="mean">The mean (center) of the distribution.</param>
        /// <param name="standardDeviation">The standard deviation (spread) of the distribution.</param>
        public static float RandomNormal(float mean, float standardDeviation)
        {
            float u1 = 1.0f - UnityEngine.Random.value;
            float u2 = 1.0f - UnityEngine.Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                                  Mathf.Cos(2.0f * Mathf.PI * u2);
            return mean + standardDeviation * randStdNormal;
        }

        /// <summary>
        /// Evaluates the probability density function (PDF) of a normal distribution at a given x.
        /// </summary>
        public static float EvaluateNormalPDF(float x, float mean, float standardDeviation)
        {
            float variance = standardDeviation * standardDeviation;
            float denominator = Mathf.Sqrt(2f * Mathf.PI * variance);
            float exponent = -((x - mean) * (x - mean)) / (2f * variance);
            return (1f / denominator) * Mathf.Exp(exponent);
        }

        /// <summary>
        /// Evaluates the cumulative distribution function (CDF) of a normal distribution at a given x.
        /// </summary>
        public static float EvaluateNormalCDF(float x, float mean, float standardDeviation)
        {
            float z = (x - mean) / (Mathf.Sqrt(2f) * standardDeviation);
            return 0.5f * (1f + Erf(z));
        }

        #endregion

        public static void Shuffle<T>(List<T> list)

        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        #region Private Methods

        /// <summary>
        /// Approximates the error function (erf) using a numerical approximation.
        /// Used for calculating the normal CDF.
        /// </summary>
        private static float Erf(float x)
        {
            float t = 1.0f / (1.0f + 0.3275911f * Mathf.Abs(x));
            float tau = t * (0.254829592f + t * (-0.284496736f + t * (1.421413741f + t * (-1.453152027f + t * 1.061405429f))));
            float sign = x < 0f ? -1f : 1f;
            return sign * (1f - tau * Mathf.Exp(-x * x));
        }

        public static Bounds CalculateTotalBounds(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                // No renderers found, return an empty bounds at object's position
                return new Bounds(go.transform.position, Vector3.zero);
            }

            Bounds totalBounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
            {
                totalBounds.Encapsulate(renderers[i].bounds);
            }

            return totalBounds;
        }

        public static void PositionAgentAtGroundLevel(GameObject agentGO)
        {
            Bounds bounds = CalculateTotalBounds(agentGO);
            float heightOffset = bounds.min.y;

            Vector3 currentPosition = agentGO.transform.position;
            Vector3 correctedPosition = new Vector3(
                currentPosition.x,
                currentPosition.y - heightOffset,
                currentPosition.z
            );

            agentGO.transform.position = correctedPosition;
        }

        #endregion
    }
}

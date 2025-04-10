using UnityEngine;

namespace StoatVsVole
{
    public static class Utils
    {
        public static float RandomNormal(float mean, float standardDeviation)
        {
            float u1 = 1.0f - UnityEngine.Random.value;
            float u2 = 1.0f - UnityEngine.Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                                  Mathf.Cos(2.0f * Mathf.PI * u2);
            return mean + standardDeviation * randStdNormal;
        }

        public static float EvaluateNormalPDF(float x, float mean, float standardDeviation)
        {
            float variance = standardDeviation * standardDeviation;
            float denominator = Mathf.Sqrt(2f * Mathf.PI * variance);
            float exponent = -((x - mean) * (x - mean)) / (2f * variance);
            return (1f / denominator) * Mathf.Exp(exponent);
        }

        public static float EvaluateNormalCDF(float x, float mean, float standardDeviation)
        {
            float z = (x - mean) / (Mathf.Sqrt(2f) * standardDeviation);
            return 0.5f * (1f + Erf(z));
        }

        private static float Erf(float x)
        {
            float t = 1.0f / (1.0f + 0.3275911f * Mathf.Abs(x));
            float tau = t * (0.254829592f + t * (-0.284496736f + t * (1.421413741f + t * (-1.453152027f + t * 1.061405429f))));
            float sign = x < 0f ? -1f : 1f;
            return sign * (1f - tau * Mathf.Exp(-x * x));
        }
    }
}

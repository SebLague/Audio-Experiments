using System.Threading.Tasks;
using Audio.Core;
using static UnityEngine.Mathf;

namespace Audio.Analysis
{
    public static class SignalGenerator
    {
        const float TAU = PI * 2;

        public static Signal GenerateSignal(FrequencyData[] waves, int sampleRate, float duration)
        {
            int numPoints = CeilToInt(sampleRate * duration);
            float[] samples = Generate(waves, sampleRate, numPoints);
            return new Signal(samples, sampleRate);
        }

        public static float[] Generate(FrequencyData[] waves, int sampleRate, int numPoints)
        {
            float duration = numPoints / (float)sampleRate;
            float[] samples = new float[numPoints];

            Parallel.For(0, numPoints, i =>
            {
                float time = i / (float)(numPoints) * duration;
                float sum = 0;

                foreach (FrequencyData w in waves)
                {
                    float angle = time * TAU * w.Frequency + w.Phase;
                    sum += Cos(angle) * w.Amplitude;
                }

                samples[i] = sum;
            });

            return samples;
        }
    }
}
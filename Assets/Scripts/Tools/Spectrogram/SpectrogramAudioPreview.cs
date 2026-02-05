using Audio.Core;
using UnityEngine;

namespace Audio.Tools
{
    public class SpectrogramAudioPreview : MonoBehaviour
    {
        public bool active;
        public float gain = 1;
        public float clip = 1;
        public bool useResampling = true;

        public int info_numChannels;
        public int info_outputSampleRate;

        Signal activeSignal;
        int index;
        bool clippedLastFrame;

        double inverseOutputSampleRate;

        public void Play(Signal signal)
        {
            activeSignal = signal;
            index = 0;
            info_outputSampleRate = AudioSettings.outputSampleRate;
            inverseOutputSampleRate = 1 / (double)info_outputSampleRate;
        }

        void Update()
        {
            if (clippedLastFrame)
            {
                clippedLastFrame = false;
                Debug.Log("Audio clipping");
            }
        }

        void OnAudioFilterRead(float[] data, int numChannels)
        {
            if (!active || activeSignal == null) return;

            bool requiresResampling = useResampling && activeSignal.SampleRate != info_outputSampleRate;
            info_numChannels = numChannels;

            for (int i = 0; i < data.Length; i++)
            {
                float val = 0;

                if (requiresResampling)
                {
                    double playbackTime = index * inverseOutputSampleRate;
                    double resampledIndex_continuous = playbackTime * activeSignal.SampleRate;
                    int resampledIndex = (int)resampledIndex_continuous;
                    
                    if (resampledIndex < activeSignal.NumSamples)
                    {
                        val = activeSignal.Samples[resampledIndex];
                        if (resampledIndex + 1 < activeSignal.NumSamples)
                        {
                            float interpolationT = (float)(resampledIndex_continuous - resampledIndex);
                            float valueNext = activeSignal.Samples[resampledIndex + 1];
                            val = val * (1 - interpolationT) + valueNext * interpolationT;
                        }
                    }
                }
                else
                {
                    if (index < activeSignal.NumSamples) val = activeSignal.Samples[index];
                }

                val *= gain;
                val = Mathf.Min(clip, Mathf.Abs(val)) * Mathf.Sign(val);
                data[i] = val;

                index++;
            }
        }
    }
}
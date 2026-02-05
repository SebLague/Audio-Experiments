using Audio.Analysis;
using Audio.Core;
using UnityEngine;

namespace Audio.Helpers
{
    public static class AudioHelper
    {
        public static float AmplitudeToDecibels(float amplitude)
        {
            return 20 * Mathf.Log10(amplitude);
        }
        
        public static Signal SignalFromAudioClip(AudioClip sourceClip)
        {
            float[] data = new float[sourceClip.samples];
            Debug.Assert(sourceClip.channels == 1, "Expected mono audioclip");
            sourceClip.GetData(data, 0);
            Signal signal = new(data, sourceClip.frequency);

            return signal;
        }
    }
}
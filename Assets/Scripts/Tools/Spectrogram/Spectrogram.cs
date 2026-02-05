using System;
using System.IO;
using Audio.Analysis;
using Audio.Core;
using Audio.Helpers;
using Audio.IO;
using Seb.Helpers;
using Seb.Visualization;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using static Seb.Helpers.ComputeHelper;

namespace Audio.Tools
{
    public class Spectrogram : MonoBehaviour
    {
        public AudioClip clip;
        public string saveName;

        [Header("STFT Settings")]
        public int windowSize;
        [Range(0.25f, 1)] public float hopSize = 1;
        public bool useHannWindow;
        public bool autoRegenerate;

        [Header("Display Settings")]
        public PaintDisplayMode paintDisplayMode;
        public float frequencyDisplayMin;
        public float frequencyDisplayMax;
        public float loudnessFloorDb;
        public Gradient gradient;
        public bool useGradient;

        [Header("Display Settings Extra")]
        public FontType fontType;
        public float fontSize;
        public Vector2 textOffset;
        public float labelSpacingTarget;
        public Color textCol;
        public Color outlineCol;
        public float outlineThick;

        [Header("References")]
        public MeshRenderer meshRenderer;
        public ComputeShader spectrogramCompute;
        public Paint paint;

        // State
        Signal signal;
        STFT.StftResult stft;
        bool hasUpdatedSinceSettingsChange;
        float amplitudeMax;
        float maxFrequencyInSpectrum;

        // Textures
        Texture2D spectrogramMap;
        RenderTexture paintMap;
        Texture2D gradientTex;


        enum Kernels
        {
            UpdatePaintMap = 0,
            Readback = 1,
        }

        public enum PaintDisplayMode
        {
            PaintOverlay,
            PaintOnly
        }

        void Start()
        {
            Regenerate();
        }

        void Update()
        {
            if (!hasUpdatedSinceSettingsChange)
            {
                TextureFromGradient(gradient, 256, ref gradientTex);
                if (autoRegenerate) Regenerate();
            }

            HandleCompute();
            ApplyShaderParams();
            HandleInput();
            DrawLabels();
        }

        void HandleCompute()
        {
            spectrogramCompute.SetFloat("maxFrequencyInSpectrum", maxFrequencyInSpectrum);
            spectrogramCompute.SetFloat("frequencyDisplayMin", frequencyDisplayMin);
            spectrogramCompute.SetFloat("frequencyDisplayMax", frequencyDisplayMax);
            spectrogramCompute.SetInts("resolution", spectrogramMap.width, spectrogramMap.height);


            spectrogramCompute.SetTexture(0, "PaintInput", paint.combined);
            spectrogramCompute.SetTexture(0, "SpectrogramMap", spectrogramMap);
            spectrogramCompute.SetTexture(0, "PaintMap", paintMap);
            Dispatch(spectrogramCompute, spectrogramMap.width, spectrogramMap.height, Kernels.UpdatePaintMap);
        }


        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                int numModes = Enum.GetNames(typeof(PaintDisplayMode)).Length;
                int nextModeIndex = ((int)paintDisplayMode + 1) % numModes;
                paintDisplayMode = (PaintDisplayMode)nextModeIndex;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Signal synthSignal = SpectrogramToSignal();
                string fileName = $"{saveName}.wav";
                string directoryPath = Path.Combine(FileHelper.ProjectDirectory, "AudioOutput");
                string path = Path.Combine(directoryPath, fileName);
                Directory.CreateDirectory(directoryPath);
                Wav.WriteWav(synthSignal, path);
                Debug.Log("Saved to " + path);
            }


            if (Input.GetKeyDown(KeyCode.Space))
            {
                Signal synthSignal = SpectrogramToSignal();
                FindFirstObjectByType<SpectrogramAudioPreview>().Play(synthSignal);
            }
        }


        void Regenerate()
        {
            hasUpdatedSinceSettingsChange = true;

            signal = AudioHelper.SignalFromAudioClip(clip);
            stft = STFT.Compute(signal, windowSize, hopSize, useHannWindow);
            RegenerateTexture(stft);
            PaintInit();
            ApplyShaderParams();
        }

        void PaintInit()
        {
            paint.Init(meshRenderer.transform.localScale, meshRenderer.transform.position);
        }

        void RegenerateTexture(STFT.StftResult stft)
        {
            int width = stft.Segments.Length;
            int height = stft.Segments[0].Spectrum.Length;

            // Spectrogram map
            spectrogramMap = new Texture2D(width, height, GraphicsFormat.R32_SFloat, TextureCreationFlags.None)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            // Paint map
            CreateRenderTexture(ref paintMap, width, height, R_SFloat);
            paintMap.filterMode = FilterMode.Point;

            //
            Color[] cols = new Color[width * height];
            maxFrequencyInSpectrum = stft.Segments[0].Spectrum[^1].Frequency;
            amplitudeMax = 0;

            // Store amplitude in red component of time-frequency color map
            for (int timeIndex = 0; timeIndex < width; timeIndex++)
            {
                FrequencyData[] spectrum = stft.Segments[timeIndex].Spectrum;

                for (int freqIndex = 0; freqIndex < spectrum.Length; freqIndex++)
                {
                    float amplitude = spectrum[freqIndex].Amplitude;
                    amplitudeMax = Mathf.Max(amplitudeMax, amplitude);

                    int pixelIndex = freqIndex * width + timeIndex;
                    cols[pixelIndex].r = amplitude;
                }
            }

            // Create texture
            CreateTexture2D(ref spectrogramMap, width, height, GraphicsFormat.R32_SFloat, FilterMode.Point);
            spectrogramMap.SetPixels(cols);
            spectrogramMap.Apply();
        }


        void ApplyShaderParams()
        {
            Material mat = meshRenderer.sharedMaterial;
            mat.SetFloat("maxFrequencyInSpectrum", maxFrequencyInSpectrum);
            mat.SetFloat("frequencyDisplayMin", frequencyDisplayMin);
            mat.SetFloat("frequencyDisplayMax", frequencyDisplayMax);
            mat.SetFloat("amplitudeMax", amplitudeMax);
            mat.SetFloat("decibelsDisplayMin", loudnessFloorDb);
            mat.SetFloat("decibelsDisplayMax", AudioHelper.AmplitudeToDecibels(amplitudeMax));
            mat.SetInt("useGradient", useGradient ? 1 : 0);
            mat.SetInt("paintMode", (int)paintDisplayMode);

            // Textures
            mat.SetTexture("SpectrogramMap", spectrogramMap);
            mat.SetTexture("PaintMap", paintMap);
            mat.SetTexture("GradientTex", gradientTex);
        }

        public Signal SpectrogramToSignal()
        {
            bool paintOnly = paintDisplayMode == PaintDisplayMode.PaintOnly;
            bool applyWindow = paintOnly;
            bool usePhase = !paintOnly;

            // Get data from texture
            ComputeBuffer buffer = CreateStructuredBuffer<float>(spectrogramMap.width * spectrogramMap.height);
            spectrogramCompute.SetTexture((int)Kernels.Readback, "PaintMap", paintMap);
            spectrogramCompute.SetTexture((int)Kernels.Readback, "SpectrogramMap", spectrogramMap);
            spectrogramCompute.SetBuffer((int)Kernels.Readback, "ReadbackBuffer", buffer);
            spectrogramCompute.SetInt("paintMode", (int)paintDisplayMode);
            Dispatch(spectrogramCompute, spectrogramMap.width, spectrogramMap.height, Kernels.Readback);

            float[] data = new float[buffer.count];
            buffer.GetData(data);
            Release(buffer);

            // Create stft from data
            STFT.StftSegment[] synthSegs = new STFT.StftSegment[stft.Segments.Length];
            int index = 0;

            for (int i = 0; i < stft.Segments.Length; i++)
            {
                STFT.StftSegment source = stft.Segments[i];
                FrequencyData[] spectrum = new FrequencyData[source.Spectrum.Length];
                for (int j = 0; j < spectrum.Length; j++)
                {
                    spectrum[j].Frequency = source.Spectrum[j].Frequency;
                    spectrum[j].Phase = usePhase ? source.Spectrum[j].Phase : 0;
                    spectrum[j].Amplitude = data[index];
                    index++;
                }

                STFT.StftSegment seg = new(spectrum, source.SampleOffset, source.SampleCount);
                synthSegs[i] = seg;
            }


            STFT.StftResult synth = new(synthSegs, signal.SampleRate, signal.NumSamples, stft.useWindow);
            Signal reconstructed = STFT.ReconstructSignal(synth, applyWindow);
            return reconstructed;
        }

        public static void TextureFromGradient(Gradient gradient, int width, ref Texture2D texture)
        {
            if (texture == null || texture.width != width || texture.height != 1)
            {
                texture = new Texture2D(width, 1);
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
            }

            Color[] colours = new Color[width];
            for (int i = 0; i < width; i++)
            {
                float t = i / (width - 1f);
                colours[i] = gradient.Evaluate(t);
            }

            texture.SetPixels(colours);
            texture.Apply();
        }

        void DrawLabels()
        {
            Vis.StartLayer(Vector2.zero, 1, false);
            Vector2 centre = meshRenderer.transform.position;
            Vector2 size = meshRenderer.transform.localScale;
            Vector2 bottomLeft = centre - size / 2;

            Vis.QuadOutline(centre, size, outlineThick, outlineCol);

            Vis.Text(fontType, "LMB to paint | Esc to clear | Tab to switch paint mode | Space to play | S to save to file", fontSize, Vector2.up * 5, Anchor.Centre, Color.gray3);

            labelSpacingTarget = Mathf.Max(0.25f, labelSpacingTarget);
            // X labels
            {
                int numX = (int)(size.x / labelSpacingTarget);
                numX = Mathf.Max(2, numX);

                for (int i = 0; i < numX; i++)
                {
                    float t = i / (numX - 1f);
                    float duration = stft.SampleCount / (float)stft.SampleRate;
                    float s = duration * t;
                    Vector2 pos = new Vector2(bottomLeft.x + size.x * t, bottomLeft.y + textOffset.y);
                    string text = $"{s:0.##}";
                    Vis.Text(fontType, text, fontSize, pos, Anchor.CentreTop, textCol);
                }
            }
            // Y labels
            {
                int numY = (int)(size.y / labelSpacingTarget);
                numY = Mathf.Max(2, numY);

                for (int i = 0; i < numY; i++)
                {
                    float t = i / (numY - 1f);
                    float freq = Mathf.Lerp(frequencyDisplayMin, frequencyDisplayMax, t);
                    string text = $"{freq:0.##}";
                    if (freq >= 1000) text = $"{freq / 1000:0.#}k";
                    Vector2 pos = new Vector2(bottomLeft.x + textOffset.x, bottomLeft.y + size.y * t);
                    Vis.Text(fontType, text, fontSize, pos, Anchor.TextCentreRight, textCol);
                }
            }
        }

        void OnDestroy()
        {
            Release(paintMap);
        }

        void OnValidate()
        {
            hasUpdatedSinceSettingsChange = false;
        }
    }
}
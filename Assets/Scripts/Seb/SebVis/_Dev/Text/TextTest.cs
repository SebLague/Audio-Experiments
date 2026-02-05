using System;
using Seb.Visualization.Text.Rendering;
using UnityEngine;

namespace Seb.Visualization.Tests
{
    [ExecuteAlways]
    public class TextTest : MonoBehaviour
    {
        public bool screenSpace;
        public FontType font;

        [Multiline(3)] public string text;

        public float fontSize;
        public float lineSpacing = 1;
        public Anchor anchor;
        public Color col;
        public Vector2 layerOffset;
        public float layerScale;
        public bool lineBreakMyMaxChars;
        public int maxCharCountPerLine;

        void Start()
        {
            if (Application.isPlaying)
            {
                string text = "𝄞";
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    bool isHighSurrogate = char.IsHighSurrogate(c);
                    bool isLowSurrogate = char.IsLowSurrogate(c);
                    Debug.Log(i + ": " + c);
                    Debug.Log($"is high {isHighSurrogate} is low {isLowSurrogate}");
                }

                uint pair = CombineSurrogatePair(text[0], text[1]);
                Debug.Log(pair);
            }
        }
        
        uint CombineSurrogatePair(char high, char low)
        {
            if (!char.IsHighSurrogate(high) || !char.IsLowSurrogate(low))
                throw new ArgumentException("Invalid surrogate pair");

            uint h = (uint)(high - 0xD800);
            uint l = (uint)(low - 0xDC00);

            return 0x10000u + (h << 10) + l;
        }

        void Update()
        {
            string displayText = text;
            if (lineBreakMyMaxChars)
            {
                displayText = UI.UI.LineBreakByCharCount(text, maxCharCountPerLine);
            }

            Vis.StartLayerIfNotInMatching(layerOffset, layerScale, screenSpace);

            TextRenderer.BoundingBox bounds = Vis.CalculateTextBounds(displayText, font, fontSize, transform.position, anchor, lineSpacing);
            Vis.QuadMinMax(bounds.BoundsMin, bounds.BoundsMax, Color.black);


            Vis.Text(font, displayText, fontSize, transform.position, anchor, col, lineSpacing);
        }
    }
}
using System.Collections.Generic;
using Seb.Visualization.Text.FontLoading;
using UnityEngine;

namespace Seb.Visualization.Text.Rendering
{
	public class TextRenderData
	{
		// [Shader Data] for each glyph, stores: bezier points
		public readonly List<Vector2> bezierPoints = new();

		// [Shader Data] for each unique glyph, stores: bezier offset, num contours, contour length/s
		public readonly List<int> glyphMetadata = new();
		public readonly Dictionary<Glyph, int> glyphMetadataIndexLookup = new();
	}
}
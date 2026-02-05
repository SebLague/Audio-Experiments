using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Seb.Stuff
{
	public static class ExtensionMethods
	{

		// --- Vector extensions ---

		public static Vector3 WithZ(this Vector3 v, float z)
		{
			return new Vector3(v.x, v.y, z);
		}

		public static Vector3 WithZ(this Vector2 v, float z)
		{
			return new Vector3(v.x, v.y, z);
		}
        
		public static Vector2 WithX(this Vector2 v, float x)
		{
			return new Vector2(x, v.y);
		}
        
		public static Vector2 WithY(this Vector2 v, float y)
		{
			return new Vector2(v.x, y);
		}
        
		public static Vector3 WithX(this Vector3 v, float x)
		{
			return new Vector3(x, v.y, v.z);
		}
        
		public static Vector3 WithY(this Vector3 v, float y)
		{
			return new Vector3(v.x, y, v.z);
		}

		// --- Colour extensions ---
		public static Color WithAlpha(this Color col, float alpha)
		{
			return new Color(col.r, col.g, col.b, Mathf.Clamp01(alpha));
		}
        
		public static Color MulRGB(this Color col, float v)
		{
			return new Color(col.r * v, col.g * v, col.b * v, col.a);
		}

		public static string ToRichTextColourTag(this Color col)
		{
			return $"<color=#{ColorUtility.ToHtmlStringRGBA(col)}>";
		}
	}
}
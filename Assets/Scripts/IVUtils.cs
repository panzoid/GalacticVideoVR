using UnityEngine;
using System;

public class IVUtils {

	public static float calculateRadius(int edgeLength, int vertices) {
		return (float) (edgeLength / (2 * Math.Sin (Math.PI / vertices)));
	}

	public static Vector3 calculateScale(float length, float distance) {
		float scale = length / distance;
		return Vector3.one * scale;
	}

	public static Color calculateTransparency(float length, float distance, bool reverse) {
		Color color = Color.white;

		if (reverse) {
			color.a = (float)Math.Pow ((2000-length)/1000, 4);
		} else {
			color.a = (float)Math.Pow (length / distance, 4);
		}

		return color;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackMaker : MonoBehaviour {
	//if (x0 != x1) {
	//    SolveQuadratic(x0, y0, FindSlope(a, b, x0), x1, y1, out a, out b, out c);
	//}

	public static float FindSlope(float a, float b, float x) {
		return 2 * a * x + b;
	}

    public static void SolveQuadratic(float x0, float y0, float y, float x1, float y1, out float a, out float b, out float c) {
		float dx = x0 - x1;
		float dy = y0 - y1;
		float d = dx * dx;
		float x02 = x0 * x0;
		float x12 = x1 * x1;
		a = (y * dx - dy) / d;
		b = (2 * x0 * dy - y * (x02 - x12)) / d;
		c = (x02 * (x1 * y + y1) - x0 * x1 * (x1 * y + 2 * y0) + x12 * y0) / d;
	}
}

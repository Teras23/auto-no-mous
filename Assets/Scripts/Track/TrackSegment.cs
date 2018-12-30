using UnityEngine;

public class TrackSegment : MonoBehaviour{
	float x0, y0, x1, y1, a, b, c;

	public void SetData(float x0, float y0, float x1, float y1, float a, float b, float c) {
		this.x0 = x0;
		this.y0 = y0;
		this.x1 = x1;
		this.y1 = y1;
		this.a = a;
		this.b = b;
		this.c = c;
	}

	public void GetData(out float x, out float y, out float a, out float b, out float c) {
		x = x0;
		y = y0;
		a = this.a;
		b = this.b;
		c = this.c;
	}
}

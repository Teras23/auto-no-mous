using UnityEngine;

public class TrackSegment : MonoBehaviour{
	float x, y, ax, bx, cx, ay, by, cy;

	public void SetData(float x, float y, float ax, float bx, float cx, float ay, float by, float cy) {
		this.x = x;
		this.y = y;
		this.ax = ax;
		this.bx = bx;
		this.cx = cx;
		this.ay = ay;
		this.by = by;
		this.cy = cy;
	}

	public void GetData(out float x, out float y, out float ax, out float bx, out float cx, out float ay, out float by, out float cy) {
		x = this.x;
		y = this.y;
		ax = this.ax;
		bx = this.bx;
		cx = this.cx;
		ay = this.ay;
		by = this.by;
		cy = this.cy;
	}
}

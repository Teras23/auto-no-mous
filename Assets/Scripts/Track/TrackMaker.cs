using System.Collections.Generic;
using UnityEngine;

public class TrackMaker : MonoBehaviour {
	public TrackSegment segment;
	public float trackWidth, wallWidth;
	public int segmentCount;
	Stack<TrackSegment> pieceHistory = new Stack<TrackSegment>();
	float x, y, ax, bx, cx, ay, by, cy;
	Camera cam;
	bool placeSucceeded = false;

	void Awake() {
		cam = Camera.main;
	}

	void Update() {
		if (!Input.GetButtonDown("Fire1") && placeSucceeded) {
			Remove();
		}
		if (!Input.GetButtonDown("Cancel")) {
			placeSucceeded = PlaceNew();
		}
	}

	public bool PlaceNew() {
		//Find coordinate on plane
		Ray forward = cam.ScreenPointToRay(Input.mousePosition);
		new Plane(Vector3.back, Vector3.zero).Raycast(forward, out float distance);
		Vector3 mouseLoc = forward.origin + forward.direction * distance;
		float x1 = mouseLoc.x;
		float y1 = mouseLoc.y;

		//Create a new piece
		float xSlope = FindSlope(ax, bx, 1);
		float ySlope = FindSlope(ay, by, 1);
		SolveQuadratic(xSlope, x, x1, out ax, out bx, out cx);
		SolveQuadratic(ySlope, y, y1, out ay, out by, out cy);
		TrackSegment newPiece = Instantiate(segment, transform);
		newPiece.SetData(x1, y1, ax, bx, cx, ay, by, cy);
		pieceHistory.Push(newPiece);

		//TEMP
		int i = 0;
		for (float t = 0, wallInterval = 1f / segmentCount; i < segmentCount; i++) {
			t += wallInterval;
			Debug.DrawLine(new Vector3(x, y), new Vector3((ax * t + bx) * t + cx, (ay * t + by) * t + cy), Color.red);
			x = (ax * t + bx) * t + cx;
			y = (ay * t + by) * t + cy;
		}

		/*
		//Find segment locations along the path and construct a mesh and colliders
		Mesh mesh = new Mesh();
		newPiece.GetComponent<MeshFilter>().mesh = mesh;
		Vector3[] vertices = new Vector3[segmentCount * 8 + 8];
		int[] triangles = new int[segmentCount * 48];
		EdgeCollider2D[] segmentColliders = newPiece.GetComponents<EdgeCollider2D>();
		segmentColliders[0].edgeRadius = wallWidth;
		segmentColliders[1].edgeRadius = wallWidth;
		Vector2[] leftColliderPoints = new Vector2[segmentCount + 1];
		Vector2[] rightColliderPoints = new Vector2[segmentCount + 1];
		int i = 0;
		float antiA = float.NaN, antiB = float.NaN, wallDX = 0;
		//First iteration
		if (localSlope > 1e-4 || localSlope < -1e-4) {
			FindAntiSlope(x, y, localSlope, out antiA, out antiB);
			wallDX = (antiA > 0 ? 1 : -1) / Mathf.Sqrt(antiA * antiA + 1);
		}
		//Left side
		FindLeftEdges(isRight, x, y, localSlope, wallDX, antiA, antiB, out float colliderX, out float outerWallX, out float innerWallX, out float colliderY, out float outerWallY, out float innerWallY);
		leftColliderPoints[0] = new Vector2(colliderX, colliderY);
		vertices[0] = new Vector3(outerWallX, outerWallY, -2 * wallWidth);
		vertices[1] = new Vector3(innerWallX, innerWallY, -2 * wallWidth);
		vertices[2] = new Vector3(innerWallX, innerWallY, 0);
		vertices[3] = new Vector3(outerWallX, outerWallY, 0);
		//Right side
		FindRightEdges(isRight, x, y, localSlope, wallDX, antiA, antiB, out colliderX, out outerWallX, out innerWallX, out colliderY, out outerWallY, out innerWallY);
		rightColliderPoints[0] = new Vector2(colliderX, colliderY);
		vertices[4] = new Vector3(outerWallX, outerWallY, -2 * wallWidth);
		vertices[5] = new Vector3(innerWallX, innerWallY, -2 * wallWidth);
		vertices[6] = new Vector3(innerWallX, innerWallY, 0);
		vertices[7] = new Vector3(outerWallX, outerWallY, 0);
		//Subsequent iterations
		for (float wallInterval = (x1 - x) / segmentCount; i < segmentCount; i++) {
			int i8 = i * 8;
			int i48 = i8 * 6;
			x += wallInterval;
			y = a * x * x + b * x + c;

			//Find wall and collider points
			localSlope = FindSlope(a, b, x);
			if (localSlope > 1e-4 || localSlope < -1e-4) {
				FindAntiSlope(x, y, localSlope, out antiA, out antiB);
				wallDX = (antiA > 0 ? 1 : -1) / Mathf.Sqrt(antiA * antiA + 1);
			}
			//Left side
			FindLeftEdges(isRight, x, y, localSlope, wallDX, antiA, antiB, out colliderX, out outerWallX, out innerWallX, out colliderY, out outerWallY, out innerWallY);
			leftColliderPoints[i + 1] = new Vector2(colliderX, colliderY);
			vertices[i8 + 8] = new Vector3(outerWallX, outerWallY, -2 * wallWidth);
			vertices[i8 + 9] = new Vector3(innerWallX, innerWallY, -2 * wallWidth);
			vertices[i8 + 10] = new Vector3(innerWallX, innerWallY, 0);
			vertices[i8 + 11] = new Vector3(outerWallX, outerWallY, 0);
			int[] indices = new int[] { 0, 8, 9, 0, 9, 1, 1, 9, 10, 1, 10, 2, 3, 10, 11, 3, 2, 10, 0, 11, 8, 0, 3, 11, 13, 12, 4, 5, 13, 4, 14, 13, 5, 6, 14, 5, 15, 14, 7, 14, 6, 7, 12, 15, 4, 15, 7, 4 };
			for (int index = 0; index < 24; index++) {
				triangles[i48 + index] = i8 + indices[index];
			}
			//Right side
			FindRightEdges(isRight, x, y, localSlope, wallDX, antiA, antiB, out colliderX, out outerWallX, out innerWallX, out colliderY, out outerWallY, out innerWallY);
			rightColliderPoints[i + 1] = new Vector2(colliderX, colliderY);
			vertices[i8 + 12] = new Vector3(outerWallX, outerWallY, -2 * wallWidth);
			vertices[i8 + 13] = new Vector3(innerWallX, innerWallY, -2 * wallWidth);
			vertices[i8 + 14] = new Vector3(innerWallX, innerWallY, 0);
			vertices[i8 + 15] = new Vector3(outerWallX, outerWallY, 0);
			for (int index = 24; index < 48; index++) {
				triangles[i48 + index] = i8 + indices[index];
			}
		}
		

		//Update last values
		segmentColliders[0].points = leftColliderPoints;
		segmentColliders[1].points = rightColliderPoints;
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		*/
		x = x1;
		y = y1;
		return true;
	}

	public void Remove() {
		//Destroy the last piece on the stack and revert to the values before that
		if (pieceHistory.Count > 0) {
			TrackSegment lastPiece = pieceHistory.Pop();
			Destroy(lastPiece.gameObject);
			if (pieceHistory.Count > 0) {
				lastPiece = pieceHistory.Peek();
				lastPiece.GetData(out x, out y, out ax, out bx, out cx, out ay, out by, out cy);
				return;
			}
		}

		//Stack is empty, use default values
		x = 0;
		y = 0;
		ax = 0;
		bx = 0;
		cx = 0;
		ay = 0;
		by = 0;
		cy = 0;
	}

	void FindLeftEdges(bool isRight, float x, float y, float localSlope, float wallDX, float antiA, float antiB, out float colliderX, out float outerWallX, out float innerWallX, out float colliderY, out float outerWallY, out float innerWallY) {
		if (localSlope > 1e-4 || localSlope < -1e-4) {
			colliderX = isRight ? x + wallDX * trackWidth : x - wallDX * trackWidth;
			outerWallX = isRight ? x + wallDX * (trackWidth + wallWidth) : x - wallDX * (trackWidth + wallWidth);
			innerWallX = isRight ? x + wallDX * (trackWidth - wallWidth) : x - wallDX * (trackWidth - wallWidth);
			colliderY = antiA * colliderX + antiB;
			outerWallY = antiA * outerWallX + antiB;
			innerWallY = antiA * innerWallX + antiB;
		} else {
			colliderX = x;
			outerWallX = x;
			innerWallX = x;
			colliderY = isRight ? y + trackWidth : y - trackWidth;
			outerWallY = isRight ? colliderY + wallWidth : colliderY - wallWidth;
			innerWallY = isRight ? colliderY - wallWidth : colliderY + wallWidth;
		}
	}

	void FindRightEdges(bool isRight, float x, float y, float localSlope, float wallDX, float antiA, float antiB, out float colliderX, out float outerWallX, out float innerWallX, out float colliderY, out float outerWallY, out float innerWallY) {
		if (localSlope > 1e-4 || localSlope < -1e-4) {
			colliderX = isRight ? x - wallDX * trackWidth : x + wallDX * trackWidth;
			outerWallX = isRight ? x - wallDX * (trackWidth + wallWidth) : x + wallDX * (trackWidth + wallWidth);
			innerWallX = isRight ? x - wallDX * (trackWidth - wallWidth) : x + wallDX * (trackWidth - wallWidth);
			colliderY = antiA * colliderX + antiB;
			outerWallY = antiA * outerWallX + antiB;
			innerWallY = antiA * innerWallX + antiB;
		} else {
			colliderX = x;
			outerWallX = x;
			innerWallX = x;
			colliderY = isRight ? y - trackWidth : y + trackWidth;
			outerWallY = isRight ? colliderY - wallWidth : colliderY + wallWidth;
			innerWallY = isRight ? colliderY + wallWidth : colliderY - wallWidth;
		}
	}

	static float FindSlope(float a, float b, float t) {
		return 2 * a * t + b;
	}

	static void FindAntiSlope(float x, float y, float slope, out float a, out float b) {
		a = -1 / slope;
		b = y + x / slope;
	}

    static void SolveQuadratic(float slope, float x0, float x1, out float a, out float b, out float c) {
		c = x0;
		b = slope;
		a = x1 - x0 - slope;
	}
}
